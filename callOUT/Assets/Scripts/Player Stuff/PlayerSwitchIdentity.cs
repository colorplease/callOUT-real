using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerSwitchIdentity : NetworkBehaviour
{
    [SerializeField] Material material1;
    [SerializeField] Material material2;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] GameObject glasses1;
    [SerializeField] GameObject glasses2;
    [SerializeField] GameObject mainCam;
    [SerializeField] LayerMask glass1Mask;
    [SerializeField] LayerMask glass2Mask;
    int colorNumGlobal;

    public void Red()
    {
        colorNumGlobal = 1;
        SceneManager.activeSceneChanged += ChangedActiveScenes;
        if(isServer)
                {
                RpcColor(1);
                }
                else
                {
                CmdColor(1);
                }
    }

    public void Blue()
    {
        colorNumGlobal = 2;
        SceneManager.activeSceneChanged += ChangedActiveScenes;
        if(isServer)
                {
                RpcColor(2);
                }
                else
                {
                CmdColor(2);
                }
    }

    void ChangedActiveScenes(Scene current, Scene next)
    {
        if(SceneManager.GetActiveScene().name == "TESTING")
        {
            switch (colorNumGlobal)
            {
                case 1: 
                GameObject.FindGameObjectWithTag("Main Camera").GetComponent<Camera>().cullingMask = glass1Mask;
                break;

                case 2:
                GameObject.FindGameObjectWithTag("Main Camera").GetComponent<Camera>().cullingMask = glass2Mask;
                break;
            }
        }
    }

    [Command]
    void CmdColor(int colorNum)
    {
        Color(colorNum);
        RpcColor(colorNum);
    }

    [ClientRpc]
    void RpcColor(int colorNum)
    {
        Color(colorNum);
    }

    void Color(int colorNum)
    {
        switch (colorNum)
        {
            case 1:
            meshRenderer.material = material1;
            glasses1.SetActive(true);
            glasses1.SetActive(false);
            break;

            case 2:
            meshRenderer.material = material2;
            glasses1.SetActive(false);
            glasses1.SetActive(true);
            break;
        }
    }

    
}
