using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class ARCubeInteractionBolt : Bolt.EntityBehaviour<IARCubeState>
{
    private ManoGestureContinuous grab;
    private ManoGestureContinuous pinch;
    private ManoGestureContinuous openPinch;
    private ManoGestureTrigger click;
    private ManoGestureTrigger grabTrigger;

    [SerializeField]
    private Material[] arCubeMaterial;

    private int actionCoolDown;

    private string handTag = "Player";
    private Renderer cubeRenderer;
    
    [SerializeField]
    private GameObject colliding = null;
    private Vector3 collidingOffset;

    private ARDebugManager _arDebugManager;

    void Start()
    {
        Initialize();
    }

    public override void Attached()
    {
        state.SetTransforms(state.ARCubeTransform, transform);
    }

    private void Initialize()
    {
        grab = ManoGestureContinuous.CLOSED_HAND_GESTURE;
        pinch = ManoGestureContinuous.HOLD_GESTURE;
        openPinch = ManoGestureContinuous.OPEN_PINCH_GESTURE;
        click = ManoGestureTrigger.CLICK;
        grabTrigger = ManoGestureTrigger.GRAB_GESTURE;
        cubeRenderer = GetComponent<Renderer>();
        cubeRenderer.sharedMaterial = arCubeMaterial[0];
        cubeRenderer.material = arCubeMaterial[0];
        actionCoolDown = 0;
        _arDebugManager = GameObject.Find("AR Session Origin").GetComponent<ARDebugManager>();
        // colliding = null;
        // FreeFall();
        if (entity.IsOwner)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = true;
        } else 
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
    }

    void Update()
    {
        if (entity.IsOwner)
        {
            if (colliding != null)
            {
                transform.position = colliding.transform.position + collidingOffset;
            }
        }
        // gameObject.GetComponent<Rigidbody>().isKinematic = state.IsKinematic;
    }

    private void FixedUpdate()
    {
        if (actionCoolDown > 0)
        {
            actionCoolDown -= 1;
            Debug.Log("Cool Down: " + actionCoolDown.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other">The collider that stays</param>
    private void OnTriggerStay(Collider other)
    {
        if (entity.IsOwner)
        {
            if (other.gameObject.tag == handTag)
            {
                // if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger == grabTrigger && actionCoolDown == 0)
                if ((other.gameObject.GetComponent<HandColliderBolt>().grabTriggering) && (actionCoolDown == 0))
                {
                    Debug.Log("Action: grabTrigger");
                    _arDebugManager.LogInfo($"Action: grabTrigger, parent: {transform.parent.gameObject.name}");
                    if (other.gameObject != colliding)
                    {
                        Debug.Log("Action: stick with hand");
                        colliding = other.gameObject;
                        collidingOffset = transform.TransformPoint(Vector3.zero) - other.transform.TransformPoint(Vector3.zero);
                        // state.IsKinematic = true;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    else
                    {
                        Debug.Log("Action: FreeFall from grab");
                        FreeFall();
                    }
                    actionCoolDown = 50;
                }
                else if (other.gameObject.GetComponent<HandColliderBolt>().pinching)
                {
                    transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
                }
            }
        }
    }


    /// <summary>
    /// If nothing is performed while hand collider is in the cube.
    /// The cube will free fall.
    /// </summary>
    private void FreeFall()
    {
        colliding = null;
        collidingOffset = Vector3.zero;
        // state.IsKinematic = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    /// <summary>
    /// Vibrate when hand collider enters the cube.
    /// </summary>
    /// <param name="other">The collider that enters</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == handTag)
        {
            // colliding = other.gameObject;
            cubeRenderer.sharedMaterial = arCubeMaterial[1];
            Handheld.Vibrate();
        }
    }

    /// <summary>
    /// Change material when exit the cube
    /// </summary>
    /// <param name="other">The collider that exits</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == handTag)
        {
            cubeRenderer.sharedMaterial = arCubeMaterial[0];
            // colliding = null;
            if (actionCoolDown == 0 && entity.IsOwner)
            {
                Debug.Log("Action: FreeFall from exit");
                FreeFall();
            }
        }
    }
}