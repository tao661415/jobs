using System; 
using System.Collections.Generic; 
using System.Diagnostics; 
using System.Runtime.CompilerServices; 
using System.Threading.Tasks; 
using UnityEngine; 

/// <summary>
/// 为 Process 类型定义一个用于异步等待的扩展方法。
/// </summary>
public static class AwaitExtensions 
{
    /// <summary>
    /// 获取一个 TaskAwaiter，用于异步等待 Process 退出。
    /// </summary>
    /// <param name="process">要等待的 Process 对象。</param>
    /// <returns>用于等待 Process 退出的 TaskAwaiter。</returns>
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>(); // 创建一个 TaskCompletionSource 用于 int 类型
        process.EnableRaisingEvents = true; // 启用 Process 的事件

        process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode); // 在 Exited 事件上设置 TaskCompletionSource 的结果

        if (process.HasExited) // 检查进程是否已退出
        {
            tcs.TrySetResult(process.ExitCode); // 设置 TaskCompletionSource 的结果
        }

        return tcs.Task.GetAwaiter(); // 返回与 TaskCompletionSource 关联的 Task 的 TaskAwaiter
    }


    // 每当从同步代码调用异步方法时，您可以使用此封装方法
    // 或者可以定义自己的 `async void` 方法，执行给定 Task 上的 await
    public static async void Coroutine(this Task task) // 为从 Task 创建协程的扩展方法
    {
        await task; // 执行给定 Task 上的 await
    }
}