using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 3;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] float respawnDelay = 3f;
    [SerializeField] Animator animator;
    [SerializeField] string deathTrigger = "Die";

    Rigidbody rb;
    PlayerController controller;
    Vector3 spawnPosition;
    int hp;
    int points;
    bool isDead;

    public int HP => hp;
    public int MaxHP => maxHP;
    public int Points => points;
    public bool IsDead => isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        hp = maxHP;
        points = 0;
        spawnPosition = transform.position;
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
        if (!SavePoint.HasCheckpoint)
            SavePoint.SetCheckpoint(transform.position);

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

    public void HealFull()
    {
        hp = maxHP;
        EventManager.OnHPChanged?.Invoke(hp);
    }

    public void SetState(int newHP, int newPoints)
    {
        hp = Mathf.Max(0, newHP);
        points = Mathf.Max(0, newPoints);
        isDead = false;
        EventManager.OnHPChanged?.Invoke(hp);
        EventManager.OnPointChanged?.Invoke(points);
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

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 pos = SavePoint.HasCheckpoint ? SavePoint.LastCheckpoint : spawnPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = pos;

        HealFull();
        isDead = false;

        if (controller != null)
            controller.ResumeInput();
    }
}
