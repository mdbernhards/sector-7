using UnityEngine;

public static class GameSettings
{
    private const string MusicKey = "settings.musicVolume";
    private const string SoundKey = "settings.soundVolume";

    private const float DefaultMusic = 0.8f;
    private const float DefaultSound = 0.8f;

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicKey, DefaultMusic);
        set
        {
            PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            ApplyToSceneAudio();
        }
    }

    public static float SoundVolume
    {
        get => PlayerPrefs.GetFloat(SoundKey, DefaultSound);
        set
        {
            PlayerPrefs.SetFloat(SoundKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            ApplyToSceneAudio();
        }
    }

    public static void ApplyToSceneAudio()
    {
        RegisterUntrackedSceneAudio();

        MenuAudioChannelSource[] audioSources = Object.FindObjectsOfType<MenuAudioChannelSource>(true);
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].ApplyCurrentSettings();
        }
    }

    public static MenuAudioChannelSource RegisterAudioSource(
        AudioSource source,
        MenuAudioChannelSource.Channel channel,
        float baseVolume)
    {
        if (source == null)
        {
            return null;
        }

        MenuAudioChannelSource channelSource = source.GetComponent<MenuAudioChannelSource>();
        if (channelSource == null)
        {
            channelSource = source.gameObject.AddComponent<MenuAudioChannelSource>();
        }

        channelSource.Configure(channel, baseVolume);
        return channelSource;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyAfterSceneLoad()
    {
        ApplyToSceneAudio();
    }

    private static void RegisterUntrackedSceneAudio()
    {
        AudioSource[] sources = Object.FindObjectsOfType<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || source.GetComponent<MenuAudioChannelSource>() != null)
            {
                continue;
            }

            RegisterAudioSource(source, InferChannel(source), source.volume);
        }
    }

    private static MenuAudioChannelSource.Channel InferChannel(AudioSource source)
    {
        string objectName = source.gameObject.name;
        string clipName = source.clip != null ? source.clip.name : string.Empty;
        string combined = $"{objectName} {clipName}";

        if (ContainsAny(combined, "music", "bgm", "ambient music", "cyberpunk"))
        {
            return MenuAudioChannelSource.Channel.Music;
        }

        return MenuAudioChannelSource.Channel.Sound;
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < needles.Length; i++)
        {
            if (value.IndexOf(needles[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
