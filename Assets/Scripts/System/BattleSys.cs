using UnityEngine;

public class BattleSys : GameSys
{
    BattleMgr battleMgr = null;

    public override void InitSys()
    {
        base.InitSys();
        Debug.Log("Init Battle Sys.....");
    }

    public void StartBattle(int characterId, int mapId)
    {
        var go = new GameObject
        {
            name = "BattleRoot"
        };

        go.transform.SetParent(GameRoot.Ins.transform);
        battleMgr = go.AddComponent<BattleMgr>();
        battleMgr.Init(characterId, mapId);
        UIMgr.Ins.OpenPanel(PanelType.PlayerCtrlPanel);
    }

    public void PlayerAtk(Orient dir)
    {
        battleMgr.PlayerAtk(dir);
    }
}