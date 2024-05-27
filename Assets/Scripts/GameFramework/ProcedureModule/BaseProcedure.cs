// 引入线程任务命名空间

using System.Threading.Tasks;

/// <summary>
/// 作者: Teddy
/// 时间: 2018/03/01
/// 功能: 基础流程类
/// </summary>
// 定义抽象基础流程类
public abstract class BaseProcedure
{
    /// <summary>
    ///  异步方法：切换流程到目标类型 T
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        // 调用游戏管理器的流程管理器的切换流程方法
        await GameManager.Procedure.ChangeProcedure<T>(value);
    }

    /// <summary>
    /// 虚拟异步方法：进入流程时执行的逻辑
    /// </summary>
    /// <param name="value"></param>
    public virtual async Task OnEnterProcedure(object value)
    {
        // 异步等待
        await Task.Yield();
    }

    /// <summary>
    /// 虚拟异步方法：离开流程时执行的逻辑
    /// </summary>
    public virtual async Task OnLeaveProcedure()
    {
        // 异步等待
        await Task.Yield();
    }
}