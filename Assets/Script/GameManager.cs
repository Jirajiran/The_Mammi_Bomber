using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] int winFlagTarget = 3;
    [SerializeField] GameObject winUI;
    [SerializeField] GameObject loseUI;
    [SerializeField] GameObject pauseUI;
    [SerializeField] PlayerHealth player;
    [SerializeField] string menuSceneName = "Menu";

    bool isPaused;
    bool gameEnded;

    public bool IsLocked => isPaused || gameEnded;

    void Awake()
    {
        instance = this;
        if (player == null)
            player = FindFirstObjectByType<PlayerHealth>();
    }

    void OnEnable()
    {
        EventManager.OnWinFlagReached += HandleWinFlag;
        EventManager.OnGameOver += HandleGameOver;
        EventManager.OnGameWin += HandleGameWin;
    }

    void OnDisable()
    {
        EventManager.OnWinFlagReached -= HandleWinFlag;
        EventManager.OnGameOver -= HandleGameOver;
        EventManager.OnGameWin -= HandleGameWin;
    }

    void Start()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        isPaused = false;
        SetUI(winUI, false);
        SetUI(loseUI, false);
        SetUI(pauseUI, false);

        WinFlagItem.ResetCount();

        if (Setting.ShouldLoadOnStart() && Setting.HasSave())
            LoadSaveGame();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(1);
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

    void HandleGameOver()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        Setting.DeleteSave();
        SetUI(loseUI, true);
        Time.timeScale = 0f;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(2);
    }

    void HandleGameWin()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        Setting.DeleteSave();
        SetUI(winUI, true);
        Time.timeScale = 0f;

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
                CollectPointStates());
        }

        GoToMenu();
    }

    void LoadSaveGame()
    {
        if (player == null)
            return;

        int hp = Setting.LoadHP();
        int points = Setting.LoadPoints();
        Vector3 pos = Setting.LoadPosition(player.transform.position);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = pos;
        player.SetState(hp, points);
        ApplyPointStates(Setting.LoadPointStates());
    }

    static bool[] CollectPointStates()
    {
        PointItem[] items = FindObjectsByType<PointItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(items, (a, b) => string.CompareOrdinal(a.name, b.name));

        bool[] states = new bool[items.Length];
        for (int i = 0; i < items.Length; i++)
            states[i] = items[i].gameObject.activeSelf;
        return states;
    }

    static void ApplyPointStates(bool[] states)
    {
        if (states == null || states.Length == 0)
            return;

        PointItem[] items = FindObjectsByType<PointItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(items, (a, b) => string.CompareOrdinal(a.name, b.name));

        int count = Mathf.Min(items.Length, states.Length);
        for (int i = 0; i < count; i++)
            items[i].gameObject.SetActive(states[i]);
    }

    static void SetUI(GameObject ui, bool active)
    {
        if (ui != null)
            ui.SetActive(active);
    }
}
