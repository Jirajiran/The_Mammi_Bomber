using UnityEngine;
using UnityEngine.Audio;

public static class Setting
{
    const string ParamMaster = "MasterVolume";
    const string ParamMusic = "MusicVolume";
    const string ParamVfx = "VFXVolume";
    const string KeyShouldLoad = "ShouldLoadSave";
    const string KeyHasSave = "HasSave";
    const string KeyScore = "PlayerScore";
    const string KeyTurn = "CurrentTurn";
    const string KeyShots = "ShotCount";
    const string KeyBallCount = "BallCount";
    const string KeyCuePrefix = "Cue_";
    const string KeyCamPrefix = "Cam_";
    const string KeyVolMaster = "VolMaster";
    const string KeyVolMusic = "VolMusic";
    const string KeyVolVfx = "VolVFX";

    public const float DefaultVolumeDb = 0f;
    public const float MinVolumeDb = -20f;
    public const float MaxVolumeDb = 10f;

    public static void PrepareNewGame()
    {
        PlayerPrefs.SetInt(KeyShouldLoad, 0);
        DeleteSave();
    }

    public static void PrepareLoadGame()
    {
        PlayerPrefs.SetInt(KeyShouldLoad, 1);
        PlayerPrefs.Save();
    }

    public static bool ShouldLoadOnStart()
    {
        return PlayerPrefs.GetInt(KeyShouldLoad, 0) == 1;
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(KeyHasSave, 0) == 1;
    }

    public static void SaveGameState(int score, int turn, int shots)
    {
        PlayerPrefs.SetInt(KeyScore, score);
        PlayerPrefs.SetInt(KeyTurn, turn);
        PlayerPrefs.SetInt(KeyShots, shots);
        PlayerPrefs.SetInt(KeyHasSave, 1);
        PlayerPrefs.Save();
    }

    public static int LoadScore(int defaultValue = 0) =>
        PlayerPrefs.GetInt(KeyScore, defaultValue);

    public static int LoadTurn(int defaultValue = 0) =>
        PlayerPrefs.GetInt(KeyTurn, defaultValue);

    public static int LoadShots(int defaultValue = 0) =>
        PlayerPrefs.GetInt(KeyShots, defaultValue);

    public static void SaveCue(Vector3 pos, float rotY, int shots, bool aiming)
    {
        PlayerPrefs.SetFloat(KeyCuePrefix + "px", pos.x);
        PlayerPrefs.SetFloat(KeyCuePrefix + "py", pos.y);
        PlayerPrefs.SetFloat(KeyCuePrefix + "pz", pos.z);
        PlayerPrefs.SetFloat(KeyCuePrefix + "rotY", rotY);
        PlayerPrefs.SetInt(KeyCuePrefix + "shots", shots);
        PlayerPrefs.SetInt(KeyCuePrefix + "aim", aiming ? 1 : 0);
    }

    public static bool TryLoadCue(out Vector3 pos, out float rotY, out int shots, out bool aiming)
    {
        pos = Vector3.zero;
        rotY = 0f;
        shots = 0;
        aiming = true;
        if (!PlayerPrefs.HasKey(KeyCuePrefix + "px"))
            return false;

        pos = new Vector3(
            PlayerPrefs.GetFloat(KeyCuePrefix + "px"),
            PlayerPrefs.GetFloat(KeyCuePrefix + "py"),
            PlayerPrefs.GetFloat(KeyCuePrefix + "pz"));
        rotY = PlayerPrefs.GetFloat(KeyCuePrefix + "rotY");
        shots = PlayerPrefs.GetInt(KeyCuePrefix + "shots");
        aiming = PlayerPrefs.GetInt(KeyCuePrefix + "aim", 1) == 1;
        return true;
    }

    public static void SaveBall(int index, int point, Vector3 pos, float rotY, bool hidden)
    {
        string key = $"Ball_{index}";
        string data = $"{point}|{pos.x}|{pos.y}|{pos.z}|{rotY}|{(hidden ? 1 : 0)}";
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.SetInt(KeyBallCount, index + 1);
    }

    public static bool TryLoadBall(int index, out int point, out Vector3 pos, out float rotY, out bool hidden)
    {
        point = 0;
        pos = Vector3.zero;
        rotY = 0f;
        hidden = false;
        string key = $"Ball_{index}";
        if (!PlayerPrefs.HasKey(key))
            return false;

        string[] parts = PlayerPrefs.GetString(key).Split('|');
        if (parts.Length < 6)
            return false;

        point = int.Parse(parts[0]);
        pos = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
        rotY = float.Parse(parts[4]);
        hidden = parts[5] == "1";
        return true;
    }

