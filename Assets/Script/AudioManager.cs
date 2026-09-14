using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // Assign clips in Inspector; play by index or by clip.name (case-insensitive).
    // Expected music names (optional): Menu, Gameplay, Lose, Win
    // Expected sfx names (optional): Typing, Jump, Pickup, Hit, GetPoint, Damage, WinSting
    // Index map (kept for MenuController / GameManager):
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

    static AudioClip FindClipByName(AudioClip[] clips, string clipName)
    {
        if (clips == null || string.IsNullOrEmpty(clipName))
            return null;

        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[i];
            if (clip != null && string.Equals(clip.name, clipName, System.StringComparison.OrdinalIgnoreCase))
                return clip;
        }
        return null;
    }

    void PlayMusicClip(AudioClip clip)
    {
        musicPlaying = false;
        musicSource.Stop();

        if (clip == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
        musicPlaying = true;
    }

    public void PlayMusic(int index)
    {
        if (music == null || index < 0 || index >= music.Length)
        {
            musicPlaying = false;
            musicSource.Stop();
            return;
        }

        PlayMusicClip(music[index]);
    }

    public void PlayMusic(string clipName)
    {
        PlayMusicClip(FindClipByName(music, clipName));
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

    public void PlaySfx(string clipName, float volume = 1f)
    {
        AudioClip clip = FindClipByName(sfx, clipName);
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
