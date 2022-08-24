using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinAgent : Agent
{
    public Camera cam;

    public override void Awake()
    {
        base.Awake();

        Vector3 lookDir = cam.transform.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    void Update()
    {
        AnimMgr.UpdateAnimations(Vector2.zero, 0, true, false, false);
    }
}
