using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Import for TextMeshPro

public class ObjectPicker : MonoBehaviour
{
    public float pickupRange = 5f;     // Maximum range to pick up objects
    public Transform holdPosition;     // Position where the picked object will be held
    public float moveSpeed = 10f;      // Speed at which the object moves to the hold position
    public float throwForce = 500f;    // Initial throw force
    public float maxThrowForce = 1500f; // Maximum throw force
    public float forceIncreaseSpeed = 500f; // How quickly the force increases when holding
    public LayerMask playerLayer;      // Layer mask for the player
    public TextMeshProUGUI forcePercentageText;   // Reference to the TextMeshPro UI element

    private GameObject pickedObject;   // Currently picked object
    private Camera playerCamera;
    private Collider objectCollider;   // Store reference to the object's collider
    private Rigidbody objectRigidbody; // Store reference to the object's Rigidbody
    private Collider playerCollider;   // Player's collider reference
    private float currentThrowForce;   // Current throw force based on hold time
    private bool isHoldingRightMouse = false; // Check if right mouse button is held

    void Start()
    {
        playerCamera = Camera.main; // Get the player's camera
        playerCollider = GetComponent<Collider>(); // Get the player's collider

        if (playerCamera == null)
        {
            Debug.LogError("Main Camera not found! Ensure the camera is tagged as 'MainCamera'.");
        }

        if (playerCollider == null)
        {
            Debug.LogError("Player object must have a Collider component.");
        }

        currentThrowForce = throwForce; // Initialize the throw force

        // Ensure the TextMeshPro UI element is assigned
        if (forcePercentageText == null)
        {
            Debug.LogError("Force Percentage Text UI is not assigned!");
        }
        else
        {
            // Initialize the force percentage to 0 at the start
            forcePercentageText.text = "Force: 0%";
        }
    }

    void Update()
    {
        // If the player presses the left mouse button, attempt to pick up or drop objects
        if (Input.GetMouseButtonDown(0))
        {
            if (pickedObject == null)
            {
                TryPickObject();
            }
            else
            {
                DropObject();
            }
        }

        // If the player presses the right mouse button, start holding to increase the throw force
        if (Input.GetMouseButtonDown(1) && pickedObject != null)
        {
            isHoldingRightMouse = true;
        }

        // If the player releases the right mouse button, throw the object
        if (Input.GetMouseButtonUp(1) && pickedObject != null)
        {
            ThrowObject();
            isHoldingRightMouse = false; // Reset holding state after throw
        }

        // If the right mouse button is being held, increase the throw force
        if (isHoldingRightMouse)
        {
            IncreaseThrowForce();
        }

        // If an object is picked up, move it smoothly to the hold position
        if (pickedObject != null)
        {
            MoveObjectToHoldPosition();
        }

        // Update the force percentage TextMeshPro UI
        UpdateForcePercentageUI();
    }

    void TryPickObject()
    {
        // Check if the camera is assigned before attempting raycasting
        if (playerCamera == null)
        {
            Debug.LogError("No player camera assigned.");
            return;
        }

        // Raycast from the center of the screen
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            // Check if the object hit by the ray has a Rigidbody
            if (hit.collider != null && hit.collider.GetComponent<Rigidbody>())
            {
                PickObject(hit.collider.gameObject);
            }
        }
    }

    void PickObject(GameObject objectToPick)
    {
        pickedObject = objectToPick;
        objectCollider = pickedObject.GetComponent<Collider>();
        objectRigidbody = pickedObject.GetComponent<Rigidbody>();

        // Disable the object's collider to prevent collisions with the player
        if (objectCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objectCollider, playerCollider, true); // Ignore collisions between player and picked object
        }

        // Disable physics for the object
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
        }

        pickedObject.transform.SetParent(holdPosition); // Parent the object to hold position
    }

    void DropObject()
    {
        // Re-enable the object's collider
        if (objectCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objectCollider, playerCollider, false); // Re-enable collision between player and object
        }

        // Re-enable physics for the object
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
        }

        // Unparent the object
        pickedObject.transform.SetParent(null);
        pickedObject = null;

        // Reset throw force to initial value
        currentThrowForce = throwForce;

        // Reset the force percentage UI
        UpdateForcePercentageUI();
    }

    void ThrowObject()
    {
        // Unparent the object and re-enable physics
        pickedObject.transform.SetParent(null);
        if (objectCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(objectCollider, playerCollider, false); // Re-enable collision between player and object
        }

        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
            // Apply the current throw force in the direction the player is facing
            objectRigidbody.AddForce(playerCamera.transform.forward * currentThrowForce);
        }

        pickedObject = null; // Clear the reference to the object after throwing

        // Reset throw force to initial value
        currentThrowForce = throwForce;

        // Reset the force percentage UI
        UpdateForcePercentageUI();
    }

    void IncreaseThrowForce()
    {
        // Increase the throw force over time but clamp it to the maximum throw force
        currentThrowForce += forceIncreaseSpeed * Time.deltaTime;
        currentThrowForce = Mathf.Clamp(currentThrowForce, throwForce, maxThrowForce);
    }

    void MoveObjectToHoldPosition()
    {
        // Smoothly move the object to the hold position
        pickedObject.transform.position = Vector3.Lerp(pickedObject.transform.position, holdPosition.position, Time.deltaTime * moveSpeed);
        pickedObject.transform.rotation = Quaternion.Lerp(pickedObject.transform.rotation, holdPosition.rotation, Time.deltaTime * moveSpeed);
    }

    void UpdateForcePercentageUI()
    {
        // Calculate the percentage of force based on currentThrowForce
        float forcePercentage = (currentThrowForce - throwForce) / (maxThrowForce - throwForce) * 100f;

        // Update the TextMeshPro UI to show the percentage
        if (forcePercentageText != null)
        {
            forcePercentageText.text = "Force: " + Mathf.RoundToInt(forcePercentage) + "%";
        }
    }
}
