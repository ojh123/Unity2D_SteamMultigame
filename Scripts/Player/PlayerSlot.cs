using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : NetworkBehaviour
{
    [SyncVar]
    public int connectionID;            // 플레이어 connectionID
    [SyncVar]
    public int playerIdNumber;          // 플레이어 숫자 몇번째인지
    [SyncVar]
    public ulong playerSteamID;        // 플레이어 스팀 아이디
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string playerName;          // 플레이어 이름
    [SyncVar]
    public Transform playerTransform; // 플레이어 트랜스폼
    [SyncVar]
    public bool isSpawned = false;

    // 준비 상태
    [SyncVar(hook = nameof(OnReadyStateChanged))]
    public bool isReady = false;

    [SerializeField]
    private Text readyText;
    [SerializeField]
    private Transform playerSlotParentPos;  // 플레이어 슬롯 생성 위치

    [Header("UI 컴포넌트")]
    [SerializeField] private Text nameText;
    [SerializeField] private Image profileImage;
    [SerializeField] private GameObject readyIcon;

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

    private void Awake()
    {
        // 슬롯 부모에 붙이기
        playerSlotParentPos = GameObject.Find("PlayerInfoPanel").transform;
        playerTransform.SetParent(playerSlotParentPos, false);
    }

    public override void OnStartAuthority()  // 자신의 플레이어오브젝트가 생성
    {
        Debug.Log("OnStartAuthority");
        gameObject.name = "LocalGamePlayer";
    }

    public override void OnStartClient() // 서버 연결 완료했을때
    {
        Debug.Log("OnStartClient");
        Manager.GamePlayers.Add(this);  // 네트워크 매니저 플레이어 리스트에 자기자신 추가

        // 초기 UI 세팅
        nameText.text = playerName;
        UpdateProfileImage();
        readyIcon.SetActive(isReady);
        CmdSetPlayerInfo(SteamFriends.GetPersonaName().ToString(), playerSteamID);
    }


    public override void OnStopClient() // 서버 연결 끊겼을때
    {
        Debug.Log("OnStopClient");
        Manager.GamePlayers.Remove(this);
    }

    // 프로필 이미지를 Steam에서 가져와 세팅
    void UpdateProfileImage()
    {
        if (playerSteamID == 0)
            return;

        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamID);
        if (imageId == -1) return;

        if (SteamUtils.GetImageSize(imageId, out uint w, out uint h))
        {
            byte[] data = new byte[4 * (int)w * (int)h];
            if (SteamUtils.GetImageRGBA(imageId, data, data.Length))
            {
                Texture2D tex = new Texture2D((int)w, (int)h, TextureFormat.RGBA32, false);
                tex.LoadRawTextureData(data);
                tex.Apply();
                profileImage.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.one * 0.5f);
            }
        }
    }


    [Command]
    void CmdSetPlayerInfo(string name, ulong steamID) // 플레이어 이름 스팀 아이디 세팅
    {
        Debug.Log("CmdSetPlayerInfo");
        playerName = name;
        playerSteamID = steamID;
    }

    void OnPlayerNameChanged(string oldName, string newName) // 이름 바뀌면 실행되는 함수
    {
        Debug.Log("OnPlayerNameChanged");
        nameText.text = newName;
    }


    // 클라이언트가 로컬 슬롯에서 호출
    public void ToggleReady()
    {
        if (!isLocalPlayer) return;
        CmdSetReady(!isReady);
    }

    [Command]
    void CmdSetReady(bool ready)
    {
        isReady = ready;
    }

    void OnReadyStateChanged(bool oldValue, bool newValue) // isReady 값이 바뀌면 실행
    {
        readyText.gameObject.SetActive(newValue);
    }

    [Command] // 선택된 셀 위치에 플레이어 스폰
    public void CmdSpawnAtCell(int cellIndex, Vector3 pos)
    {
        if (isSpawned) return; // 중복 스폰 방지
        isSpawned = true;
        GameManager.Instance.PlayerSpawn(pos, connectionID, this.connectionToClient, cellIndex, playerName); // 서버에서 실행하는 함수
    }


}
