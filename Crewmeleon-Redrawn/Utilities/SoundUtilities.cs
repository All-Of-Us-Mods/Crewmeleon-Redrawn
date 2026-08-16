using UnityEngine;

namespace CrewmeleonRedrawn.Utilities;

public static class SoundUtilities
{
    private const float DefaultFalloffStart = 3f;
    private const float DefaultFalloffEnd = 15f;
    
    private const int PositionalSlotCount = 16;
    
    private static int nextPositionalSlot;
    
    public static AudioSource? Play(AudioClip? clip, float volume = 1f, bool loop = false)
    {
        if (!clip || !SoundManager.Instance)
            return null;

        return SoundManager.Instance.PlaySound(clip, loop, volume, SoundManager.Instance.SfxChannel);
    }
    
    public static AudioSource? PlayAtPosition(
        AudioClip? clip,
        Vector2 position,
        float volume = 1f,
        float falloffStart = DefaultFalloffStart,
        float falloffEnd = DefaultFalloffEnd)
    {
        if (!clip || !SoundManager.Instance)
            return null;
        
        // slotted names to stop the same clip from cutting off the previous ones
        var slot = nextPositionalSlot;
        nextPositionalSlot = (nextPositionalSlot + 1) % PositionalSlotCount;
        
        var source = SoundManager.Instance.PlayNamedSound(
            $"CrewmeleonPositional{slot}",
            clip,
            false,
            SoundManager.Instance.SfxChannel);
        
        if (source)
        {
            source.volume = volume * GetFalloff(position, falloffStart, falloffEnd);
            source.panStereo = GetPan(position);
        }

        return source;
    }

    private static float GetPan(Vector2 position)
    {
        var camera = Camera.main;
        if (!camera) return 0f;
        Vector2 listenerPosition = camera!.transform.position;

        var offset = position - listenerPosition;
        return offset.sqrMagnitude > 0f ? Mathf.Clamp(offset.normalized.x, -1f, 1f) : 0f;
    }
    
    private static float GetFalloff(Vector2 position, float falloffStart, float falloffEnd)
    {
        var camera = Camera.main;
        if (camera)
            return SoundManager.GetSoundVolume(position, camera!.transform.position, falloffStart, falloffEnd, 0f);
        
        return 1f;
    }
}