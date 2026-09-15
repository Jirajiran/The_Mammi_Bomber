using UnityEngine;
using UnityEngine.Audio;

public static class Setting
{
    const string ParamMaster = "MasterVolume";
    const string ParamMusic = "MusicVolume";
    const string ParamVfx = "VFXVolume";
    const string KeyShouldLoad = "ShouldLoadSave";
    const string KeyHasSave = "HasSave";
    const string KeyHP = "PlayerHP";
    const string KeyPoints = "PlayerPoints";
    const string KeyPosX = "PlayerPosX";
    const string KeyPosY = "PlayerPosY";
    const string KeyPosZ = "PlayerPosZ";
    const string KeyPointCount = "PointItemCount";
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

    // collected[i] == true → already taken → do not clone
    public static void SaveGame(int hp, int points, Vector3 position, bool[] collected)
    {
        PlayerPrefs.SetInt(KeyHP, hp);
        PlayerPrefs.SetInt(KeyPoints, points);
        PlayerPrefs.SetFloat(KeyPosX, position.x);
        PlayerPrefs.SetFloat(KeyPosY, position.y);
        PlayerPrefs.SetFloat(KeyPosZ, position.z);

        int count = collected != null ? collected.Length : 0;
        PlayerPrefs.SetInt(KeyPointCount, count);
        for (int i = 0; i < count; i++)
            PlayerPrefs.SetInt($"PointCollected_{i}", collected[i] ? 1 : 0);

        PlayerPrefs.SetInt(KeyHasSave, 1);
        PlayerPrefs.Save();
    }

    public static int LoadHP(int defaultValue = 3) =>
        PlayerPrefs.GetInt(KeyHP, defaultValue);

    public static int LoadPoints(int defaultValue = 0) =>
        PlayerPrefs.GetInt(KeyPoints, defaultValue);

    public static Vector3 LoadPosition(Vector3 defaultValue)
    {
        if (!PlayerPrefs.HasKey(KeyPosX))
            return defaultValue;

        return new Vector3(
            PlayerPrefs.GetFloat(KeyPosX),
            PlayerPrefs.GetFloat(KeyPosY),
            PlayerPrefs.GetFloat(KeyPosZ));
    }

    public static bool[] LoadPointCollected()
    {
        int count = PlayerPrefs.GetInt(KeyPointCount, 0);
        bool[] states = new bool[count];
        for (int i = 0; i < count; i++)
            states[i] = PlayerPrefs.GetInt($"PointCollected_{i}", 0) == 1;
        return states;
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
        PlayerPrefs.DeleteKey(KeyHP);
        PlayerPrefs.DeleteKey(KeyPoints);
        PlayerPrefs.DeleteKey(KeyPosX);
        PlayerPrefs.DeleteKey(KeyPosY);
        PlayerPrefs.DeleteKey(KeyPosZ);

        int count = PlayerPrefs.GetInt(KeyPointCount, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"PointCollected_{i}");
            PlayerPrefs.DeleteKey($"PointActive_{i}");
        }
        PlayerPrefs.DeleteKey(KeyPointCount);

        PlayerPrefs.Save();
    }
}
