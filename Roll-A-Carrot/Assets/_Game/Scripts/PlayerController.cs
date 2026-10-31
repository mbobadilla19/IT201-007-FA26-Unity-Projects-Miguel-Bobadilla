using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0;

    [Header("Jump")]
    public float jumpStrength = 6f;

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
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    void Update()
    {
        Vector3 direction = new Vector3(movementX, 0f, movementY);
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
        rb.AddForce(movement * speed);

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