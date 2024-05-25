// 引入必要的命名空间
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 资源组件
    /// </summary>
    // [Module(1)]
    // public static AssetModule Asset { get => TGameFramework.Instance.GetModule<AssetModule>(); }
    /// <summary>
    /// 流程组件
    /// </summary>
    // [Module(2)]
    // public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }
    // [Module(3)]
    // public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    //[Module(4)]
    //public static TimeModule Time { get => TGameFramework.Instance.GetModule<TimeModule>(); }
    //[Module(5)]
    //public static AudioModule Audio { get => TGameFramework.Instance.GetModule<AudioModule>(); }

    [Module(7)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    // [Module(8)]
    // public static GameControllersModule Controllers => TGameFramework.Instance.GetModule<GameControllersModule>();
    // [Module(9)]
    // public static ECSModule ECS { get => TGameFramework.Instance.GetModule<ECSModule>(); }
    //
    // [Module(97)]
    // public static NetModule Net { get => TGameFramework.Instance.GetModule<NetModule>(); }

    //[Module(98)]
    //public static SaveModule Save { get => TGameFramework.Instance.GetModule<SaveModule>(); }
    ///// 定时器模块
    ///// </summary>
    //[Module(99)]
    //public static ScheduleModule Schedule { get => TGameFramework.Instance.GetModule<ScheduleModule>(); }

    private bool activing;

    public static GameManager Instance;
    // public Role MainRole = null; 

    // 在游戏对象唤醒时执行的方法
    private void Awake()
    {
        // 设置Instance为当前实例
        Instance = this;
        // 如果TGameFramework实例不为空，则销毁当前游戏对象，防止重复实例化
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        activing = true;
        // 在场景切换时不销毁当前游戏对象
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        UnityLog.StartupEditor();
#else
            UnityLog.Startup();
#endif

        // 注册日志接收事件
        Application.logMessageReceived += OnReceiveLog;
        // 初始化TGameFramework
        TGameFramework.Initialize();
        // 启动模块
        StartupModules();
        // 初始化所有模块
        TGameFramework.Instance.InitModules();
    }

    // 在游戏开始时执行的方法
    private void Start()
    {
        // 启动所有模块
        TGameFramework.Instance.StartModules();
        // Procedure.StartProcedure().Coroutine();
    }

    // 在每帧更新时执行的方法
    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    // 在每帧更新后执行的方法
    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    // 在固定更新时执行的方法
    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }

    // 在游戏对象销毁时执行的方法
    private void OnDestroy()
    {
        if (activing)
        {
            // 移除日志接收事件
            Application.logMessageReceived -= OnReceiveLog;
            // 销毁TGameFramework实例
            TGameFramework.Instance.Destroy();
        }
    }

    // 在应用程序退出时执行的方法
    private void OnApplicationQuit()
    {
        //UnityLog.Teardown();
    }

    /// <summary>
    /// 初始化模块
    /// </summary>
    public void StartupModules()
    {
        // 初始化模块属性列表
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        // 获取当前类的所有属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        // 模块的基类类型
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;

            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;

            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
            moduleAttrs.Add(moduleAttr);
        }

        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });

        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    // 处理日志消息的方法
    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
            if (type == LogType.Exception)
            {
                UnityLog.Fatal($"{condition}\n{stackTrace}");
            }
#endif
    }
}