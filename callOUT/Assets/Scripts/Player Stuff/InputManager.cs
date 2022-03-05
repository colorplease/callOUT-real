using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InputManager : NetworkBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }
    PlayerMovement playerMovement;

    void Awake()
    {
        playerMovement = new PlayerMovement();
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void OnEnable()
    {
        playerMovement.Enable();
    }

    void OnDisable()
    {
        playerMovement.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerMovement.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetPlayerLook()
    {
        return playerMovement.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerUsedThisFrame()
    {
        return playerMovement.Player.Use.triggered;
    }
    
    public bool PlayerInteractedThisFrame()
    {
        return playerMovement.Player.Interact.triggered;
    }

    public bool PlayerUsedFlashlightThisFrame()
    {
        return playerMovement.Player.Flashlight.triggered;
    }

    public bool PlayerJumpedThisFrame()
    {
        return playerMovement.Player.Jump.triggered;
    }

    public bool PlayerCrouching()
    {
        return playerMovement.Player.Crouch.ReadValue<float>() > 0.1f;
    }

    public bool PlayerSprinting()
    {
        return playerMovement.Player.Sprint.ReadValue<float>() > 0.1f;
    }
    



}
