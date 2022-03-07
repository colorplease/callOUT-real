using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControllerRedux : NetworkBehaviour
{
    [Header("Network")]
    public GameObject PlayerModel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPosition()
    {
        Transform spawnPoint = GameObject.Find("Player 1 Spawn").transform;
        transform.position = spawnPoint.position;
    }
}
