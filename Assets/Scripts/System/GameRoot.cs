using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Ins = null;


    public void Start()
    {
        DontDestroyOnLoad(this);
        Ins = this;
        Init();

        Debug.Log("Game Start....");
        
    }

    void Init()
    {
        // 服务模块初始化
        TimerSvc.Ins.InitSvc();
        ResSvc.Ins.InitSvc(this, true);
        AudioSvc.Ins.InitSvc();
    }

}