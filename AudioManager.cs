using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("#BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    private AudioSource bgmPlayer;
    private AudioHighPassFilter bgmEffect;
    
    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    // 조원분의 Coin 항목을 추가한 최종 Sfx 열거형 정의
    public enum Sfx { Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win, Coin }

    private void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        GameObject bgmObj = new GameObject("BgmPlayer");
        bgmObj.transform.parent = transform;
        bgmPlayer = bgmObj.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // 메인 카메라 및 컴포넌트 예외 방어선 (우준님 아키텍처)
        if (Camera.main != null)
        {
            bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();
            if (bgmEffect == null)
            {
                bgmEffect = Camera.main.gameObject.AddComponent<AudioHighPassFilter>();
            }
            bgmEffect.enabled = false;
        }
        
        GameObject sfxObj = new GameObject("SfxPlayer");
        sfxObj.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < channels; i++)
        {
            sfxPlayers[i] = sfxObj.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].bypassListenerEffects = true;
            sfxPlayers[i].volume = sfxVolume;
        }   
    }

    public void PlayBgm(bool isPlay)
    {
        if (bgmPlayer == null) return;

        if (isPlay)
        {
            bgmPlayer.Play();
        }
        else
        {
            bgmPlayer.Stop();
        }
    }
    
    public void EffectBgm(bool isPlay)
    {
        if (bgmEffect != null)
        {
            bgmEffect.enabled = isPlay;
        }
    }

    // 대소문자 표기 및 채널 인덱스 갱신 로직 통합
    public void PlaySfx(Sfx sfx)
    {
        for (int i = 0; i < channels; i++)
        {
            int loopIndex = (i + channelIndex) % channels;

            if (sfxPlayers[loopIndex].isPlaying)
            {
                continue;
            }

            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx == Sfx.Melee)
            {
                ranIndex = Random.Range(0, 2);
            }
            
            channelIndex = (loopIndex + 1) % channels;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
}