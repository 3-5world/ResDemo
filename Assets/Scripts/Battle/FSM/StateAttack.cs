using UnityEngine;

public class StateAttack : IState
{
    public void Enter(EntityBase entity, params object[] args)
    {
        entity.CurState = EnityState.Attack;
    }

    public void Exit(EntityBase entity, params object[] args)
    {
        entity.ExitCurSkill();
    }

    public void Process(EntityBase entity, params object[] args)
    {
        entity.SkillAttack((int)args[0], (Orient)args[1]);
    }
}