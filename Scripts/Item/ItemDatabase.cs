using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems;  // 아이템들을 담을 리스트
}
