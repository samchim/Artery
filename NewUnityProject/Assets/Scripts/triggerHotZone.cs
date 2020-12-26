using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerHotZone : MonoBehaviour
{
    public Material materialSafe;
    public Material materialOut;
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.sharedMaterial = materialSafe;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object Enter trigger");
        if (other.name != "BaseCube")
        {
            Debug.Log("Name: "+other.name);
            rend.sharedMaterial = materialOut;
        }
    }
}
