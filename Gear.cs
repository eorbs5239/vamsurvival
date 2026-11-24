using UnityEngine;

public class Gear : MonoBehaviour
{
    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        name = "Gear " + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;
        
        type = data.type;
        rate = data.damages[0];
        ApplyGear();
    }

    public void LevelUp(float rate)
    {
        this.rate = rate;
        ApplyGear();
    }

    void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
        }
    }

    public void RateUp()
    {
        // 폐기된 Weapon 대신 새로운 SkillController들을 찾아옵니다.
        SkillController[] skills = transform.parent.GetComponentsInChildren<SkillController>();

        foreach (var skill in skills)
        {
            // 1. 공전(Orbit) 형태를 사용하는 스킬인 경우 회전 속도 증가
            if (skill.shape is OrbitShapeSO)
            {
                OrbitRotator rotator = skill.GetComponent<OrbitRotator>();
                if (rotator != null)
                {
                    float speed = 150 * Character.WeaponSpeed;
                    rotator.speed = speed + (speed * rate);
                }
            }
            // 2. 투사체 등 다른 형태의 스킬인 경우 쿨타임 감소 배율 적용
            else
            {
                // 장갑을 레벨업 할수록 rate가 커지고, 쿨타임 배율은 줄어듦 (예: 1 - 0.2 = 0.8배)
                skill.cooldownMultiplier = 1f - rate;
            }
        }
    }

    void SpeedUp()
    {
        float speed = 3 * Character.Speed;
        GameManager.instance.player.speed = speed + speed * rate;
    }
}