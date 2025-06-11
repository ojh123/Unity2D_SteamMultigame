using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemPickup : NetworkBehaviour
{
    public int cellIndex; // 아이템의 위치
    public Item itemData;            // SO  할당
    private SpriteRenderer sr;   // 스프라이트

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemData.itemImage;  // 보이는 스프라이트 세팅
        sr.enabled = false;  // 숨기기
    }

    [ClientRpc]
    public void RpcShow()
    {
        sr.enabled = true;
    }

    // 보이기 제어
    [TargetRpc]
    public void TargetShow(NetworkConnection target, bool v)
    {
        sr.enabled = v;
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return; // 서버에서만 실행

        // 충돌한 객체가 Player 컴포넌트를 갖고 있는지 확인
        var player = other.GetComponent<Player>();
        if (player == null) return;
        if (player.isDead) return;

        // 아이템 효과 실행
        var effect = itemData.effectSO as IItemEffect;
        if (effect != null)
        {
            effect.Effect(player);
        }

        //아이템 삭제
        NetworkServer.Destroy(this.gameObject);
    }
}
