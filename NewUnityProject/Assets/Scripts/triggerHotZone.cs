using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerHotZone : MonoBehaviour
{
    public Material materialSafe;
    public Material materialOut;
    private List<GameObject> touched;
    private int touchedCount;
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.sharedMaterial = materialSafe;
        touchedCount = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object Enter trigger: " + other.name);
        if (other.tag != "GameBase" && other.tag != "player")
        {
            // touched.Add(other.gameObject);
            touchedCount += 1;
            rend.sharedMaterial = materialOut;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Object Exit trigger: " + other.name);
        // touched.Remove(other.gameObject);
        // if (touched.FindAll(collided => (collided.tag != "BaseCube" && collided.tag != "player")).Count == 0){
        //     rend.sharedMaterial = materialSafe;
        // }
        if (other.tag != "GameBase" && other.tag != "player")
        {
            touchedCount -= 1;
        }
        if (touchedCount == 0) {
            rend.sharedMaterial = materialSafe;
        }
    }
}
