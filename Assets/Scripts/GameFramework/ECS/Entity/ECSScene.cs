using System.Collections.Generic;

// ECS�����࣬�̳���ECSEntity
public class ECSScene : ECSEntity
{
    // ʵ���ֵ䣬�洢�����е�ʵ��
    private Dictionary<long, ECSEntity> entities;

    // ���캯������ʼ������ʵ���ֵ�
    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }

    // ��дDispose�������ͷų�����Դ
    public override void Dispose()
    {
        if (Disposed)
            return;

        // ��ȡ����������ʵ���ID�б�
        List<long> entityIDList = ListPool<long>.Obtain();
        foreach (var entityID in entities.Keys)
        {
            entityIDList.Add(entityID);
        }

        // ����ͷų����е�ʵ����Դ
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];
            entity.Dispose();
        }

        // �ͷ�ID�б���Դ
        ListPool<long>.Release(entityIDList);

        base.Dispose();
    }

    // ���ʵ�嵽������
    public void AddEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        // ��ʵ���ԭ�������Ƴ�
        ECSScene oldScene = entity.Scene;
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);
        }

        // ��ʵ����ӵ�����ʵ���ֵ���
        entities.Add(entity.InstanceID, entity);
        entity.SceneID = InstanceID;
        UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
    }

    // �ӳ������Ƴ�ָ��ID��ʵ��
    public void RemoveEntity(long entityID)
    {
        if (entities.Remove(entityID))
        {
            UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
        }
    }

    // ���Ҿ���ָ�����͵�ʵ�壬������ID��ӵ��б���
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

    // ���Ҿ���ָ��������͵�ʵ�壬������ID��ӵ��б���
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

    // ��ȡ����������ʵ���ID�б�
    public void GetAllEntities(List<long> list)
    {
        foreach (var item in entities)
        {
            list.Add(item.Key);
        }
    }
}