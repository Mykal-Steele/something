using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPicker : MonoBehaviour
{
    public float pickupRange = 5f;     // Maximum range to pick up objects
    public Transform holdPosition;     // Position where the picked object will be held
    public float moveSpeed = 10f;      // Speed at which the object moves to the hold position
    public LayerMask playerLayer;      // Layer mask for the player

    private GameObject pickedObject;   // Currently picked object
    private Camera playerCamera;
    private Collider objectCollider;   // Store reference to the object's collider
    private Rigidbody objectRigidbody; // Store reference to the object's Rigidbody
    private Collider playerCollider;   // Player's collider reference

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

        // If an object is picked up, move it smoothly to the hold position
        if (pickedObject != null)
        {
            MoveObjectToHoldPosition();
        }
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
    }

    void MoveObjectToHoldPosition()
    {
        // Smoothly move the object to the hold position
        pickedObject.transform.position = Vector3.Lerp(pickedObject.transform.position, holdPosition.position, Time.deltaTime * moveSpeed);
        pickedObject.transform.rotation = Quaternion.Lerp(pickedObject.transform.rotation, holdPosition.rotation, Time.deltaTime * moveSpeed);
    }
}
