using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Steamworks;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;  // 전역변수

    [Header("채팅 UI 구성요소")]
    public InputField inputField;        // 메시지 입력창
    public Transform content;            // 메시지들이 들어갈 Content
    public GameObject msgPrefab;     // 메시지 프리팹 (Text 또는 Panel)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    [Server]
    public void BroadcastMessage(string sender, string msg) // 서버에서 실행
    {
        Debug.Log("BroadcastMessage");
        RpcReceiveMessage(sender, msg);
    }


    [ClientRpc]
    void RpcReceiveMessage(string playerName, string msg)  // 모든 클라이언트에게 실행
    {
        GameObject go = Instantiate(msgPrefab, content);
        Text msgText = go.GetComponent<Text>();
        msgText.text = $"<b>{playerName}:</b> {msg}";
    }

    public void OnSubmit_Chat()  // 인풋필드 OnSubmit 연결
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("OnEndEdit_Chat");
            PlayerChat player = NetworkClient.connection.identity.GetComponent<PlayerChat>();
            player.SendChatMsg(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField(); // 입력창 다시 포커스
        }
    }
}
