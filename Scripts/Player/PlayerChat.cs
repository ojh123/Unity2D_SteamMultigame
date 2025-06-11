using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerChat : NetworkBehaviour
{
    public void SendChatMsg(string msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            string playerName = SteamManager.Initialized ? SteamFriends.GetPersonaName() : "Player";
            CmdSendMsg(playerName, msg);
        }

    }

    [Command]
    void CmdSendMsg(string sender, string message)
    {
        Debug.Log("CmdSendMsg");
        ChatManager.Instance.BroadcastMessage(sender, message);
    }
}
