using System.Threading.Tasks;
using UnityEngine;
using Config;


public class LaunchProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("enter LaunchProcedure");
        ConfigManager.LoadAllConfigsByAddressable("Assets/BundleAssets/Config");
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

