using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerControllerRedux : NetworkBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float airMultiplier = 0.4f;
    [SerializeField] float jumpForce = 5f;
    float movementMultiplier = 10f;
    [SerializeField]float gravity;
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrouching;
    


    [Header("Sprinting")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Keybinds")]
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.C;
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode flashLightKey = KeyCode.F;

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

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

    Rigidbody rb;

    RaycastHit slopeHit;
    [Header("Network")]
    public GameObject PlayerModel;
    public GameObject cameraClient;

    [Header("Items")]
    public GameObject flashLight;
    public int flashLightState;
    
    public void SetPosition()
    {
        //Transform spawnPoint = GameObject.Find("Player 1 Spawn").transform;
        transform.position = new Vector3(Random.Range(0.1f, -5f), 0.9f, Random.Range(-3f, 5f));
    }

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
        PlayerModel.SetActive(false);

        if (!isLocalPlayer)
        {
            Destroy(cameraClient);   
        }
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "TESTING")
        {
            if (PlayerModel.activeSelf == false)
            {
                SetPosition();
                PlayerModel.SetActive(true);
            }
        }
        if (hasAuthority)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            MyInput();
            ControlDrag();
            ControlSpeed();
            Crouch();
            Jump();
            slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);

            if (transform.position.y <= -1)
            {
                SetPosition();
            }
        }
        FlashLight();
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(sprintKey) && !isCrouching)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if(Input.GetKey(crouchKey))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        if(Input.GetKeyDown(flashLightKey))
        {
            if (flashLightState == 0)
            {
                if(isServer)
                {
                RpcFlashLightOff();
                }
                else
                {
                CmdFlashLightOff();
                }
            }
            else
            {
                if(isServer)
                {
                RpcFlashLightOn();
                }
                else
                {
                CmdFlashLightOn();
                }
            }
        }
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void ControlSpeed()
    {
        if (isSprinting && isGrounded)
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
        if (hasAuthority)
        {
            MovePlayer();
        }
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

    void Jump()
    {
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Crouch()
    {
        if (isCrouching)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, crouchScale, crouchSpeed * Time.deltaTime);
            movementMultiplier = 6f;

        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, startingScale, crouchSpeed * Time.deltaTime);
            movementMultiplier = 10f;
        }
    }

    void FlashLight()
    {
        switch(flashLightState)
        {
            case 0:
            if (flashLight.activeSelf == false)
            {
            flashLight.SetActive(true);
            Debug.Log("On");
            }
            break;

            case 1:
            if (flashLight.activeSelf == true)
            {
            flashLight.SetActive(false);
            Debug.Log("Off");
            }
            break;
        }
    }

    [Command]
    void CmdFlashLightOn()
    {
        //Apply it to all other clients
        flashLightState = 0;
        FlashLight();
        RpcFlashLightOn();
    }

    [ClientRpc]
    void RpcFlashLightOn()
    {
            flashLightState = 0;
            FlashLight();
    }

    [Command]
    void CmdFlashLightOff()
    {
        //Apply it to all other clients
        flashLightState = 1;
        FlashLight();
        RpcFlashLightOff();
    }

    [ClientRpc]
    void RpcFlashLightOff()
    {
            flashLightState = 1;
            FlashLight();
    }
}
