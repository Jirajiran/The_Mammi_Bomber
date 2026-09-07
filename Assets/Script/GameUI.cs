using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text pointText;
    [SerializeField] TMP_Text damageText;
    [SerializeField] Animator damageAnimator;
    [SerializeField] string damageTrigger = "ShowDamage";

    void OnEnable()
    {
        EventManager.OnHPChanged += UpdateHP;
        EventManager.OnPointChanged += UpdatePoints;
        EventManager.OnTakeDamage += ShowDamage;
    }

    void OnDisable()
    {
        EventManager.OnHPChanged -= UpdateHP;
        EventManager.OnPointChanged -= UpdatePoints;
        EventManager.OnTakeDamage -= ShowDamage;
    }

    void UpdateHP(int newHP)
    {
        if (hpText != null)
            hpText.text = $"HP : {newHP}";
    }

    void UpdatePoints(int newPoints)
    {
        if (pointText != null)
            pointText.text = $"Point : {newPoints}";
    }

    void ShowDamage(int damageAmount)
    {
        if (damageText != null)
            damageText.text = $"-{damageAmount}";

        if (damageAnimator != null && !string.IsNullOrEmpty(damageTrigger))
            damageAnimator.SetTrigger(damageTrigger);
    }
}
