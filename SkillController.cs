using UnityEngine;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    public SkillTriggerSO trigger;
    public SkillShapeSO shape;
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    public float baseDamage;
    public int baseCount;
    public float cooldownMultiplier = 1f;
    public float timer;
    
    public void Init(ItemData data)
    {
        name = "Skill_" + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        trigger = data.defaultTrigger;
        shape = data.defaultShape;
        
        effects.Clear();
        if (data.defaultEffect != null)
        {
            effects.Add(data.defaultEffect);
        }

        baseDamage = data.baseDamage * Character.Damage;
        baseCount = data.baseCount + Character.Count;
        
        Player player = GameManager.instance.player;
        int handIndex = (int)data.type;
        
        if (handIndex >= 0 && handIndex < player.hands.Length)
        {
            Hand hand = player.hands[handIndex];
            hand.sr.sprite = data.Hand;
            hand.gameObject.SetActive(true);
        }
        
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void AddEffect(SkillEffectSO newEffect)
    {
        if (newEffect != null && !effects.Contains(newEffect))
        {
            effects.Add(newEffect);
        }
    }

    // [핵심 추가] 형태 모듈이 실시간으로 교체될 때 호출되는 수술(재조립) 파이프라인
    public void SwitchShape(SkillShapeSO newShape)
    {
        if (newShape == null) return;

        // 1. [범용 청소] 이 스킬 컨트롤러가 가진 모든 자식 중 "SkillPart" 태그를 가진 것들을 삭제
        // 금요일 병합 전, 가장 확실하고 빠른 방법은 Destroy입니다.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            
            // 프리팹들에 'SkillPart' 태그를 달아주세요!
            if (child.CompareTag("SkillPart"))
            {
                Destroy(child.gameObject);
            }
        }

        // 2. 뼈대 교체
        shape = newShape;

        // 3. 타이머 리셋
        timer = 0f;

        // 4. 즉시 격발
        shape.Execute(this.transform, effects, baseDamage, baseCount);
        
        Debug.Log($"[형태 변환 완료] {name}이(가) {newShape.name}으로 교체되었습니다.");
    }

    void Update()
    {
        if (!GameManager.instance.isLive) 
        {
            return;
        }
        
        if (trigger == null || shape == null) 
        {
            return;
        }

        if (trigger.CheckCondition(this))
        {
            shape.Execute(this.transform, effects, baseDamage, baseCount);
        }
    }
}