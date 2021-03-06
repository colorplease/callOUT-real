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
    [SerializeField] KeyCode interactKey = KeyCode.E;

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
    public bool flashLightInvertCheck;

    [Header("Identity")]
    public PlayerSwitchIdentity playerSwitchIdentity;
    
    public void SetPosition()
    {
        if(SceneManager.GetActiveScene().name == "TESTING")
        {
            if (playerSwitchIdentity.colorNumGlobal == 1)
        {
            transform.position = GameObject.FindGameObjectWithTag("Player 1 Spawn").transform.position;
        }
        if (playerSwitchIdentity.colorNumGlobal == 2)
        {
            transform.position = GameObject.FindGameObjectWithTag("Player 2 Spawn").transform.position;
        }
        }
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
            if(isServer)
                {
                RpcFlashLight();
                }
                else
                {
                CmdFlashLight();
                }
        }

        if (Input.GetKeyDown(interactKey))
        {
            Interactions();
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
            }
            break;

            case 1:
            if (flashLight.activeSelf == true)
            {
            flashLight.SetActive(false);
            }
            break;
        }
    }

    
    [Command]
    void CmdFlashLight()
    {
        //Apply it to all other clients
        if (flashLightState == 1)
        {
            flashLightState = 0;
            flashLightInvertCheck = true;
        }
        else
        {
            flashLightState = 1;
            flashLightInvertCheck = true;
        }
        FlashLight();
        RpcFlashLight();
    }

    [ClientRpc]
    void RpcFlashLight()
    {
        if (flashLightInvertCheck == false)
        {
            if (flashLightState == 1)
            {
            flashLightState = 0;
            
            }
            else
            {
            flashLightState = 1;
            }
        }
        FlashLight();
        flashLightInvertCheck = false;
    }

    void Interactions()
    {
        //some raycast jazz to check what you interacted with
            RaycastHit hit;
            if (Physics.Raycast(cameraClient.transform.position, cameraClient.transform.forward, out hit, 50f))
            {
                if (hit.transform.tag == "door")
                {
                    if (hit.transform.gameObject.GetComponent<Door>().IsOpen)
                    {
                        hit.transform.gameObject.GetComponent<Door>().Close();
                    }
                    else
                    {
                        hit.transform.gameObject.GetComponent<Door>().Open(transform.localPosition);
                    }
                    
                }
            }
    }
}
