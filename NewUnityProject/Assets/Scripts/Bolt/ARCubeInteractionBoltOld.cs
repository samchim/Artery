using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class ARCubeInteractionBoltOld : Bolt.EntityBehaviour<ICubeState>
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
    // private GameObject colliding;

    private GameObject defaultParent;

    private ARDebugManager _arDebugManager;

    void Start()
    {
        Initialize();
    }

    public override void Attached()
    {
        state.SetTransforms(state.CubeTransform, transform);
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
        defaultParent = transform.parent.gameObject;
        _arDebugManager = GameObject.Find("AR Session Origin").GetComponent<ARDebugManager>();
        // colliding = null;
        // FreeFall();
        // if (BoltNetwork.IsServer)
        // {
        //     _arDebugManager.LogInfo($"You are the server, you can move me");
        // }
    }

    void Update()
    {
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
    private void OnTriggerStay(Collider collider)
    {
        if (BoltNetwork.IsServer)
        {
            if (collider.gameObject.tag == handTag)
            {
                // if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger == grabTrigger && actionCoolDown == 0)
                if ((collider.gameObject.GetComponent<HandColliderBolt>().grabTriggering) && (actionCoolDown == 0))
                {
                    Debug.Log("Action: grabTrigger");
                    _arDebugManager.LogInfo($"Action: grabTrigger, parent: {transform.parent.gameObject.name}");
                    if (transform.parent.gameObject != collider.gameObject.transform.gameObject)
                    {
                        Debug.Log("Action: stick with hand");
                        transform.parent = collider.gameObject.transform;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    else
                    {
                        Debug.Log("Action: FreeFall from grab");
                        FreeFall();
                    }
                    actionCoolDown = 50;
                }
                else if (collider.gameObject.GetComponent<HandColliderBolt>().pinching)
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
        transform.parent = defaultParent.transform;
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
            if (actionCoolDown == 0)
            {
                Debug.Log("Action: FreeFall from exit");
                FreeFall();
            }
        }
    }
}