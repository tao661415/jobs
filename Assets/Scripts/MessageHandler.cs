using System;
using System.Threading.Tasks;

// 定义一个消息处理器接口
public interface IMessageHander
{
    Type GetHandlerType(); // 获取处理器类型
}

// 消息处理器基类，泛型类型为 T，T 必须是结构体
[MessageHandler]
public abstract class MessageHandler<T> : IMessageHander where T : struct
{
    // 实现接口中的方法，返回处理器类型
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    // 抽象方法，子类需要实现消息处理逻辑
    public abstract Task HandleMessage(T arg); // 处理消息任务
}

// 定义一个特性，用于标记消息处理器类
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class MessageHandlerAttribute : Attribute { } // 消息处理器特性