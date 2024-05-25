using System;
using System.Collections.Generic;

// 对象池类，实现了 IDisposable 接口
public class ObjectPool<T> : IDisposable where T : new()
{
    public int MaxCacheCount = 32;  // 最大缓存数量

    private static LinkedList<T> cache;  // 缓存链表
    private Action<T> onRelease;  // 释放对象的动作

    // 构造方法，初始化缓存链表和释放对象的动作
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>();
        this.onRelease = onRelease;
    }

    // 获取对象方法
    public T Obtain()
    {
        T value;
        if (cache.Count == 0)  // 如果缓存为空
        {
            value = new T();  // 创建新的对象
        }
        else
        {
            value = cache.First.Value;  // 从缓存中获取对象
            cache.RemoveFirst();  // 在缓存中移除对象
        }
        return value;
    }

    // 释放对象方法
    public void Release(T value)
    {
        if (cache.Count >= MaxCacheCount)  // 如果缓存数量达到最大值
            return;

        onRelease?.Invoke(value);  // 执行释放对象的动作
        cache.AddLast(value);  // 将对象添加到缓存中
    }

    // 清空缓存方法
    public void Clear()
    {
        cache.Clear();  // 清空缓存
    }

    // 释放资源方法
    public void Dispose()
    {
        cache = null;  // 清空缓存
        onRelease = null;  // 清空释放对象的动作
    }
}

// 队列对象池类
public class QueuePool<T>
{
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());  // 队列对象池实例
    public static Queue<T> Obtain() => pool.Obtain();  // 获取队列对象的静态方法
    public static void Release(Queue<T> value) => pool.Release(value);  // 释放队列对象的静态方法
    public static void Clear() => pool.Clear();  // 清空队列对象池的静态方法
}

// 列表对象池类
public class ListPool<T>
{
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());  // 列表对象池实例
    public static List<T> Obtain() => pool.Obtain();  // 获取列表对象的静态方法
    public static void Release(List<T> value) => pool.Release(value);  // 释放列表对象的静态方法
    public static void Clear() => pool.Clear();  // 清空列表对象池的静态方法
}

// 哈希集对象池类
public class HashSetPool<T>
{
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());  // 哈希集对象池实例
    public static HashSet<T> Obtain() => pool.Obtain();  // 获取哈希集对象的静态方法
    public static void Release(HashSet<T> value) => pool.Release(value);  // 释放哈希集对象的静态方法
    public static void Clear() => pool.Clear();  // 清空哈希集对象池的静态方法
}

// 字典对象池类
public class DictionaryPool<K, V>
{
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());  // 字典对象池实例
    public static Dictionary<K, V> Obtain() => pool.Obtain();  // 获取字典对象的静态方法
    public static void Release(Dictionary<K, V> value) => pool.Release(value);  // 释放字典对象的静态方法
    public static void Clear() => pool.Clear();  // 清空字典对象池的静态方法
}