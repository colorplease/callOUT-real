using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSwitchIdentity : NetworkBehaviour
{
    public Material material1;
    public Material material2;
    public MeshRenderer meshRenderer;

    public void Red()
    {
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
        if(isServer)
                {
                RpcColor(2);
                }
                else
                {
                CmdColor(2);
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
            break;

            case 2:
            meshRenderer.material = material2;
            break;
        }
    }

    
}
