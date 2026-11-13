using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyShip : MonoBehaviour
{
    [Header("Status")]
    public int health = 3;
    public int scoreValue = 100;

    [Header("Posição à frente do Player")]
    public Vector2 distanceRange = new Vector2(70f, 100f);

    [Range(0.1f, 1f)]
    public float areaScale = 0.6f;

    [Header("Movimento")]
    public float smoothTime = 1.5f;
    public float maxSpeed = 25f;
    public Vector2 retargetInterval = new Vector2(4f, 7f);

    [Range(0f, 1f)]
    public float idleChance = 0.35f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Vector2 fireInterval = new Vector2(2f, 4f);
    public float projectileSpeed = 45f;
    public int projectileDamage = 1;
    public float aimSpread = 3f;
    public float muzzleOffset = 3f;

    [Header("Visual")]
    public Vector3 modelRotationOffset =
        new Vector3(90f, 0f, 0f);

    public float bankAmount = 2.5f;
    public float maxBank = 35f;

    Rigidbody body;
    PlayerShip ship;
    PlayerHealth playerHealth;

    Vector3 localOffset;
    Vector3 targetOffset;
    Vector3 offsetVelocity;

    float slotDistance;
    float retargetTimer;
    float nextShot;

    bool isDead;
    bool combatStopped;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        GameObject target =
            GameObject.FindGameObjectWithTag("Player");

        if (target == null)
        {
            Debug.LogError(
                "Nenhum objeto com a tag Player foi encontrado."
            );
            return;
        }

        ship = target.GetComponent<PlayerShip>();

        if (ship == null)
        {
            Debug.LogError(
                "O objeto Player não possui PlayerShip."
            );
            return;
        }

        playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "O Player não possui PlayerHealth."
            );
        }

        slotDistance =
            Random.Range(distanceRange.x, distanceRange.y);

        localOffset =
            Quaternion.Inverse(ship.RailRotation) *
            (transform.position - ship.RailCenter);

        PickNewTarget(false);
        ScheduleNextShot(true);
    }

    void FixedUpdate()
    {
        if (ship == null || isDead || combatStopped) return;

        float dt = Time.fixedDeltaTime;

        retargetTimer -= dt;

        if (retargetTimer <= 0f)
        {
            PickNewTarget(true);
        }

        localOffset = Vector3.SmoothDamp(
            localOffset,
            targetOffset,
            ref offsetVelocity,
            smoothTime,
            maxSpeed,
            dt
        );

        Quaternion railRotation = ship.RailRotation;

        Vector3 worldPosition =
            ship.RailCenter +
            railRotation * localOffset;

        Vector3 toPlayer =
            ship.transform.position - worldPosition;

        if (toPlayer.sqrMagnitude < 0.01f)
        {
            toPlayer = -ship.RailForward;
        }

        float bank = Mathf.Clamp(
            -offsetVelocity.x * bankAmount,
            -maxBank,
            maxBank
        );

        Quaternion look = Quaternion.LookRotation(
            toPlayer,
            railRotation * Vector3.up
        );

        Quaternion finalRotation =
            look *
            Quaternion.Euler(0f, 0f, bank) *
            Quaternion.Euler(modelRotationOffset);

        body.MovePosition(worldPosition);
        body.MoveRotation(finalRotation);
    }

    void Update()
    {
        if (ship == null || isDead || combatStopped) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.GameWon)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        bool arrived =
            localOffset.z <= slotDistance + 20f;

        if (arrived && Time.time >= nextShot)
        {
            Fire();
            ScheduleNextShot(false);
        }
    }

    void ScheduleNextShot(bool first)
    {
        float minimumInterval =
            first
                ? fireInterval.x * 0.5f
                : fireInterval.x;

        nextShot = Time.time +
                   Random.Range(
                       minimumInterval,
                       fireInterval.y
                   );
    }

    void Fire()
    {
        Vector2 jitter =
            Random.insideUnitCircle * aimSpread;

        Vector3 aimPoint =
            ship.transform.position +
            ship.RailRotation *
            new Vector3(jitter.x, jitter.y, 0f);

        Vector3 origin =
            firePoint != null
                ? firePoint.position
                : transform.position;

        Vector3 direction =
            (aimPoint - origin).normalized;

        if (firePoint == null)
        {
            origin += direction * muzzleOffset;
        }

        EnemyProjectile.Spawn(
            projectilePrefab,
            origin,
            direction,
            projectileSpeed,
            projectileDamage
        );
    }

    void PickNewTarget(bool allowIdle)
    {
        retargetTimer =
            Random.Range(
                retargetInterval.x,
                retargetInterval.y
            );

        if (allowIdle && Random.value < idleChance)
        {
            return;
        }

        Vector2 area =
            ship.moveLimits * areaScale;

        targetOffset = new Vector3(
            Random.Range(-area.x, area.x),
            Random.Range(-area.y, area.y),
            slotDistance
        );
    }

    public void TakeDamage(int amount)
    {
        if (isDead || combatStopped || amount <= 0)
        {
            return;
        }

        health = Mathf.Max(0, health - amount);

        if (health == 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.EnemyDestroyed();
        }

        Destroy(gameObject);
    }

    public void StopCombat()
    {
        combatStopped = true;
    }
}