using UnityEngine;

public class PathDefine : MonoBehaviour 
{
    public static string ABLoadPath = Application.streamingAssetsPath + "/AssetBundles/";

    #region config
    public const string MapConfig = "Assets/RawRes/Config/MapConfig.xml";
    public const string SkillConfig = "Assets/RawRes/Config/SkillConfig.xml";
    public const string SkillMoveConfig = "Assets/RawRes/Config/SkillMoveConfig.xml";
    public const string DamageDirConfig = "Assets/RawRes/Config/DamageDirConfig.xml";
    public const string MonsterCfg = "Assets/RawRes/Config/MonsterConfig.xml";
    public const string CharacterCfg = "Assets/RawRes/Config/CharacterConfig.xml";
    #endregion

    #region Player
    public const string PlayerDir = "Assets/Prefab/Character/";
    #endregion
}