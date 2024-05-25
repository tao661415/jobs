using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

// 消息模块类，继承自基础游戏模块
public class MessageModule : BaseGameModule
{
    // 定义一个委托，用于处理消息的事件参数
    public delegate Task MessageHandlerEventArgs<T>(T arg);

    // 全局消息处理器字典和本地消息处理器字典
    private Dictionary<Type, List<object>> globalMessageHandlers;
    private Dictionary<Type, List<object>> localMessageHandlers;

    // 公开的监视器属性
    public Monitor Monitor { get; private set; }

    // 模块初始化方法
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    // 模块停止方法
    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }

    // 加载所有消息处理器方法
    private void LoadAllMessageHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;

            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
            if (messageHandlerAttribute != null)
            {
                IMessageHander messageHandler = Activator.CreateInstance(type) as IMessageHander;
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());
                }
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);
            }
        }
    }

    // 订阅消息处理器方法
    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        Type argType = typeof(T);
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))
        {
            handlerList = new List<object>();
            localMessageHandlers.Add(argType, handlerList);
        }

        handlerList.Add(handler);
    }

    // 取消订阅消息处理器方法
    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList))
            return;

        handlerList.Remove(handler);
    }

    // 发送消息方法
    public async Task Post<T>(T arg) where T : struct
    {
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;

                await messageHandler.HandleMessage(arg);
            }
        }

        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            List<object> list = ListPool<object>.Obtain();
            list.AddRangeNonAlloc(localHandlerList);
            foreach (var handler in list)
            {
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg);
            }
            ListPool<object>.Release(list);
        }
    }
}