using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking.PlayerConnection;
using Newtonsoft.Json.Linq;

public class Player : NetworkBehaviour
{
    public static Player local;

    [Header("상태변수들")]
    [SyncVar(hook = nameof(HpChanged))] public int hp = 5;  // 체력
    public int maxHp = 5; // 최대체력
    [SyncVar] public bool isMyTurn = false;  // 내턴인지
    [SyncVar] public int hiddenCellIndex = -1;  // 내가 숨은칸 인덱스
    [SyncVar] public bool hasAttacked = false; // 공격했는지 
    [SyncVar] public bool hasMoved = false;   // 움직였는지
    [SyncVar] public bool isDead = false;  // 사망 여부
    [SyncVar(hook = nameof(MoveRangeChanged))] public int moveRange = 1;    // 움직일 수 있는 칸
    [SyncVar(hook = nameof(DoubleAtkChanged))] public bool doubleAtkItem = false;  // 더블어택 아이템이 있는지
    [SyncVar] public int atkChance = 1;  // 공격기회
    public bool _hasAttacked = false; // SyncVar hasAttacked와는 별개로 공격 했는지 표시할 로컬 변수
    [SyncVar(hook = nameof(IsTeleporting))]public bool isTeleporting = false;

    public string playerName; // 플레이어 스팀 닉네임

    private int attackSell = -1; // 내가 공격할 셀 인덱스

    [SerializeField]
    private float attackDelay = 0.5f;  // 공격 딜레이

    private PlayerAnimController anim; // 애니메이션

    [Header("텔레포트 이펙트")]
    [SerializeField] private GameObject teleportEffectPrefab;

    [Header("HP바")]
    [SerializeField] private GameObject hpBarObj;
    [SerializeField] private Image hpFillImage;

    [Header("상태 아이콘")]
    [SerializeField] private GameObject doubleAtkIcon;
    [SerializeField] private GameObject speedIcon;

    private void Awake()
    {
        anim = GetComponent<PlayerAnimController>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        local = this;
    }

    //클라이언트 → 서버 공격 요청
    [Command]
    public void CmdAttack(int cellIndex)
    {
        if (isDead || !isMyTurn || hasAttacked || _hasAttacked) return;  // 죽었거나 자기턴이 아니거나 공격을 했다면 리턴

        _hasAttacked = true;
        anim.DoAttack();  // 공격 애니메이션
        attackSell = cellIndex;  // 공격할 셀 인덱스
        StartCoroutine(DelayAttack());
    }

    private IEnumerator DelayAttack() // 애니메이션 끝난후 실행되게
    {
        yield return new WaitForSeconds(attackDelay);

        // 공격 후 공격할 셀 인덱스 초기화
        GameManager.Instance.PlayerAttack(connectionToClient, attackSell);
        attackSell = -1;
        _hasAttacked = false; // 공격 초기화
    }

    //클라이언트 → 서버 움직임 요청
    [Command]
    public void CmdMove(int CellIndex, Vector3 movePos)
    {
        if (isDead || !isMyTurn || hasMoved) return;
        GameManager.Instance.PlayerMove(this.gameObject, CellIndex, movePos);

    }
    [ClientRpc]
    public void RpcTeleport()
    {
        StartCoroutine(TeleportCor());
    }
    
    IEnumerator TeleportCor()
    {
        isTeleporting = true;
        yield return new WaitForSeconds(1f);
        isTeleporting = false;
    }

    // 텔레포트 이펙트
    [TargetRpc]
    public void TargetTeleportEffect(NetworkConnection target, Vector3 oldPos, Vector3 newPos)
    {
        // 출발 위치
        Instantiate(teleportEffectPrefab, oldPos, Quaternion.identity);
        // 도착 위치
        Instantiate(teleportEffectPrefab, newPos, Quaternion.identity);
    }

    [Command]
    public void CmdShowItems()
    {
        // 서버에서 이 플레이어 주변 아이템 판단 후
        ShowItems();
    }

    // 주변에 아이템이 있는지 있으면 보이게
    public void ShowItems()
    {
        foreach (var pickup in FindObjectsOfType<ItemPickup>())
        {
            bool near = GameManager.Instance.IsAdjacentCell(hiddenCellIndex, pickup.cellIndex, moveRange);
            bool broken = GameManager.Instance.cells[pickup.cellIndex].isDisable;

            if (broken) // 부서진칸이면
            {
                // 모든 클라이언트에게 보여주기
                pickup.RpcShow();
            }
            else if (near) // 내 주변에 있으면
            {
                // 나에게만 보이게
                pickup.TargetShow(connectionToClient, true);
            }
        }
    }

    [ClientRpc]
    public void RpcOnDamaged() // 데미지 RPC
    {

        anim.DoDamaged();
        Debug.Log($"데미지! 남은 HP: {hp}");
        if (hp <= 0)
        {
            isDead = true;
            anim.DoDeath(); // 사망처리
        }
    }

    // 아이템 효과로 moveRange를 변경할 때 사용
    public void SetMoveRange(int newRange)
    {
        moveRange = newRange;
    }

    public void SetHpBarVisible(bool visible)
    {
        hpBarObj.SetActive(visible);
    }

    public void UpdateHpBar()
    {
        hpFillImage.fillAmount = Mathf.Clamp01((float)hp / maxHp);
    }

    // SyncVar hook
    public void HpChanged(int oldHp, int newHp)
    {
        UpdateHpBar();

        if (!isLocalPlayer) // 상대일 경우 데미지 받으면 HP 바 보이게
        {
            SetHpBarVisible(true);
        }
    }

    // SyncVar hook
    void MoveRangeChanged(int oldValue, int newValue)
    {
        speedIcon.SetActive(newValue > 1); // MoveRange가 1보다 크면 부츠 아이콘 표시
    }

    // SyncVar hook
    void DoubleAtkChanged(bool oldValue, bool newValue)
    {
        doubleAtkIcon.SetActive(newValue);
    }

    // SyncVar hook
    void IsTeleporting(bool oldValue, bool newValue)
    {
        PlayerHideSprite hideSprite = GetComponent<PlayerHideSprite>();

        Debug.Log("텔레포트중");
        // Player 객체가 자기꺼라면
        if (netIdentity.isOwned)
        {
            hideSprite.SetVisible(true); // Player가 항상 보이게
        }
        // Player 객체가 다른 플레이어의 캐릭터라면
        else
        {
            if (newValue == true) // 텔레포트 중이면 숨김
            {
                hideSprite.SetVisible(false);
            }
            else // 텔레포트가 끝나면 위치에 따라 숨김 결정
            {
                CellController cell = GameManager.Instance.cells[hiddenCellIndex];
                if (cell != null && !cell.isDisable) // 셀이 파괴가 안됐을경우
                {
                    hideSprite.SetVisible(false);
                }
                else if((cell != null && cell.isDisable)) //셀이 파괴가 됐을경우
                {
                    hideSprite.SetVisible(true);
                }
            }
        }
    }
}
