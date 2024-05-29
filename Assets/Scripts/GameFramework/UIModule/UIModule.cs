using Config;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;

// UI模块，负责管理UI的显示和关闭
public partial class UIModule : BaseGameModule
{
    public Transform normalUIRoot; // 普通UI的根节点
    public Transform modalUIRoot; // 模态UI的根节点
    public Transform closeUIRoot; // 关闭UI的根节点
    public Image imgMask; // 遮罩图片
    public QuantumConsole prefabQuantumConsole; // Quantum Console的预制体

    private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING; // UI的Mediator映射表
    private static Dictionary<UIViewID, Type> ASSET_MAPPING; // UI的Asset映射表

    private readonly List<UIMediator> usingMediators = new List<UIMediator>(); // 正在使用中的Mediator列表
    private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>(); // 空闲的Mediator队列
    private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>(); // UI对象池
    private QuantumConsole quantumConsole; // Quantum Console实例

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        //quantumConsole = Instantiate(prefabQuantumConsole);
        //quantumConsole.transform.SetParentAndResetAll(transform);
        //quantumConsole.OnActivate += OnConsoleActive;
        //quantumConsole.OnDeactivate += OnConsoleDeactive;
    }

    protected internal override void OnModuleStop()
    {
        //base.OnModuleStop();
        //quantumConsole.OnActivate -= OnConsoleActive;
        //quantumConsole.OnDeactivate -= OnConsoleDeactive;
    }

    // 缓存UI的Mediator和Asset映射表
    private static void CacheUIMapping()
    {
        if (MEDIATOR_MAPPING != null)
            return;

        MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();
        ASSET_MAPPING = new Dictionary<UIViewID, Type>();

        Type baseViewType = typeof(UIView);
        foreach (var type in baseViewType.Assembly.GetTypes())
        {
            if (type.IsAbstract)
                continue;

            if (baseViewType.IsAssignableFrom(type))
            {
                object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);
                if (attrs.Length == 0)
                {
                    Debug.LogError($"{type.FullName} 没有绑定 Mediator，请使用UIMediatorAttribute绑定一个Mediator以正确使用");
                    continue;
                }

                foreach (UIViewAttribute attr in attrs)
                {
                    MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);
                    ASSET_MAPPING.Add(attr.ID, type);
                    break;
                }
            }
        }
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        uiObjectPool.UpdateLoadRequests();
        foreach (var mediator in usingMediators)
        {
            mediator.Update(deltaTime);
        }
        UpdateMask(deltaTime);
    }

    private void OnConsoleActive()
    {
        //GameManager.Input.SetEnable(false);
    }

    private void OnConsoleDeactive()
    {
        //GameManager.Input.SetEnable(true);
    }

    // 获取指定模式下最顶层的Mediator的sortingOrder值
    private int GetTopMediatorSortingOrder(UIMode mode)
    {
        int lastIndexMediatorOfMode = -1;
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            UIMediator mediator = usingMediators[i];
            if (mediator.UIMode != mode)
                continue;

            lastIndexMediatorOfMode = i;
            break;
        }

        if (lastIndexMediatorOfMode == -1)
            return mode == UIMode.Normal ? 0 : 1000;

        return usingMediators[lastIndexMediatorOfMode].SortingOrder;
    }

    // 根据UIViewID获取对应的Mediator实例
    private UIMediator GetMediator(UIViewID id)
    {
        CacheUIMapping();

        if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))
        {
            Debug.LogError($"找不到 {id} 对应的Mediator");
            return null;
        }

        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
        {
            mediatorQ = new Queue<UIMediator>();
            freeMediators.Add(mediatorType, mediatorQ);
        }

        UIMediator mediator;
        if (mediatorQ.Count == 0)
        {
            mediator = Activator.CreateInstance(mediatorType) as UIMediator;
        }
        else
        {
            mediator = mediatorQ.Dequeue();
        }

        return mediator;
    }

    // 回收Mediator到空闲队列中
    private void RecycleMediator(UIMediator mediator)
    {
        if (mediator == null)
            return;

        Type mediatorType = mediator.GetType();
        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
        {
            mediatorQ = new Queue<UIMediator>();
            freeMediators.Add(mediatorType, mediatorQ);
        }
        mediatorQ.Enqueue(mediator);
    }

    // 获取正在打开的UI对应的Mediator实例
    public UIMediator GetOpeningUIMediator(UIViewID id)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            return null;

        Type requiredMediatorType = mediator.GetType();
        foreach (var item in usingMediators)
        {
            if (item.GetType() == requiredMediatorType)
                return item;
        }
        return null;
    }

    // 将指定UI的Mediator移到最顶层
    public void BringToTop(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator == null)
            return;

        int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);
        if (mediator.SortingOrder == topSortingOrder)
            return;

        int sortingOrder = topSortingOrder + 10;
        mediator.SortingOrder = sortingOrder;

        usingMediators.Remove(mediator);
        usingMediators.Add(mediator);

        Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder;
        }
    }

    // 判断指定的UI是否已经打开
    public bool IsUIOpened(UIViewID id)
    {
        return GetOpeningUIMediator(id) != null;
    }

    // 打开指定的UI，并返回对应的Mediator实例
    public UIMediator OpenUISingle(UIViewID id, object arg = null)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator != null)
            return mediator;

        return OpenUI(id, arg);
    }

    // 打开指定的UI，并返回对应的Mediator实例
    public UIMediator OpenUI(UIViewID id, object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            return null;

        GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();
            mediator.InitMediator(newView);
        })).gameObject;
        return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
    }

    // 异步打开指定的UI，并返回对应的Mediator实例
    public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
    {
        if (!IsUIOpened(id))
        {
            yield return OpenUIAsync(id, arg);
        }
    }

    // 异步打开指定的UI，并返回对应的Mediator实例
    public IEnumerator OpenUIAsync(UIViewID id,  object arg = null)
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);
        if (uiConfig.IsNull)
            yield break;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            yield break;

        bool loadFinish = false;
        uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
        {
            GameObject uiObject = asset.gameObject;
            OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
            loadFinish = true;
        }, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();
            mediator.InitMediator(newView);
        });
        while (!loadFinish)
        {
            yield return null;
        }
        yield return null;
        yield return null;
    }

    // 当UI对象加载完成后的回调方法
    private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
    {
        if (uiObject == null)
        {
            Debug.LogError($"加载UI失败:{uiConfig.Asset}");
            RecycleMediator(mediator);
            return null;
        }

        UIView view = uiObject.GetComponent<UIView>();
        if (view == null)
        {
            Debug.LogError($"UI Prefab不包含UIView脚本:{uiConfig.Asset}");
            RecycleMediator(mediator);
            uiObjectPool.UnloadGameObject(view.gameObject);
            return null;
        }

        mediator.UIMode = uiConfig.Mode;
        int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;

        usingMediators.Add(mediator);

        Canvas canvas = uiObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //canvas.worldCamera = GameManager.Camera.uiCamera;
        if (uiConfig.Mode == UIMode.Normal)
        {
            uiObject.transform.SetParentAndResetAll(normalUIRoot);
            canvas.sortingLayerName = "NormalUI";
        }
        else
        {
            uiObject.transform.SetParentAndResetAll(modalUIRoot);
            canvas.sortingLayerName = "ModalUI";
        }

        mediator.SortingOrder = sortingOrder;
        canvas.sortingOrder = sortingOrder;

        uiObject.SetActive(true);
        mediator.Show(uiObject, obj);
        return mediator;
    }

    // 关闭指定的UI
    public void CloseUI(UIMediator mediator)
    {
        if (mediator != null)
        {
            // 回收View
            uiObjectPool.UnloadGameObject(mediator.ViewObject);
            mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

            // 回收Mediator
            mediator.Hide();
            RecycleMediator(mediator);

            usingMediators.Remove(mediator);
        }
    }

    // 关闭所有UI
    public void CloseAllUI()
    {
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            CloseUI(usingMediators[i]);
        }
    }

    // 关闭指定的UI
    public void CloseUI(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator == null)
            return;

        CloseUI(mediator);
    }

    // 设置所有普通UI的可见性
    public void SetAllNormalUIVisibility(bool visible)
    {
        normalUIRoot.gameObject.SetActive(visible);
    }

    // 设置所有模态UI的可见性
    public void SetAllModalUIVisibility(bool visible)
    {
        modalUIRoot.gameObject.SetActive(visible);
    }

    // 显示遮罩
    public void ShowMask(float duration = 0.5f)
    {
        destMaskAlpha = 1;
        maskDuration = duration;
    }

    // 隐藏遮罩
    public void HideMask(float? duration = null)
    {
        destMaskAlpha = 0;
        if (duration.HasValue)
        {
            maskDuration = duration.Value;
        }
    }

    private float destMaskAlpha = 0;
    private float maskDuration = 0;
    private void UpdateMask(float deltaTime)
    {
        Color c = imgMask.color;
        c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
        c.a = Mathf.Clamp01(c.a);
        imgMask.color = c;
        imgMask.enabled = imgMask.color.a > 0;
    }

    // 显示Quantum Console
    public void ShowConsole()
    {
        quantumConsole.Activate();
    }
}

// UIView的特性，用于绑定Mediator
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; }
    public Type MediatorType { get; }

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id;
        MediatorType = mediatorType;
    }
}
