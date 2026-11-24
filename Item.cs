using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;

    Image icon;
    Text text;
    private Text textName;
    private Text textDesc;

    private SkillController skillController;
    private Gear gear;

    void Awake()
    {
        // 최적화를 위해 Awake에서는 컴포넌트 캐싱만 수행합니다.
        icon = GetComponentsInChildren<Image>()[1];
        Text[] texts = GetComponentsInChildren<Text>();
        text = texts[0];
        textName = texts[1];
        textDesc = texts[2];
    }

    // OnEnable을 삭제하고 동적 주입을 위한 Setup 메서드를 생성합니다.
    public void Setup(ItemData newData, int currentLevel)
    {
        data = newData;
        level = currentLevel;

        icon.sprite = data.itemIcon;
        textName.text = data.itemName;
        text.text = "Lv. " + (level + 1);

        string typeStr = data.type.ToString();
        if (typeStr == "BaseSkill" || typeStr == "Melee" || typeStr == "Range")
        {
            textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
        }
        else if (typeStr == "Glove" || typeStr == "Shoe")
        {
            textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
        }
        else
        {
            textDesc.text = data.itemDesc;
        }
    }

public void OnClick()
    {
        SkillController[] currentSkills = GameManager.instance.player.GetComponentsInChildren<SkillController>(true);
        SkillController targetSkill = null;

        string typeStr = data.type.ToString();

        // 1. [탐색 로직 수정] 일반 무기일 때와 셰이프 오버라이드일 때 찾을 무기의 타겟 기준을 분리합니다.
        if (typeStr == "ShapeOverride")
        {
            // 셰이프 오버라이드는 특정 ID를 찾지 않고, 현재 플레이어가 가지고 있는 베이스 무기(첫 번째 스킬 등)를 타겟으로 잡습니다.
            // 만약 특정 슬롯 무기를 변경해야 한다면 data.targetHandType 등으로 맵핑 기준을 정하는 것이 안전합니다.
            if (currentSkills.Length > 0)
            {
                targetSkill = currentSkills[0]; // 현재 장착 중인 기본 무기 컨트롤러 타겟팅
            }
        }
        else
        {
            // 일반 무기는 기존대로 아이템 ID 매칭으로 찾음
            foreach (SkillController skill in currentSkills)
            {
                if (skill.name.EndsWith(data.itemId.ToString()))
                {
                    targetSkill = skill;
                    break;
                }
            }
        }

        // 2. 입력 분기 처리 시작
        if (typeStr == "BaseSkill" || typeStr == "Melee" || typeStr == "Range")
        {
            if (targetSkill != null)
            {
                if (level < data.damages.Length)
                {
                    targetSkill.baseCount += data.counts[level];
                    targetSkill.baseDamage += data.baseDamage * data.damages[level];
                }
            }
            else
            {
                GameObject newSkill = new GameObject("Skill_" + data.itemId);
                newSkill.transform.parent = GameManager.instance.player.transform;
                newSkill.transform.localPosition = Vector3.zero;
                
                targetSkill = newSkill.AddComponent<SkillController>();
                targetSkill.Init(data);
            }
            level++;
        }
        else if (typeStr == "ShapeOverride")
        {
            if (targetSkill != null)
            {
                // 수치 누적 (배열 에러 방어)
                if (data.counts != null && data.counts.Length > level)
                {
                    targetSkill.baseCount += data.counts[level];
                }
                if (data.damages != null && data.damages.Length > level)
                {
                    targetSkill.baseDamage += data.baseDamage * data.damages[level];
                }

                // 컨트롤러에게 새로운 부품을 던져주며 변환 명령 호출
                if (data.shapePart != null) 
                {
                    targetSkill.SwitchShape(data.shapePart);
                }
                else
                {
                    Debug.LogError($"[에러] {data.name} 에셋의 shapePart(SO) 슬롯이 비어있습니다!");
                }
            }
            else
            {
                Debug.LogWarning("[경고] 오버라이드할 기본 무기(SkillController)가 플레이어 자식에 없습니다!");
            }
            level++;
        }
        else if (typeStr == "Glove" || typeStr == "Shoe")
        {
            if (level == 0)
            {
                GameObject newGear = new GameObject("Gear_" + data.itemId);
                Gear gearObj = newGear.AddComponent<Gear>();
                gearObj.Init(data);
            }
            else
            {
                Gear[] gears = GameManager.instance.player.GetComponentsInChildren<Gear>(true);
                foreach (Gear g in gears)
                {
                    if (g.type == data.type)
                    {
                        g.LevelUp(data.damages[level]);
                        break;
                    }
                }
            }
            level++;
        }

        if (data.type != ItemData.ItemType.Heal)
        {
            GameManager.instance.uiLevelUp.itemLevels[data] = level;
        }
    }
}