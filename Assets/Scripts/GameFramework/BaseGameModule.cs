using UnityEngine;

/// <summary>
/// 游戏模块的基类，继承自 MonoBehaviour
/// </summary>
public abstract class BaseGameModule : MonoBehaviour
{
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }

    /// <summary>
    /// 模块初始化方法
    /// </summary>
    protected internal virtual void OnModuleInit() { }

    /// <summary>
    /// 模块启动方法
    /// </summary>
    protected internal virtual void OnModuleStart() { }

    /// <summary>
    /// 模块停止方法
    /// </summary>
    protected internal virtual void OnModuleStop() { }

    /// <summary>
    /// 模块更新方法，传入 deltaTime
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    protected internal virtual void OnModuleUpdate(float deltaTime) { }

    /// <summary>
    /// 模块延迟更新方法，传入 deltaTime
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    protected internal virtual void OnModuleLateUpdate(float deltaTime) { }

    /// <summary>
    /// 模块固定更新方法，传入 deltaTime
    /// </summary>
    /// <param name="deltaTime">时间间隔</param>
    protected internal virtual void OnModuleFixedUpdate(float deltaTime) { }
}