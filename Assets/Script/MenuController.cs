using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] Button newGameButton;
    [SerializeField] Button loadGameButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button settingButton;
    [SerializeField] Button CloseSettingButton;
    [SerializeField] GameObject settingPanel;

    float loadDelay = 0.08f;
    string loadingSceneName = "Loading";

    void Awake()
    {
        WireButton(newGameButton, NewGame);
        WireButton(loadGameButton, LoadGame);
        WireButton(exitButton, ExitGame);
        WireButton(settingButton, () => OpenSetting(true));
        WireButton(CloseSettingButton, () => OpenSetting(false));
    }

    void Start()
    {
        UpdateLoadButton();
        AudioManager.instance.PlayMusic(0);
    }

    void UpdateLoadButton()
    {
        if (loadGameButton != null)
            loadGameButton.interactable = Setting.HasSave();
    }

    void WireButton(Button btn, UnityEngine.Events.UnityAction onClick)
    {
        if (btn == null)
            return;

        btn.onClick.RemoveListener(onClick);
        btn.onClick.AddListener(PlayTyping);
        btn.onClick.AddListener(onClick);

        var hook = btn.GetComponent<UiPointerEnterHook>();
        if (hook == null)
            hook = btn.gameObject.AddComponent<UiPointerEnterHook>();
        hook.onEnter = PlayTyping;
    }

    void PlayTyping()
    {
        AudioManager.instance.PlaySfx(0);
    }

    public void NewGame()
    {
        Setting.PrepareNewGame();
        StartCoroutine(LoadGameScene());
    }

    public void LoadGame()
    {
        if (!Setting.HasSave())
            return;
        Setting.PrepareLoadGame();
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        if (loadDelay > 0f)
            yield return new WaitForSecondsRealtime(loadDelay);
        SceneManager.LoadScene(loadingSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenSetting(bool flag)
    {
        settingPanel.SetActive(flag);
    }
}

public class UiPointerEnterHook : MonoBehaviour, IPointerEnterHandler
{
    public System.Action onEnter;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onEnter?.Invoke();
    }
}
