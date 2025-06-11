using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Items/Effects/DoubleAtkEffect")]
public class DoubleAtkEffect : ScriptableObject, IItemEffect
{
    public void Effect(Player player)
    {
        player.doubleAtkItem = true;
    }
}
