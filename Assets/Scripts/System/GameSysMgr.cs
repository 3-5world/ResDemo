using System.Collections.Generic;
using UnityEngine;


public abstract class GameSys
{
    protected ResSvc resSvc;
    protected AudioSvc audioSvc;
    protected NetSvc netSvc = null;

    public virtual void InitSys()
    {
        resSvc = ResSvc.Ins;
        audioSvc = AudioSvc.Ins;
        netSvc = NetSvc.Ins;
    }

    public virtual void Start()
    {

    }

    public virtual void Destroy()
    {

    }
}


public class GameSysMgr : MonoSingleton<GameSysMgr> 
{
    private BattleSys battleSys;

    private List<GameSys> allSys = new List<GameSys>();

    public BattleSys BattleSys
    {
        get { return battleSys; }
    }

    public void InitSys()
    {
        allSys.Clear();
        battleSys = new BattleSys();
        allSys.Add(battleSys);


        for (int i = 0;i < allSys.Count; ++i)
        {
            allSys[i].InitSys();
        }
    }

    public void OnDestroy()
    {
        for (int i = 0; i < allSys.Count; ++i)
        {
            allSys[i].Destroy();
        }
    }


}