using System.Threading.Tasks;
using UnityEngine;


public class InitProcedure : BaseProcedure
{
   

    public override async Task OnEnterProcedure(object value)
    {
        Debug.Log("enter init procedure");
        await Task.Yield();



    }
}

