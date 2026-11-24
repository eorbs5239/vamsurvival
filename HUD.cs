using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType
    {
        Exp, Level, Kill, Time, Hp
    }
    public InfoType type;
    
    Text myText;
    Slider mySlider;

    // 문자열 재생성을 막기 위한 캐싱 변수
    private float cachedValue = -1f;
    private int cachedTime = -1;

    void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Exp:
                float maxExp = GameManager.instance.nextExp[Mathf.Min(GameManager.instance.level, GameManager.instance.nextExp.Length - 1)];
                // 방어 코드: 0으로 나누기 방지
                if (maxExp > 0) 
                {
                    mySlider.value = GameManager.instance.exp / maxExp;
                }
                break;

            case InfoType.Level:
                // 값이 변했을 때만 문자열 갱신
                if (cachedValue != GameManager.instance.level)
                {
                    cachedValue = GameManager.instance.level;
                    myText.text = string.Format("Lv.{0:F0}", cachedValue);
                }
                break;

            case InfoType.Kill:
                if (cachedValue != GameManager.instance.kill)
                {
                    cachedValue = GameManager.instance.kill;
                    myText.text = string.Format("{0:F0}", cachedValue);
                }
                break;

            case InfoType.Time:
                float remainTime = GameManager.instance.gameTime;
                int currentSec = Mathf.FloorToInt(remainTime);
                
                // 소수점 프레임이 아닌, 실제 1초 단위가 지났을 때만 문자열 갱신
                if (cachedTime != currentSec)
                {
                    cachedTime = currentSec;
                    int min = cachedTime / 60;
                    int sec = cachedTime % 60;
                    myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                }
                break;

            case InfoType.Hp:
                float maxHp = GameManager.instance.maxHp;
                if (maxHp > 0)
                {
                    mySlider.value = GameManager.instance.hp / maxHp;
                }
                break;
        }
    }
}