using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    [Header("스폰할 아이템 데이터베이스")]
    public ItemDatabase itemDatabase;      // ScriptableObject: allItems 리스트

    [Header("스폰 위치들")]
    public Transform[] spawnPoints;        // 씬에 미리 배치한 빈 오브젝트들

    public void SpawnItem() // 아이템 스폰
    {
        var item = itemDatabase.allItems[Random.Range(0, itemDatabase.allItems.Count)]; // 데이터 베이스에서 랜덤 아이템 가져오기
        int randomIndex = Random.Range(0, spawnPoints.Length);
        var pos = spawnPoints[randomIndex].position;     // 랜덤 위치에 스폰

        GameObject go = Instantiate(item.itemPrefab, pos, Quaternion.identity); // 아이템 생성
        var itemPickup = go.GetComponent<ItemPickup>();
        itemPickup.cellIndex = randomIndex;
        NetworkServer.Spawn(go);
    }
}
