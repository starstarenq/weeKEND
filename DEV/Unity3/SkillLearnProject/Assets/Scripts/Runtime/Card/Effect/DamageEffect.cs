using UnityEngine;

[System.Serializable]
public class DamageEffect : CardEffect
{
    public int damageAmount;

    // 이제 SerializeReference 내부에서도 인스펙터 드래그 앤 드롭이 작동합니다.
    public UnityObjectReference vfxPrefab;

    public override void Apply(GameContext context)
    {
        // context.target에게 데미지를 주는 로직
        Debug.Log($"에게 {damageAmount}의 데미지를 줍니다.");

        // 직렬화된 유틸리티를 통해 게임 오브젝트에 접근
        GameObject prefab = vfxPrefab.Value;


        if (prefab != null)
        {
            Object.Instantiate(prefab);
        }
    }
}

