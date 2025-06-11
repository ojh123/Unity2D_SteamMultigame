using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;


public class CustomNetworkManager : NetworkManager
{
    [Header("플레이어 슬롯")]
    [SerializeField]
    private PlayerSlot GamePlayerSlotPrefab; // 플레이어 슬롯 프리팹
    public Transform playerSlotParentPos;  // 플레이어 슬롯 생성 위치

  
    
    public List<PlayerSlot> GamePlayers  = new List<PlayerSlot>();

    private new void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    // 클라이언트에서 새 플레이어가 추가될 때 서버에서 호출
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "InGameScene")
        {
            Debug.Log("서버에 사람 추가");
            PlayerSlot GamePlayerInstance = Instantiate(GamePlayerSlotPrefab);
            GamePlayerInstance.connectionID = conn.connectionId;
            GamePlayerInstance.playerIdNumber = GamePlayers.Count + 1;
            GamePlayerInstance.playerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
            
        }
    }

    //클라이언트가 연결 끊겼을 때, 내가 서버랑 연결이 끊김
    public override void OnClientDisconnect() 
    {
        base.OnClientDisconnect();

        // 스팀 로비 나가기
        SteamMatchmaking.LeaveLobby(new CSteamID(SteamLobby.Instance.CurrentLobbyID));
        SteamLobby.Instance.CurrentLobbyID = 0;

        
    }

    // 클라이언트가 연결이 해제될 때 서버에서 호출된다, 어떤 클라이언트가 연결 끊음
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerDisconnect");

        PlayerSlot playerToRemove = GamePlayers.Find(p => p.connectionID == conn.connectionId);
        if (playerToRemove != null)
        {
            GamePlayers.Remove(playerToRemove);
            NetworkServer.Destroy(playerToRemove.gameObject); // 서버에서 네트워크 오브젝트 삭제
        }

        base.OnServerDisconnect(conn);
    }


    public override void OnStopServer()
    {
        GamePlayers.Clear();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        // 호스트가 중지된 후, 호스트 자신도 로비로 돌아가게
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        // 일반 클라이언트가 중지된 후, 클라이언트만 로비로 돌아가게
        if (!NetworkServer.active)
            SceneManager.LoadScene("LobbyScene");
    }
}