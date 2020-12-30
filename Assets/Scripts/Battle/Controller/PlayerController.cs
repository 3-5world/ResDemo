using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{
    public List<GameObject> FXList;
    public override void Init(Vector3 pos, Orient dir)
    {
        base.Init(pos, dir);
        var go = GameObject.FindGameObjectWithTag("CameraFollow");
        cameraFollow = go.GetComponent<CameraFollow>();
    }

    public void Start()
    {
        cameraFollow.Target = this.transform;
        fxDic.Clear();
        for (int i = 0; i < FXList.Count; ++i)
        {
            var fx = FXList[i];
            fxDic.Add(fx.name, fx);
        }
    }

    public override void SetFx(string name, float destroyTime)
    {
        GameObject goFx = null;
        if (fxDic.TryGetValue(name, out goFx))
        {
            goFx.SetActive(true);
            timer.AddTimeTask((int tid) =>
            {
                goFx.SetActive(false);
            }, destroyTime);
        }
    }
}