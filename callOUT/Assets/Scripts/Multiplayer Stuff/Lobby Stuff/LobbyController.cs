using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LobbyController : NetworkBehaviour
{
    public static LobbyController Instance;

    //UI Elements

    //Player Data
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    [SerializeField]int localPlayerColorNum;
    [SerializeField]int otherPlayerColorNum;
    GameObject[] playerCheck;
    PlayerSwitchIdentity[] playerCheckComponents;
    [SerializeField]bool debugSingle;
    public bool JoinColor;

    //Other Data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalplayerController;

    //Manager
    private CustomNetworkManager manager;

     private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;

        }
    }

    //Ready
    public Button StartGameButton;
    public Button BlueButton;
    public Button RedButton;
    public Button ReadyButton;
    public TextMeshProUGUI ReadyButtonText;

    void Awake()
    {
        if (Instance == null) {Instance = this;}
    }

    void Update()
    {
        CheckIfAllReady();
        JoinColorCheck();
    }

    public void JoinColorCheck()
    {
        playerCheck = GameObject.FindGameObjectsWithTag("Player");
        playerCheckComponents = new PlayerSwitchIdentity[playerCheck.Length];
        for (int i = 0; i < playerCheck.Length; ++i)
        {
            playerCheckComponents[i] = playerCheck[i].GetComponent<PlayerSwitchIdentity>();
        }
        localPlayerColorNum = playerCheckComponents[0].colorNumGlobal;
        otherPlayerColorNum = playerCheckComponents[playerCheck.Length - 1].colorNumGlobal;
        if (localPlayerColorNum != otherPlayerColorNum)
                        {
                            if (localPlayerColorNum != 0)
                            {
                                if (otherPlayerColorNum != 0)
                                    {
                                         JoinColor = true;
                                    }
                                    else
                                    {
                                        JoinColor = false;
                                    }
                            }
                            else
                            {
                                JoinColor = false;
                            }
                        }
                        else
                        {
                            JoinColor = false;
                        }
    }

    public void ReadyPlayer()
    {
        LocalplayerController.ChangeReady();
    }

    public void RedButtonVoid()
    {
        LocalPlayerObject.GetComponent<PlayerSwitchIdentity>().Red();
    } 

    public void BlueButtonVoid()
    {
        LocalPlayerObject.GetComponent<PlayerSwitchIdentity>().Blue();
    }

    public void Clear()
    {
        LocalPlayerObject.GetComponent<PlayerSwitchIdentity>().ClearWhite();
        
    }
    
    public void UpdateButton()
    {
        if(LocalplayerController.Ready)
        {
            ReadyButtonText.SetText("Unready");
        }
        else
        {
            ReadyButtonText.SetText("Ready");
        }
    }

    public void CheckIfAllReady()
    {
        bool AllReady = false;

        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Ready)
            {
                AllReady = true;
            }
            else
            {
                AllReady = false;
                break;
            }
        }

        if (JoinColor || debugSingle)
        {
            ReadyButton.interactable = true;
            if (AllReady)
            {
                if(LocalplayerController.PlayerIdNumber == 1)
                {
                StartGameButton.interactable = true;
                }
                else
                {
                StartGameButton.interactable = false;
                }
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }

    
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalplayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void UpdatePlayerList()
    {
        if(!PlayerItemCreated) {CreateHostPlayerItem();} //Host
        if (PlayerListItems.Count < Manager.GamePlayers.Count) {CreateClientPlayerItem();}
        if (PlayerListItems.Count > Manager.GamePlayers.Count) {RemovePlayerItem();}
        if (PlayerListItems.Count == Manager.GamePlayers.Count) {UpdatePlayerItem();}
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);

            PlayerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if(!PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);

            PlayerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            foreach(PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if(PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.Ready = player.Ready;
                    PlayerListItemScript.SetPlayerValues();
                    if(player == LocalplayerController)
                    {
                        UpdateButton();
        
                    }
                }
            }
        }
        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach(PlayerListItem playerlistItem in PlayerListItems)
        {
            if(!Manager.GamePlayers.Any(b => b.ConnectionID == playerlistItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerlistItem);
            }
        }

        if (playerListItemToRemove.Count > 0)
        {
            foreach(PlayerListItem playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                PlayerListItems.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }

    public void StartGame(string SceneName)
    {
        LocalplayerController.CanStartGame(SceneName);
    }
}
