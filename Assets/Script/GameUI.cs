using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] TMP_Text hpText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] TMP_Text pointText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] Animator UXHpAnimator;
    [SerializeField] Animator UXPointAnimator;
    [SerializeField] Animator UXFullHpAnimator;

    string damageTrigger = "ShowDamage";
    string pointTrigger = "ShowPoint";
    string spawnTrigger = "GetSpawnTrigger";

    void OnEnable()
    {
        EventManager.OnHPChanged += UpdateHP;
        EventManager.OnPointChanged += UpdatePoints;
        EventManager.OnTakeDamage += ShowDamage;
        EventManager.OnGetSpawn += ShowGetSpawn;
    }

    void OnDisable()
    {
        EventManager.OnHPChanged -= UpdateHP;
        EventManager.OnPointChanged -= UpdatePoints;
        EventManager.OnTakeDamage -= ShowDamage;
        EventManager.OnGetSpawn -= ShowGetSpawn;
    }

    void UpdateHP(int HpCurrent, int hpvalue)
    {
        hpText.text = $"{HpCurrent}";
        hpSlider.value = (float)HpCurrent / hpvalue;
    }

    void UpdatePoints(int newPoints)
    {
        pointText.text = $"Point : {newPoints}";
        if (UXPointAnimator != null)
            UXPointAnimator.SetTrigger(pointTrigger);
    }

    void ShowDamage(int damageAmount)
    {
        if (damageText != null)
            damageText.text = $"-{damageAmount}";
        if (UXHpAnimator != null)
            UXHpAnimator.SetTrigger(damageTrigger);
    }

    void ShowGetSpawn()
    {
        if (UXFullHpAnimator != null)
            UXFullHpAnimator.SetTrigger(spawnTrigger);
    }
}
