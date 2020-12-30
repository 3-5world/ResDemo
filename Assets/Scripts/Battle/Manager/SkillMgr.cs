using System.Collections.Generic;
using UnityEngine;

public class SkillMgr : MonoBehaviour
{
    public void Init()
    {

    }

    public void SkillAttack(EntityBase entity, int skillID, Orient dir)
    {
        entity.SetDir(dir);
    }
}