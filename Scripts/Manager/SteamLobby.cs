using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    // 콜백들
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<LobbyMatchList_t> lobbyListCallback;

    public ulong CurrentLobbyID;
    private const string HostAddressKey = "CustomHostAddress";
    private CustomNetworkManager manager;

    // 방 정보들
    private string roomName;
    public string RoomName
    {
        get { return roomName; }
    }
    private string password;
    private bool isPrivate;

    private void Start()
    {
        if (!SteamManager.Initialized)
            return;

        if (Instance == null)
            Instance = this;

        manager = NetworkManager.singleton as CustomNetworkManager;

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyListReceived);
    }

    //로비 호스트할 때 실행
    public void HostLobby(string roomName, string password, bool isPrivate)
    {
        this.roomName = roomName;
        this.password = password;
        this.isPrivate = isPrivate;

        ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic; // 모두 목록에 표시
        SteamMatchmaking.CreateLobby(lobbyType, manager.maxConnections);
    }

    //로비가 생성되었을 때 콜백
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        Debug.Log("로비 생성 성공");

        manager.StartHost();

        NetworkManager.singleton.ServerChangeScene("InGameScene"); // 씬전환

        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, "roomName", roomName);                // 방 이름
        SteamMatchmaking.SetLobbyData(lobbyID, "isPrivate", isPrivate ? "1" : "0");  // 비밀방 공개방인지
        SteamMatchmaking.SetLobbyData(lobbyID, "password", password);                // 패스워드
        SteamMatchmaking.SetLobbyData(lobbyID, "PlayerCount", "1");                  // 플레이어수

    }

    //로비 참여 시 콜백
    public void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("로비 참여 요청");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    //로비 입장 시 콜백
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        Debug.Log("로비 입장");
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (NetworkServer.active)
            return;

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        manager.StartClient();
    }

    // 방 정보 요청 콜백
    void OnLobbyListReceived(LobbyMatchList_t result)
    {
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);

            string name = SteamMatchmaking.GetLobbyData(lobbyID, "roomName");
            string isPrivate = SteamMatchmaking.GetLobbyData(lobbyID, "isPrivate");
            string playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID).ToString();
            string passWord = SteamMatchmaking.GetLobbyData(lobbyID, "password");


            // 방목록에 방 추가
            LobbyUIManager.Instance.CreateRoomListItem(lobbyID, name, isPrivate, playerCount, passWord);
        }
    }

    public void LeaveLobby() // 로비 나가기
    {
        Debug.Log("로비 나가기");

        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;

    }

    public void InviteLobby()  // 로비로 초대
    {
        CSteamID  currentLobbyID = new CSteamID(CurrentLobbyID);
        if (currentLobbyID.IsValid())
        {
            Debug.Log("초대하기");
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
        }
    }






}