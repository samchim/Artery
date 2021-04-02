using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class CubeBehaviour : Bolt.EntityBehaviour<ICubeState>
{
    public override void Attached()
    {
        state.SetTransforms(state.CubeTransform, transform);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
