using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PointSlot
{
    public Vector3 pos;
    public bool collected;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] int winFlagTarget = 3;
    [SerializeField] GameObject winUI;
    [SerializeField] GameObject pauseUI;
    [SerializeField] PlayerHealth player;
    [SerializeField] Transform[] posPointRoot;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] string menuSceneName = "Menu";

    PointSlot[] points;

    bool isPaused;
    bool gameEnded;

    public bool IsLocked => isPaused;

    void Awake()
    {
        instance = this;
        if (player == null)
            player = FindFirstObjectByType<PlayerHealth>();
    }

    void OnEnable()
    {
        EventManager.OnWinFlagReached += HandleWinFlag;
        EventManager.OnGameWin += HandleGameWin;
    }

    void OnDisable()
    {
        EventManager.OnWinFlagReached -= HandleWinFlag;
        EventManager.OnGameWin -= HandleGameWin;
    }

    void Start()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        isPaused = false;
        SetUI(winUI, false);
        SetUI(pauseUI, false);

        WinFlagItem.ResetCount();
        SavePoint.ClearCheckpoint();

        if (Setting.ShouldLoadOnStart() && Setting.HasSave())
            LoadSaveGame();
        else
        {
            if (player != null)
                SavePoint.SetCheckpoint(player.transform.position);
            InitAndSpawnPoints(null);
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(1);

        PlayerController controller = player != null
            ? player.GetComponent<PlayerController>()
            : FindFirstObjectByType<PlayerController>();
        if (controller != null)
            controller.SnapGameplayCursor();
    }

    void Update()
    {
        if (gameEnded)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    void HandleWinFlag(int currentCount)
    {
        if (gameEnded)
            return;

        if (currentCount >= winFlagTarget)
            EventManager.OnGameWin?.Invoke();
    }

    void HandleGameWin()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        Setting.DeleteSave();
        SetUI(winUI, true);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(3);
            AudioManager.instance.PlaySfx(6);
        }
    }

    public void TogglePause()
    {
        if (gameEnded)
            return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        SetUI(pauseUI, isPaused);

        if (isPaused)
            ShowUiCursor();
        else
            RestoreGameplayCursor();

        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicPaused(isPaused);
    }

    public void Resume()
    {
        if (!isPaused || gameEnded)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        SetUI(pauseUI, false);
        RestoreGameplayCursor();

        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicPaused(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Setting.PrepareNewGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void SaveAndGoToMenu()
    {
        if (!gameEnded && player != null)
        {
            Setting.SaveGame(
                player.HP,
                player.Points,
                player.transform.position,
                GetCollectedStates());
        }

        GoToMenu();
    }

    public void MarkPointCollected(int index)
    {
        if (points == null || index < 0 || index >= points.Length)
            return;

        points[index].collected = true;
    }

    public bool[] GetCollectedStates()
    {
        if (points == null || points.Length == 0)
            return System.Array.Empty<bool>();

        bool[] states = new bool[points.Length];
        for (int i = 0; i < points.Length; i++)
            states[i] = points[i].collected;
        return states;
    }

    void LoadSaveGame()
    {
        if (player == null)
            return;

        int hp = Setting.LoadHP();
        int pointsValue = Setting.LoadPoints();
        Vector3 pos = Setting.LoadPosition(player.transform.position);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = pos;
        player.SetState(hp, pointsValue);
        SavePoint.SetCheckpoint(pos);
        InitAndSpawnPoints(Setting.LoadPointCollected());
    }

    void InitAndSpawnPoints(bool[] collectedFromSave)
    {
        if (posPointRoot == null || posPointRoot.Length == 0 || pointPrefab == null)
            return;

        int count = posPointRoot.Length;
        points = new PointSlot[count];

        for (int i = 0; i < count; i++)
        {
            bool collected = collectedFromSave != null
                && i < collectedFromSave.Length
                && collectedFromSave[i];

            points[i] = new PointSlot
            {
                pos = posPointRoot[i].position,
                collected = collected
            };
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].collected)
                continue;

            GameObject go = Instantiate(pointPrefab, points[i].pos, Quaternion.identity);
            PointItem item = go.GetComponent<PointItem>();
            if (item == null)
                item = go.GetComponentInChildren<PointItem>();
            if (item != null)
                item.Setup(i);
        }
    }

    static void SetUI(GameObject ui, bool active)
    {
        if (ui != null)
            ui.SetActive(active);
    }

    static void ShowUiCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestoreGameplayCursor()
    {
        PlayerController controller = player != null
            ? player.GetComponent<PlayerController>()
            : FindFirstObjectByType<PlayerController>();

        if (controller != null)
            controller.RefreshCursor();
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
