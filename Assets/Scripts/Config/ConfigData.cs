using System.Collections.Generic;
using UnityEngine;


public class MapConfig
{
    /// <summary>
    /// 地图ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 地图名字
    /// </summary>
    public string Name;

    /// <summary>
    /// 场景名字
    /// </summary>
    public string SceneName;

    /// <summary>
    /// 玩家出生点
    /// </summary>
    public Vector3 PlayerBornPos;

    /// <summary>
    /// 玩家出生方向
    /// </summary>
    public Orient PlayerBornDir;

    /// <summary>
    /// 怪物出生信息
    /// </summary>
    public List<MonsterBornConfig> MonsterList;
}


public class CharacterConfig
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 角色预制件路径
    /// </summary>
    public string PrefabPath;

    /// <summary>
    /// 角色属性
    /// </summary>
    public BattleAttribute Attribute;
}


public class MonsterBornConfig
{
    /// <summary>
    /// 刷怪波数
    /// </summary>
    public int Wave;

    /// <summary>
    /// 
    /// </summary>
    public int Index;

    /// <summary>
    /// 怪物数据
    /// </summary>
    public MonsterConfig MonsterData;

    /// <summary>
    /// 怪物等级
    /// </summary>
    public int Level;

    /// <summary>
    /// 怪物出生点
    /// </summary>
    public Vector3 BornPos;

    /// <summary>
    /// 怪物出生后方向
    /// </summary>
    public Vector3 BornRot;
}

public class MonsterConfig
{
    /// <summary>
    /// 怪物ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 怪物名字
    /// </summary>
    public string Name;

    /// <summary>
    /// 怪物预制件路径
    /// </summary>
    public string PrefabPath;

    /// <summary>
    /// 怪物属性
    /// </summary>
    public BattleAttribute Attribute;
}

public class SkillConfig
{
    /// <summary>
    /// 技能ID
    /// </summary>
    public int ID;       

    /// <summary>
    /// 技能名字
    /// </summary>
    public string Name;
    
    /// <summary>
    /// 技能CD
    /// </summary>
    public int CDTime;
    
    /// <summary>
    /// 技能时间
    /// </summary>
    public int Time;

    /// <summary>
    /// 动作动画ID
    /// </summary>
    public int AniAction;
    //public int delayEffect;

    /// <summary>
    /// 动作播放技能特效
    /// </summary>
    public string Effect;

    /// <summary>
    /// 动作过程中角色移动信息
    /// </summary>
    public List<int> MoveList;

    /// <summary>
    /// 伤害范围信息
    /// </summary>
    public List<int> DamageDirList;

    /// <summary>
    /// 伤害数值
    /// </summary>
    public List<int> DamageNumList;
}

public enum EffectType
{
    Screen,
    Follow,
    Missile,
}

public class NormalSkillEffect
{
    public int ID;
    public int DelayTime;
    public int Time;
    public EffectType type;
    public string EffectName;
}

public enum MissileType
{
    Fly,
    Stand,
}

public class MissileSkillEffect
{
    public int ID;
    public int DelayTime;
    public int Time;
    public MissileType type;
    public string EffectName;
    public string BodyRoot;
}


public class SkillMoveConfig
{

    /// <summary>
    /// 移动ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 延迟时间
    /// </summary>
    public int DelayTime;

    /// <summary>
    /// 移动时间
    /// </summary>
    public int MoveTime;

    /// <summary>
    /// 移动距离
    /// </summary>
    public float MoveDis; 
}

public class DamageDirConfig
{
    /// <summary>
    /// 方向ID
    /// </summary>
    public int ID;

    /// <summary>
    /// 延迟时间
    /// </summary>
    public int DelayTime;

    /// <summary>
    /// 伤害计算范围
    /// </summary>
    public float Dis;

    /// <summary>
    /// 伤害方向
    /// </summary>
    public int Dir;
}