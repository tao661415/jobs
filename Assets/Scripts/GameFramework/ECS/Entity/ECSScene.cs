using System.Collections.Generic;

// ECS场景类，继承自ECSEntity
public class ECSScene : ECSEntity
{
    // 实体字典，存储场景中的实体
    private Dictionary<long, ECSEntity> entities;

    // 构造函数，初始化场景实体字典
    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }

    // 重写Dispose方法，释放场景资源
    public override void Dispose()
    {
        if (Disposed)
            return;

        // 获取场景中所有实体的ID列表
        List<long> entityIDList = ListPool<long>.Obtain();
        foreach (var entityID in entities.Keys)
        {
            entityIDList.Add(entityID);
        }

        // 逐个释放场景中的实体资源
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];
            entity.Dispose();
        }

        // 释放ID列表资源
        ListPool<long>.Release(entityIDList);

        base.Dispose();
    }

    // 添加实体到场景中
    public void AddEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        // 将实体从原场景中移除
        ECSScene oldScene = entity.Scene;
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);
        }

        // 将实体添加到场景实体字典中
        entities.Add(entity.InstanceID, entity);
        entity.SceneID = InstanceID;
        UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
    }

    // 从场景中移除指定ID的实体
    public void RemoveEntity(long entityID)
    {
        if (entities.Remove(entityID))
        {
            UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
        }
    }

    // 查找具有指定类型的实体，并将其ID添加到列表中
    public void FindEntities<T>(List<long> list) where T : ECSEntity
    {
        foreach (var item in entities)
        {
            if (item.Value is T)
            {
                list.Add(item.Key);
            }
        }
    }

    // 查找具有指定组件类型的实体，并将其ID添加到列表中
    public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
    {
        foreach (var item in entities)
        {
            if (item.Value.HasComponent<T>())
            {
                list.Add(item.Key);
            }
        }
    }

    // 获取场景中所有实体的ID列表
    public void GetAllEntities(List<long> list)
    {
        foreach (var item in entities)
        {
            list.Add(item.Key);
        }
    }
}