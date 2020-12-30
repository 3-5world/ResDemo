using UnityEngine;

public class SystemRoot : MonoBehaviour 
{
    protected ResSvc resSvc;
    protected AudioSvc audioSvc;
    protected NetSvc netSvc = null;

    public virtual void InitSys()
    {
        resSvc = ResSvc.Ins;
        audioSvc = AudioSvc.Ins;
        netSvc = NetSvc.Ins;
    }
}