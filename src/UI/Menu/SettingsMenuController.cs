using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider soundSlider;
    public TMP_Text musicValueText;
    public TMP_Text soundValueText;

    [Header("Controls Text")]
    public TMP_Text controlsText;
    [TextArea(2, 10)] public string controlsDisplay = "WASD - Move\nTAB - Open Data Rig";

    [Header("Optional Panels")]
    public GameObject settingsRoot;

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdatePercentText(musicValueText, musicSlider.value);
        }

        if (soundSlider != null)
        {
            soundSlider.SetValueWithoutNotify(GameSettings.SoundVolume);
            soundSlider.onValueChanged.AddListener(OnSoundChanged);
            UpdatePercentText(soundValueText, soundSlider.value);
        }

        if (controlsText != null)
        {
            controlsText.text = controlsDisplay;
        }

        GameSettings.ApplyToSceneAudio();
    }

    public void Open()
    {
        if (settingsRoot != null)
        {
            settingsRoot.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        if (settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnMusicChanged(float value)
    {
        GameSettings.MusicVolume = value;
        UpdatePercentText(musicValueText, value);
    }

    private void OnSoundChanged(float value)
    {
        GameSettings.SoundVolume = value;
        UpdatePercentText(soundValueText, value);
    }

    private static void UpdatePercentText(TMP_Text target, float sliderValue)
    {
        if (target != null)
        {
            target.text = $"{Mathf.RoundToInt(sliderValue * 100f)}%";
        }
    }
}
