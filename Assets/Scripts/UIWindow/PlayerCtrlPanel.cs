using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrlPanel : PanelRoot 
{
    public Button btnLeft;
    public Button btnRight;
    public Button btnReInitRes;

    protected override void InitPanel()
    {
        base.InitPanel();
        btnLeft.onClick.RemoveAllListeners();
        btnLeft.onClick.AddListener(ClickLeft);

        btnRight.onClick.RemoveAllListeners();
        btnRight.onClick.AddListener(ClickRight);

        btnReInitRes.onClick.RemoveAllListeners();
        btnReInitRes.onClick.AddListener(ClickReInitRes);
    }

    void ClickLeft()
    {
        GameSysMgr.Ins.BattleSys.PlayerAtk(Orient.Left);
    }

    void ClickRight()
    {
        GameSysMgr.Ins.BattleSys.PlayerAtk(Orient.Right);
    }

    void ClickReInitRes()
    {
        ConfigSvc.Ins.ReInitTest();
    }
}