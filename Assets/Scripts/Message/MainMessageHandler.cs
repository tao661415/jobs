using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMessageHandler : MessageHandler<MessageType.RequestAllInfo>
{

    public override async Task HandleMessage(MessageType.RequestAllInfo arg)
    {
        

        // GameManager.Net.RegisterProtocol<SCKnapsackInfoAck>(async proto =>
        // {
        //     SCKnapsackInfoAck knapsackInfo = proto as SCKnapsackInfoAck;
        //     //ItemData.Instance.SetMaxKnapsackValidNum(knapsackInfo.max_knapsack_valid_num);
        //     ////ItemData.Instance.SetMaxStorageValidNum(knapsackInfo.max_storage_valid_num);
        //     //ItemData.Instance.SetDataList(knapsackInfo.info_list);
        //     // var packageInfo = GameManager.ECS.World.GetComponent<PackageInfoComponent>().packageInfo;
        //     // packageInfo.info_list = knapsackInfo.info_list;
        //     // packageInfo.item_count = knapsackInfo.item_count;
        //     // packageInfo.max_knapsack_valid_num = knapsackInfo.max_knapsack_valid_num;
        //     // packageInfo.max_storage_valid_num = knapsackInfo.max_storage_valid_num;
        // });
        // CSAllInfoReq msg = ProtocolPool.Instance.GetProtocol<CSAllInfoReq>() as CSAllInfoReq;
        // msg.EncodeAndSend();
        



        await Task.Yield();
    }




}