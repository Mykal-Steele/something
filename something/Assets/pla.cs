using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerScript : MonoBehaviour {

    public float speed = 9.8f;
    public float gravity = 15f;
    public float jumpHeight = 2.0f;
    public float sensitivity = 1.0f;
    private float translation;
    private float straffe;
    private Vector3 movementDirection = Vector3.zero;
    private CharacterController controller;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start () {
        // Get the CharacterController component
        controller = GetComponent<CharacterController>();

        // Turn off the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update () {
        // Check if the character is grounded
        isGrounded = controller.isGrounded;

        if (isGrounded) {
            // Get input for movement
            translation = Input.GetAxisRaw("Vertical") * speed;
            straffe = Input.GetAxisRaw("Horizontal") * speed;

            // Set movement direction based on input
            movementDirection = new Vector3(straffe, 0, translation);
            movementDirection = transform.TransformDirection(movementDirection);

            // Jumping
            if (Input.GetButtonDown("Jump")) {
                movementDirection.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
            }
        }

        // Apply gravity
        movementDirection.y -= gravity * Time.deltaTime;

        // Move the player
        controller.Move(movementDirection * Time.deltaTime);

        // Unlock the cursor on escape
        if (Input.GetKeyDown("escape")) {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}