    public static int LoadBallCount() =>
        PlayerPrefs.GetInt(KeyBallCount, 0);

    public static void SaveCamera(int index, Vector3 pos, Vector3 euler)
    {
        PlayerPrefs.SetInt(KeyCamPrefix + "index", index);
        PlayerPrefs.SetFloat(KeyCamPrefix + "px", pos.x);
        PlayerPrefs.SetFloat(KeyCamPrefix + "py", pos.y);
        PlayerPrefs.SetFloat(KeyCamPrefix + "pz", pos.z);
        PlayerPrefs.SetFloat(KeyCamPrefix + "rx", euler.x);
        PlayerPrefs.SetFloat(KeyCamPrefix + "ry", euler.y);
        PlayerPrefs.SetFloat(KeyCamPrefix + "rz", euler.z);
    }

    public static bool TryLoadCamera(out int index, out Vector3 pos, out Vector3 euler)
    {
        index = 0;
        pos = Vector3.zero;
        euler = Vector3.zero;
        if (!PlayerPrefs.HasKey(KeyCamPrefix + "px"))
            return false;

        index = PlayerPrefs.GetInt(KeyCamPrefix + "index", 0);
        pos = new Vector3(
            PlayerPrefs.GetFloat(KeyCamPrefix + "px"),
            PlayerPrefs.GetFloat(KeyCamPrefix + "py"),
            PlayerPrefs.GetFloat(KeyCamPrefix + "pz"));
        euler = new Vector3(
            PlayerPrefs.GetFloat(KeyCamPrefix + "rx"),
            PlayerPrefs.GetFloat(KeyCamPrefix + "ry"),
            PlayerPrefs.GetFloat(KeyCamPrefix + "rz"));
        return true;
    }

    public static void SaveVolumeMaster(float db) => SaveVolume(KeyVolMaster, db);
    public static void SaveVolumeMusic(float db) => SaveVolume(KeyVolMusic, db);
    public static void SaveVolumeVfx(float db) => SaveVolume(KeyVolVfx, db);

    public static float LoadVolumeMaster(float defaultDb = DefaultVolumeDb) =>
        LoadVolume(KeyVolMaster, defaultDb);

    public static float LoadVolumeMusic(float defaultDb = DefaultVolumeDb) =>
        LoadVolume(KeyVolMusic, defaultDb);

    public static float LoadVolumeVfx(float defaultDb = DefaultVolumeDb) =>
        LoadVolume(KeyVolVfx, defaultDb);

    public static void ApplyVolumes(AudioMixer mixer)
    {
        if (mixer == null)
            return;

        mixer.SetFloat(ParamMaster, LoadVolumeMaster());
        mixer.SetFloat(ParamMusic, LoadVolumeMusic());
        mixer.SetFloat(ParamVfx, LoadVolumeVfx());
    }

    static void SaveVolume(string key, float db)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp(db, MinVolumeDb, MaxVolumeDb));
        PlayerPrefs.Save();
    }

    static float LoadVolume(string key, float defaultDb)
    {
        if (!PlayerPrefs.HasKey(key))
            return defaultDb;
        return Mathf.Clamp(PlayerPrefs.GetFloat(key), MinVolumeDb, MaxVolumeDb);
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(KeyHasSave);
        PlayerPrefs.DeleteKey(KeyScore);
        PlayerPrefs.DeleteKey(KeyTurn);
        PlayerPrefs.DeleteKey(KeyShots);

        PlayerPrefs.DeleteKey(KeyCuePrefix + "px");
        PlayerPrefs.DeleteKey(KeyCuePrefix + "py");
        PlayerPrefs.DeleteKey(KeyCuePrefix + "pz");
        PlayerPrefs.DeleteKey(KeyCuePrefix + "rotY");
        PlayerPrefs.DeleteKey(KeyCuePrefix + "shots");
        PlayerPrefs.DeleteKey(KeyCuePrefix + "aim");

        PlayerPrefs.DeleteKey(KeyCamPrefix + "index");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "px");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "py");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "pz");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "rx");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "ry");
        PlayerPrefs.DeleteKey(KeyCamPrefix + "rz");

        int count = PlayerPrefs.GetInt(KeyBallCount, 0);
        for (int i = 0; i < count; i++)
            PlayerPrefs.DeleteKey($"Ball_{i}");
        PlayerPrefs.DeleteKey(KeyBallCount);

        PlayerPrefs.Save();
    }
}
