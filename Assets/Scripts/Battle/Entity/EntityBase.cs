using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase
{
    public EnityState CurState = EnityState.None;
    protected BattleMgr battleMgr = null;
    protected StateMgr stateMgr = null;
    protected SkillMgr skillMgr = null;
    protected Controller controller = null;
    protected BattleAttribute battleAttribute = null;
    protected int[] comboSkill;
    protected Queue<int> comboQue = new Queue<int>();
    public int nextSkillID = 0;
    public double lastAtTime = 0f;
    public int comboIndex = 0;

    public virtual void Init(BattleMgr battleMgr, StateMgr stateMgr, SkillMgr skillMgr, Controller ctrl, BattleAttribute attribute, Vector3 pos, Orient dir)
    {
        this.battleMgr = battleMgr;
        this.stateMgr = stateMgr;
        this.skillMgr = skillMgr;
        this.controller = ctrl;
        this.battleAttribute = attribute;

        ctrl.Init(pos, dir);
    }

    public virtual void SetupSkill(List<int> comboSkill)
    {
        this.comboSkill = comboSkill.ToArray();
    }

    public void ComboAttack(Orient dir)
    {
        double nowAtTime = TimerSvc.Ins.GetNowTime();

        if (CurState == EnityState.Attack)
        {
            if (nowAtTime - lastAtTime < Constants.ComboInterval && lastAtTime != 0)
            {
                if (comboSkill[comboIndex] != comboSkill[comboSkill.Length - 1])
                {
                    Debug.Log("==========combo");
                    comboIndex += 1;
                    comboQue.Enqueue(comboSkill[comboIndex]);
                    lastAtTime = nowAtTime;
                }
                else
                {
                    lastAtTime = 0;
                    comboIndex = 0;
                }
            }
        }
        else if (CurState == EnityState.Idle)
        {
            lastAtTime = nowAtTime;
            comboIndex = 0;
            stateMgr.ChangeState(this, EnityState.Attack, comboSkill[comboIndex], dir);
        }

    }

    public void Attack(Orient dir, int skillId)
    {
        if (controller.Dir != dir)
        {
            controller.Dir = dir;
        }

        stateMgr.ChangeState(this, EnityState.Attack, skillId, dir);
    }

    public void Idle(Orient dir)
    {
        stateMgr.ChangeState(this, EnityState.Idle, dir);
    }

    public void SkillAttack(int skillID, Orient dir)
    {
        skillMgr.SkillAttack(this, skillID, dir);
    }

    public void ExitCurSkill()
    {
        if (comboQue.Count > 0)
        {
            nextSkillID = comboQue.Dequeue();
        }
        else
        {
            nextSkillID = 0;
        }
        SetAction(Constants.AniDefault);
    }

    public void SetDir(Orient dir)
    {
        if (controller.Dir != dir)
        {
            controller.Dir = dir;
        }
    }

    public void SetAction(int action)
    {
        if (controller != null)
            controller.SetAction(action);
    }

    public virtual void SetFx(string name, float destroy)
    {
        if (controller != null)
            controller.SetFx(name, destroy);
    }

    public void SetSkillMoveState(bool move, float speed = 0f)
    {
        if (controller != null)
            controller.SetSkillMoveState(move, speed);
    }

}