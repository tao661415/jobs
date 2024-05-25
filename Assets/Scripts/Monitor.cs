using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// 监视器类
public class Monitor
{
    // 存储不同类型的等待对象
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();

    // 等待方法，返回特定类型的等待对象
    public WaitObject<T> Wait<T>() where T : struct
    {
        WaitObject<T> o = new WaitObject<T>();
        waitObjects.Add(typeof(T), o);
        return o;
    }

    // 设置结果方法，根据类型设置对应结果
    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);
        if (!waitObjects.TryGetValue(type, out object o))
            return;

        waitObjects.Remove(type);
        ((WaitObject<T>)o).SetResult(result);
    }

    // 等待对象类
    public class WaitObject<T> : INotifyCompletion where T : struct
    {
        // 是否完成标志
        public bool IsCompleted { get; private set; }
        // 结果
        public T Result { get; private set; }

        private Action callback;

        // 设置结果方法
        public void SetResult(T result)
        {
            Result = result;
            IsCompleted = true;

            Action c = callback;
            callback = null;
            c?.Invoke();
        }

        // 获取等待对象
        public WaitObject<T> GetAwaiter()
        {
            return this;
        }

        // 完成时回调方法
        public void OnCompleted(Action callback)
        {
            this.callback = callback;
        }

        // 获取结果方法
        public T GetResult()
        {
            return Result;
        }
    }
}