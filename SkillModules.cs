using UnityEngine;
using System.Collections.Generic;

public abstract class SkillTriggerSO : ScriptableObject
{
    public abstract bool CheckCondition(SkillController controller);
}

public abstract class SkillEffectSO : ScriptableObject
{
    public abstract void Apply(Collider2D target, float damage);
}

public abstract class SkillShapeSO : ScriptableObject
{
    // 단일 SkillEffectSO에서 List<SkillEffectSO>로 서명 변경
    public abstract void Execute(Transform spawnPoint, List<SkillEffectSO> effects, float baseDamage, int baseCount);
}