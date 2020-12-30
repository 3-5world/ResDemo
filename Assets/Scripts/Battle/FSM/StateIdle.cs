using UnityEngine;

public class StateIdle : IState 
{
    public void Enter(EntityBase entity, params object[] args)
    {
        entity.CurState = EnityState.Idle;

    }

    public void Exit(EntityBase entity, params object[] args)
    {

    }

    public void Process(EntityBase entity, params object[] args)
    {
        if (entity.nextSkillID != 0)
        {
            Debug.Log("==========combo next");
            entity.Attack((Orient)args[0], entity.nextSkillID);
        }
        else
        {
            entity.SetDir((Orient)args[0]);
            entity.SetAction(Constants.AniIdle);
        }
    }
}