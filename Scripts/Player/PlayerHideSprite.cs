using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Principal;

public class PlayerHideSprite : NetworkBehaviour
{
    public SpriteRenderer[] renderers;
    public GameObject playerPrefab;
    public GameObject hpBar;

    public float delay = 1f;

    void Awake()
    {
        // 모든 자식 스프라이트 렌더러 가져오기
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!netIdentity.isOwned) // 자기 자신이 아니라면 
        {
            SetVisible(false); // 다른 플레이어는 안보이게
        }
        else
        {
            SetVisible(true); // 로컬 플레이어는 보이게
        }

    }
    
    private void OnTriggerEnter2D(Collider2D other)  // 상대가 서클 뒤에 있으면 안보이게
    {
        if (netIdentity.isOwned) return;

        if (other.CompareTag("Player")) // 플레이어 끼리 겹치면 보이게
        {
            SetVisible(true);
        }
       
    }


    public void SetVisible(bool v) // 스프라이트 렌더러 설정
    {
        foreach (var sr in renderers)
            sr.enabled = v;

        hpBar.SetActive(v);

    }

}
