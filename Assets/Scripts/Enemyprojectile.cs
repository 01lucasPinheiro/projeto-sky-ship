using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 45f;
    public float lifetime = 5f;
    public int damage = 1;
    public float hitRadius = 0.7f;

    Vector3 direction = Vector3.forward;
    static readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    // Cria o tiro. Se não houver prefab, gera uma esfera vermelha simples.
    public static EnemyProjectile Spawn(GameObject prefab, Vector3 position, Vector3 direction,
                                        float speed, int damage)
    {
        GameObject go = prefab != null
            ? Instantiate(prefab, position, Quaternion.LookRotation(direction))
            : CreateDefaultVisual(position, direction);

        EnemyProjectile projectile = go.GetComponent<EnemyProjectile>();
        if (projectile == null) projectile = go.AddComponent<EnemyProjectile>();

        projectile.speed = speed;
        projectile.damage = damage;
        projectile.direction = direction.normalized;
        return projectile;
    }

    static GameObject CreateDefaultVisual(Vector3 position, Vector3 direction)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnemyProjectile";
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation(direction);
        go.transform.localScale = Vector3.one * 1.5f;

        // O dano é detectado por SphereCast, então o collider padrão não é necessário.
        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            Color red = new Color(1f, 0.1f, 0.1f);
            rend.material.color = red;
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", red * 3f);
        }
        return go;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        // Varre o caminho deste frame para não "atravessar" a nave em alta velocidade.
        int count = Physics.SphereCastNonAlloc(transform.position, hitRadius, direction,
                                               hitBuffer, step, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            PlayerHealth health = hitBuffer[i].collider.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        transform.position += direction * step;
    }
}