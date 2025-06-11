using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskChildSprite : MonoBehaviour
{
    // 원하는 Mask Interaction 타입으로 설정
    [SerializeField]
    SpriteMaskInteraction _maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

    void Awake()
    {
        ChangemaskInteraction();
    }

    public void ChangemaskInteraction() // 이 오브젝트와 자식에 있는 모든 SpriteRenderer 검색
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in renderers)
        {
            sprite.maskInteraction = _maskInteraction;
        }
    }
}
