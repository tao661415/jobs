using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

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
        
        // 遍历当前程序集中的所有类型
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            // 如果类型为抽象类型，则跳过当前循环
            if (type.IsAbstract)
                continue;

            // 获取类型的MessageHandlerAttribute特性
            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);
           
            // 如果存在MessageHandlerAttribute特性
            if (messageHandlerAttribute != null)
            {
                // 创建消息处理器实例
                IMessageHander messageHandler = Activator.CreateInstance(type) as IMessageHander;
                // 将消息处理器添加到全局消息处理器字典
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
        // 查找是否有全局消息处理器，如果有则依次处理消息
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                // 判断是否为MessageHandler<T>类型，然后处理消息
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;

                await messageHandler.HandleMessage(arg); // 处理消息
            }
        }

        // 查找本地消息处理器并处理消息
        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            List<object> list = ListPool<object>.Obtain();
            list.AddRangeNonAlloc(localHandlerList);
            foreach (var handler in list)
            {
                // 判断是否为MessageHandlerEventArgs<T>类型，然后处理消息
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg); // 处理消息
            }
            ListPool<object>.Release(list);
        }
    }
}