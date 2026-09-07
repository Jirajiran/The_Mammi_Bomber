using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 3;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] Animator animator;
    [SerializeField] string deathTrigger = "Die";

    Rigidbody rb;
    PlayerController controller;
    int hp;
    int points;
    bool isDead;

    public int HP => hp;
    public int Points => points;
    public bool IsDead => isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        hp = maxHP;
        points = 0;
    }

    void OnEnable()
    {
        EventManager.OnTakeDamage += ApplyDamage;
    }

    void OnDisable()
    {
        EventManager.OnTakeDamage -= ApplyDamage;
    }

    void Start()
    {
        EventManager.OnHPChanged?.Invoke(hp);
        EventManager.OnPointChanged?.Invoke(points);
    }

    void ApplyDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0)
            return;

        hp -= damageAmount;
        if (hp < 0)
            hp = 0;

        rb.AddForce(Vector3.back * knockbackForce, ForceMode.Impulse);
        EventManager.OnHPChanged?.Invoke(hp);

        if (hp <= 0)
            Die();
    }

    public void AddPoints(int amount)
    {
        if (isDead || amount == 0)
            return;

        points += amount;
        EventManager.OnPointChanged?.Invoke(points);
    }

    public void SetState(int newHP, int newPoints)
    {
        hp = Mathf.Max(0, newHP);
        points = Mathf.Max(0, newPoints);
        isDead = false;
        EventManager.OnHPChanged?.Invoke(hp);
        EventManager.OnPointChanged?.Invoke(points);

        if (hp <= 0)
            Die();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        if (controller != null)
            controller.StopMovement();

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        EventManager.OnGameOver?.Invoke();
    }
}
