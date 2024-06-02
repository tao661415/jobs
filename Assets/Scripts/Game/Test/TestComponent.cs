using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestComponent : ECSComponent
{
    public int i = 0;
}
public class TestComponentAwakeSystem : AwakeSystem<TestComponent>
{
    public override void Awake(TestComponent c)
    {
        Debug.Log("TestComponentAwakeSystem");
    }
}
public class TestComponentUpdateSystem : UpdateSystem<TestComponent>
{
    public override void Update(ECSEntity entity)
    {
        entity.GetComponent<TestComponent>().i++;
        if (entity.GetComponent<TestComponent>().i<20)
        {
            Debug.Log("TestComponentUpdateSystem");
        }
        else
        {
            GameManager.ECS.World.RemoveComponent<TestComponent>();
        }
       
    }
}
public class TestComponentFixedUpdateSystem : FixedUpdateSystem<TestComponent>
{
    public override void FixedUpdate(ECSEntity entity)
    {
        Debug.Log("TestComponentFixedUpdateSystem");
    }
}
public class TestComponentDestroySystem : DestroySystem<TestComponent>
{
    public override void Destroy(TestComponent c)
    {
        Debug.Log("TestComponentDestroySystem");
    }
}