// using Config;
 
using System.Threading.Tasks;
using UnityEngine;


public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        UnityLog.Info("enter init procedure");
        
        await LoadConfigs();
        await ChangeProcedure<InitProcedure>();
    }

    private async Task LoadConfigs()
    {
        UnityLog.Info("===>加载配置");


        await Task.Yield();
        UnityLog.Info("<===配置加载完毕");


    
    }
}

