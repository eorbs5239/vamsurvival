using UnityEngine;

public class WheelScroll : MonoBehaviour
{
    [Header("오브젝트 연결")]
    public RectTransform characterGroup; // 실제 움직일 내용물 (CharacterGroup)

    [Header("스크롤 속도 설정")]
    public float scrollSpeed = 50f;

    [Header("🔥 수동 스크롤 한계치 (셀 크기 무시)")]
    [Tooltip("캐릭터 목록이 위로 최대 몇 픽셀까지 밀려 올라갈지 직접 지정합니다.")]
    public float maxScrollDistance = 300f;

    void Start()
    {
        // 시작 위치를 맨 위(0)로 깔끔하게 정렬
        characterGroup.anchoredPosition = new Vector2(characterGroup.anchoredPosition.x, 0);
    }

    void Update()
    {
        // 마우스 휠 입력 감지
        float wheelInput = Input.mouseScrollDelta.y;
        if (wheelInput == 0) wheelInput = Input.GetAxis("Mouse ScrollWheel") * 10f;

        // 휠을 굴렸을 때만 실행
        if (wheelInput != 0)
        {
            Vector2 currentPos = characterGroup.anchoredPosition;

            // Y축 이동값 계산
            currentPos.y -= wheelInput * scrollSpeed;

            // 유니티 자동 계산을 완전히 무시하고, 유저가 입력한 maxScrollDistance까지만 스크롤 허용
            currentPos.y = Mathf.Clamp(currentPos.y, 0, maxScrollDistance);

            // 최종 좌표 적용
            characterGroup.anchoredPosition = currentPos;
        }
    }
}