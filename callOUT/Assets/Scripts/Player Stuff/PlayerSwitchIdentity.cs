using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSwitchIdentity : NetworkBehaviour
{
    //Color Pick
    [SerializeField] Material material1;
    [SerializeField] Material material2;
    [SerializeField] MeshRenderer meshRenderer;
    public int colorNumGlobal;
    //Glasses Character Visibility
    [SerializeField] GameObject glasses1;
    [SerializeField] GameObject glasses2;
    [SerializeField] GameObject mainCam;
    [SerializeField] LayerMask glass1Mask;
    [SerializeField] LayerMask glass2Mask;
    //UI Image Color Pick
    [SerializeField]Image localPlayerColor;
    [SerializeField]Image otherPlayerColor;
    [SerializeField] Color specialRed;
    [SerializeField] Color specialBlue;

    public void Red()
    {
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

    public void ClearWhite()
    {
        SceneManager.activeSceneChanged += ChangedActiveScenes;
        if(isServer)
                {
                RpcColor(0);
                }
                else
                {
                CmdColor(0);
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

    void ColorUIPickRed()
    {
        if (isLocalPlayer)
        {
            localPlayerColor.color = specialRed;
        }
        else
        {
            otherPlayerColor.color = specialRed;
        }
    }

    void ColorUIPickBlue()
    {
        if (isLocalPlayer)
        {
            localPlayerColor.color = specialBlue;
        }
        else
        {
            otherPlayerColor.color = specialBlue;
        }
    }

    void ColorUIPickWhite()
    {
        if (isLocalPlayer)
        {
            localPlayerColor.color = new Color32(255,255,225,100);
        }
        else
        {
            otherPlayerColor.color = new Color32(255,255,225,100);
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
            case 0:
            colorNumGlobal = 0;
            localPlayerColor = GameObject.FindGameObjectWithTag("LocalPlayerColorUI").GetComponent<Image>();
            otherPlayerColor = GameObject.FindGameObjectWithTag("OtherPlayerColorUI").GetComponent<Image>();
            ColorUIPickWhite();
            
            break;
            case 1:
            colorNumGlobal = 1;
            localPlayerColor = GameObject.FindGameObjectWithTag("LocalPlayerColorUI").GetComponent<Image>();
            otherPlayerColor = GameObject.FindGameObjectWithTag("OtherPlayerColorUI").GetComponent<Image>();
            meshRenderer.material = material1;
            glasses1.SetActive(true);
            glasses2.SetActive(false);
            ColorUIPickRed();
            break;

            case 2:
            colorNumGlobal = 2;
            localPlayerColor = GameObject.FindGameObjectWithTag("LocalPlayerColorUI").GetComponent<Image>();
            otherPlayerColor = GameObject.FindGameObjectWithTag("OtherPlayerColorUI").GetComponent<Image>();
            meshRenderer.material = material2;
            glasses1.SetActive(false);
            glasses2.SetActive(true);
            ColorUIPickBlue();
            break;
        }
    }

    
}
