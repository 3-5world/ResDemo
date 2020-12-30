using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public static readonly Vector3 MIN_MARGIN = new Vector3(0.0025f, 0.0025f, 0.0025f);
    public static readonly float MIN_MARGIN_MAGINITUDE = 0.01f;

    public Vector3 margin = MIN_MARGIN;                     // Distance in the each axis the player can move before the camera follows.
    public Vector3 smooth = new Vector3(10f, 10f, 10f);     // How smoothly the camera catches up with it's target movement in each x axis.
    public Vector3 retargetSmooth = new Vector3(3f, 3f, 3f);
    public float retargetMaxTolerance = 0.02f;

    public Vector2 maxXAndY;		// The maximum x and y coordinates the camera can have.
    public Vector2 minXAndY;		// The minimum x and y coordinates the camera can have.

    public Vector3 offset = Vector3.zero; // Offset to target position.

    Transform _target;              // Target to follow.

    Transform _cachedTransform;
    Vector3 _targetPos = Vector3.zero;

    Vector3 _smooth = Vector3.zero;
    bool _isRetargeting = false; // 是否正在重新绑定


    public Vector2 MaxBound
    {
        set { maxXAndY = value; }
        get { return maxXAndY; }
    }

    public Vector2 MinBound
    {
        set { minXAndY = value; }
        get { return minXAndY; }
    }

    public Vector3 Offset
    {
        get { return offset; }
    }

    public Transform Target
    {
        set
        {
            if (_target == value)
                return;
            _target = value;

            IsRetargeting = _target != null;
            // Sync position.
            _targetPos = _cachedTransform.position;
            //if (Global.LOG) Debug.Log(string.Format("Camera follow target changed, target: {0}.", _target));
        }
        get { return _target; }
    }

    bool IsRetargeting
    {
        get { return _isRetargeting; }
        set
        {
            if (_isRetargeting == value)
                return;
            _isRetargeting = value;

            if (_isRetargeting)
                _smooth = retargetSmooth;
            else
                _smooth = smooth;
        }
    }

    public Vector3 Position
    {
        get { return _cachedTransform.position; }
    }

    void Awake()
    {

        if (margin.magnitude < MIN_MARGIN_MAGINITUDE)
        {
            margin = MIN_MARGIN;
        }

        _cachedTransform = transform;
        // By default the target x and y coordinates of the camera are it's current x and y coordinates.
        _targetPos = _cachedTransform.position;
    }

    void Start()
    {
        offset.z = _cachedTransform.position.z;
    }




    bool CheckXMargin()
    {
        return Mathf.Abs(_cachedTransform.position.x - Mathf.Clamp(_target.position.x + offset.x, minXAndY.x, maxXAndY.x)) > margin.x;
    }

    bool CheckYMargin()
    {
        return Mathf.Abs(_cachedTransform.position.y - Mathf.Clamp(_target.position.y + offset.y, minXAndY.y, maxXAndY.y)) > margin.y;
    }

    bool CheckZMargin()
    {
        return Mathf.Abs(_cachedTransform.position.z - _target.position.z - offset.z) > margin.z;
    }

    void LateUpdate()
    {
        Follow();
    }

    void Follow()
    {
        if (_target == null)
        {
            return;
        }

        var deltaTime = Time.deltaTime;

        // The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
        var targetX = Mathf.Clamp(_target.position.x + offset.x, minXAndY.x, maxXAndY.x);
        // if the distance between the camera and the player in the x axis is greater than the x margin.
        var distanceX = Mathf.Abs(_cachedTransform.position.x - targetX);
        if (distanceX > margin.x) // If the player has moved beyond the x margin...
        {
            // ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
            _targetPos.x = Mathf.Lerp(_cachedTransform.position.x, targetX, _smooth.x * deltaTime);
        }

        var targetY = Mathf.Clamp(_target.position.y + offset.y, minXAndY.y, maxXAndY.y);
        var distanceY = Mathf.Abs(_cachedTransform.position.y - targetY);
        if (distanceY > margin.y) // If the player has moved beyond the y margin...
        {
            // ... the target y coordinate should be a Lerp between the camera's current y position and the target's current y position.
            _targetPos.y = Mathf.Lerp(_cachedTransform.position.y, targetY, _smooth.y * deltaTime);
        }

        var zChanged = false;
        if (CheckZMargin()) // If the target has moved beyond the z margin...
        {
            zChanged = true;
            _targetPos.z = _target.position.z + offset.z;
            // ... the target z coordinate should be a Lerp between the camera's current z position and the target's current z position.
            _targetPos.z = Mathf.Lerp(_cachedTransform.position.z, _targetPos.z, _smooth.z * deltaTime);
        }

        // Finish retarget
        if (IsRetargeting)
        {
            if (distanceX < retargetMaxTolerance && distanceY < retargetMaxTolerance && !zChanged)
            {
                IsRetargeting = false;
            }
        }

        _cachedTransform.position = _targetPos;
    }

    public Vector3 GetClampedTarget(Vector3 target)
    {
        target += offset;
        target.x = Mathf.Clamp(target.x, minXAndY.x, maxXAndY.x);
        target.y = Mathf.Clamp(target.y, minXAndY.y, maxXAndY.y);
        return target;
    }
}
