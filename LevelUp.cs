using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelUp : MonoBehaviour
{
    private RectTransform rect;
    
    [Header("UI & Data")]
    public Item[] uiButtons; // 레벨업 창 하위의 3개 고정된 UI 버튼
    public ItemData[] allItems; // 게임 내 존재하는 모든 아이템 데이터 배열 (인스펙터에서 할당)
    
    // 각 아이템별 현재 레벨을 전역적으로 추적하는 상태 저장소
    public Dictionary<ItemData, int> itemLevels = new Dictionary<ItemData, int>();

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        uiButtons = GetComponentsInChildren<Item>(true);
        
        // 게임 시작 시 모든 아이템의 레벨을 0으로 초기화
        foreach (ItemData data in allItems)
        {
            itemLevels[data] = 0;
        }
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    public void Select(int index)
    {
        uiButtons[index].OnClick();
    }

    void Next()
    {
        foreach (Item btn in uiButtons)
        {
            btn.gameObject.SetActive(false);
        }

        List<ItemData> availableData = new List<ItemData>();
        foreach (ItemData data in allItems)
        {
            if (data == null) continue;
            if (data.type == ItemData.ItemType.Heal) continue;

            if (!itemLevels.ContainsKey(data))
            {
                itemLevels[data] = 0;
            }

            // === [디버깅 로그 삽입] ===
            Debug.Log($"[필터링 검사] 에셋 이름: {data.name} | 현재 레벨: {itemLevels[data]} | Damages 배열 길이: {data.damages.Length}");

            if (itemLevels[data] < data.damages.Length)
            {
                availableData.Add(data);
                Debug.Log($"<color=green>[후보군 진입 성공]</color> {data.name}이 선택지 풀에 들어갔습니다.");
            }
            else
            {
                Debug.Log($"<color=red>[후보군 탈락]</color> {data.name}은 만렙 판정으로 제외되었습니다.");
            }
        }

        // 중복 없는 랜덤 선택지 추출을 위한 셔플 알고리즘 적용 (기존의 불안정한 while문 제거)
        List<ItemData> selectedData = new List<ItemData>();
        
        // 피셔-예이츠 셔플로 후보군을 무작위로 섞음
        for (int i = 0; i < availableData.Count; i++)
        {
            int rand = Random.Range(i, availableData.Count);
            ItemData temp = availableData[i];
            availableData[i] = availableData[rand];
            availableData[rand] = temp;
        }

        // 최대 3개의 아이템을 무작위 선택지에 담음
        int countToSelect = Mathf.Min(3, availableData.Count);
        for (int i = 0; i < countToSelect; i++)
        {
            selectedData.Add(availableData[i]);
        }

        // UI 버튼에 데이터 매핑
        for (int i = 0; i < selectedData.Count; i++)
        {
            if (i >= uiButtons.Length) break;
            uiButtons[i].gameObject.SetActive(true);
            uiButtons[i].Setup(selectedData[i], itemLevels[selectedData[i]]);
        }

        // 남은 빈자리가 있다면 힐 아이템으로 채우되, 널 참조 에러 방어선 구축
        if (selectedData.Count < 3)
        {
            ItemData healData = System.Array.Find(allItems, x => x != null && x.type == ItemData.ItemType.Heal);
            
            for (int i = selectedData.Count; i < 3; i++)
            {
                if (i >= uiButtons.Length) break;
                
                if (healData != null)
                {
                    uiButtons[i].gameObject.SetActive(true);
                    uiButtons[i].Setup(healData, 0);
                }
                else
                {
                    // 프로젝트에 힐 아이템 에셋이 없다면 버튼을 켜지 않고 그냥 숨김 처리
                    uiButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
    }
}