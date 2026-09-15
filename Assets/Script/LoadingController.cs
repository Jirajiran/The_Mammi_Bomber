using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingController : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Scene_01";
    [SerializeField] Slider loadingSlider; // เปลี่ยนจาก Image เป็น Slider
    [SerializeField] TMP_Text percentText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] float minShowSeconds = 1.2f;

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        if (statusText != null)
            statusText.text = "Loading...";

        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        if (op == null)
        {
            if (statusText != null)
                statusText.text = $"Missing scene: {gameSceneName}";
            yield break;
        }

        op.allowSceneActivation = false;
        float shown = 0f;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            shown = Mathf.MoveTowards(shown, progress, Time.unscaledDeltaTime);

            if (loadingSlider != null)
                loadingSlider.value = shown;

            if (percentText != null)
                percentText.text = $"{Mathf.RoundToInt(shown * 100f)}%";

            bool ready = op.progress >= 0.9f && shown >= 0.99f && Time.timeSinceLevelLoad >= minShowSeconds;
            if (ready)
                op.allowSceneActivation = true;

            yield return null;
        }
    }
}