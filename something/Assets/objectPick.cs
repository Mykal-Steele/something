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
    public bool isInPickMode = true;
    [Header("Shooting Settings")]
    public Transform gunTip;  // Assign the gun tip transform in inspector
    public GameObject bulletPrefab;  // Assign your bullet prefab in inspector
    public float bulletSpeed = 50f;
    public float fireRate = 0.1f;
    public float darkenAmount = 0.2f;
    public int maxAmmo = 40;
    public float reloadTime = 3f;

    [Header("Recoil Settings")]
    public float recoilVerticalStrength = 2f; // Increase vertical strength
    public float recoilHorizontalStrength = 1f; // Increase horizontal strength
    public float maxRecoil = 25f; // Increase max recoil
    public float recoilRecoverySpeed = 10f; // Increase recovery speed


    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;
    private Vector3 currentRecoil;
    private bool isShooting;
    
    void Start()
    {
    
        currentAmmo = maxAmmo;
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
        // Switch between Pick Mode and Gun Mode
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isInPickMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (forcePercentageText != null)
        {
            forcePercentageText.text = $"Ammo: {currentAmmo}";
        }
            isInPickMode = false;
        }

        if (isInPickMode)
        {
            // Pick Mode: Left-click to pick up or drop, right-click to throw
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

            if (Input.GetMouseButtonDown(1) && pickedObject != null)
            {
                isHoldingRightMouse = true;
            }

            if (Input.GetMouseButtonUp(1) && pickedObject != null)
            {
                ThrowObject();
                isHoldingRightMouse = false; // Reset holding state after throw
            }

            if (isHoldingRightMouse)
            {
                IncreaseThrowForce();
            }

            if (pickedObject != null)
            {
                MoveObjectToHoldPosition();
            }

            UpdateForcePercentageUI();
        }
        else
        {         
            // Handle shooting
            if (Input.GetMouseButton(0) && !isReloading && currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                isShooting = false;
            }

            // Handle reloading
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            {
                StartCoroutine(ReloadRoutine());
            }

            // Apply recoil
            
            ApplyRecoil();
        }
    }
       void Shoot()
{
    if (isReloading || Time.time < nextFireTime || currentAmmo <= 0)
        return;

    // Update firing time and ammo
    nextFireTime = Time.time + fireRate;
    currentAmmo--;
    isShooting = true;

    // Log shoot action
    Debug.Log("Shooting... Current Ammo: " + currentAmmo);

    // Create and shoot bullet
    GameObject bullet = Instantiate(bulletPrefab, gunTip.position, gunTip.rotation);
    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

    // Add this script to the bullet
    bullet.AddComponent<BulletBehavior>().Initialize(darkenAmount);

    // Play sound at bullet's position
    FindObjectOfType<SoundManager>().PlayBulletSound(bullet.transform.position); // Call the sound manager here

    // Shoot the bullet
    bulletRb.velocity = playerCamera.transform.forward * bulletSpeed;

    // Apply recoil
    AddRecoil(); // Ensure this is being called

    // Destroy bullet after 5 seconds if it doesn't hit anything
    Destroy(bullet, 5f);

    // Update UI if you want to show ammo count
    if (forcePercentageText != null)
    {
        forcePercentageText.text = $"Ammo: {currentAmmo}";
    }
}




    // Add these new methods to your class
    void AddRecoil()
    {
        // Check if these values are set correctly
        Debug.Log($"Recoil Horizontal Strength: {recoilHorizontalStrength}, Recoil Vertical Strength: {recoilVerticalStrength}");

        // Add randomized recoil
        float horizontalRecoil = Random.Range(-recoilHorizontalStrength, recoilHorizontalStrength);
        currentRecoil += new Vector3(
            horizontalRecoil,
            recoilVerticalStrength,
            0
        );

        // Clamp recoil
        currentRecoil = Vector3.ClampMagnitude(currentRecoil, maxRecoil);

        // Log the current recoil
        Debug.Log("Current Recoil: " + currentRecoil);
    }


    void ApplyRecoil()
{
    // Log the current camera rotation before applying recoil
    Debug.Log("Current Camera Rotation Before: " + playerCamera.transform.localRotation.eulerAngles);

    // Apply recoil to camera rotation
    if (playerCamera != null)
    {
        playerCamera.transform.localRotation = Quaternion.Euler(
            playerCamera.transform.localRotation.eulerAngles + currentRecoil
        );

        // Log the applied recoil
        Debug.Log("Applying recoil: " + currentRecoil);

        // Log the current camera rotation after applying recoil
        Debug.Log("Current Camera Rotation After: " + playerCamera.transform.localRotation.eulerAngles);
    }

    // Recover from recoil when not shooting
    if (!isShooting)
    {
        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);
    }
}




    IEnumerator ReloadRoutine()
    {
        FindObjectOfType<SoundManager>().PlayReloadSound(transform.position);
        isReloading = true;
        if (forcePercentageText != null)
        {
            forcePercentageText.text = "Reloading...";
        }
        
        yield return new WaitForSeconds(reloadTime);
        
        currentAmmo = maxAmmo;
        isReloading = false;
        
        if (forcePercentageText != null)
        {
            forcePercentageText.text = $"Ammo: {currentAmmo}";
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
