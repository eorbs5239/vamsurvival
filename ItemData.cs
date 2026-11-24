using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType { BaseSkill, ShapeOverride, EffectAdd, Glove, Shoe, Heal }
    public ItemType type;

    [Header("# Main Info")]
    public int itemId;
    public string itemName;
    [TextArea]
    public string itemDesc;
    public Sprite itemIcon;
    public Sprite Hand;

    [Header("# Skill Modules (BaseSkill 용)")]
    public SkillTriggerSO defaultTrigger;
    public SkillShapeSO defaultShape;
    public SkillEffectSO defaultEffect;

    [Header("# Module Parts (부품 조립용)")]
    public SkillShapeSO shapePart;   // ShapeOverride 타입일 때 장착될 형태 모듈
    public SkillEffectSO effectPart; // EffectAdd 타입일 때 추가될 효과 모듈

    [Header("# Level Data")]
    public float baseDamage;
    public int baseCount;
    public float[] damages;
    public int[] counts;
}