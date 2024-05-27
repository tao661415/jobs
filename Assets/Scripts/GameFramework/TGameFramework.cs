using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 作者: Teddy
/// 时间: 2018/03/02
/// 功能: 游戏框架类，负责管理游戏模块
/// </summary>
public sealed class TGameFramework
{
    public static TGameFramework Instance { get; private set; }
    public static bool Initialized { get; private set; }
    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();

    /// <summary>
    /// 初始化游戏框架
    /// </summary>
    public static void Initialize()
    {
        Instance = new TGameFramework();
    }

    /// <summary>
    /// 获取指定类型的模块
    /// </summary>
    /// <typeparam name="T">模块类型</typeparam>
    /// <returns>返回指定类型的模块</returns>
    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))
        {
            return module as T;
        }

        return default(T);
    }

    /// <summary>
    /// 添加游戏模块
    /// </summary>
    /// <param name="module">要添加的游戏模块</param>
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
            Debug.Log($"Module添加失败，重复:{moduleType.Name}");
            return;
        }
        m_modules.Add(moduleType, module);
    }

    /// <summary>
    /// 更新游戏框架
    /// </summary>
    public void Update()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleUpdate(deltaTime);
        }
    }

    /// <summary>
    /// 晚期更新游戏框架
    /// </summary>
    public void LateUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleLateUpdate(deltaTime);
        }
    }

    /// <summary>
    /// 固定更新游戏框架
    /// </summary>
    public void FixedUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        float deltaTime = UnityEngine.Time.fixedDeltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleFixedUpdate(deltaTime);
        }
    }

    /// <summary>
    /// 初始化游戏模块
    /// </summary>
    public void InitModules()
    {
        if (Initialized)
            return;

        Initialized = true;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleInit();
        }
    }

    /// <summary>
    /// 启动游戏模块
    /// </summary>
    public void StartModules()
    {
        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        foreach (var module in m_modules.Values)
        {
            module.OnModuleStart();
        }
    }

    /// <summary>
    /// 销毁游戏框架
    /// </summary>
    public void Destroy()
    {
        if (!Initialized)
            return;

        if (Instance != this)
            return;

        if (Instance.m_modules == null)
            return;

        foreach (var module in Instance.m_modules.Values)
        {
            module.OnModuleStop();
        }

        //Destroy(Instance.gameObject);
        Instance = null;
        Initialized = false;
    }
}