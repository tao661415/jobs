using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMessageHandler : MessageHandler<MessageType.RequestAllInfo>
{

    public override async Task HandleMessage(MessageType.RequestAllInfo arg)
    {
        Debug.Log("MainMessageHandler运行");
        await Task.Yield();
    }




}