using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    [Header("Voo")]
    public float forwardSpeed = 50f;
    public float boostSpeed = 45f;
    public float pitchSpeed = 80f;
    public float yawSpeed = 90f;
    public float rollAngle = 25f;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireInterval = 0.18f;

    Rigidbody body;
    float nextShot;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextShot)
        {
            nextShot = Time.time + fireInterval;
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }

    void FixedUpdate()
    {
        float yaw = Input.GetAxis("Horizontal");
        float pitch = Input.GetAxis("Vertical");
        float speed = Input.GetKey(KeyCode.LeftShift) ? boostSpeed : forwardSpeed;

        body.linearVelocity = transform.forward * speed;
        Quaternion turn = Quaternion.Euler(-pitch * pitchSpeed * Time.fixedDeltaTime,
                                            yaw * yawSpeed * Time.fixedDeltaTime, 0f);
        body.MoveRotation(body.rotation * turn);

        float targetRoll = -yaw * rollAngle;
        Vector3 angles = transform.localEulerAngles;
        float currentRoll = angles.z > 180f ? angles.z - 360f : angles.z;
        float smoothRoll = Mathf.Lerp(currentRoll, targetRoll, 5f * Time.fixedDeltaTime);
        transform.localEulerAngles = new Vector3(angles.x, angles.y, smoothRoll);
    }
}
