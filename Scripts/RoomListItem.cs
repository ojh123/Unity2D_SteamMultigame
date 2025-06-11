using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    private CSteamID lobbyID; // 스팀 로비ID
    [SerializeField]
    private Text nameText;  // 방 제목 텍스트
    [SerializeField]
    private Text statusText;  // 방 상태 텍스트
    [SerializeField]
    private Text playerCountText;  // 플레이어 숫자 텍스트
    [SerializeField]
    private GameObject privacyIcon; // 비밀방인지 아닌지 아이콘 오브젝트
    private bool privacy; // 비밀방인지 아닌지
    private string lobbyPassword; // 비밀번호 저장용


    private void Start()
    {
        // 클릭 이벤트 등록
        GetComponent<Button>().onClick.AddListener(JoinLobby);
    }

    public void SetInfo(CSteamID id, string name, bool isPrivate, string playerCount, string passWord) // 룸리스트 정보 세팅
    {
        lobbyID = id;
        nameText.text = name;
        statusText.text = "대기중";
        playerCountText.text = playerCount;
        privacy = isPrivate;
        privacyIcon.SetActive(isPrivate);
        lobbyPassword = passWord;
    }

    void JoinLobby() // 로비 참여
    {
        Debug.Log("로비 참가 시도: " + lobbyID);
        if (privacy) // 비공개라면
        {
            // 비밀번호 입력 UI 띄우기
            LobbyUIManager.Instance.ShowPasswordPopup(OnPasswordEntered);
        }
        else
        {
            // 공개방이면 바로 입장
            SteamMatchmaking.JoinLobby(lobbyID);
        }

    }

    void OnPasswordEntered(string inputPassword)  // 비밀번호 입력
    {
        if (inputPassword == lobbyPassword)
        {
            Debug.Log("비밀번호 일치! 로비 참가.");
            SteamMatchmaking.JoinLobby(lobbyID);
            LobbyUIManager.Instance.pwConfrimInputField.text = string.Empty;
            LobbyUIManager.Instance.SetActivePanel("PassWord", false);
        }
        else
        {
            Debug.Log("비밀번호 불일치!");
            LobbyUIManager.Instance.PassWordError();
        }
    }
}
