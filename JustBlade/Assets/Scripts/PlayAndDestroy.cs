using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="PlayAndDestroy"/>.
/// PlayAndDestroy game objects are used to play a sound, and self destruct after the sound has been played.
/// Naturally, the attached game object will also require an <see cref="AudioSource"/>, thus this script
/// automatically adds one. The sound effect prefabs reside under the "Resources/SoundEffects" folder.
/// To use this script, attach it to an empty game object. Once that's done, Unity will automatically
/// add an <see cref="AudioSource"/> component. Drag and drop this <see cref="AudioSource"/> component
/// to this script's <see cref="audioSourceComponent"/> field in the Inspector menu.
/// Then, drag and drop the <see cref="AudioClip"/> (ie, sound effect) you wish to play (they will most
/// likely reside under the "Imported Assets" fodler). Finally, give this sound effect a name by filling in
/// the <see cref="soundName"/> field. Do not forget to configure the <see cref="SoundEffectManager"/>
/// by adding the <see cref="soundName"/> to the array of sound names to be played (create one if it doesn't exist).
/// Optionally, you can play around with the other fields such as volume, pitch, etc.
/// Note that the sound effects are played based on a simple <see cref="AudioSource.priority"/> system,
/// where (currently) the sound effects which are closer to the <see cref="Camera.main"/> are played with
/// a higher priority.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayAndDestroy : MonoBehaviour
{
    /// <summary>
    /// Name of the sound effect, which will be needed by <see cref="SoundEffectManager"/>
    /// , set in the Inspector menu.
    /// </summary>
    public string soundName;
    /// <summary>
    /// The <see cref="AudioSource"/> component on the game object where the
    /// <see cref="PlayAndDestroy"/> script is attached, set in the Inspector menu.
    /// </summary>
    public AudioSource audioSourceComponent;
    /// <summary>
    /// The <see cref="AudioClip"/> (ie, sound effect) to be played, set in the Inspector menu.
    /// </summary>
    public AudioClip audioClip;
    /// <summary>
    /// Volume of the sound effect, set in the Inspector menu.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float volume;
    /// <summary>
    /// Manual setting for the pitch of the sound, set in the Inspector menu.
    /// </summary>
    [Range(0.1f, 2.0f)]
    public float pitch;

    /// <summary>
    /// Whether or not the sound effect be played with a random pitch, set in the Inspector menu.
    /// </summary>
    public bool useRandomPitch;

    const float BaseSelfDestructDuration = 0.1f;
    const float AudioMinDistance = 1f;
    const float AudioMaxDistance = 10f;
    const float MinPitch = 0.8f;
    const float MaxPitch = 1.25f;

    const bool UsePriority = true; // giving sounds priority based on their distance to main camera.
    const float PriorityDistance = 4f;

    /// <summary>
    /// Plays a <see cref="PlayAndDestroy"/> (ie, sound effect) game object at a world position,
    /// and then self destructs it after the sound effect has been played.
    /// </summary>
    /// <param name="worldPosition">World position to play the sound effect.</param>
    public void PlayAndSelfDestruct(Vector3 worldPosition)
    {
        audioSourceComponent.spatialBlend = 1f;
        audioSourceComponent.minDistance = AudioMinDistance;
        audioSourceComponent.maxDistance = AudioMaxDistance;
        transform.position = worldPosition;

        audioSourceComponent.pitch = pitch;
        if (useRandomPitch)
        {
            audioSourceComponent.pitch = Random.Range(MinPitch, MaxPitch);
        }
        audioSourceComponent.clip = audioClip;
        audioSourceComponent.volume = volume;

        if (UsePriority)
        {
            audioSourceComponent.priority = CalculatePriority(worldPosition);
        }

        audioSourceComponent.Play();

        Destroy(this.gameObject, BaseSelfDestructDuration + audioSourceComponent.clip.length);
    }

    /// <summary>
    /// Calculate the priority of the sound effect to be played, based on the world position on which it is
    /// meant to be played. The priority calculation is a simple distance between the sound effect's
    /// world position and the <see cref="Camera.main"/>.
    /// </summary>
    /// <param name="worldPosition">World position to play the sound effect.</param>
    /// <returns>An integer priority value for the <see cref="AudioSource.priority"/>.</returns>
    int CalculatePriority(Vector3 worldPosition)
    {
        // Priority calculation.
        float distFromCamera = Vector3.Distance(worldPosition, Camera.main.transform.position);
        float distNormalized = Mathf.Clamp01(distFromCamera / PriorityDistance);

        // In Unity, highest priority is 0; and lowest is 255.
        int priority = System.Convert.ToInt32(distNormalized * 255);
        return Mathf.Clamp(priority, 0, 255);
    }
}
