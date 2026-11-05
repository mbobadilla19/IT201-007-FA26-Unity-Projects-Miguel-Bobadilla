using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 40f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Quaternion turn = Quaternion.Euler(
            0f, rotationSpeed * Time.fixedDeltaTime, 0f
        );

        rb.MoveRotation(rb.rotation * turn);
    }
}