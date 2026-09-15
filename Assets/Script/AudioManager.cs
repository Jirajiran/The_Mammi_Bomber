using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    // music: 0=CoolHipHop(Menu), 1=Community_gamemusic(Gameplay), 2=Lose(reuse), 3=WinGame
    // sfx:   0=Typing, 1=JumpSFX, 2=CoinSFX, 3=HurtSFX, 4=CheckPointSFX, 5=HurtVocal_1, 6=WinGame,
    //        + vocals: OnSpawn, JumpVocal, HurtVocal_2, HurtVocal_3, idleTime_1, IdleTime_2
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
