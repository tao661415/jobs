using System;
using System.Collections.Generic;

// 定义 ECS 实体类
public class ECSEntity : IDisposable
{
    // 实体的唯一标识
    public long InstanceID { get; private set; }
    // 父实体的唯一标识
    public long ParentID { get; private set; }
    // 标识实体是否已销毁
    public bool Disposed { get; private set; }

    // 获取父实体
    public ECSEntity Parent
    {
        get
        {
            if (ParentID == 0)
                return default;

            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(ParentID);
        }
    }

    // 场景ID
    public long SceneID { get; set; }
    // 获取场景
    public ECSScene Scene
    {
        get
        {
            if (SceneID == 0)
                return default;

            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(SceneID) as ECSScene;
        }
    }

    // 存储子实体的列表
    private List<ECSEntity> children = new List<ECSEntity>();
    // 存储组件的字典，键为类型，值为组件实例
    private Dictionary<Type, ECSComponent> componentMap = new Dictionary<Type, ECSComponent>();

    // 构造方法，初始化实体
    public ECSEntity()
    {
        InstanceID = IDGenerator.NewInstanceID();
        TGameFramework.Instance.GetModule<ECSModule>().AddEntity(this);
    }

    // 销毁实体
    public virtual void Dispose()
    {
        if (Disposed)
            return;

        Disposed = true;
        // 销毁子实体
        for (int i = children.Count - 1; i >= 0; i--)
        {
            ECSEntity child = children[i];
            children.RemoveAt(i);
            child?.Dispose();
        }

        // 销毁组件
        List<ECSComponent> componentList = ListPool<ECSComponent>.Obtain();
        foreach (var component in componentMap.Values)
        {
            componentList.Add(component);
        }

        foreach (var component in componentList)
        {
            componentMap.Remove(component.GetType());
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent(component);
        }
        ListPool<ECSComponent>.Release(componentList);

        // 从父节点移除
        Parent?.RemoveChild(this);
        // 从世界中移除
        TGameFramework.Instance.GetModule<ECSModule>().RemoveEntity(this);
    }

    // 判断实体是否拥有特定类型的组件
    public bool HasComponent<C>() where C : ECSComponent
    {
        return componentMap.ContainsKey(typeof(C));
    }

    // 获取特定类型的组件
    public C GetComponent<C>() where C : ECSComponent
    {
        componentMap.TryGetValue(typeof(C), out var component);
        return component as C;
    }

    // 添加新的组件
    public C AddNewComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    // 添加带参数的新组件
    public C AddNewComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    // 添加带参数的新组件
    public C AddNewComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        return component;
    }

    // 添加组件
    public C AddComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    // 添加带参数的组件
    public C AddComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    // 添加带参数的组件
    public C AddComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        return component;
    }

    // 移除组件
    public void RemoveComponent<C>() where C : ECSComponent, new()
    {
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        componentMap.Remove(componentType);
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component);
    }

    // 从实体上移除指定类型的组件，并销毁该组件
    public void RemoveComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        // 获取指定类型的组件
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        // 从组件列表中移除该组件
        componentMap.Remove(componentType);
        // 销毁该组件
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1);
    }

    // 从实体上移除指定类型的组件，并销毁该组件
    public void RemoveComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        // 获取指定类型的组件
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        // 从组件列表中移除该组件
        componentMap.Remove(componentType);
        // 销毁该组件
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1, p2);
    }

    // 添加子实体
    public void AddChild(ECSEntity child)
    {
        if (child == null)
            return;

        if (child.Disposed)
            return;

        // 若子实体已有父节点，则先将其从原父节点移除
        ECSEntity oldParent = child.Parent;
        if (oldParent != null)
        {
            oldParent.RemoveChild(child);
        }

        // 将子实体添加到当前实体的子实体列表中
        children.Add(child);
        child.ParentID = InstanceID;
    }

    // 移除子实体
    public void RemoveChild(ECSEntity child)
    {
        if (child == null)
            return;

        // 从子实体列表中移除指定子实体
        children.Remove(child);
        child.ParentID = 0;
    }

    // 查找子实体
    public T FindChild<T>(long id) where T : ECSEntity
    {
        // 遍历子实体列表，根据ID查找指定类型的子实体
        foreach (var child in children)
        {
            if (child.InstanceID == id)
                return child as T;
        }

        return default;
    }

    // 查找子实体
    public T FindChild<T>(Predicate<T> predicate) where T : ECSEntity
    {
        // 遍历子实体列表，根据条件查找指定类型的子实体
        foreach (var child in children)
        {
            T c = child as T;
            if (c == null)
                continue;

            if (predicate.Invoke(c))
            {
                return c;
            }
        }

        return default;
    }

    // 查找子实体
    public void FindChildren<T>(List<T> list) where T : ECSEntity
    {
        // 遍历子实体列表，将符合条件的子实体添加到列表中
        foreach (var child in children)
        {
            if (child is T)
            {
                list.Add(child as T);
            }
        }
    }
}

