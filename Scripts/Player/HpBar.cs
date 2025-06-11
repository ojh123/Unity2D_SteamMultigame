using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public static HpBar Instance;

    [SerializeField] 
    private Image fillImage; // HP_Fill 
    

    private void Awake()
    {
        Instance = this;
    }

    public void HpUpdate(int hp, int maxHp)
    {
        fillImage.fillAmount = Mathf.Clamp01((float)hp / maxHp);
    }

    // 아이템들 hp 뺴고 적용안되는거 같고 움직일때 살짝씩 보이는거

   
}
