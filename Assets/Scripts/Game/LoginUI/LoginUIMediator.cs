using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Nirvana;
using UnityEngine.Playables;
using System.Collections.Generic;

public class LoginUIMediator : UIMediator<LoginUIView>
{
    
    protected override void OnInit(LoginUIView view)
    {
        base.OnInit(view);
        
    }

    protected override void OnShow(object arg)
    {
        base.OnShow(arg);
        view.close.onClick.AddListener(() =>
        {
            GameManager.UI.CloseUI(UIViewID.LoginUI);
        });
    }
}
