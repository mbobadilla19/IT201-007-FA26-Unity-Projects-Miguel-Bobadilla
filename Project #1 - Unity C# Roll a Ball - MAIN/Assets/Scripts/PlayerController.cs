using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    //testing commits
    public float speed = 0;
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

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
        }
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