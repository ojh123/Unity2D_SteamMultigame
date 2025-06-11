using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

 
    [Header("방 정보들")]
    [SerializeField]
    private InputField roomNameInputField; // 방제목 인풋필드
    [SerializeField]
    private InputField passwordInputField; // 비밀번호 인풋필드
    [SerializeField]
    private Toggle privateToggle; // 방 비공개 여부
    [SerializeField]
    private GameObject roomListItemPrefab; // 방 리스트 프리팹
    [SerializeField]
    private Transform roomListContent; // 방 리스트 프리팹 부모 트랜스폼



    [Header("방 UI 오브젝트")]
    [SerializeField]
    private GameObject menuPanel;      // 메뉴 UI 오브젝트
    [SerializeField]
    private GameObject createRoomPanel; // 방만들기 UI 오브젝트
    [SerializeField]
    private GameObject gameRoomPanel; // 게임방 UI 오브젝트
    [SerializeField]
    private GameObject RoomListPanel; // 방리스트 오브젝트
    [SerializeField] 
    private GameObject passwordPanel;  // 패스워드 입력 UI 오브젝트
    public InputField pwConfrimInputField;  // 패스워드 입력 인풋필드


   

    private System.Action<string> pwSubmitCallback; // RoomListItem의 함수를 저장하기 위한 변수

    private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>(); // 방 UI 제어를 위한 딕셔너리

    private void Awake()
    {
        Instance = this;
        // 패널 등록
        panelDict.Add("Menu", menuPanel);
        panelDict.Add("CreateRoom", createRoomPanel);
        panelDict.Add("GameRoom", gameRoomPanel);
        panelDict.Add("RoomList", RoomListPanel);
        panelDict.Add("PassWord", passwordPanel);
    }

    #region "온클릭함수"
    public void OnClick_OpenCreateRoomPanel() // 방만들기 버튼
    {
        SetActivePanel("CreateRoom", true);
    }

    public void OnClick_CloseCreateRoomPanel() // 방만들기 취소 버튼
    {
        SetActivePanel("CreateRoom", false);
    }

    public void OnClick_HostRoom() // 방만들기 확인 버튼
    {
        string roomName = roomNameInputField.text;
        string password = passwordInputField.text;
        bool isPrivate = privateToggle.isOn;

        ResetCreateRoomFields(); // 방 정보 인풋필드 초기화 함수

        SteamLobby.Instance.HostLobby(roomName, password, isPrivate);
    }

    

    public void OnClick_RoomList()  // 게임참가 버튼, 방리스트 열기
    {
        SetActivePanel("Menu", false);
        SetActivePanel("RoomList", true);
        ClearRoomList();
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(50); // 최대 50개 방 받아오기
        SteamMatchmaking.RequestLobbyList(); // 방리스트 요청
    }

    public void OnClick_RoomListQuit() // 방리스트 닫기
    {
        SetActivePanel("Menu", true);
        SetActivePanel("RoomList", false);
    }

    public void OnValueChange_PrivateToggle()  // 비밀방 여부
    {
        passwordInputField.interactable = privateToggle.isOn;
    }

    public void OnClick_PWInputCancle() // 비밀번호 입력창 닫기
    {
        pwConfrimInputField.text = string.Empty;
        SetActivePanel("PassWord", false);
    }

    public void OnClick_GameQuit()  // 게임종료 버튼
    {
        #if UNITY_EDITOR
             UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중일 때는 플레이 모드를 종료
        #else
            Application.Quit(); // 빌드된 게임에서는 게임을 종료
        #endif
    }

   
    #endregion

    public void SetActivePanel(string panelName, bool isActive)  // 방 UI 오브젝트 제어 함수 
    {
        if (panelDict.ContainsKey(panelName))
        {
            panelDict[panelName].SetActive(isActive);
        }
        else
        {
            Debug.LogWarning(" 패널을 찾을 수 없습니다.");
        }
    }


    public void ResetCreateRoomFields()  // 방 정보 인풋필드 초기화 함수 
    {
        roomNameInputField.text = string.Empty;       // 방 제목 초기화
        passwordInputField.text = string.Empty;       // 비밀번호 초기화
        privateToggle.isOn = false;                   // 비공개 여부 해제
        passwordInputField.interactable = false;      // 비밀번호 입력 못하게
    }




    public void CreateRoomListItem(CSteamID lobbyID, string name, string isPrivate, string playerCount, string passWord)  // 룸리스트 프리팹 생성
    {
        GameObject item = Instantiate(roomListItemPrefab, roomListContent);
        RoomListItem listItem = item.GetComponent<RoomListItem>();
        listItem.SetInfo(lobbyID, name, isPrivate == "1", playerCount, passWord);
    }

    public void ClearRoomList()  // 룸리스트 프리팹 삭제
    {
        Debug.Log("방 리스트 삭제");
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowPasswordPopup(System.Action<string> onSubmit) // 비밀번호 입력창 보여주기
    {
        SetActivePanel("PassWord", true);
        pwConfrimInputField.text = string.Empty;
        pwSubmitCallback = onSubmit;
    }

    

    public void OnPasswordSubmit()  // 비밀번호 확인
    {
        if (pwSubmitCallback != null)
        {
            pwSubmitCallback(pwConfrimInputField.text);
        }
    }

    public void PassWordError()
    {
        LobbyUIManager.Instance.pwConfrimInputField.text = string.Empty;
        Debug.Log("비밀번호가 틀렸습니다.");  
    }

    
    
}


