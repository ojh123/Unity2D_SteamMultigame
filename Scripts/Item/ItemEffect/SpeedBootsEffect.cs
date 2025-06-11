using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/SpeedBootsEffect")]
public class SpeedBootsEffect : ScriptableObject, IItemEffect
{
    public int addRange = 1; // 움직일 추가 이동 거리

    public void Effect(Player player) // 아이템 효과
    {
        player.SetMoveRange(player.moveRange + addRange);
    }
}
