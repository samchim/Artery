using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCubeInteraction : MonoBehaviour
{
    private ManoGestureContinuous grab;
    private ManoGestureContinuous pinch;
    private ManoGestureContinuous openPinch;
    private ManoGestureTrigger click;
    private ManoGestureTrigger grabTrigger;

    [SerializeField]
    private Material[] arCubeMaterial;
    [SerializeField]
    private GameObject smallCube;
    private GameObject handCollider;

    private int actionCoolDown;

    private string handTag = "Player";
    private Renderer cubeRenderer;

    private GameObject defaultParent;

    void Start()
    {
        Initialize();
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
        handCollider = GameObject.FindGameObjectsWithTag("Player")[0];
        actionCoolDown = 0;
        defaultParent = transform.parent.gameObject;
        FreeFall();
    }

    void Update()
    {
        Debug.Log("Gravity: " + gameObject.GetComponent<Rigidbody>().useGravity.ToString() + ", Kinematic :" + gameObject.GetComponent<Rigidbody>().isKinematic.ToString());
        if (transform.parent == defaultParent)
        {
            Debug.Log("transform.parent: defaultParent");
        }
        else
        {
            Debug.Log("transform.parent: hand");
        }
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
        if (other.gameObject.tag == handTag)
        {
            if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger == grabTrigger && actionCoolDown == 0)
            {
                Debug.Log("Action: grabTrigger");
                if (transform.parent == defaultParent)
                {
                    Debug.Log("Action: stick with hand");
                    transform.parent = handCollider.transform;
                    gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    Debug.Log("Action: FreeFall from grab");
                    FreeFall();
                }
                actionCoolDown = 50;
            }
            else if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == pinch)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
            }
            // SpawnWhenClicking(other);
        }
    }

    /// <summary>
    /// If pinch is performed while hand collider is in the cube.
    /// The cube will start rotate.
    /// </summary>
    private void RotateWhenHolding(Collider other)
    {
        if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_continuous == openPinch)
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
        }
        // else if ()
        // {
        //     transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
        // }
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
    /// If pick is performed while hand collider is in the cube.
    /// The cube will follow the hand.
    /// </summary>
    private void SpawnWhenClicking(Collider other)
    {
        if (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger == click)
        {
            Instantiate(smallCube, new Vector3(transform.position.x, transform.position.y + transform.localScale.y / 1.5f, transform.position.z), Quaternion.identity);
        }
    }

    /// <summary>
    /// Vibrate when hand collider enters the cube.
    /// </summary>
    /// <param name="other">The collider that enters</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == handTag)
        {
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
            if (actionCoolDown == 0)
            {
                Debug.Log("Action: FreeFall from exit");
                FreeFall();
            }
        }
    }
}


///////////////////////////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// If grab is performed while hand collider is in the cube.
/// The cube will follow the hand.
/// </summary>
// private void MoveWhenGrab(Collider other)
// {
//     transform.parent = other.gameObject.transform;
//     gameObject.GetComponent<Rigidbody>().useGravity = false;
// }

/// <summary>
/// If open pinch is performed while hand collider is in the cube.
/// The cube will float.
/// </summary>
// private void FloatWhenOpenPinch(Collider other)
// {
//     gameObject.GetComponent<Rigidbody>().useGravity = false;
// }