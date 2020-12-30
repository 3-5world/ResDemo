using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleMgr : MonoBehaviour 
{
    private StateMgr stateMgr;
    private SkillMgr skillMgr;
    private MapMgr mapMgr;
    private EntityPlayer selfPlayer;
    private MapConfig mapCfg;

    private Dictionary<string, EntityMonster> monsterDic = new Dictionary<string, EntityMonster>();

    private Action proAct = null;

    private void Update()
    {
        if (proAct != null)
        {
            proAct();
        }
    }

    public void Init(int characterID, int mapId)
    {

        // TODO: 单个实体独立状态管理
        stateMgr = gameObject.AddComponent<StateMgr>();
        stateMgr.Init();

        skillMgr = gameObject.AddComponent<SkillMgr>();
        skillMgr.Init();

        mapCfg = ConfigSvc.Ins.GetMapCfg(mapId);
        AsyncLoadScene(mapCfg.SceneName, () =>
        {
            var go = GameObject.FindGameObjectWithTag("MapRoot");
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            mapMgr = go.GetComponent<MapMgr>();
            mapMgr.Init(this);

            LoadPlayer(characterID, mapCfg);
            selfPlayer.Idle(mapCfg.PlayerBornDir);
        });
    }


    public void AsyncLoadScene(string sceneName, Action finishCallback)
    {
        var loadingPanel = UIMgr.Ins.OpenPanel(PanelType.LoadingPanel) as LoadingPanel;
        loadingPanel.transform.SetAsLastSibling();
        var async = SceneManager.LoadSceneAsync(sceneName);
        proAct = () =>
        {
            float val = async.progress;
            //TODO:90%
            loadingPanel.UpdateProgress(val);
            if (val == 1)
            {
                if (finishCallback != null)
                    finishCallback();
                async = null;
                loadingPanel.SetPanelState(false);
                proAct = null;
            }
        };

    }

    void LoadPlayer(int characterId, MapConfig mapCfg)
    {
        var characterCfg = ConfigSvc.Ins.GetCharacterCfg(characterId);
        var player = ObjSvc.Ins.InsObj(PathDefine.PlayerDir + characterCfg.PrefabPath);
  
        var ctrl = player.GetComponent<PlayerController>();
        selfPlayer = new EntityPlayer();
        selfPlayer.Init(this, stateMgr, skillMgr, ctrl, characterCfg.Attribute, mapCfg.PlayerBornPos, mapCfg.PlayerBornDir);
        selfPlayer.SetupSkill(new List<int> { 201, 202, 203, 204, 205, 206 });
        selfPlayer.Idle(mapCfg.PlayerBornDir);
    }

    public void LoadMonsterByWave(int waveIndex)
    {

    }

    public void PlayerAtk(Orient dir)
    {
        selfPlayer.ComboAttack(dir);
    }
}