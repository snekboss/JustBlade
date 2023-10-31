using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAndDestroy : MonoBehaviour
{
    public string soundName;
    public AudioSource audioSourceComponent;
    [Range(0.0f, 1.0f)]
    public float volume;
    const float BaseSelfDestructDuration = 0.1f;
    const float AudioMinDistance = 1f;
    const float AudioMaxDistance = 10f;
    const float MinPitch = 0.8f;
    const float MaxPitch = 1.25f;

    public void PlayAndSelfDestruct(Vector3 worldPosition)
    {
        audioSourceComponent.spatialBlend = 1f;
        audioSourceComponent.minDistance = AudioMinDistance;
        audioSourceComponent.maxDistance = AudioMaxDistance;
        transform.position = worldPosition;

        audioSourceComponent.pitch = Random.Range(MinPitch, MaxPitch);

        audioSourceComponent.volume = volume;
        audioSourceComponent.Play();

        Destroy(this.gameObject, BaseSelfDestructDuration + audioSourceComponent.clip.length);
    }
}
