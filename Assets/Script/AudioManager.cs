using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    const int PriorityHurt = 32;
    const int PrioritySfx = 96;
    const int PriorityIdle = 256;

    [SerializeField] AudioClip[] music;
    [SerializeField] AudioClip[] sfx;
    [SerializeField] AudioMixer mixer;

    AudioSource musicSource;
    AudioSource sfxSource;
    AudioSource hurtSource;
    AudioSource idleSource;
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
        hurtSource = GetOrAddSource("HurtSource");
        idleSource = GetOrAddSource("IdleSource");

        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        hurtSource.playOnAwake = false;
        idleSource.playOnAwake = false;
        idleSource.loop = false;

        sfxSource.priority = PrioritySfx;
        hurtSource.priority = PriorityHurt;
        idleSource.priority = PriorityIdle;

        if (mixer == null)
            return;

        AudioMixerGroup[] musicGroups = mixer.FindMatchingGroups("Music");
        AudioMixerGroup[] vfxGroups = mixer.FindMatchingGroups("VFX");
        if (musicGroups.Length > 0)
            musicSource.outputAudioMixerGroup = musicGroups[0];
        if (vfxGroups.Length > 0)
        {
            sfxSource.outputAudioMixerGroup = vfxGroups[0];
            hurtSource.outputAudioMixerGroup = vfxGroups[0];
            idleSource.outputAudioMixerGroup = vfxGroups[0];
        }
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

    public static int PickIndex(int count, int salt, Vector3 pos, int points)
    {
        if (count <= 0)
            return 0;

        int n = salt + Mathf.Abs(Mathf.FloorToInt(pos.x + pos.y + pos.z)) + points;
        return n % count;
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

    public void PlaySfxPicked(string[] clipNames, int salt, Vector3 pos, int points, float volume = 1f)
    {
        if (clipNames == null || clipNames.Length == 0)
            return;

        int i = PickIndex(clipNames.Length, salt, pos, points);
        PlaySfx(clipNames[i], volume);
    }

    public void PlayHurt(string clipName, float volume = 1f)
    {
        AudioClip clip = FindClipByName(sfx, clipName);
        if (clip == null)
            return;

        hurtSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlayHurtPicked(string[] clipNames, int salt, Vector3 pos, int points, float volume = 1f)
    {
        if (clipNames == null || clipNames.Length == 0)
            return;

        int i = PickIndex(clipNames.Length, salt, pos, points);
        PlayHurt(clipNames[i], volume);
    }

    public void PlayIdle(string clipName, float volume = 1f)
    {
        AudioClip clip = FindClipByName(sfx, clipName);
        if (clip == null)
            return;

        idleSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void StopIdle()
    {
        if (idleSource == null)
            return;

        idleSource.Stop();
    }
}
