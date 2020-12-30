using System.Collections.Generic;
using UnityEngine;

public enum Orient
{
    Right = -1,
    Left = 1,
}

public abstract class Controller : MonoBehaviour 
{
    public Animator Animator;
    public Rigidbody2D Rigbody;

    public Transform HpRoot;

    protected Orient dir = Orient.Right;

    protected Dictionary<string, GameObject> fxDic = new Dictionary<string, GameObject>();

    protected CameraFollow cameraFollow;
    protected TimerSvc timer = null;

    protected bool skillMove = false;
    protected float skillMoveSpeed = 0f;
    private float continueTime;
    public void FixedUpdate()
    {
        if (skillMove)
        {
            var tmpValue = Time.fixedDeltaTime * skillMoveSpeed * 1000f;
            Rigbody.velocity = transform.right * tmpValue + transform.up * tmpValue;
        }
    }

    public virtual void Init(Vector3 pos, Orient dir)
    {
        transform.position = pos;
        this.dir = dir;
        transform.localEulerAngles = new Vector3(0, dir == Orient.Left ? 180f : 0, 0);
        timer = TimerSvc.Ins;
    }

    public virtual void Attack(Orient dir)
    {

    }

    public virtual void SetAction(int action)
    {
        Animator.SetInteger("Action", action);
    }

    public Orient Dir
    {
        get
        {
            return dir;
        }
        set
        {
            if (dir != value)
            {
                dir = value;
                transform.localEulerAngles = new Vector3(0, dir == Orient.Left ? 180f : 0, 0);
            }
        }
    }

    public virtual void SetFx(string name, float destroyTime)
    {

    }

    public void SetSkillMoveState(bool move, float speed = 0f)
    {
        Debug.Log("move:" + move + transform.position);
        continueTime = 0f;
        skillMove = move;
        skillMoveSpeed = speed;
    }

}