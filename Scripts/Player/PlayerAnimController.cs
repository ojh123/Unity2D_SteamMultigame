using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private SPUM_Prefabs spum;

    void Awake()
    {
        spum = GetComponent<SPUM_Prefabs>();
        spum.PopulateAnimationLists();
        spum.OverrideControllerInit();
    }

    // 공격할 때
    public void DoAttack()
    { 
        spum.PlayAnimation(PlayerState.ATTACK, 0);
    }

    // 이동할 때
    public void DoMove()
    {
        // 텔레포트 이펙트 넣기
        
    }

    // Idle 상태로 돌아갈 때
    public void DoIdle()
    {
        spum.PlayAnimation(PlayerState.IDLE, 0);
    }

    // 데미지 입었을 때
    public void DoDamaged()
    {
        spum.PlayAnimation(PlayerState.DAMAGED, 0);
    }

    // 죽었을 때
    public void DoDeath()
    {
        spum.PlayAnimation(PlayerState.DEATH, 0);   
    }
}
