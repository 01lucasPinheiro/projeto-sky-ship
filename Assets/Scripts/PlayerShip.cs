using UnityEngine;

// Roda antes dos outros scripts no FixedUpdate, para inimigos e câmera lerem o trilho já atualizado.
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    [Header("Trilho (trajetória reta)")]
    public float forwardSpeed = 25f;
    public float boostSpeed = 50f;
    public float speedChangeRate = 60f;

    [Header("Movimento em X / Y")]
    [Tooltip("Até onde a nave pode ir a partir do centro do trilho (X = lateral, Y = vertical)")]
    public Vector2 moveLimits = new Vector2(25f, 14f);
    public float moveSpeed = 30f;
    public float moveAcceleration = 90f;
    public bool invertY = false;

    [Header("Inclinação visual")]
    public float maxRoll = 35f;
    public float maxPitchTilt = 15f;
    public float maxYawTilt = 12f;
    public float tiltSmooth = 8f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireInterval = 0.18f;

    // Dados do trilho, usados por inimigos, spawner e câmera.
    public Vector3 RailCenter { get; private set; }
    public Quaternion RailRotation { get; private set; }
    public Vector3 RailForward { get { return RailRotation * Vector3.forward; } }
    public Vector3 RailVelocity { get { return RailForward * currentSpeed; } }
    public Vector2 LocalOffset { get; private set; }

    private Rigidbody body;
    private float nextShot;
    private float currentSpeed;
    private Vector2 lateralVelocity;
    private Quaternion smoothTilt = Quaternion.identity;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        // O trilho segue a direção em que a nave está virada na cena.
        RailRotation = transform.rotation;
        RailCenter = transform.position;
        currentSpeed = forwardSpeed;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextShot)
        {
            nextShot = Time.time + fireInterval;
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 1) O centro do trilho avança sempre em linha reta.
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? boostSpeed : forwardSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * dt);
        RailCenter += RailForward * currentSpeed * dt;

        // 2) A nave se move em X/Y dentro dos limites, em relação ao trilho.
        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical") * (invertY ? -1f : 1f));

        lateralVelocity = Vector2.MoveTowards(lateralVelocity, input * moveSpeed, moveAcceleration * dt);

        Vector2 newOffset = LocalOffset + lateralVelocity * dt;

        if (Mathf.Abs(newOffset.x) >= moveLimits.x) lateralVelocity.x = 0f;
        if (Mathf.Abs(newOffset.y) >= moveLimits.y) lateralVelocity.y = 0f;

        newOffset.x = Mathf.Clamp(newOffset.x, -moveLimits.x, moveLimits.x);
        newOffset.y = Mathf.Clamp(newOffset.y, -moveLimits.y, moveLimits.y);
        LocalOffset = newOffset;

        // 3) Leva o Rigidbody até a posição calculada (mantém a física e a velocidade corretas).
        Vector3 targetPosition = RailCenter + RailRotation * new Vector3(LocalOffset.x, LocalOffset.y, 0f);
        body.linearVelocity = (targetPosition - body.position) / dt;
        body.angularVelocity = Vector3.zero;

        // 4) Inclinação visual conforme o movimento.
        float nx = moveSpeed > 0f ? lateralVelocity.x / moveSpeed : 0f;
        float ny = moveSpeed > 0f ? lateralVelocity.y / moveSpeed : 0f;

        Quaternion tilt = Quaternion.Euler(-ny * maxPitchTilt, nx * maxYawTilt, -nx * maxRoll);
        smoothTilt = Quaternion.Slerp(smoothTilt, tilt, tiltSmooth * dt);

        body.MoveRotation(RailRotation * smoothTilt);
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile Prefab ou FirePoint não configurado.");
            return;
        }

        // Os tiros sempre saem retos, na direção do trilho.
        Vector3 direction = RailForward;
        Quaternion projectileRotation = Quaternion.LookRotation(direction, RailRotation * Vector3.up);

        GameObject newProjectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        Projectile projectile = newProjectile.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError("O prefab selecionado não possui o script Projectile.");
            Destroy(newProjectile);
            return;
        }

        projectile.Launch(RailVelocity, direction);
    }
}