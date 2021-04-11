using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandColliderBolt : Bolt.EntityBehaviour<IHandColliderState>
{
    #region Singleton
    private static HandCollider _instance;
    public static HandCollider Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    #endregion
    public int smoothingFrame;
    public bool movingAverage;
    public bool linearWeightedMovingAverage;

    private TrackingInfo tracking;
    public Vector3 currentScenePosition;
    public Vector3 currentPosition;
    public Vector3 smoothedPostion;
    private List<Collider> collidings;

    private Queue<Vector3> smoothingBuffer;
    private int i;
    private float xSum;
    private float ySum;
    private float zSum;
    private Vector3 droping;
    private Vector3[] smoothingBufferArray;
    private float trianglurNumber;

    public ManomotionManager _manomotionManager;

    private ManoGestureTrigger grabTrigger;
    private ManoGestureContinuous pinch;

    public bool grabTriggering = false;
    public bool pinching = false;

    public GameObject worldOrigin;
    public Vector3 worldOriginOffsetPosition;
    public Quaternion worldOriginOffsetRotaion;

    /// <summary>
    /// Set the hand collider tag.
    /// </summary>
    private void Start()
    {
        gameObject.tag = "Player";
        collidings = new List<Collider>();
        smoothingBuffer = new Queue<Vector3>();
        for (i = 0; i < smoothingFrame; i++)
        {
            smoothingBuffer.Enqueue(Vector3.zero);
            xSum = 0;
            ySum = 0;
            zSum = 0;
        }
        trianglurNumber = smoothingFrame * (smoothingFrame + 1) / 2;

        if (entity.IsOwner)
        {
            _manomotionManager = GameObject.FindGameObjectWithTag("ManomotionManager").GetComponent<ManomotionManager>();
        }

        grabTrigger = ManoGestureTrigger.GRAB_GESTURE;
        pinch = ManoGestureContinuous.HOLD_GESTURE;

        worldOrigin = GameObject.FindGameObjectWithTag("WorldOrigin");
        // worldOriginOffsetPosition = worldOrigin.transform.position;
        // worldOriginOffsetRotaion = worldOrigin.transform.rotation;
        // transform.rotation = worldOriginOffsetRotaion;
        // transform.rotation = worldOrigin.transform.InverseTransformDirection(Vector3.zero);
    }

    public override void Attached()
    {
        state.SetTransforms(state.HandColliderTransform, transform);
        transform.parent = worldOrigin.transform;
    }

    /// <summary>
    /// Get the tracking information from the ManoMotionManager and set the position of the hand Collider according to that.
    /// </summary>
    void FixedUpdate()
    {
        try
        {
            if (entity.IsOwner)
            {
                tracking = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info;
                // TODO!!! offset the currentPosition
                currentPosition = Camera.main.ViewportToWorldPoint(new Vector3(tracking.palm_center.x, tracking.palm_center.y, tracking.depth_estimation));
                // currentScenePosition = Camera.main.ViewportToWorldPoint(new Vector3(tracking.palm_center.x, tracking.palm_center.y, tracking.depth_estimation));
                // currentPosition = worldOrigin.transform.InverseTransformPoint(currentScenePosition);

                // transform.position = currentPosition;
                if (movingAverage)
                {
                    smoothedPostion = MovingAverage(currentPosition);
                }
                else if (linearWeightedMovingAverage)
                {
                    smoothedPostion = LinearWeightedMovingAverage(currentPosition);
                }
                else
                {
                    smoothedPostion = currentPosition;
                }

                transform.position = smoothedPostion;
                Debug.Log("Both Before and After: (" + currentPosition.x.ToString() + ", " + currentPosition.y.ToString() + ", " + currentPosition.z.ToString() + "), (" + smoothedPostion.x.ToString() + ", " + smoothedPostion.y.ToString() + ", " + smoothedPostion.z.ToString() + ")");
                Debug.Log("Before Only: " + currentPosition.x.ToString() + ", " + currentPosition.y.ToString() + ", " + currentPosition.z.ToString() + ")");
                Debug.Log("After Only: " + smoothedPostion.x.ToString() + ", " + smoothedPostion.y.ToString() + ", " + smoothedPostion.z.ToString() + ")");

                state.GrabTrigger = (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger == grabTrigger);
                state.PinchCountinuous = (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == pinch);
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.Log($"No tracking, manomotionManager = {_manomotionManager}");
        }
        grabTriggering = state.GrabTrigger;
        pinching = state.PinchCountinuous;
    }

    Vector3 MovingAverage(Vector3 current)
    {
        droping = smoothingBuffer.Dequeue();
        xSum -= droping.x;
        ySum -= droping.y;
        zSum -= droping.z;

        smoothingBuffer.Enqueue(current);
        xSum += current.x;
        ySum += current.y;
        zSum += current.z;

        return new Vector3(xSum / smoothingFrame, ySum / smoothingFrame, zSum / smoothingFrame);
    }

    Vector3 LinearWeightedMovingAverage(Vector3 current)
    {
        droping = smoothingBuffer.Dequeue();
        smoothingBuffer.Enqueue(current);

        xSum = 0;
        ySum = 0;
        zSum = 0;
        smoothingBufferArray = smoothingBuffer.ToArray();

        for (i = 0; i < smoothingFrame; i++)
        {
            xSum += smoothingBufferArray[i].x * (i + 1);
            ySum += smoothingBufferArray[i].y * (i + 1);
            zSum += smoothingBufferArray[i].z * (i + 1);
        }

        return new Vector3(xSum / trianglurNumber, ySum / trianglurNumber, zSum / trianglurNumber);
    }

    void OnTriggerEnter(Collider other)
    {
        collidings.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        collidings.Remove(other);
    }

    public int getCollidingsCount()
    {
        return collidings.Count;
    }

    public bool getGrabTrigger()
    {
        return state.GrabTrigger;
    }

    public bool getPinchCountinuous()
    {
        return state.PinchCountinuous;
    }
}