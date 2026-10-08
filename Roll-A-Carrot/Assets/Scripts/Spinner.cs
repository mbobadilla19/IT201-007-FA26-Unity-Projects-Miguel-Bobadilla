using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 40f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Quaternion turn = Quaternion.Euler(
            0f, rotationSpeed * Time.fixedDeltaTime, 0f
        );

        rb.MoveRotation(rb.rotation * turn);
    }
}