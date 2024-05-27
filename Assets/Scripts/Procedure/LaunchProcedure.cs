using System.Threading.Tasks;
using UnityEngine;


public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("enter LaunchProcedure");
        
        await LoadConfigs();
        await ChangeProcedure<InitProcedure>();
    }

    private async Task LoadConfigs()
    {
        Debug.Log("===>加载配置");


        await Task.Yield();
        Debug.Log("<===配置加载完毕");


    
    }
}

