using System;
using System.Collections;
using UnityEngine;

public class AchieveManager : MonoBehaviour
{
    // ⭐ [핵심 추가] GachaManager에서 나를 찾을 수 있도록 대문(싱글톤)을 선언합니다.
    // 반드시 함수 바깥(클래스 바로 아래)에 있어야 에러가 나지 않습니다.
    public static AchieveManager instance;

    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;

    // ⭐ [변경] 가챠 캐릭터(unlockGacha)를 enum 맨 뒤에 추가했습니다. (총 3개)
    enum Achieve { unlockPotato, unlockBean, unlockGacha }
    Achieve[] achieves;
    WaitForSecondsRealtime wait;

    void Awake()
    {
        // ⭐ [핵심 추가] 게임이 켜질 때 이 오브젝트 자신을 대문(instance)에 등록합니다.
        if (instance == null)
        {
            instance = this;
        }

        achieves = (Achieve[])Enum.GetValues(typeof(Achieve));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);

        foreach (Achieve achieve in achieves)
        {
            PlayerPrefs.SetInt(achieve.ToString(), 0);
        }
    }

    void Start()
    {
        UnlockCharacter();
    }

    void UnlockCharacter()
    {
        // 인스펙터 창의 배열 크기가 Enum 개수와 맞지 않을 때를 대비한 안전장치 추가
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            if (index >= achieves.Length) break;

            string achieveName = achieves[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achieveName) == 1;
            lockCharacter[index].SetActive(!isUnlock);
            unlockCharacter[index].SetActive(isUnlock);
        }
    }

    void LateUpdate()
    {
        foreach (Achieve achieve in achieves)
        {
            ChechAchieve(achieve);
        }
    }

    // 유저님의 기존 함수명(ChechAchieve)을 그대로 유지합니다.
    void ChechAchieve(Achieve achieve)
    {
        if (PlayerPrefs.GetInt(achieve.ToString()) == 1) return;

        // ⚠️ [안전장치 추가] 혹시 가챠 씬에 GameManager가 없을 때 에러가 나는 것을 방지합니다.
        if (GameManager.instance == null || !GameManager.instance.isLive) return;

        bool isAchieve = false;

        switch (achieve)
        {
            case Achieve.unlockPotato:
                isAchieve = GameManager.instance.kill >= 10;
                break;
            case Achieve.unlockBean:
                isAchieve = GameManager.instance.gameTime >= GameManager.instance.maxGameTime;
                break;
                // unlockGacha는 플레이 업적이 아니라 가챠 버튼으로 깨는 것이므로 switch문에서는 생략합니다.
        }

        if (isAchieve)
        {
            PlayerPrefs.SetInt(achieve.ToString(), 1);
            PlayerPrefs.Save(); // 데이터 즉시 저장

            if (uiNotice != null)
            {
                for (int index = 0; index < uiNotice.transform.childCount; index++)
                {
                    bool isActive = index == (int)achieve;
                    uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
                }
                StartCoroutine(NoticeRoutine());
            }

            UnlockCharacter();
        }
    }

    IEnumerator NoticeRoutine()
    {
        if (uiNotice != null) uiNotice.SetActive(true);
        if (AudioManager.instance != null) AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        yield return wait;
        if (uiNotice != null) uiNotice.SetActive(false);
    }

    // ⭐ [새로 추가] GachaManager에서 상자를 열어 당첨됐을 때 호출할 해금 함수입니다.
    public void UnlockByGacha(string achieveName)
    {
        PlayerPrefs.SetInt(achieveName, 1);
        PlayerPrefs.Save(); // 하드디스크에 저장

        UnlockCharacter(); // 즉시 화면 이미지 갱신
        Debug.Log($"[해금 성공] {achieveName} 캐릭터가 정상적으로 해금 및 저장되었습니다.");
    }
}