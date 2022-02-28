using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;
    public InputManager inputManager;
    [SerializeField]float gravity;

    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Jumping")]
    [SerializeField] float jumpForce; 

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;
    public bool isGrounded { get; private set; }

    [Header("Crouching")]
    [SerializeField]Vector3 startingScale;
    [SerializeField]Vector3 crouchScale;
    [SerializeField]float crouchSpeed;

    [Header("Items")]
    [SerializeField]int flashLightToggle = 0;
    [SerializeField]GameObject flashlight; 

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    public Rigidbody rb;

    RaycastHit slopeHit;

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputManager = InputManager.Instance; 
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        MyInput();
        ControlDrag();
        ControlSpeed();
        Crouch();
        Jump();
        Flashlight();
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    void MyInput()
    {
        Vector2 movement = inputManager.GetPlayerMovement();
        horizontalMovement = movement.x;
        verticalMovement = movement.y;

        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void ControlSpeed()
    {
        if (inputManager.PlayerSprinting() && isGrounded)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = airDrag;
            rb.AddForce(Vector3.down * gravity * rb.mass);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }

    void Crouch()
    {
        if (inputManager.PlayerCrouching())
        {
            transform.localScale = Vector3.Lerp(transform.localScale, crouchScale, crouchSpeed * Time.deltaTime);
            movementMultiplier = 6f;
            rb.AddForce(Vector3.down * gravity * rb.mass);

        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, startingScale, crouchSpeed * Time.deltaTime);
            movementMultiplier = 10f;
        }
    }

    void Jump()
    {
        if(inputManager.PlayerJumpedThisFrame() && isGrounded && !inputManager.PlayerCrouching())
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

    }

    void Flashlight()
    {
        if(inputManager.PlayerUsedFlashlightThisFrame())
        {
            switch(flashLightToggle)
            {
                case 0:
                flashlight.SetActive(false);
                flashLightToggle = 1;
                break;

                case 1:
                flashlight.SetActive(true);
                flashLightToggle = 0;
                break;
            }
        }
    }
}
