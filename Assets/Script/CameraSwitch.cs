using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] Camera[] cameras;
    int index;

    public int CurrentIndex => index;

    void Start()
    {
        // New game: pick camera 0. Load game: GameManager.LoadCameraState applies saved index.
        if (!(Setting.ShouldLoadOnStart() && Setting.HasSave()))
            Show(0);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.IsLocked)
            return;


        if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevCamera();
            AudioManager.instance.PlaySfx(0);
        }
        if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCamera();
            AudioManager.instance.PlaySfx(0);
        }
    }

    public void NextCamera()
    {
        Show(index + 1);
    }

    public void PrevCamera()
    {
        Show(index - 1);
    }

    public void ApplySavedIndex(int savedIndex)
    {
        Show(savedIndex);
    }

    void Show(int next)
    {
        if (cameras == null || cameras.Length == 0)
            return;

        index = (next % cameras.Length + cameras.Length) % cameras.Length;
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null)
                continue;
            bool on = i == index;
            cameras[i].enabled = on;
            var listener = cameras[i].GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = on;
        }
    }
}
