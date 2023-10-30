using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAndDestroy : MonoBehaviour
{
    public string soundName;
    public AudioSource audioSourceComponent;
    static readonly float BaseSelfDestructDuration = 0.1f;
    static readonly float AudioMinDistance = 1f;
    static readonly float AudioMaxDistance = 10f;

    public void PlayAndSelfDestruct(Vector3 worldPosition)
    {
        audioSourceComponent.spatialBlend = 1f;
        audioSourceComponent.minDistance = AudioMinDistance;
        audioSourceComponent.maxDistance = AudioMaxDistance;
        audioSourceComponent.Play();
        Destroy(audioSourceComponent.gameObject, BaseSelfDestructDuration + audioSourceComponent.clip.length);
    }
}
