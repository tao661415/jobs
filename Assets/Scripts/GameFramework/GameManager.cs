using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    /// <summary>
    /// 流程组件
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure
    {
        get => TGameFramework.Instance.GetModule<ProcedureModule>();
    }



    /// <summary>
    /// 消息组件
    /// </summary>
      [Module(7)]
      public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
     




    private bool activing;

    public static GameManager Instance;
    
    private void Awake()
    {
        Instance = this;
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        activing = true;
        DontDestroyOnLoad(gameObject);
        

        Application.logMessageReceived += OnReceiveLog;
        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();
    }
    
    private void Start()
    {
        TGameFramework.Instance.StartModules();
        Procedure.StartProcedure().Coroutine();
        Message.Post<MessageType.RequestAllInfo>(new MessageType.RequestAllInfo()).Coroutine();
    }
    
    private void Update()
    {
        TGameFramework.Instance.Update();
    }


    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }


    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }


    private void OnDestroy()
    {
        if (activing)
        {
            Application.logMessageReceived -= OnReceiveLog;
            TGameFramework.Instance.Destroy();
        }
    }

    /// <summary>
    /// 在应用程序退出时执行的方法
    /// </summary>
    private void OnApplicationQuit()
    {
        //UnityLog.Teardown();
    }
    
    /// <summary>
    /// 初始化模块
    /// </summary>
    public void StartupModules()
    {
        // 创建一个列表用于存储模块特性
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();

        // 获取当前类的所有属性信息，包括公共、非公共和静态属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        // 获取基础组件类型，用于判断属性类型是否为BaseGameModule或其子类
        Type baseCompType = typeof(BaseGameModule);

        // 循环遍历所有属性信息
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            // 获取当前属性信息
            PropertyInfo property = propertyInfos[i];

            // 如果属性类型不是BaseGameModule或其子类，则跳过当前循环
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;

            // 获取当前属性的所有特性
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);

            // 如果属性没有ModuleAttribute特性，则跳过当前循环
            if (attrs.Length == 0)
                continue;

            // 在场景中查找指定类型的组件
            Component comp = GetComponentInChildren(property.PropertyType);

            // 如果找不到对应类型的组件，则记录错误信息并跳过当前循环
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            // 获取属性的第一个ModuleAttribute特性，并将组件赋给其Module属性
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;

            // 将处理后的模块特性添加到列表中
            moduleAttrs.Add(moduleAttr);
        }

        // 根据优先级对模块特性进行排序
        moduleAttrs.Sort((a, b) => { return a.Priority - b.Priority; });

        // 遍历排序后的模块特性列表，并将模块添加到TGameFramework中
        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
        // Message.Post<MessageType.RequestAllInfo>(new MessageType.RequestAllInfo()).Coroutine();
    }
    
    /// <summary>
    /// 模块特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    /// <summary>
    /// 处理日志消息的方法
    /// </summary>
    /// <param name="condition">日志消息条件</param>
    /// <param name="stackTrace">堆栈跟踪信息</param>
    /// <param name="type">日志类型</param>
    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
    if (type == LogType.Exception)
    {
        Debug.Log($"{condition}\n{stackTrace}");
    }
#endif
    }
}