using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Effects/HealEffect")]
public class HealEffect : ScriptableObject, IItemEffect
{
    public int amount; // 힐량

    public void Effect(Player player) // 아이템 효과
    {
        Debug.Log("체력회복!");
        player.hp = Mathf.Min(player.maxHp, player.hp + amount); // 체력회복 최대체력 넘지 않도록
    }
}
