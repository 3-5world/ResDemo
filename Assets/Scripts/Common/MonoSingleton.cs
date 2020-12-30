using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T ins;

    public static T Ins
    {
        get
        {
            if (ins == null)
            {
                ins = (T)FindObjectOfType(typeof(T));

                if (FindObjectsOfType(typeof(T)).Length > 1)
                    return ins;

                if (ins == null)
                {
                    GameObject go = new GameObject();
                    ins = go.AddComponent<T>();
                    go.name = typeof(T).ToString();
                }
                else
                {
                    //Debug.LogFormat("[Singleton] {0} Using instance of already created {1}!", typeof(T), ins.gameObject.name);
                }
            }

            return ins;
        }
    }

}