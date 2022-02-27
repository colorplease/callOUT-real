using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadbobController : MonoBehaviour
{
    [SerializeField]bool _enable = true;

    [SerializeField, Range(0, 0.2f)] float amplitude = 0.015f;
    [SerializeField, Range(0, 30)] float frequency = 10.0f;

    [SerializeField] Transform camera = null;
    [SerializeField] Transform cameraHolder = null;

    float toggleSpeed = 0.1f;
    Vector3 startPos;
    PlayerController controller;
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
        startPos = camera.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enable) return;
         ResetPosition();
        CheckMotion();
        camera.LookAt(FocusTarget());
        print(controller.rb.velocity);
    }

    Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
        return pos;
    }

    Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + cameraHolder.localPosition.y, transform.position.z);
        pos += cameraHolder.forward * 15.0f;
        return pos;
    }

    void CheckMotion()
    {
        float speed = new Vector3(controller.rb.velocity.x, 0, controller.rb.velocity.z).magnitude;
        if(speed < toggleSpeed) return;
        if(!controller.isGrounded) return;
        PlayMotion(FootStepMotion());
    }

    void ResetPosition()
    {
        if (camera.localPosition == startPos) return;
        camera.localPosition = Vector3.Slerp(camera.localPosition, startPos, 1 * Time.deltaTime);
    }

    void PlayMotion(Vector3 motion){
camera.localPosition += motion; 
}


}
