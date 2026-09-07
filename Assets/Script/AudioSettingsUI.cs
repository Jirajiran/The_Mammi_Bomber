using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    const string ParamMaster = "MasterVolume";
    const string ParamMusic = "MusicVolume";
    const string ParamVfx = "VFXVolume";

    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider vfxSlider;

    void Awake()
    {
        if (masterSlider == null)
            masterSlider = transform.Find("SliderMaster")?.GetComponent<Slider>();
        if (musicSlider == null)
            musicSlider = transform.Find("SliderMusic")?.GetComponent<Slider>();
        if (vfxSlider == null)
            vfxSlider = transform.Find("SliderVFX")?.GetComponent<Slider>();

        if (mixer == null && AudioManager.instance != null)
            mixer = AudioManager.instance.Mixer;
    }

    void Start()
    {
        BindSlider(masterSlider, ParamMaster, Setting.LoadVolumeMaster(), Setting.SaveVolumeMaster);
        BindSlider(musicSlider, ParamMusic, Setting.LoadVolumeMusic(), Setting.SaveVolumeMusic);
        BindSlider(vfxSlider, ParamVfx, Setting.LoadVolumeVfx(), Setting.SaveVolumeVfx);
    }

    void BindSlider(Slider slider, string param, float savedDb, System.Action<float> save)
    {
        if (slider == null)
            return;

        slider.minValue = Setting.MinVolumeDb;
        slider.maxValue = Setting.MaxVolumeDb;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(savedDb);
        ApplyVolume(param, savedDb);

        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(value =>
        {
            float db = Mathf.Clamp(value, Setting.MinVolumeDb, Setting.MaxVolumeDb);
            ApplyVolume(param, db);
            save(db);
        });
    }

    void ApplyVolume(string param, float db)
    {
        if (mixer == null)
            return;
        mixer.SetFloat(param, db);
    }
}
