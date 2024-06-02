using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;


public class ECSModule : BaseGameModule
{
    public ECSWorld World { get; private set; }

    private Dictionary<Type, IAwakeSystem> awakeSystemMap;
    private Dictionary<Type, IDestroySystem> destroySystemMap;

    private Dictionary<Type, IUpdateSystem> updateSystemMap;
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;

    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;

    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;

    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        LoadAllSystems();
        World = new ECSWorld();
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        DriveUpdateSystem();
        
    }

    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        base.OnModuleLateUpdate(deltaTime);
        DriveLateUpdateSystem();
    }

    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// 模块初始化时调用，加载所有系统
    /// </summary>
    public void LoadAllSystems()
    {
        // 初始化各个系统的字典
        awakeSystemMap = new Dictionary<Type, IAwakeSystem>();
        destroySystemMap = new Dictionary<Type, IDestroySystem>();

        updateSystemMap = new Dictionary<Type, IUpdateSystem>();
        updateSystemRelatedEntityMap = new Dictionary<IUpdateSystem, List<ECSEntity>>();

        lateUpdateSystemMap = new Dictionary<Type, ILateUpdateSystem>();
        lateUpdateSystemRelatedEntityMap = new Dictionary<ILateUpdateSystem, List<ECSEntity>>();

        fixedUpdateSystemMap = new Dictionary<Type, IFixedUpdateSystem>();
        fixedUpdateSystemRelatedEntityMap = new Dictionary<IFixedUpdateSystem, List<ECSEntity>>();

        entityMessageHandlerMap = new Dictionary<Type, List<IEntityMessageHandler>>();
        entityRpcHandlerMap = new Dictionary<Type, IEntityRpcHandler>();

        // 遍历当前程序集中的所有类型
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;

            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)
            {
                // 初始化AwakeSystem
                Type awakeSystemType = typeof(IAwakeSystem);
                if (awakeSystemType.IsAssignableFrom(type))
                {
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem;
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // 初始化DestroySystem
                Type destroySystemType = typeof(IDestroySystem);
                if (destroySystemType.IsAssignableFrom(type))
                {
                    if (destroySystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    IDestroySystem destroySytem = Activator.CreateInstance(type) as IDestroySystem;
                    destroySystemMap.Add(type, destroySytem);
                }

                // 初始化UpdateSystem
                Type updateSystemType = typeof(IUpdateSystem);
                if (updateSystemType.IsAssignableFrom(type))
                {
                    if (updateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Update System:{type.FullName}");
                        continue;
                    }

                    IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                    updateSystemMap.Add(type, updateSystem);

                    updateSystemRelatedEntityMap.Add(updateSystem, new List<ECSEntity>());
                }

                // 初始化LateUpdateSystem
                Type lateUpdateSystemType = typeof(ILateUpdateSystem);
                if (lateUpdateSystemType.IsAssignableFrom(type))
                {
                    if (lateUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    ILateUpdateSystem lateUpdateSystem = Activator.CreateInstance(type) as ILateUpdateSystem;
                    lateUpdateSystemMap.Add(type, lateUpdateSystem);

                    lateUpdateSystemRelatedEntityMap.Add(lateUpdateSystem, new List<ECSEntity>());
                }

                // 初始化FixedUpdateSystem
                Type fixedUpdateSystemType = typeof(IFixedUpdateSystem);
                if (fixedUpdateSystemType.IsAssignableFrom(type))
                {
                    if (fixedUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                    fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                    fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                }
            }

            if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
            {
                // 初始化EntityMessageHandler
                Type entityMessageType = typeof(IEntityMessageHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;

                    if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                    {
                        list = new List<IEntityMessageHandler>();
                        entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                    }

                    list.Add(entityMessageHandler);
                }
            }

            if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
            {
                // 初始化EntityRpcHandler
                Type entityMessageType = typeof(IEntityRpcHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;

                    if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                    {
                        UnityLog.Error($"Duplicate Entity Rpc, type:{entityRpcHandler.RpcType().FullName}");
                        continue;
                    }

                    entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                }
            }
        }
    }

    // 驱动UpdateSystem，遍历所有注册的UpdateSystem，对关联的实体进行循环更新
    private void DriveUpdateSystem()
    {
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池中获取一个临时列表，将updateSystem关联的实体全部添加到这个临时列表中
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                // 检查updateSystem是否观察了该实体，若未观察则跳过该实体的更新
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // 调用updateSystem的Update方法对该实体进行更新
                updateSystem.Update(entity);
            }

            // 将临时列表释放回对象池
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    // 驱动LateUpdateSystem，遍历所有注册的LateUpdateSystem，对关联的实体进行循环 LateUpdate
    private void DriveLateUpdateSystem()
    {
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
            if (lateUpdateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池中获取一个临时列表，将lateUpdateSystem关联的实体全部添加到这个临时列表中
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                // 检查lateUpdateSystem是否观察了该实体，若未观察则跳过该实体的LateUpdate
                if (!lateUpdateSystem.ObservingEntity(entity))
                    continue;

                // 调用lateUpdateSystem的LateUpdate方法对该实体进行更新
                lateUpdateSystem.LateUpdate(entity);
            }

            // 将临时列表释放回对象池
            ListPool<ECSEntity>.Release(entityList);
        }
    }
    // 驱动FixedUpdateSystem，遍历所有注册的FixedUpdateSystem，对关联的实体进行循环FixedUpdate
    private void DriveFixedUpdateSystem()
    {
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (fixedUpdateSystemRelatedEntities.Count == 0)
                continue;

            // 从对象池中获取一个临时列表，将fixedUpdateSystem关联的实体全部添加到这个临时列表中
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                // 检查fixedUpdateSystem是否观察了该实体，若未观察则跳过该实体的FixedUpdate
                if (!fixedUpdateSystem.ObservingEntity(entity))
                    continue;

                // 调用fixedUpdateSystem的FixedUpdate方法对该实体进行更新
                fixedUpdateSystem.FixedUpdate(entity);
            }

            // 将临时列表释放回对象池
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    // 根据组件类型获取对应的AwakeSystem列表
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            // 判断该AwakeSystem是否与指定组件类型C匹配，若匹配则将其添加到列表中
            if (awakeSystem.ComponentType() == typeof(C))
            {
                list.Add(awakeSystem);
            }
        }
    }

    // 触发指定组件类型C的Awake操作
    public void AwakeComponent<C>(C component) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的AwakeSystem
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // 将AwakeSystem转换为特定类型的AwakeSystem<C>，若不能转换则继续下一轮循环
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            // 调用AwakeSystem的Awake方法来处理指定组件C的Awake操作
            awakeSystem.Awake(component);
            found = true;
        }

        // 将临时列表释放回对象池
        ListPool<IAwakeSystem>.Release(list);

        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}>");
        }
    }

    // 触发指定组件类型C和参数P1的Awake操作
    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的AwakeSystem
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        // 通过ECSModule获取与指定组件类型C匹配的AwakeSystem列表
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // 将AwakeSystem转换为特定类型的AwakeSystem<C, P1>，若不能转换则继续下一轮循环
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            // 调用AwakeSystem的Awake方法来处理指定组件C和参数P1的Awake操作
            awakeSystem.Awake(component, p1);
            found = true;
        }

        // 将临时列表释放回对象池
        ListPool<IAwakeSystem>.Release(list);

        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}>");
        }
    }

    // 触发指定组件类型C和参数P1、P2的Awake操作
    public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的AwakeSystem
        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        // 通过ECSModule获取与指定组件类型C匹配的AwakeSystem列表
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            // 将AwakeSystem转换为特定类型的AwakeSystem<C, P1, P2>，若不能转换则继续下一轮循环
            AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
            if (awakeSystem == null)
                continue;

            // 调用AwakeSystem的Awake方法来处理指定组件C和参数P1、P2的Awake操作
            awakeSystem.Awake(component, p1, p2);
            found = true;
        }

        // 将临时列表释放回对象池
        ListPool<IAwakeSystem>.Release(list);

        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
        }
    }

    // 根据组件类型获取对应的DestroySystem列表
    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // 判断该DestroySystem是否与指定组件类型C匹配，若匹配则将其添加到列表中
            if (destroySystem.ComponentType() == typeof(C))
            {
                list.Add(destroySystem);
            }
        }
    }
    // 根据组件类型获取对应的DestroySystem列表
    private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            // 判断该DestroySystem是否与指定组件类型匹配，若匹配则将其添加到列表中
            if (destroySystem.ComponentType() == componentType)
            {
                list.Add(destroySystem);
            }
        }
    }

    // 销毁指定类型C的组件
    public void DestroyComponent<C>(C component) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的DestroySystem
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        // 获取与指定类型C匹配的DestroySystem列表
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            // 将DestroySystem转换为特定类型的DestroySystem<C>，若不能转换则继续下一轮循环
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            // 调用DestroySystem的Destroy方法来处理销毁指定类型C的组件操作，同时标记组件为已销毁状态
            destroySystem.Destroy(component);
            component.Disposed = true;
        }

        // 将临时列表释放回对象池
        ListPool<IDestroySystem>.Release(list);
    }

    // 销毁任意类型的组件
    public void DestroyComponent(ECSComponent component)
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的DestroySystem
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        // 获取与该组件类型匹配的DestroySystem列表
        GetDestroySystems(component.GetType(), list);
        foreach (var item in list)
        {
            // 调用DestroySystem的Destroy方法来处理销毁该组件操作，同时标记组件为已销毁状态
            item.Destroy(component);
            component.Disposed = true;
        }

        // 将临时列表释放回对象池
        ListPool<IDestroySystem>.Release(list);
    }

    // 销毁指定类型C和参数P1的组件
    public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的DestroySystem
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        // 获取与指定类型C匹配的DestroySystem列表
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            // 将DestroySystem转换为特定类型的DestroySystem<C, P1>，若不能转换则继续下一轮循环
            DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
            if (destroySystem == null)
                continue;

            // 调用DestroySystem的Destroy方法来处理销毁指定类型C和参数P1的组件操作，同时标记组件为已销毁状态
            destroySystem.Destroy(component, p1);
            component.Disposed = true;
        }

        // 将临时列表释放回对象池
        ListPool<IDestroySystem>.Release(list);
    }

    // 销毁指定类型C和参数P1、P2的组件
    public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        // 更新与该组件关联的实体列表
        UpdateSystemEntityList(component.Entity);

        // 从对象池中获取一个临时列表存储匹配组件类型的DestroySystem
        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        // 获取与指定类型C匹配的DestroySystem列表
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            // 将DestroySystem转换为特定类型的DestroySystem<C, P1, P2>，若不能转换则继续下一轮循环
            DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
            if (destroySystem == null)
                continue;

            // 调用DestroySystem的Destroy方法来处理销毁指定类型C和参数P1、P2的组件操作，同时标记组件为已销毁状态
            destroySystem.Destroy(component, p1, p2);
            component.Disposed = true;
        }

        // 将临时列表释放回对象池
        ListPool<IDestroySystem>.Release(list);
    }

    // 更新与实体关联的系统的实体列表
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 更新与UpdateSystem关联的实体列表
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];
            if (!entityList.Contains(entity))
            {
                // 如果实体不在列表中，检查是否应该关注该实体，如果是则添加到列表中
                if (updateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                // 如果实体在列表中，检查是否不再关注该实体，如果是则从列表中移除
                if (!updateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }

        // 类似逻辑适用于LateUpdateSystem和FixedUpdateSystem
        // 省略...

    }

    // 向实体列表中添加实体
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    // 从实体列表中移除实体
    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        entities.Remove(entity.InstanceID);
        // 如果实体存在于场景中，调用场景的RemoveEntity方法将其从场景中移除
        ECSScene scene = entity.Scene;
        scene?.RemoveEntity(entity.InstanceID);
    }

    // 根据给定的实体ID查找对应的实体
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id);
    }

    // 根据给定的实体ID查找对应的特定类型T的实体
    public T FindEntity<T>(long id) where T : ECSEntity
    {
        // 从实体列表中根据ID获取对应的实体，并转换为指定类型T返回
        entities.TryGetValue(id, out ECSEntity entity);
        return entity as T;
    }

    // 查找在实体上的指定组件，通过指定的实体ID
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        // 查找具有指定ID的实体，并返回该实体上的指定类型T的组件
        return FindEntity(entityID)?.GetComponent<T>();
    }

    // 向实体发送消息
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        if (id == 0)
            return;

        // 获取指定ID的实体
        ECSEntity entity = FindEntity(id);
        if (entity == null)
            return;

        // 获取消息类型
        Type messageType = m.GetType();
        // 根据消息类型查找对应的实体消息处理程序列表
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return;

        // 从对象池中获取一个临时列表来存储实体消息处理程序
        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();
        entityMessageHandlers.AddRangeNonAlloc(list);
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
        {
            // 调用实体消息处理程序的Post方法处理消息，并等待异步操作完成
            await handler.Post(entity, m);
        }

        // 释放临时列表
        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
    }

    // 向实体发送RPC请求，并等待RPC响应
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request) where Response : IEntityRpcResponse, new()
    {
        if (entityID == 0)
            return new Response() { Error = true };

        // 获取指定ID的实体
        ECSEntity entity = FindEntity(entityID);
        if (entity == null)
            return new Response() { Error = true };

        // 获取请求类型
        Type messageType = request.GetType();
        // 根据请求类型查找对应的实体RPC处理程序
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true };

        // 将实体RPC处理程序转换为特定类型的处理程序，并发送RPC请求，并等待RPC响应
        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
        if (handler == null)
            return new Response() { Error = true };

        return await handler.Post(entity, request);
    }
}

