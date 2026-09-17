using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 3;
    [SerializeField] float knockbackForce = 8f;
    [SerializeField] float respawnDelay = 3f;
    [SerializeField] float meshPartDelay = 1f;
    [SerializeField] Animator animator;
    [SerializeField] GameObject meshPartPrefab;
    [SerializeField] GameObject speakiUsed;
    [SerializeField] string onSpawnSfxName = "OnSpawn";
    [SerializeField] Transform spawnPoint;
    [SerializeField] float spawnYOffset = 1.5f;
    [SerializeField] float fallDeathY = 50f;

    static readonly string[] HurtVocals = { "HurtVocal_1", "HurtVocal_2", "HurtVocal_3" };

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
    public Animator Animator => animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        hp = maxHP;
        points = 0;
        spawnPosition = transform.position;

        if (animator == null && speakiUsed != null)
            animator = speakiUsed.GetComponent<Animator>();
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
            SavePoint.SetCheckpoint(GetSpawnBasePosition());

        EventManager.OnHPChanged?.Invoke(hp, maxHP);
        EventManager.OnPointChanged?.Invoke(points);
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (transform.position.y < fallDeathY)
            EventManager.OnTakeDamage?.Invoke(999);
    }

    void ApplyDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0)
            return;

        hp -= damageAmount;
        if (hp < 0)
            hp = 0;

        rb.AddForce(Vector3.back * knockbackForce, ForceMode.Impulse);
        EventManager.OnHPChanged?.Invoke(hp, maxHP);

        if (animator != null)
            animator.SetTrigger("GetHurt");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopWalk();
            AudioManager.instance.PlayHurt("HurtSFX");
            AudioManager.instance.PlayHurtPicked(HurtVocals, 3, transform.position, points);
        }

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
        EventManager.OnHPChanged?.Invoke(hp, maxHP);
    }

    public void SetState(int newHP, int newPoints)
    {
        hp = Mathf.Max(0, newHP);
        points = Mathf.Max(0, newPoints);
        isDead = false;
        if (animator != null)
            animator.SetBool("IsDead", false);
        EventManager.OnHPChanged?.Invoke(hp, maxHP);
        EventManager.OnPointChanged?.Invoke(points);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        if (controller != null)
            controller.StopMovement();

        if (animator != null)
            animator.SetBool("IsDead", true);

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(meshPartDelay);

        if (meshPartPrefab != null)
            Instantiate(meshPartPrefab, transform.position, transform.rotation);

        AudioManager.instance?.PlaySfx("DieSFX");

        if (speakiUsed != null)
            speakiUsed.SetActive(false);

        float remaining = respawnDelay - meshPartDelay;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        Vector3 pos = GetRespawnPosition();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = pos;

        HealFull();
        isDead = false;

        if (animator != null)
            animator.SetBool("IsDead", false);

        if (AudioManager.instance != null && !string.IsNullOrEmpty(onSpawnSfxName))
            AudioManager.instance.PlaySfx(onSpawnSfxName);

        EventManager.OnGetSpawn?.Invoke();

        if (speakiUsed != null)
            speakiUsed.SetActive(true);

        if (controller != null)
            controller.ResumeInput();
    }

    Vector3 GetSpawnBasePosition()
    {
        if (SavePoint.HasCheckpoint)
            return SavePoint.LastCheckpoint;

        if (spawnPoint != null)
            return spawnPoint.position;

        return spawnPosition;
    }

    Vector3 GetRespawnPosition()
    {
        Vector3 pos = GetSpawnBasePosition();
        pos.y += spawnYOffset;
        return pos;
    }
}
