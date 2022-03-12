using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerControllerRedux : NetworkBehaviour
{
    [Header("Movement")]
    public float speed = 0.1f;
    [Header("Network")]
    public GameObject PlayerModel;
    // Start is called before the first frame update
    void Start()
    {
        PlayerModel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "TESTING")
        {
            if (PlayerModel.activeSelf == false)
            {
                PlayerModel.SetActive(true);
            }
        }
        if (hasAuthority)
        {
            Movement();
        }
    }

    public void SetPosition()
    {
        Transform spawnPoint = GameObject.Find("Player 1 Spawn").transform;
        transform.position = spawnPoint.position;
    }

    void Movement()
    {
       float xDir = Input.GetAxis("Horizontal");
       float zDIR = Input.GetAxis("Vertical");

       Vector3 moveDirection = new Vector3(xDir, 0.0f, zDIR);

       transform.position += moveDirection * speed; 
    }
}
