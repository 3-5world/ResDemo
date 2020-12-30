using System.Collections.Generic;
using UnityEngine;

public class StateMgr : MonoBehaviour 
{
    private Dictionary<EnityState, IState> fsmDic = new Dictionary<EnityState, IState>();

    public void Init()
    {
        fsmDic.Add(EnityState.Born, new StateBorn());
        fsmDic.Add(EnityState.Idle, new StateIdle());
        fsmDic.Add(EnityState.Dodge, new StateDodge());
        fsmDic.Add(EnityState.Attack, new StateAttack());
        fsmDic.Add(EnityState.Hit, new StateHit());
        fsmDic.Add(EnityState.Die, new StateDie());
        Debug.Log("Init StateMgr Done");
    }

    public void ChangeState(EntityBase entity, EnityState newState, params object[] args)
    {
        if (entity.CurState == newState)
            return;

        if (fsmDic.ContainsKey(newState))
        {
            if (entity.CurState != EnityState.None)
                fsmDic[entity.CurState].Exit(entity, args);
            fsmDic[newState].Enter(entity, args);
            fsmDic[newState].Process(entity, args);
        }
    }
}