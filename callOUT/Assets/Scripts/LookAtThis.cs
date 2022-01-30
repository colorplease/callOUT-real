using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtThis : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;

    [SerializeField] Transform cam = null;
    [SerializeField] Transform orientation = null;

    [SerializeField]InputManager inputManager;

    float mouseX;
    float mouseY;

    float multiplier = 0.001f;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputManager = InputManager.Instance; 
    }

    private void Update()
    {
        Vector2 lookDir = inputManager.GetPlayerLook();
        mouseX = lookDir.x;
        mouseY = lookDir.y;
         
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, cam.transform.rotation.z);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
