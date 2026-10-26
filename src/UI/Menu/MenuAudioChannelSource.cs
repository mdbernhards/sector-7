using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuAudioChannelSource : MonoBehaviour
{
    public enum Channel
    {
        Sound,
        Music
    }

    public Channel channel = Channel.Sound;
    [Range(0f, 1f)] public float baseVolume = 1f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (baseVolume <= 0f && source != null)
        {
            baseVolume = source.volume;
        }
    }

    private void OnEnable()
    {
        ApplyCurrentSettings();
    }

    public void ApplyCurrentSettings()
    {
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        float channelVolume = channel == Channel.Music
            ? GameSettings.MusicVolume
            : GameSettings.SoundVolume;

        source.volume = baseVolume * channelVolume;
    }

    public void Configure(Channel audioChannel, float sourceBaseVolume)
    {
        channel = audioChannel;
        baseVolume = Mathf.Clamp01(sourceBaseVolume);
        ApplyCurrentSettings();
    }
}
