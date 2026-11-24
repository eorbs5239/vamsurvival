using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive = false;
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# Player Info")]
    public int playerId;
    public float hp;
    public float maxHp = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 10, 100, 150, 210, 280, 360, 450, 600 };
    public ItemData startingItem;
    public int coinCount;

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject enemyCleaner;

    [Header("# Click형 Skill System (조원 구현)")]
    public GameObject bombPrefab;
    public float bombDamage = 100f; 
    public float bombCooldownDuration = 30f;
    public Image bombImage;
    public Button bombButton;
    private bool isBombOnCooldown = false;

    [Header("# 버프 스킬 시스템")]
    public float speedBuffAmount = 3f;
    public float buffDuration = 30f;
    public float cooldownDuration = 60f;
    public Image speedUpImage;
    public Button speedUpButton;
    private bool isSpeedUpActive = false;

    [Header("# 재화 UI Reference")]
    public UnityEngine.UI.Text coinText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 세이브된 총 코인 데이터 로드 및 반영
        coinCount = PlayerPrefs.GetInt("TotalCoins", 0);

        if (coinText != null)
        {
            coinText.text = "Coin : " + coinCount.ToString();
        }
    }

    public void GameStart(int id)
    {
        playerId = id;
        hp = maxHp;
        gameTime = 0;
        coinCount = 0;

        if (coinText != null)
        {
            coinText.text = "Coin : " + coinCount.ToString();
        }

        player.gameObject.SetActive(true);

        // 스타팅 아이템이 존재할 경우 모듈러 스킬 컨트롤러 동적 생성 및 주입 (우준님 아키텍처)
        if (startingItem != null)
        {
            GameObject newSkill = new GameObject("Skill_" + startingItem.itemId);
            newSkill.transform.parent = player.transform;
            newSkill.transform.localPosition = Vector3.zero;
            
            SkillController skillController = newSkill.AddComponent<SkillController>();
            skillController.Init(startingItem);
        }

        uiLevelUp.Select(playerId % 2);
        Resume();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
    }

    private void Update()
    {
        if (!isLive) return;

        gameTime += Time.deltaTime;

        if (gameTime >= maxGameTime)
        {
            gameTime = maxGameTime;

            // 만렙 또는 클리어 시 코인 저장 프로세스
            int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            PlayerPrefs.SetInt("TotalCoins", savedCoins + coinCount);
            PlayerPrefs.Save();

            GameVictory();
        }
    }

    public void GetExp()
    {
        if (!isLive) return;

        exp++;

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", savedCoins + coinCount);
        PlayerPrefs.Save();

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        isLive = false;
        yield return new WaitForSeconds(0.5f);
        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
        }
    }

    public void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }

    private IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
        }
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    // 클릭형 스킬: 폭탄 사용
    public void UseBomb()
    {
        if (isBombOnCooldown) return;
        StartCoroutine(BombRoutine());
    }

    private IEnumerator BombRoutine()
    {
        isBombOnCooldown = true;
        bombButton.interactable = false;
        bombImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        GameObject explosion = Instantiate(bombPrefab, player.transform.position, Quaternion.identity);
        explosion.GetComponent<Bullet>().Init(bombDamage, -1, Vector3.zero, null);

        yield return new WaitForSeconds(bombCooldownDuration);

        bombImage.color = Color.white;
        bombButton.interactable = true;
        isBombOnCooldown = false;
    }

    // 클릭형 스킬: 스피드 업
    public void SpeedUp()
    {
        if (isSpeedUpActive) return;
        StartCoroutine(SpeedUpRoutine());
    }

    private IEnumerator SpeedUpRoutine()
    {
        isSpeedUpActive = true;

        speedUpButton.interactable = false;
        speedUpImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        player.speed += speedBuffAmount;
        yield return new WaitForSeconds(buffDuration);

        player.speed -= speedBuffAmount;

        float remainingCooldown = cooldownDuration - buffDuration;
        yield return new WaitForSeconds(remainingCooldown);

        speedUpImage.color = Color.white;
        speedUpButton.interactable = true;
        isSpeedUpActive = false;
    }

    // 인게임 코인 획득 처리 루틴 (조원 드랍 연동용)
    public void GetCoin(int amount)
    {
        coinCount += amount;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Coin);
        }
        if (coinText != null)
        {
            coinText.text = "Coin : " + coinCount.ToString();
        }
    }
}