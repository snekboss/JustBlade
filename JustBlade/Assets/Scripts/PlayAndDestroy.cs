using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAndDestroy : MonoBehaviour
{
    public string soundName;
    public AudioSource audioSourceComponent;
    const float BaseSelfDestructDuration = 0.1f;
    const float AudioMinDistance = 1f;
    const float AudioMaxDistance = 10f;
    const float MinPitch = 0.8f;
    const float MaxPitch = 1.2f;

    public void PlayAndSelfDestruct(Vector3 worldPosition, float pitch = 1f)
    {
        audioSourceComponent.spatialBlend = 1f;
        audioSourceComponent.minDistance = AudioMinDistance;
        audioSourceComponent.maxDistance = AudioMaxDistance;
        transform.position = worldPosition;

        audioSourceComponent.pitch = Random.Range(MinPitch, MaxPitch);

        audioSourceComponent.Play();

        Destroy(this.gameObject, BaseSelfDestructDuration + audioSourceComponent.clip.length);
    }
}
