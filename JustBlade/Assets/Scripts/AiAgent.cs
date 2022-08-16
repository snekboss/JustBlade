using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiAgent : Agent
{
    // These are temporary
    public float moveX;
    public float moveY;
    public bool isGrounded;
    public bool isAtk;
    public bool isDef;
    public CombatDirection combatDir;

    void Update()
    {
        AnimMgr.UpdateCombatDirection(combatDir);
        AnimMgr.UpdateAnimations(moveX, moveY, isGrounded, isAtk, isDef);
    }
}
