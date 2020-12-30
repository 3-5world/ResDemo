using UnityEngine;
using UnityEngine.UI;

public abstract class PanelRoot : MonoBehaviour 
{
    public PanelType PanelType;
    protected ResSvc resSvc = null;
    protected AudioSvc audioSvc = null;
    protected NetSvc netSvc = null;
    protected TimerSvc timerSvc = null;

    public void SetPanelState(bool isActive = true)
    {
        if (gameObject.activeSelf != isActive)
            gameObject.SetActive(isActive);
        if (isActive)
            InitPanel();
        else
            ClearPanel();
    }

    protected virtual void InitPanel()
    {
        resSvc = ResSvc.Ins;
        audioSvc = AudioSvc.Ins;
        netSvc = NetSvc.Ins;
        timerSvc = TimerSvc.Ins;
    }

    protected virtual void ClearPanel()
    {
        resSvc = null;
        audioSvc = null;
        netSvc = null;
        timerSvc = null;
    }

    protected void SetText(Text txt, string content = "")
    {
        txt.text = content;
    }

    protected void SetText(Text txt, int value)
    {
        txt.text = value.ToString();
    }

    protected void PlayClickAudio()
    {
        audioSvc.PlayUIAudio(Constants.UI_Click_Btn);
    }
}