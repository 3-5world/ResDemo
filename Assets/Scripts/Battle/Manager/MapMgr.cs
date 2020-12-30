using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapMgr : MonoBehaviour
{
    private int waveIndex = 1;
    private BattleMgr battleMgr;
    private Action proAct = null;

    public void Init(BattleMgr battle)
    {
        this.battleMgr = battle;
        LoadMonsterByWave(waveIndex);
    }

    public void LoadMonsterByWave(int waveIndex)
    {
        battleMgr.LoadMonsterByWave(waveIndex);
    }

    private void Update()
    {
        if (proAct != null)
        {
            proAct();
        }
    }

    public void AsyncLoadMap(int mapId, Action finishLoad)
    {
        var mapCfg = ConfigSvc.Ins.GetMapCfg(mapId);
        var loadingPanel = UIMgr.Ins.OpenPanel(PanelType.LoadingPanel) as LoadingPanel;
        loadingPanel.transform.SetAsLastSibling();
        var async = SceneManager.LoadSceneAsync(mapCfg.SceneName);
        proAct = () =>
        {
            float val = async.progress;
   
            loadingPanel.UpdateProgress(val);
            //TODO:90%
            if (val == 1)
            {
                if (finishLoad != null)
                    finishLoad();
                async = null;
                loadingPanel.SetPanelState(false);
                proAct = null;
            }
        };
    }
}