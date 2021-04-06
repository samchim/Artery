using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollider : MonoBehaviour
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

    public ManomotionManager manomotionManager;

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

        manomotionManager = GameObject.Find("ManomotionManager").GetComponent<ManomotionManager>();
    }

    /// <summary>
    /// Get the tracking information from the ManoMotionManager and set the position of the hand Collider according to that.
    /// </summary>
    void FixedUpdate()
    {
        try
        {
            tracking = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info;

            currentPosition = Camera.main.ViewportToWorldPoint(new Vector3(tracking.palm_center.x, tracking.palm_center.y, tracking.depth_estimation));

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
        } catch (System.NullReferenceException e)
        {
            Debug.Log($"No tracking, manomotionManager = {manomotionManager}");
        }
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
}