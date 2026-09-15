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

    
    string damageTrigger = "ShowDamage";
    string pointTrigger = "ShowPoint";

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

    void UpdateHP(int HpCurrent, int hpvalue)
    {
        hpText.text = $"{HpCurrent}";
        hpSlider.value =  (float)HpCurrent/hpvalue;
    }

    void UpdatePoints(int newPoints)
    {
        pointText.text = $"Point : {newPoints}";
        UXPointAnimator.SetTrigger(pointTrigger);
    }

    void ShowDamage(int damageAmount)
    {
            damageText.text = $"-{damageAmount}";
            UXHpAnimator.SetTrigger(damageTrigger);
    }
}
