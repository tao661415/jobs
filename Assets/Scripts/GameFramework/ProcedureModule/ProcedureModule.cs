using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 作者: Teddy
/// 时间: 2018/03/15
/// 功能: 处理游戏总流程,控制游戏过程
/// </summary>
public partial class ProcedureModule : BaseGameModule
{
    [SerializeField]
    // 存储所有流程的名称数组
    private string[] proceduresNames = null;
    [SerializeField]
    // 默认流程的名称
    private string defaultProcedureName = null;

    // 当前流程
    public BaseProcedure CurrentProcedure { get; private set; }
    // 是否正在运行
    public bool IsRunning { get; private set; }
    // 是否正在切换流程
    public bool IsChangingProcedure { get; private set; }

    private Dictionary<Type, BaseProcedure> procedures;
    private BaseProcedure defaultProcedure;
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>();

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        procedures = new Dictionary<Type, BaseProcedure>();
        bool findDefaultState = false;
        // 遍历流程名称数组
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            string procedureTypeName = proceduresNames[i];
            if (string.IsNullOrEmpty(procedureTypeName))
                continue;

            Type procedureType = Type.GetType(procedureTypeName, true);
            if (procedureType == null)
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");
                continue;
            }
            // 实例化流程对象
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;
            bool isDefaultState = procedureTypeName == defaultProcedureName;
            procedures.Add(procedureType, procedure);

            if (isDefaultState)
            {
                defaultProcedure = procedure;
                findDefaultState = true;
            }
        }
        if (!findDefaultState)
        {
            Debug.LogError($"You have to set a correct default procedure to start game");
        }
    }

    protected internal override void OnModuleStart()
    {
        base.OnModuleStart();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        // 清空请求池和切换队列
        changeProcedureRequestPool.Clear();
        changeProcedureQ.Clear();
        IsRunning = false;
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }

    // 开始运行流程
    public async Task StartProcedure()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        // 创建切换流程请求
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();
        changeProcedureRequest.TargetProcedure = defaultProcedure;
        changeProcedureQ.Enqueue(changeProcedureRequest);
        await ChangeProcedureInternal();
    }

    // 切换流程
    public async Task ChangeProcedure<T>() where T : BaseProcedure
    {
        await ChangeProcedure<T>(null);
    }

    // 切换流程带有参数
    public async Task ChangeProcedure<T>(object value) where T : BaseProcedure
    {
        if (!IsRunning)
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))
        {
            UnityLog.Error($"Change Procedure Failed, Can't find Proecedure:${typeof(T).FullName}");
            return;
        }

        // 创建切换流程请求
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();
        changeProcedureRequest.TargetProcedure = procedure;
        changeProcedureRequest.Value = value;
        changeProcedureQ.Enqueue(changeProcedureRequest);

        if (!IsChangingProcedure)
        {
            await ChangeProcedureInternal();
        }
    }

    // 内部方法：执行切换流程的逻辑
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure)
            return;

        IsChangingProcedure = true;
        // 遍历切换队列，执行切换流程的逻辑
        while (changeProcedureQ.Count > 0)
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();
            if (request == null || request.TargetProcedure == null)
                continue;

            if (CurrentProcedure != null)
            {
                // 退出当前流程
                await CurrentProcedure.OnLeaveProcedure();
            }
            CurrentProcedure = request.TargetProcedure;
            // 进入目标流程
            await CurrentProcedure.OnEnterProcedure(request.Value);
        }
        IsChangingProcedure = false;
    }
}

// 切换流程请求类
public class ChangeProcedureRequest
{
    // 目标流程
    public BaseProcedure TargetProcedure { get; set; }
    // 参数值
    public object Value { get; set; }
}