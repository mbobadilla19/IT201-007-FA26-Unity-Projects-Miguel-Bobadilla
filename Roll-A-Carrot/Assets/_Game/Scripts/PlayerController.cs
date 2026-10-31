using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0;

    [Header("Jump")]
    public float jumpStrength = 6f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public Transform characterModel;
    public float turnSpeed = 12f;
    public Animator characterAnimator;

    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    public GameObject grassLevelDoor;
    public GameObject desertDoor;
    public GameObject snowDoor;
    public GameObject castleDoor;

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private bool jumpRequested;
    private bool dashRequested;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private float speedBeforeDash;
    private Vector3 dashDirection;
    private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        winTextObject.SetActive(false);
        SetCountText();
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    // PlayerInput sends this when the existing Jump action (Space) is pressed.
    void OnJump(InputValue value)
    {
        if (value.isPressed && Time.timeScale > 0f)
        {
            jumpRequested = true;
        }
    }

    // Left Shift is already bound to the Sprint input action.
    void OnSprint(InputValue value)
    {
        if (value.isPressed && Time.timeScale > 0f)
        {
            dashRequested = true;
        }
    }

    void Update()
    {
        Vector3 direction = new Vector3(movementX, 0f, movementY);
        if (dashTimeRemaining > 0f)
        {
            direction = dashDirection;
        }
        bool isMoving = direction.sqrMagnitude > 0.01f;

        // Switch between Idle and Run using movement input.
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsMoving", isMoving);
        }

        // Turn the visual pivot while preserving the facing offset.
        if (characterModel != null && isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            characterModel.rotation = Quaternion.Slerp(
                characterModel.rotation,
                targetRotation,
                1f - Mathf.Exp(-turnSpeed * Time.deltaTime)
            );
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0f, movementY);
        if (!UpdateDash(movement))
        {
            rb.AddForce(movement * speed);
        }

        if (jumpRequested)
        {
            groundContacts.RemoveWhere(surface => surface == null ||
                !surface.enabled || !surface.gameObject.activeInHierarchy);

            if (groundContacts.Count > 0 && rb.linearVelocity.y <= 0.1f)
            {
                // Keep horizontal movement while giving each jump a consistent lift.
                Vector3 velocity = rb.linearVelocity;
                velocity.y = 0f;
                rb.linearVelocity = velocity;
                rb.AddForce(Vector3.up * jumpStrength, ForceMode.VelocityChange);
                groundContacts.Clear();
            }

            // An airborne press is discarded rather than waiting until landing.
            jumpRequested = false;
        }
    }

    private bool UpdateDash(Vector3 movement)
    {
        dashCooldownRemaining = Mathf.Max(0f,
            dashCooldownRemaining - Time.fixedDeltaTime);

        if (dashTimeRemaining > 0f)
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            if (dashTimeRemaining <= 0f)
            {
                // End the burst without restoring velocity into a wall we hit.
                Vector3 velocity = rb.linearVelocity;
                Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
                horizontal = Vector3.ClampMagnitude(horizontal, speedBeforeDash);
                rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
            }
        }

        if (dashRequested && dashCooldownRemaining <= 0f && dashTimeRemaining <= 0f)
        {
            Vector3 facing = characterModel != null ? characterModel.forward : transform.forward;
            dashDirection = movement.sqrMagnitude > 0.01f ? movement : facing;
            dashDirection.y = 0f;
            dashDirection.Normalize();

            if (dashDirection.sqrMagnitude > 0f)
            {
                Vector3 velocity = rb.linearVelocity;
                speedBeforeDash = new Vector3(velocity.x, 0f, velocity.z).magnitude;
                rb.linearVelocity = new Vector3(
                    dashDirection.x * dashSpeed, velocity.y, dashDirection.z * dashSpeed);
                dashTimeRemaining = Mathf.Max(Time.fixedDeltaTime, dashDuration);
                dashCooldownRemaining = Mathf.Max(dashTimeRemaining, dashCooldown);
            }
        }

        // Holding Shift does not repeatedly dash; press it again after the cooldown.
        dashRequested = false;
        return dashTimeRemaining > 0f;
    }

    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();

        if (count >= 2 && grassLevelDoor != null)
        {
            grassLevelDoor.SetActive(false);
        }

        if (count >= 5 && desertDoor != null)
        {
            desertDoor.SetActive(false);
        }

        if (count >= 8 && snowDoor != null)
        {
            snowDoor.SetActive(false);
        }

        if (count >= 12 && castleDoor != null)
        {
            castleDoor.SetActive(false);
        }

        // Includes the final sky collectible.
        if (count >= 14)
        {
            winTextObject.SetActive(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TrackGroundContact(collision);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        TrackGroundContact(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        groundContacts.Remove(collision.collider);
    }

    private void TrackGroundContact(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            // Floors and walkable slopes support a jump; vertical walls do not.
            if (Vector3.Dot(collision.GetContact(i).normal, Vector3.up) >= 0.6f)
            {
                groundContacts.Add(collision.collider);
                return;
            }
        }

        groundContacts.Remove(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeInHierarchy &&
            other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();
        }
    }
}