using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3f, -9f);

    [Tooltip("Quanto a câmera acompanha o deslocamento lateral/vertical da nave (0 = fixa, 1 = cola na nave)")]
    [Range(0f, 1f)] public float lateralFollow = 0.6f;

    [Tooltip("Quanto a câmera olha na direção da nave (0 = olha reto para frente)")]
    [Range(0f, 1f)] public float lookFollow = 0.5f;

    public float lookAhead = 40f;
    public float positionSmooth = 8f;
    public float rotationSmooth = 8f;

    PlayerShip ship;

    void Start()
    {
        if (target != null) ship = target.GetComponent<PlayerShip>();
        if (ship == null) return;

        // Já começa na posição correta, sem "voar" até ela.
        ComputeDesired(out Vector3 pos, out Quaternion rot);
        transform.SetPositionAndRotation(pos, rot);
    }

    void LateUpdate()
    {
        if (ship == null) return;

        ComputeDesired(out Vector3 desiredPosition, out Quaternion desiredRotation);

        transform.position = Vector3.Lerp(transform.position, desiredPosition,
                                          positionSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation,
                                              rotationSmooth * Time.deltaTime);
    }

    void ComputeDesired(out Vector3 position, out Quaternion rotation)
    {
        Quaternion rail = ship.RailRotation;

        // Deslocamento da nave em relação ao centro do trilho.
        Vector3 shift = rail * new Vector3(ship.LocalOffset.x, ship.LocalOffset.y, 0f);
        Vector3 railPoint = target.position - shift;

        position = railPoint + shift * lateralFollow + rail * offset;

        Vector3 lookPoint = railPoint + shift * lookFollow + ship.RailForward * lookAhead;
        rotation = Quaternion.LookRotation(lookPoint - position, rail * Vector3.up);
    }
}