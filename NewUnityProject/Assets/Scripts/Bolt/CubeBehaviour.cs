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

    private void Awake() {
        
    }
    
    public void AdjustOffset(Transform inputTransform)
    {

    }
}
