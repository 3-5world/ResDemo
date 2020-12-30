using UnityEngine;
using UnityEngine.UI;

public class WelcomePanel : PanelRoot
{
    public Button btnStart;

    protected override void InitPanel()
    {
        base.InitPanel();
        btnStart.onClick.RemoveAllListeners();
        btnStart.onClick.AddListener(ClickStart);
    }

    void ClickStart()
    {
        SetPanelState(false);
        GameSysMgr.Ins.BattleSys.StartBattle(1, 1001);
    }
}