using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the functionality of UI elements that control game settings.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    #region PlayerPref Keys

    public const string MasterVolumeKey = "Settings.Volume.Master";
    public const string MusicVolumeKey = "Settings.Volume.Music";
    public const string SFXVolumeKey = "Settings.Volume.SFX";

    public const string InvertYKey = "Settings.Controls.InvertY";
    public const string MouseSensitivityKey = "Settings.Controls.Sensitivity.Mouse";
    public const string ControllerSensitivityKey = "Settings.Controls.Sensitivity.Controller";

    #endregion

    #region Default Values

    public const float DefaultMasterVolume = 1.0f;
    public const float DefaultMusicVolume = 1.0f;
    public const float DefaultSFXVolume = 1.0f;

    public const bool DefaultInvertY = true;
    public const float DefaultMouseSensitivity = 1.0f;
    public const float DefaultControllerSensitivity = 1.0f;

    #endregion

    #region Inspector

    [Header("Volume")]

    [Tooltip("The slider that controls the master volume.")]
    [SerializeField] private Slider masterVolumeSlider;

    [Tooltip("The slider that controls the music volume.")]
    [SerializeField] private Slider musicVolumeSlider;

    [Tooltip("The slider that controls the sfx volume.")]
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Controls")]

    [Tooltip("Invert Y-axis for controller.")]
    [SerializeField] private Toggle invertYToggle;

    [Tooltip("The slider that controls the mouse rotation speed multiplier.")]
    [SerializeField] private Slider mouseSensitivitySlider;

    [Tooltip("The slider that controls the controller rotation speed multiplier.")]
    [SerializeField] private Slider controllerSensitivitySlider;

    #endregion

    #region Unity Event Functions

    private void Start()
    {
        Initialize(masterVolumeSlider, MasterVolumeKey, DefaultMasterVolume);
        Initialize(musicVolumeSlider, MusicVolumeKey, DefaultMusicVolume);
        Initialize(sfxVolumeSlider, SFXVolumeKey, DefaultSFXVolume);

        Initialize(invertYToggle, InvertYKey, DefaultInvertY);
        Initialize(mouseSensitivitySlider, MouseSensitivityKey, DefaultMouseSensitivity);
        Initialize(controllerSensitivitySlider, ControllerSensitivityKey, DefaultControllerSensitivity);
    }

    #endregion

    /// <summary>
    /// Initialize a <see cref="Slider"/> by binding the value set to a value set in the <see cref="PlayerPrefs"/>.
    /// </summary>
    /// <param name="slider">The <see cref="Slider"/> to bind.</param>
    /// <param name="key">The <see cref="PlayerPrefs"/> key to bind.</param>
    /// <param name="defaultValue">The default value for the <paramref name="slider"/> when the <see cref="PlayerPrefs"/> key does not exist yet.</param>
    private void Initialize(Slider slider, string key, float defaultValue)
    {
        // Set the value in the slider without onValueChanged being invoked.
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(key, defaultValue));
        // Set the slider value in the PlayerPrefs if the slider is changed through the UI.
        slider.onValueChanged.AddListener((float value) =>
        {
            PlayerPrefs.SetFloat(key, value);
        });
    }

    /// <summary>
    /// Initialize a <see cref="Toggle"/> by binding the value set to a value set in the <see cref="PlayerPrefs"/>.
    /// </summary>
    /// <param name="toggle">The <see cref="Toggle"/> to bind.</param>
    /// <param name="key">The <see cref="PlayerPrefs"/> key to bind.</param>
    /// <param name="defaultValue">The default value for the <paramref name="toggle"/> when the <see cref="PlayerPrefs"/> key does not exist yet.</param>
    private void Initialize(Toggle toggle, string key, bool defaultValue)
    {
        // Set the value in the toggle without onValueChanged being invoked.
        toggle.SetIsOnWithoutNotify(GetBool(key, defaultValue));
        // Set the toggle value in the PlayerPrefs if the toggle is changed through the UI.
        toggle.onValueChanged.AddListener((bool value) =>
        {
            SetBool(key, value);
        });
    }

    #region PlayerPrefs

    /// <summary>
    /// Sets a single boolean value for the preference identified by the given <paramref name="key"/>. You can use <see cref="GetBool"/> to retrieve this value.
    /// </summary>
    /// <param name="key">The key of the preference.</param>
    /// <param name="value">The value to set.</param>
    public static void SetBool(string key, bool value)
    {
        int intValue = value ? 1 : 0;
        PlayerPrefs.SetInt(key, intValue);
    }

    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    /// <param name="key">The key of the preference.</param>
    /// <param name="defaultValue">The default value if the preference does not exist.</param>
    /// <returns>The value of the preference.</returns>
    public static bool GetBool(string key, bool defaultValue = false)
    {
        int defaultIntValue = defaultValue ? 1 : 0;
        int intValue = PlayerPrefs.GetInt(key, defaultIntValue);
        return intValue != 0;
    }

    #endregion
}
