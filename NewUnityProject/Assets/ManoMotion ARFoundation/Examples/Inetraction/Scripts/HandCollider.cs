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

    private TrackingInfo tracking;
    public Vector3 currentPosition;
    private List<Collider> collidings;
    private Queue<Vector3> smoothingBuffer;

    private int i;
    private float xSum;
    private float ySum;
    private float zSum;
    private Vector3 droping;

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
    }

    /// <summary>
    /// Get the tracking information from the ManoMotionManager and set the position of the hand Collider according to that.
    /// </summary>
    void FixedUpdate()
    {
        tracking = ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info;
        currentPosition = Camera.main.ViewportToWorldPoint(new Vector3(tracking.palm_center.x, tracking.palm_center.y, tracking.depth_estimation));
        // transform.position = currentPosition;
        transform.position = SmoothingPostion(currentPosition);
    }
    
    Vector3 SmoothingPostion(Vector3 current)
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