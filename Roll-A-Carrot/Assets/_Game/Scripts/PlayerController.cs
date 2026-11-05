using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0;

    [Header("Level Exit")]
    [Min(1)] public int requiredPickups = 12;
    public bool winAfterPickups = false;
    public GameObject levelExit;
    public bool HasWon => gameOver && winAfterPickups && count >= requiredPickups && livesRemaining > 0;
    public bool CanUseExit => !gameOver && count >= requiredPickups;

    [Header("Lives")]
    public int startingLives = 3;
    public TextMeshProUGUI livesText;
    public float respawnProtection = 1.5f;

    [Header("Pickup Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 0.6f;

    [Header("Respawn")]
    public Transform respawnPoint;

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
    private AudioSource pickupAudioSource;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private int count;
    private int livesRemaining;
    public int LivesRemaining => livesRemaining;
    private bool gameOver;
    private float protectedUntil;
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

        // A dedicated source lets the sound finish after its coin is hidden.
        pickupAudioSource = gameObject.AddComponent<AudioSource>();
        pickupAudioSource.playOnAwake = false;
        pickupAudioSource.loop = false;
        pickupAudioSource.spatialBlend = 0f;
        startingPosition = rb.position;
        startingRotation = rb.rotation;
        count = 0;
        if (levelExit != null)
        {
            levelExit.SetActive(false);
        }
        livesRemaining = Mathf.Max(1, startingLives);
        UpdateLivesText();
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
        if (value.isPressed && Time.timeScale > 0f && !gameOver)
        {
            jumpRequested = true;
        }
    }

    // Left Shift is already bound to the Sprint input action.
    void OnSprint(InputValue value)
    {
        if (value.isPressed && Time.timeScale > 0f && !gameOver)
        {
            dashRequested = true;
        }
    }

    void Update()
    {
        if (gameOver)
        {
            return;
        }

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
        if (gameOver)
        {
            return;
        }

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

    public void Respawn()
    {
        if (gameOver)
        {
            return;
        }

        // Stop the fall and any active dash before returning to a safe point.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        jumpRequested = false;
        dashRequested = false;
        dashTimeRemaining = 0f;
        dashCooldownRemaining = 0f;
        groundContacts.Clear();

        rb.position = respawnPoint != null ? respawnPoint.position : startingPosition;
        rb.rotation = respawnPoint != null ? respawnPoint.rotation : startingRotation;
    }

    public void LoseLife()
    {
        if (gameOver || Time.time < protectedUntil)
        {
            return;
        }

        livesRemaining--;
        UpdateLivesText();

        if (livesRemaining <= 0)
        {
            gameOver = true;
            movementX = 0f;
            movementY = 0f;
            jumpRequested = false;
            dashRequested = false;
            dashTimeRemaining = 0f;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            if (characterAnimator != null)
            {
                characterAnimator.SetBool("IsMoving", false);
            }

            if (winTextObject != null)
            {
                winTextObject.GetComponent<TextMeshProUGUI>().text = "Game Over!";
                winTextObject.SetActive(true);
            }
            return;
        }

        Respawn();
        protectedUntil = Time.time + Mathf.Max(0f, respawnProtection);
    }

    public bool TryRestoreLife()
    {
        if (gameOver || livesRemaining >= Mathf.Max(1, startingLives))
            return false;
        livesRemaining++;
        UpdateLivesText();
        return true;
    }

    private void UpdateLivesText()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + livesRemaining;
        }
    }

    void SetCountText()
    {
        countText.text = "Coins: " + count + " / " + requiredPickups;

        // Enable this only in the final scene; other scenes still unlock their exit.
        if (winAfterPickups && count >= requiredPickups && !gameOver)
        {
            gameOver = true;
            movementX = 0f;
            movementY = 0f;
            jumpRequested = false;
            dashRequested = false;
            dashTimeRemaining = 0f;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            if (characterAnimator != null)
            {
                characterAnimator.SetBool("IsMoving", false);
            }

            if (winTextObject != null)
            {
                winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";
                winTextObject.SetActive(true);
            }
            return;
        }

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

        if (count >= 11 && castleDoor != null)
        {
            castleDoor.SetActive(false);
        }

        // Collecting the coins reveals the exit; the final win happens in the next scene.
        if (CanUseExit && levelExit != null)
        {
            levelExit.SetActive(true);
            if (winTextObject != null)
            {
                winTextObject.GetComponent<TextMeshProUGUI>().text = "Exit unlocked!";
                winTextObject.SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TrackGroundContact(collision);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        TrackGroundContact(collision);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseLife();
        }
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
        if (gameOver)
        {
            return;
        }

        if (other.gameObject.activeInHierarchy &&
            other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            if (pickupSound != null)
            {
                pickupAudioSource.PlayOneShot(pickupSound, pickupVolume);
            }
            count = count + 1;
            SetCountText();
        }
    }
}