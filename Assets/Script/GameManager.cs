using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class BallSpawn
{
    public BallColor ballColor;
    public Transform point;
}

[System.Serializable]
public class BallLook
{
    public BallColor ballColor;
    public Material ballMaterial;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    const int WinScore = 9;
    const int MaxShots = 5;
    const int CuePotPenalty = 4;

    [SerializeField] int playerScore;
    [SerializeField] int currentTurn;

    [Header("Spawn")]
    [SerializeField] BallSpawn[] ballSpawns;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] BallLook[] ballLooks;

    [Header("UI")]
    [SerializeField] TMP_Text pointText;
    [SerializeField] TMP_Text endGameText;
    [SerializeField] TMP_Text TurnText;
    [SerializeField] TMP_Text GetPoint;
    [SerializeField] TMP_Text MinusPoint;
    [SerializeField] Animator pauseAnimator;
    [SerializeField] Animator endGameAnimator;
    [SerializeField] Animator UxPoint;
    [SerializeField] Animator characterAnimator;
    [SerializeField] string menuSceneName = "Menu";

    bool isPaused;
    bool gameEnded;

    public bool IsLocked => isPaused || gameEnded;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
            pauseAnimator.SetBool("pauseUi", false);
            endGameAnimator.SetBool("ShowEndGame", false);
        {
            characterAnimator.SetBool("WinPose", false);
            characterAnimator.SetBool("LosePose", false);
        }

        if (Setting.ShouldLoadOnStart() && Setting.HasSave())
            LoadSaveGame();
        else
            StartNewGame();

        AudioManager.instance.PlayMusic(1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    void StartNewGame()
    {
        gameEnded = false;
        playerScore = 0;
        currentTurn = 0;
        SpawnAllBalls();
        RefreshUI();
    }

    void RefreshUI()
    {
        if (pointText != null)
            pointText.text = $"Point : {playerScore}";
        UpdateTurnText();
    }

    void SpawnAllBalls()
    {
        if (ballPrefab == null || ballSpawns == null)
            return;

        foreach (BallSpawn spawn in ballSpawns)
        {
            if (spawn.point == null || spawn.ballColor == BallColor.White)
                continue;
            SetBall(spawn.ballColor, spawn.point);
        }
    }

    void SetBall(BallColor ballColor, Transform point)
    {
        GameObject ballObject = Instantiate(ballPrefab, point.position, Quaternion.identity);
        ball spawnedBall = ballObject.GetComponent<ball>();
        if (spawnedBall == null)
            spawnedBall = ballObject.AddComponent<ball>();

        spawnedBall.Apply((int)ballColor, GetBallMaterial(ballColor));
    }

    Material GetBallMaterial(BallColor ballColor)
    {
        if (ballLooks == null)
            return null;
        foreach (BallLook look in ballLooks)
        {
            if (look.ballColor == ballColor)
                return look.ballMaterial;
        }
        return null;
    }

    public void BallPotted(ball pottedBall)
    {
        if (gameEnded || pottedBall == null)
            return;

        if (pottedBall.GetComponent<DriveBall>() != null)
        {
            playerScore = Mathf.Max(0, playerScore - CuePotPenalty);
            pointText.text = $"Point : {playerScore}";
            ShowUxPoint(-CuePotPenalty);
            DriveBall.Instance.RespawnAtCuePoint();
            return;
        }

        int gained = pottedBall.Point;
        playerScore += gained;
        pottedBall.HideBall();
        pointText.text = $"Point : {playerScore}";
        ShowUxPoint(gained);
        TryEndGame(-1);
    }

    void ShowUxPoint(int delta)
    {
        if (UxPoint == null)
            return;

        if (delta > 0)
        {
            if (GetPoint != null)
                GetPoint.text = $"+{delta}";
            UxPoint.SetTrigger("GetPointAni");
            AudioManager.instance.PlaySfx(4);
        }
        else if (delta < 0)
        {
            if (MinusPoint != null)
                MinusPoint.text = delta.ToString();
            UxPoint.SetTrigger("MinusPointAni");
            AudioManager.instance.PlaySfx(5);
        }
    }

    public void SetTurn(int turn)
    {
        currentTurn = turn;
        UpdateTurnText();
    }

    void UpdateTurnText()
    {
        if (TurnText != null)
            TurnText.text = $"Turn : {currentTurn} / {MaxShots}";
    }

    public void OnCueStopped(int shots)
    {
        SetTurn(shots);
        TryEndGame(shots);
    }

    void TryEndGame(int shots)
    {
        if (gameEnded)
            return;

        bool win = playerScore > WinScore;
        bool lose = shots >= 0 && shots >= MaxShots && !win;
        if (!win && !lose)
            return;

        gameEnded = true;
        Setting.DeleteSave();

            pauseAnimator.SetBool("pauseUi", false);
            endGameAnimator.SetBool("ShowEndGame", true);
            endGameText.text = win
                ? $"You are win\nYour score is {playerScore} > 9"
                : $"Your are lose\nYour score is {playerScore} < 9";
        {
            if (win)
            {
                AudioManager.instance.PlayMusic(3);
                AudioManager.instance.PlaySfx(6);
            }
            else
            {
                AudioManager.instance.PlayMusic(2);
            }
        }
        
            characterAnimator.SetBool("WinPose", win);
            characterAnimator.SetBool("LosePose", !win);
    
    }

    public void PauseGame()
    {
        if (gameEnded)
            return;
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        pauseAnimator.SetBool("pauseUi", isPaused);
        AudioManager.instance.SetMusicPaused(isPaused);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Setting.PrepareNewGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        SaveAndGoToMenu();
    }

    public void SaveAndGoToMenu()
    {
        if (!gameEnded)
            SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void SaveGame()
    {
        int shots = DriveBall.Instance != null ? DriveBall.Instance.ShotCount : 0;
        Setting.SaveGameState(playerScore, currentTurn, shots);

        if (DriveBall.Instance != null)
        {
            Transform cue = DriveBall.Instance.transform;
            Setting.SaveCue(
                cue.position,
                cue.eulerAngles.y,
                shots,
                DriveBall.Instance.CanForce);
        }

        ball[] balls = GetSortedPlayBalls();
        for (int i = 0; i < balls.Length; i++)
        {
            ball b = balls[i];
            Setting.SaveBall(
                i,
                b.Point,
                b.transform.position,
                b.transform.eulerAngles.y,
                b.IsHidden);
        }

        SaveCameraState();
    }

    void SaveCameraState()
    {
        var camSwitch = FindFirstObjectByType<CameraSwitch>();
        int camIndex = camSwitch != null ? camSwitch.CurrentIndex : 0;

        var followCam = FindFirstObjectByType<FollowBallCamera>();
        if (followCam != null)
        {
            Transform t = followCam.transform;
            Setting.SaveCamera(camIndex, t.position, t.eulerAngles);
        }
    }

    void LoadCameraState()
    {
        var camSwitch = FindFirstObjectByType<CameraSwitch>();
        if (!Setting.TryLoadCamera(out int camIndex, out Vector3 camPos, out Vector3 camEuler))
        {
            camSwitch?.ApplySavedIndex(0);
            return;
        }

        var followCam = FindFirstObjectByType<FollowBallCamera>();
        if (followCam != null)
            followCam.ApplySavedTransform(camPos, camEuler);

        if (camSwitch != null)
            camSwitch.ApplySavedIndex(camIndex);
    }

    ball[] GetSortedPlayBalls()
    {
        ball[] all = FindObjectsByType<ball>(FindObjectsSortMode.None);
        var list = new List<ball>();
        foreach (ball b in all)
        {
            if (b.GetComponent<DriveBall>() == null)
                list.Add(b);
        }
        list.Sort((a, b) =>
        {
            int byPoint = a.Point.CompareTo(b.Point);
            if (byPoint != 0)
                return byPoint;
            int byX = a.transform.position.x.CompareTo(b.transform.position.x);
            if (byX != 0)
                return byX;
            return a.transform.position.z.CompareTo(b.transform.position.z);
        });
        return list.ToArray();
    }

    public void LoadSaveGame()
    {
        gameEnded = false;
        playerScore = Setting.LoadScore();
        currentTurn = Setting.LoadTurn();
        int shots = Setting.LoadShots();

        SpawnAllBalls();

        ball[] balls = GetSortedPlayBalls();
        for (int i = 0; i < balls.Length; i++)
        {
            if (!Setting.TryLoadBall(i, out _, out Vector3 pos, out float rotY, out bool hidden))
                continue;

            balls[i].transform.position = pos;
            balls[i].transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            if (hidden)
                balls[i].HideBall();
            else
                balls[i].ShowBall();
        }

        if (DriveBall.Instance != null &&
            Setting.TryLoadCue(out Vector3 cuePos, out float cueRotY, out int cueShots, out bool aiming))
        {
            DriveBall.Instance.ApplySaveState(cuePos, cueRotY, cueShots, aiming);
            currentTurn = cueShots;
        }
        else if (DriveBall.Instance != null)
        {
            DriveBall.Instance.SetShotCount(shots);
            currentTurn = shots;
        }

        LoadCameraState();
        RefreshUI();
    }
}
