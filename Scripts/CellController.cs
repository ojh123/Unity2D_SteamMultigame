using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Collider2D))]
public class CellController : NetworkBehaviour
{
    // 씬에서 Inspector로 1~25 순서대로 할당
    public int cellIndex;

    [SerializeField]
    private GameObject outLine; // 아웃라인 오브젝트

    [SerializeField] 
    private GameObject attackEffectPrefab; // 공격 이펙트 프리팹

    private SpriteRenderer circleSprite;  // 서클 이미지
    private SpriteMask mask;      // 서클 마스크
    private BoxCollider2D col;    // 콜라이더
    public bool isDisable;    // 파괴 여부

    
    private int cellLayer;  // 레이어

    void Awake()
    {
        circleSprite = GetComponent<SpriteRenderer>();
        mask = GetComponent<SpriteMask>();
        col = GetComponent<BoxCollider2D>();

        var outLineobj = transform.Find("Outline");
        outLine = outLineobj.gameObject;

        cellLayer = 1 << LayerMask.NameToLayer("Circle");
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭이 시작되면
        if (Input.GetMouseButtonDown(0))
        {
            // 화면 좌표 → 월드 좌표로 변환
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rayOrigin = new Vector2(worldPoint.x, worldPoint.y);

            // Cells 레이어만 필터링해서 Raycast
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f, cellLayer);

            // 만약 레이캐스트가 CellController의 콜라이더를 잡았다면
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                Click();
            }
        }
    }

    void Click() // 이 오브젝트에 마우스 클릭 했을때
    {
        if (!GameManager.Instance.game) return;

        if (GameManager.Instance.isSpawnPhase) SpawnPlayer();
        else if (GameManager.Instance.isAttackPhase) AttackPlayer();
        else if (GameManager.Instance.isMovePhase) MovePlayer();

    }

    void SpawnPlayer() // 플레이어 스폰
    {
        // 로컬 플레이어의 PlayerSlot 컴포넌트 
        var identity = NetworkClient.connection?.identity;
        if (identity == null) return;

        var player = identity.GetComponent<PlayerSlot>();
        if (player == null || !player.isLocalPlayer) return;

        Vector3 spawnPos = transform.position;
        spawnPos.y -= 0.3f;

        // 서버에 스폰 요청
        player.CmdSpawnAtCell(cellIndex,spawnPos);
    }

    void AttackPlayer()  // 플레이어 공격
    {
        var localPlayer = Player.local;
        if (localPlayer.isDead || !localPlayer.isMyTurn || localPlayer.hasAttacked || localPlayer._hasAttacked)
            return;

        // 공격 커맨드
        localPlayer.CmdAttack(cellIndex);
    }

    [ClientRpc]
    void RpcAttackEffect()
    {
       Instantiate(attackEffectPrefab, transform.position, Quaternion.identity);
    }

    void MovePlayer()
    {
        var localPlayer = Player.local;
        if (localPlayer.isDead || !localPlayer.isMyTurn || localPlayer.hasMoved) return;

        Vector3 movePos = transform.position;
        movePos.y -= 0.3f;

        localPlayer.CmdMove(cellIndex, movePos);
    }

    
    [ClientRpc]
    public void RpcDisableCircle(int index)  // circle 지우는 rpc
    {
        if (index == cellIndex)
        {
            if(isServer)
            RpcAttackEffect();

            isDisable = true;
            circleSprite.enabled = false;
            mask.enabled = false;
        }

        // 그 칸에 있던 플레이어 표시
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.hiddenCellIndex == cellIndex && !player.isLocalPlayer)
            {
                var hide = player.GetComponent<PlayerHideSprite>();
                if (hide != null)
                    hide.SetVisible(true);
            }
        }
    }

    [ClientRpc]
    public void RpcResetCell()  // 원 모양과 마스크를 모두 켜서 초기 상태로
    {
        circleSprite.enabled = true;
        mask.enabled = true;
        col.enabled = true;
        isDisable = false;
    }

    public void ShowOutLine(bool on)
    {
        outLine.SetActive(on);
    }
}
