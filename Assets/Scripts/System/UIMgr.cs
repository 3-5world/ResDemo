using System.Collections.Generic;
using UnityEngine;

public enum PanelType
{
    None,
    WelcomePanel = 1,
    LoadingPanel = 2,
    PlayerCtrlPanel = 3,
}

public class UIMgr : MonoSingleton<UIMgr> 
{
    public List<PanelRoot> Panels;
    public List<PanelType> InitOpenPanels;
    private Dictionary<PanelType, PanelRoot> panelDic = null;

    public void Init()
    {
        panelDic = new Dictionary<PanelType, PanelRoot>();
        for (int i = 0; i < Panels.Count; ++i)
        {
            var panel = Panels[i];
            if (InitOpenPanels.Contains(panel.PanelType))
                panel.SetPanelState(true);
            else
                panel.SetPanelState(false);
            panelDic.Add(panel.PanelType, panel);
        }
    }

    public PanelRoot OpenPanel(PanelType type)
    {
        if (!panelDic.ContainsKey(type))
        {
            Debug.LogWarningFormat("not exist panel type:{0}", type);
            return null;
        }
        panelDic[type].SetPanelState();
        return panelDic[type];
    }

    public void ClosePanel(PanelType type)
    {
        if (!panelDic.ContainsKey(type))
        {
            Debug.LogWarningFormat("not exist panel type:{0}", type);
            return;
        }
        panelDic[type].SetPanelState(false);
    }
}