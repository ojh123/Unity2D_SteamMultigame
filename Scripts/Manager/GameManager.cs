using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private GameObject gameBoard; // 게임보드 오브젝트

    [Header("인게임 플레이어")]
    public GameObject[] inGamePlayerPrefab; // 플레이어 프리팹

    [Header("아이템 스포너")]
    [SerializeField] private ItemSpawner itemSpawner;

    private CustomNetworkManager networkManager; // 네트워크 매니저


    int spawnCounter = 0; // 스폰확인용 카운터

    [SyncVar]
    private int currentTurnIndex = 0;       // 현재 턴 플레이어 인덱스
    [SyncVar]
    public float turnTime = 30f;  // 남은 턴 시간
    [SyncVar]
    private int turnsCount = 0;  // 아이템 스폰을 위한 턴 카운트

    private Coroutine turnTimerCoroutine; // 코루틴 변수


    [SyncVar]
    public bool game = false; // 게임중인지 아닌지 체크
    [SyncVar]
    public bool isSpawnPhase = false;
    [SyncVar]
    public bool isAttackPhase = false;
    [SyncVar]
    public bool isMovePhase = false;

    [Header("셀 리스트")]
    public CellController[] cells;

    // 그리드 너비(열 개수)
    private const int GRID_WIDTH = 5;

    [Header("플레이어 리스트")]
    public List<Player> inGamePlayers = new List<Player>();

    private void Awake()
    {
        if (!SteamManager.Initialized)
            return;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        networkManager = FindObjectOfType<CustomNetworkManager>();
    }

    // 호스트가 준비 체크 후 호출, GameUIManager에서 사용
    [Server]
    public void StartGame()
    {
        Debug.Log("StartGame");
        if (game) return;

        game = true;

        // 게임 시작전 레디 풀기
        foreach (var slot in networkManager.GamePlayers)
        {
            slot.isReady = false;
        }

        // 모든 클라이언트에 게임 시작 알리기
        RpcOnGameStarted();
    }

    [ClientRpc] // 게임 시작 Rpc
    void RpcOnGameStarted()
    {
        Debug.Log("RpcOnGameStarted");
        isSpawnPhase = true;  // 가이드페이즈 
        GameUIManager.Instance.PlayerSpawnGuide(true);  // 스폰가이드 키기

        // 나가기 버튼과 게임 시작 버튼 비활성화
        GameUIManager.Instance.DisableQuitButton();
        GameUIManager.Instance.DisableStartButton();
    }

    #region Spawn
    // 플레이어 스폰
    [Server]
    public void PlayerSpawn(Vector3 spawnpos, int index, NetworkConnectionToClient conn, int cellIndex, string name)
    {
        // 인게임 플레이어 생성
        GameObject go = Instantiate(inGamePlayerPrefab[index], spawnpos, Quaternion.identity);
        var player = go.GetComponent<Player>();
        player.playerName = name;
        player.hiddenCellIndex = cellIndex; // 숨은칸 인덱스
        inGamePlayers.Add(player); // 생성된 플레이어 리스트에 등록

        // 서버에서 플레이어 스폰
        NetworkServer.Spawn(go, conn);

        spawnCounter++;

        // 카운터가 전체 플레이어 수와 같아지면 모두 스폰 완료
        if (spawnCounter >= networkManager.GamePlayers.Count)
        {
            RpcAllPlayersSpawn();
        }
    }

    [ClientRpc]
    void RpcAllPlayersSpawn()
    {
        Debug.Log("모든 플레이어 스폰 완료!");

        // 공격 페이즈로
        isSpawnPhase = false;
        isAttackPhase = true;
        isMovePhase = false;

        GameUIManager.Instance.PlayerSpawnGuide(false);  // 가이드 텍스트 끄기
        if (isServer)  // 서버에서만 실행
        {
            // 첫 턴을 랜덤으로 뽑고
            int playerCount = networkManager.GamePlayers.Count;
            currentTurnIndex = UnityEngine.Random.Range(0, playerCount);

            // 턴 시작
            StartTurn();
        }
    }
    #endregion

    #region Turn
    [Server]
    void StartTurn()  // 턴 시작
    {
        // 턴 초기화
        turnTime = 30f;

        // 이전 가이드 모두 끄기
        foreach (var p in inGamePlayers)
            TargetHideAllGuides(p.connectionToClient);

        // 이번 턴 플레이어의 PlayerSlot 가져오기
        var slot = networkManager.GamePlayers[currentTurnIndex];
        string playerName = slot.playerName; // Steam 닉네임

        // 플레이어 리셋
        foreach (var player in inGamePlayers)
        {
            var p = player.GetComponent<Player>();
            p.isMyTurn = false;
            p.hasAttacked = false;
            p.hasMoved = false;
            p.atkChance = 1;
        }

        // 이번 턴 플레이어 셋팅
        var curPlayer = inGamePlayers[currentTurnIndex].GetComponent<Player>();

        curPlayer.isMyTurn = true;
        curPlayer.hasAttacked = false;
        curPlayer.hasMoved = false;

        // 지금 턴인 플레이어에게만 공격 가이드 띄우기
        TargetShowAttackGuide(curPlayer.connectionToClient);


        // 기존 타이머가 돌고 있으면 정지
        if (turnTimerCoroutine != null)
            StopCoroutine(turnTimerCoroutine);

        // 타이머 코루틴 시작
        turnTimerCoroutine = StartCoroutine(TurnTimer());


        // 매 턴마다 공격 페이즈로
        isSpawnPhase = false;
        isAttackPhase = true;
        isMovePhase = false;

        // RPC로 턴 시작 알림
        RpcTurnStarted(playerName);
    }

    IEnumerator TurnTimer() // 타이머 코루틴
    {
        while (turnTime > 0f)
        {
            yield return new WaitForSeconds(1f);
            turnTime -= 1f;
            RpcUpdateTimer(turnTime);
        }
        // 시간이 다 되면 자동으로 다음 턴
        NextTurn();
    }

    [Server]
    public void NextTurn() // 다음턴
    {
        // 플레이어 수만큼 순환
        int playerCount = networkManager.GamePlayers.Count;
        currentTurnIndex = (currentTurnIndex + 1) % playerCount;

        // 라운드 카운트
        turnsCount++;
        if (turnsCount >= playerCount * 2) // 2턴마다 아이템 스폰
        {
            // 아이템 스폰
            turnsCount = 0;
            itemSpawner.SpawnItem();

            Player.local.CmdShowItems();
        }

        StartTurn();
    }
    #endregion

    #region ClientRpcUI
    // 클라이언트에 턴 시작 알림 
    [ClientRpc]
    void RpcTurnStarted(string steamNickname)
    {
        GameUIManager.Instance.ShowTurn(steamNickname);
    }

    // 클라이언트에 타이머 업데이트 알림
    [ClientRpc]
    void RpcUpdateTimer(float time)
    {
        GameUIManager.Instance.UpdateTimerDisplay(time);
    }

    // 공격 가이드
    [TargetRpc]
    void TargetShowAttackGuide(NetworkConnection target)
    {
        GameUIManager.Instance.PlayerAttackGuide(true);
        GameUIManager.Instance.PlayerSpawnGuide(false);
        GameUIManager.Instance.PlayerMoveGuide(false);

        Player.local.CmdShowItems();
    }

    // 이동 가이드
    [TargetRpc]
    void TargetShowMoveGuide(NetworkConnection target)
    {
        GameUIManager.Instance.PlayerMoveGuide(true);
        GameUIManager.Instance.PlayerAttackGuide(false);
        GameUIManager.Instance.PlayerSpawnGuide(false);


        // 내 현재 위치 찾기
        var player = Player.local;
        int myIndex = player.hiddenCellIndex;

        // 모든 셀 돌면서 인접셀 아웃라인 켜기
        foreach (var cell in cells)
        {
            bool canMove = IsAdjacentCell(myIndex, cell.cellIndex, player.moveRange);
            cell.ShowOutLine(canMove);
        }
    
        Player.local.CmdShowItems();
    }

    // 아웃라인 끄기
    [TargetRpc]
    void TargetHideOutLine(NetworkConnection target)
    {
        foreach (var cell in cells)
            cell.ShowOutLine(false);
    }


    // 모든 가이드 끄기
    [TargetRpc]
    void TargetHideAllGuides(NetworkConnection target)
    {
        GameUIManager.Instance.PlayerSpawnGuide(false);
        GameUIManager.Instance.PlayerAttackGuide(false);
        GameUIManager.Instance.PlayerMoveGuide(false);
    }
    #endregion

    #region Attack
    [Server]
    public void PlayerAttack(NetworkConnectionToClient conn, int targetCell)
    {
        // circle 지우기
        cells[targetCell].RpcDisableCircle(targetCell);

        // 숨은 플레이어가 그 칸에 있다면 데미지
        foreach (var player in inGamePlayers)
        {
            if (player.hiddenCellIndex == targetCell)
            {
                player.hp -= 1; // 서버에서 플레이어 피 감소
                player.RpcOnDamaged();      // 클라이언트에 데미지 표시
            }
        }

        // attacker를 inGamePlayers에서 찾기
        Player attacker = inGamePlayers.Find(p => p.connectionToClient == conn);

        if (attacker.doubleAtkItem) // 아이템 여부에 따라 공격기회 부여
        {
            attacker.atkChance = 2;
            attacker.doubleAtkItem = false; // 더블공격 아이템 변수 초기화
        }
        else
        {
            attacker.atkChance = 1;
        }

        //  남은 공격 횟수 차감
        attacker.atkChance--;

        // 남은 공격 기회가 0이면 이동 페이즈로 전환
        if (attacker.atkChance <= 0)
        {
            attacker.hasAttacked = true;
            MovePhase();  // 이동 페이즈로
            TargetShowMoveGuide(conn);  // 이동 가이드
        }

        //누군가 HP가 0이면 곧바로 게임 종료
        if (inGamePlayers.Exists(p => p.hp <= 0))
        {
            EndGame();
            return;  // 더 이상 이동 페이즈로 넘어가지 않도록 리턴
        }
    }
    #endregion

    #region Move
    [Server]
    void MovePhase()
    {
        isSpawnPhase = false;
        isAttackPhase = false;
        isMovePhase = true;
    }

    // Player 스크립트에 IsTeleporting 함수 여기에 넣으면 움직일때 잠깐 보이는거 해결 가능 할듯
    [Server]
    public void PlayerMove(GameObject pl, int targetCellIndex, Vector3 movePos)
    {
        var player = pl.GetComponent<Player>();
        Vector3 oldPos = player.gameObject.transform.position;


        // 플레이어가 숨어있는 셀
        int curCell = player.hiddenCellIndex;
        if (!IsAdjacentCell(curCell, targetCellIndex, player.moveRange))
        {
            // 인접하지 않으면 무시
            return;
        }

        // 인접할 때만 이동 로직 실행
        player.RpcTeleport();
        player.TargetTeleportEffect(player.connectionToClient, oldPos, movePos);
        pl.gameObject.transform.position = movePos;
        RpcMovePlayer(pl, movePos);

        player.hiddenCellIndex = targetCellIndex;
        player.hasMoved = true;
        player.SetMoveRange(1); // 플레이어의 moveRange를 1로 설정 만약 아이템을 먹었을경우 moveRange를 2 -> 1 로 바꿔줌

        // 이동 끝나면 턴 종료
        isMovePhase = false;
        TargetHideOutLine(player.connectionToClient);
        NextTurn();
    }

    // 1칸 이내의 칸인지 검사
    public bool IsAdjacentCell(int fromIndex, int toIndex, int range)
    {
        int r1 = fromIndex / GRID_WIDTH;  // 행 구하기 
        int c1 = fromIndex % GRID_WIDTH;  // 열 구하기
        int r2 = toIndex / GRID_WIDTH;
        int c2 = toIndex % GRID_WIDTH;

        int dr = Mathf.Abs(r1 - r2);
        int dc = Mathf.Abs(c1 - c2);

        // 상하좌우 대각선 포함
        return (dr <= range && dc <= range);
    }

    [ClientRpc]
    void RpcMovePlayer(GameObject pl, Vector3 movePos)
    {
        pl.gameObject.transform.position = movePos;  // 해당 셀로 이동
    }
    #endregion



    #region EndGame
    public string ResultText()
    {
        // 1명만 있으면 그 사람이 자동 승리
        if (inGamePlayers.Count == 1)
            return $"{inGamePlayers[0].playerName} 승리!";

        if (inGamePlayers[0].hp > inGamePlayers[1].hp)  // 첫번째 플레이어가 hp가 많을때
            return $"{inGamePlayers[0].playerName} 승리!";
        else if (inGamePlayers[0].hp < inGamePlayers[1].hp) // 두번째 플레이어가 hp가 많을때
            return $"{inGamePlayers[1].playerName} 승리!";
        else
            return "무승부!";

    }

    [Server]
    public void EndGame()
    {
        string resultText = ResultText();
        // 게임 종료
        RpcOnGameEnded(resultText);
        // 결과 보여준 뒤 5초 후 리셋
        Invoke(nameof(ResetBoard), 5f);
    }

    [ClientRpc] // 게임 종료 Rpc
    void RpcOnGameEnded(string resultText)
    {
        // 결과 화면, 리셋 UI 등...
        GameUIManager.Instance.ShowResultPanel(resultText);

    }

    [Server]
    public void ResetBoard()
    {
        // 게임 상태 플래그 초기화
        game = false;
        isSpawnPhase = false;
        isAttackPhase = false;
        isMovePhase = false;

        // PlayerSlot에 중복 스폰방지 플래그 리셋
        RpcResetSpawn();

        // 타이머 코루틴 정지
        StopCoroutine(turnTimerCoroutine);

        // 셀 리셋 (원 모양, 마스크 다시 켜기)
        foreach (var cell in cells)
        {
            cell.RpcResetCell();  // ClientRpc로 모두 동기화
        }

        // 기존에 소환한 플레이어들 제거
        foreach (var player in inGamePlayers)
        {
            NetworkServer.Destroy(player.gameObject);
        }
        inGamePlayers.Clear();
        spawnCounter = 0;

        // 아이템들 제거
        foreach (var item in FindObjectsOfType<ItemPickup>())
        {
            NetworkServer.Destroy(item.gameObject);
        }

        // UI 리셋 (결과창 닫기, 가이드 숨기기 등)
        RpcResetUI();
    }

    [ClientRpc]
    void RpcResetSpawn()
    {
        // 로컬 플레이어의 PlayerSlot 컴포넌트 
        var identity = NetworkClient.connection?.identity;
        if (identity == null) return;

        var player = identity.GetComponent<PlayerSlot>();
        if (player == null || !player.isLocalPlayer) return;
        player.isSpawned = false;
    }

    // 셀 하나를 초기 상태로 되돌리는 ClientRpc
    [ClientRpc]
    void RpcResetUI()
    {
        GameUIManager.Instance.ResetUI();
    }
    #endregion
}
