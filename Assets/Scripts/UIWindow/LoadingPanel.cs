using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : PanelRoot
{
    public Image imgPro;
    public Image impPoint;
    public Text txtPro;
    private float proWidth;

    protected override void InitPanel()
    {
        SetText(txtPro, "0%");
        imgPro.fillAmount = 0;
        proWidth = imgPro.GetComponent<RectTransform>().sizeDelta.x;
        impPoint.transform.localPosition = new Vector3(-proWidth / 2, 0, 0);
    }


    public void UpdateProgress(float val)
    {
        SetText(txtPro, (int)(val * 100) + "%");
        imgPro.fillAmount = val;
        float posX = val * proWidth - proWidth / 2;
        impPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }
}