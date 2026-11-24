using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingUIManager : MonoBehaviour
{
    // 로딩창에 있는 슬라이더 UI를 인스펙터에서 연결해줄 변수입니다.
    [SerializeField] private Slider progressBar;

    // ✨ [핵심 추가] 다음으로 이동할 목적지 씬 이름을 기억하는 공유 변수입니다.
    // 가챠 씬에서 게임 씬으로 갈 때 사용할 기본값("Main Scene")을 적어둡니다.
    // ⚠️ 유저님의 실제 게임 플레이 씬 이름과 대소문자/띄어쓰기까지 똑같이 맞춰주세요!
    public static string nextSceneName = "SampleScene";

    private void Start()
    {
        // 로딩 씬이 시작되자마자 설정된 목적지 씬을 비동기(Async)로 로드하기 시작합니다.
        StartCoroutine(LoadNextSceneAsync());
    }

    private IEnumerator LoadNextSceneAsync()
    {
        // 🔄 [수정] 고정된 "GachaScene" 대신, 변수에 담긴 목적지 씬을 백그라운드에서 로드합니다.
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false; // 로딩이 다 되어도 바로 화면을 넘기지 않고 대기

        float timer = 0f;
        bool resetTimerForEndPhase = false; // 게이지가 90% 이상일 때 연출을 위한 타이머 리셋용

        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;

            if (op.progress < 0.9f)
            {
                // 실제 유니티 로딩 진척도를 슬라이더에 반영 (0 ~ 0.9)
                if (progressBar != null)
                    progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
            }
            else
            {
                // 💡 뒷부분 게이지가 툭 끊기지 않고 부드럽게 차오르도록 타이머를 한 번 리셋해줍니다.
                if (!resetTimerForEndPhase)
                {
                    timer = 0f;
                    resetTimerForEndPhase = true;
                }

                // 로딩이 거의 끝나면 부드럽게 게이지를 100%(1.0)까지 채우는 연출
                if (progressBar != null)
                    progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);

                if (progressBar == null || progressBar.value >= 0.99f)
                {
                    // 슬라이더가 끝까지 차면 드디어 목적지 씬을 활성화(화면 전환)합니다.
                    op.allowSceneActivation = true;
                }
            }
        }
    }
}