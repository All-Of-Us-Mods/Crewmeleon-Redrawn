using Il2CppInterop.Runtime;
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
        
        var dynamics = DelegateSupport.ConvertDelegate<DynamicSound.GetDynamicsFunction>(new Action<AudioSource, float>((source, _) => UpdatePositionalSource(source, position, volume, falloffStart, falloffEnd)));
        var source = SoundManager.Instance.PlayDynamicSound(
            $"CrewmeleonPositional{slot}",
            clip,
            false,
            dynamics,
            SoundManager.Instance.SfxChannel);
        
        if (source) UpdatePositionalSource(source, position, volume, falloffStart, falloffEnd);

        return source;
    }

    private static void UpdatePositionalSource(AudioSource source, Vector2 position, float volume, float falloffStart, float falloffEnd)
    {
        source.volume = volume * GetFalloff(position, falloffStart, falloffEnd);
        source.panStereo = GetPan(position);
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