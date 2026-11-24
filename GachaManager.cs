using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GachaManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Text coinText;               // 보유 코인을 보여줄 텍스트
    public GameObject warningPanel;     // 경고창 팝업 오브젝트
    public GameObject clearWindow;      // ✨ [새로 추가] 모두 오픈 시 나타날 클리어창 오브젝트
    public Button[] boxButtons;         // 5개의 상자 버튼 배열

    [Header("상자 스프라이트(이미지) 설정")]
    public Sprite defaultBoxSprite;    // 열기 전 원래 상자 이미지
    public Sprite characterSprite;     // 당첨(1) 시 보여줄 캐릭터 이미지
    public Sprite bombSprite;          // 꽝(0) 시 보여줄 폭탄 이미지

    [Header("상자 내용물 (코드 내부 계산용)")]
    private int[] boxContents;
    private int openedBoxCount = 0;     // 현재 열린 상자의 개수

    void Start()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
        if (clearWindow != null) clearWindow.SetActive(false); // ✨ 시작할 때 클리어창을 숨깁니다.

        UpdateCoinUI();
        ResetGacha();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            Add100Coins();
        }
    }

    void UpdateCoinUI()
    {
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (coinText != null)
        {
            coinText.text = "Coin : " + totalCoins.ToString();
        }
    }

    void ShuffleBoxes()
    {
        boxContents = new int[] { 0, 0, 0, 0, 1 };

        for (int i = 0; i < boxContents.Length; i++)
        {
            int rand = Random.Range(i, boxContents.Length);
            int temp = boxContents[i];
            boxContents[i] = boxContents[rand];
            boxContents[rand] = temp;
        }
    }

    public void OpenBox(int boxIndex)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins < 500)
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }
            return;
        }

        currentCoins -= 500;
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        PlayerPrefs.Save();
        UpdateCoinUI();

        boxButtons[boxIndex].interactable = false;
        openedBoxCount++;

        // 상자 내용물 체크 및 이미지 변경
        if (boxContents[boxIndex] == 1)
        {
            Debug.Log("🎉 캐릭터 획득 성공!");
            boxButtons[boxIndex].image.sprite = characterSprite;

            if (AchieveManager.instance != null)
            {
                AchieveManager.instance.UnlockByGacha("unlockGacha");
            }
            else
            {
                PlayerPrefs.SetInt("unlockGacha", 1);
                PlayerPrefs.Save();
            }
        }
        else
        {
            Debug.Log("💣 폭탄! 꽝입니다.");
            boxButtons[boxIndex].image.sprite = bombSprite;
        }

        // ✨ [수정] 상자 5개가 다 열렸다면 즉시 클리어 윈도우 팝업을 활성화합니다.
        if (openedBoxCount >= boxButtons.Length)
        {
            if (clearWindow != null)
            {
                clearWindow.SetActive(true);
                Debug.Log("🏆 모든 상자 오픈! ClearWindow가 활성화되었습니다.");
            }
        }
    }

    // ✨ [새로 추가] ClearWindow에 있는 '닫기(X) 버튼'과 연결할 함수입니다.
    public void CloseClearWindow()
    {
        if (clearWindow != null)
        {
            clearWindow.SetActive(false); // 클리어창을 닫고
        }

        ResetGacha(); // 새 게임을 할 수 있도록 가챠 판을 초기화합니다!
        Debug.Log("🔄 클리어창이 닫히고 가챠 판이 새로 리셋되었습니다.");
    }

    public void CloseWarning()
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    public void ResetGacha()
    {
        openedBoxCount = 0;
        ShuffleBoxes();

        for (int i = 0; i < boxButtons.Length; i++)
        {
            boxButtons[i].interactable = true;
            boxButtons[i].image.sprite = defaultBoxSprite;
        }
    }

    public void Add100Coins()
    {
        int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        totalCoins += 100;

        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.Save();

        UpdateCoinUI();
        Debug.Log($"[치트 코인 충전] 100코인이 지급되었습니다. 현재 보유: {totalCoins}개");
    }

    public void GoToGameScene()
    {
        LoadingUIManager.nextSceneName = "SampleScene";
        SceneManager.LoadScene("Loading Scene");
    }
}   