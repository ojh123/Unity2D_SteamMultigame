using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class Item : ScriptableObject
{
    [TextArea]
    public string itemDesc;     // 아이템 설명
    public string itemName;      // 아이템 이름
    public Sprite itemImage;     // 아이템 이미지
    public GameObject itemPrefab;  // 아이템 프리팹

    public ScriptableObject effectSO;  // IItemEffect를 구현한 ScriptableObject
}
