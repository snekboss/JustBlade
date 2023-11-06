using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvaderAgentData : HordeAgentData
{
    public HordeRewardData invaderRewardDataPrefab;
    public bool isAggressive; // attack relentlessly, without defending at all
}
