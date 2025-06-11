using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("게임방 제목 표시 텍스트")]
    public Text roomNameText; // 방 제목 텍스트

    [Header("시작버튼 텍스트")]
    public Text startText;

    [Header("안내 텍스트")]
    public GameObject spawnGuideText;  // 스폰 가이드 텍스트
    public GameObject attackGuideText;  // 공격 가이드 텍스트 
    public GameObject moveGuideText;   // 움직임 가이드 텍스트

    [Header("타이머 텍스트")]
    [SerializeField] Text playerText;
    [SerializeField] Text timerText;

    [Header("나가기 버튼")]
    [SerializeField] private Button quitButton;

    [Header("시작 버튼")]
    [SerializeField] private Button startButton;

    [Header("게임판")]
    [SerializeField]
    private GameObject gameBoard; // 게임판 오브젝트

    [Header("결과 패널")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;


    void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "InGameScene") return;

        SetRoomName(SteamLobby.Instance.RoomName);
        roomNameText.text = SteamLobby.Instance.RoomName;
        UpdateStartReadyUI();
    }

    #region "온클릭함수"

    public void OnClick_QuitRoom()  // 게임방 나가기 버튼
    {

        // 1. 서버와 클라이언트 연결 끊기
        if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }

        // 4. Steam 로비 나가기
        SteamLobby.Instance.LeaveLobby();
    }


    public void Onclick_Invite() // 초대하기
    {
        SteamLobby.Instance.InviteLobby();
    }

    // 준비 버튼
    public void OnClick_Ready()
    {
        // 연결도 되어 있고, 내 플레이어 오브젝트도 존재할 때
        if (NetworkClient.connection == null || NetworkClient.connection.identity == null)
            return;

        // 호스트면 준비버튼 안되게
        if (!NetworkServer.active)
        {
            var player = NetworkClient.localPlayer.GetComponent<PlayerSlot>();
            player.ToggleReady();
        }
    }

    // 호스트 전용: 시작 버튼을 눌렀을 때 호출
    public void OnClick_StartGame()
    {
        Debug.Log("OnClick_StartGame");
        // 호스트인지 확인
        if (!NetworkServer.active) return;

        var manager = CustomNetworkManager.singleton as CustomNetworkManager;
        var hostSteamID = SteamUser.GetSteamID().m_SteamID;

        // 호스트를 제외한 모든 플레이어가 준비됐는지 검사
        foreach (var p in manager.GamePlayers)
        {
            // 만약 플레이어가 호스트라면 다음 플레이어로
            if (p.playerSteamID == hostSteamID)
                continue;

            if (!p.isReady)
            {
                Debug.Log("아직 모두 준비되지 않았습니다!");
                return;
            }
        }

        GameManager.Instance.StartGame();
    }
    #endregion


    public void SetRoomName(string name) // 방 제목 세팅
    {
        roomNameText.text = name;
    }

    // 시작 버튼 업데이트 방장이면 시작 아니면 준비
    public void UpdateStartReadyUI()
    {
        bool isHost = NetworkServer.active;
        if (NetworkServer.active)
            startText.text = "시작";
        else
            startText.text = "준비";
    }

    public void DisableQuitButton()  // 나가기 버튼 비활성화
    {
        if (quitButton != null)
            quitButton.interactable = false;
    }

    public void EnableQuitButton() // 나가기 버튼 활성화
    {
        if (quitButton != null)
            quitButton.interactable = true;
    }

    public void DisableStartButton()  // 시작 버튼 비활성화
    {
        if (quitButton != null)
            startButton.interactable = false;
    }

    public void EnableStartButton() // 시작 버튼 활성화
    {
        if (quitButton != null)
            startButton.interactable = true;
    }

    public void PlayerSpawnGuide(bool v) // 스폰 안내 오브젝트 활성화 비활성화 
    {
        spawnGuideText.SetActive(v);
    }

    public void PlayerAttackGuide(bool v) // 공격 안내 오브젝트 활성화 비활성화 
    {
        attackGuideText.SetActive(v);
    }

    public void PlayerMoveGuide(bool v)  // 움직임 안내 오브젝트 활성화 비활성화 
    {
        moveGuideText.SetActive(v);
    }

    public void ShowTurn(string steamNickname) // 누구턴인지
    {
        playerText.text = $"{steamNickname} 님의 턴: ";
        timerText.text = $"30";
    }

    public void UpdateTimerDisplay(float time)  // 남은 시간
    {
        timerText.text = $"{time:0}";
    }

    public void ShowResultPanel(string resultText)  // 결과 판넬
    {
        resultPanel.SetActive(true);
        this.resultText.text = resultText;
    }


    public void ResetUI()
    {
        // 결과 패널 닫기
        resultPanel.SetActive(false);

        // 가이드 텍스트 모두 숨기기
        spawnGuideText.SetActive(false);
        attackGuideText.SetActive(false);
        moveGuideText.SetActive(false);

        // 버튼도 기본 상태로
        quitButton.interactable = true;
        startButton.interactable = true;

        // 타이머/턴 표시 초기화
        playerText.text = "";
        timerText.text = "";
    }

}


