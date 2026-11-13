using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Configuração")]
    public float speed = 100f;
    public float lifetime = 3f;
    public int damage = 1;

    private Rigidbody projectileRigidbody;
    private bool launched;

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();

        projectileRigidbody.useGravity = false;
        projectileRigidbody.isKinematic = false;
        projectileRigidbody.linearDamping = 0f;
        projectileRigidbody.angularDamping = 0f;
        projectileRigidbody.collisionDetectionMode =
        CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 shipVelocity, Vector3 direction)
    {
        direction = direction.normalized;

        transform.forward = direction;

        // Velocidade da nave + velocidade adicional do laser.
        projectileRigidbody.linearVelocity =
        shipVelocity + direction * speed;

        launched = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        EnemyShip enemy = other.GetComponentInParent<EnemyShip>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}