using UnityEngine;

public class AudioSvc : MonoSingleton<AudioSvc>
{
    public AudioSource bgAudio;
    public AudioSource uiAudio;

    public void InitSvc()
    {
        Debug.Log("Init AudioSvc...");
    }

    public void PlayBgMusic(string name, bool isLoop = true)
    {
        //var audio = ResSvc.Ins.LoadAudio("Assets/RawRes/Audio/" + name, true);
        ResSvc.Ins.AsyncLoadAsset("Assets/RawRes/Audio/" + name, AsynPlayBgMusic, ResPriority.High);
        //if (bgAudio.clip == null || bgAudio.clip.name != name)
        //{
        //    bgAudio.clip = audio;
        //    bgAudio.loop = isLoop;
        //    bgAudio.Play();
        //}
    }

    void AsynPlayBgMusic(string path, Object obj, object param)
    {
        if (bgAudio.clip == null || bgAudio.clip.name != name)
        {
            bgAudio.clip = obj as AudioClip;
            bgAudio.loop = true;
            bgAudio.Play();
        }
    }

    public void RelaseBgMusic(string name)
    {
        bgAudio.Stop();
        var clip = bgAudio.clip;
        bgAudio.clip = null;
        ResSvc.Ins.ReleaseRes(clip, true);
    }

    public void StopBgMusic()
    {
        if (bgAudio.clip != null)
            bgAudio.Stop();
    }

    public void PlayUIAudio(string name)
    {
        var audio = ResSvc.Ins.LoadAsset<AudioClip>("Assets/RawRes/Audio/" + name);
        uiAudio.clip = audio;
        uiAudio.Play();
    }

    public void RelaseUIMusic(string name)
    {
        uiAudio.Stop();
        uiAudio.clip = null;
        ResSvc.Ins.ReleaseRes("Assets/RawRes/Audio/" + name, true);
    }
}