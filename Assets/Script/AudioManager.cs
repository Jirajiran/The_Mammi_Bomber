using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // music: 0=Menu, 1=Gameplay, 2=Lose, 3=Win
    // sfx:   0=Typing, 1=Jump/Action, 2=Pickup, 3=Hit, 4=GetPoint, 5=Damage, 6=WinSting
    [SerializeField] AudioClip[] music;
    [SerializeField] AudioClip[] sfx;
    [SerializeField] AudioMixer mixer;

    AudioSource musicSource;
    AudioSource sfxSource;
    bool musicPlaying;

    public AudioMixer Mixer => mixer;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();
        Setting.ApplyVolumes(mixer);
    }

    void EnsureSources()
    {
        musicSource = GetOrAddSource("MusicSource");
        sfxSource = GetOrAddSource("SfxSource");
        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        if (mixer == null)
            return;

        AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups("Music");
        AudioMixerGroup[] vfxGroups = mixer.FindMatchingGroups("VFX");
        if (musicGroups.Length > 0)
            musicSource.outputAudioMixerGroup = musicGroups[0];
        if (vfxGroups.Length > 0)
            sfxSource.outputAudioMixerGroup = vfxGroups[0];
    }

    AudioSource GetOrAddSource(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            child = go.transform;
        }
        var source = child.GetComponent<AudioSource>();
        if (source == null)
            source = child.gameObject.AddComponent<AudioSource>();
        return source;
    }

    public void PlayMusic(int index)
    {
        musicPlaying = false;
        musicSource.Stop();

        if (music == null || index < 0 || index >= music.Length)
            return;

        AudioClip clip = music[index];
        if (clip == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
        musicPlaying = true;
    }

    public void SetMusicPaused(bool paused)
    {
        if (!musicPlaying)
            return;
        if (paused)
            musicSource.Pause();
        else
            musicSource.UnPause();
    }

    public void PlaySfx(int index, float volume = 1f)
    {
        if (sfx == null || index < 0 || index >= sfx.Length)
            return;

        AudioClip clip = sfx[index];
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
