using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Scanner scanner;
    public Vector2 inputVec;
    public float speed;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCons;

    [Header("Dash Settings (Hardcoded)")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float ghostInterval = 0.04f;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;
    Collider2D myCollider; // 플레이어의 콜라이더 캐싱용

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
        myCollider = GetComponent<Collider2D>(); // Awake에서 미리 캐싱
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCons[GameManager.instance.playerId];
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void OnDash()
    {
        if (!GameManager.instance.isLive || isDashing || Time.time < nextDashTime) return;

        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }

        Vector2 dashDir = inputVec.normalized;
        if (dashDir == Vector2.zero)
        {
            dashDir = new Vector2(sr.flipX ? -1f : 1f, 0f);
        }

        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

        float startTime = Time.time;
        float lastGhostTime = 0f;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;

            if (Time.time >= lastGhostTime + ghostInterval)
            {
                CreateGhostTrail();
                lastGhostTime = Time.time;
            }
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        sr.color = originalColor;

        // [버그 수정 방어선]
        if (myCollider != null)
        {
            // 몬스터 전용 레이어가 있다면 ("Enemy") 아래 1번 방식으로 레이어 마스크를 지정하는 것이 가장 확실합니다.
            // 만약 레이어가 따로 없다면 2번 방식을 사용해 플레이어 자신을 제외하고 검사합니다.
            
            // 1번 방법: 몬스터 레이어만 필터링 (강력 추천)
            int enemyLayerMask = LayerMask.GetMask("Enemy"); 
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Enemy")); // 인스펙터에 설정된 몬스터 레이어 이름 입력
            filter.useLayerMask = true;

            Collider2D[] results = new Collider2D[5];

            // 주변에 '몬스터'만 겹쳐 있는지 검사합니다. (자기 자신이나 바닥 타일은 무시됨)
            while (Physics2D.OverlapCollider(myCollider, filter, results) > 0)
            {
                // 완전히 빠져나갈 때까지 관통 상태 유지하며 미세하게 이동
                rb.linearVelocity = dashDir * (speed * 0.5f); 
                yield return null;
            }
        }

        // 몬스터 군집을 완벽히 탈출했으므로 물리 충돌 원복 및 대시 종료
        rb.linearVelocity = Vector2.zero;
        if (myCollider != null) myCollider.isTrigger = false;
        
        isDashing = false;
    }

    void CreateGhostTrail()
    {
        GameObject ghost = new GameObject("GhostTrail");
        ghost.transform.position = transform.position;
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
        ghostSR.sprite = sr.sprite;
        ghostSR.flipX = sr.flipX;
        ghostSR.color = new Color(0.5f, 0.8f, 1f, 0.5f);
        ghostSR.sortingOrder = sr.sortingOrder - 1;

        StartCoroutine(FadeAndDestroyGhost(ghost, ghostSR));
    }

    IEnumerator FadeAndDestroyGhost(GameObject ghost, SpriteRenderer ghostSR)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Color initColor = ghostSR.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(initColor.a, 0f, elapsed / duration);
            ghostSR.color = new Color(initColor.r, initColor.g, initColor.b, alpha);
            yield return null;
        }
        Destroy(ghost);
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive) return;
        
        if (!isDashing)
        {
            Vector2 moveVec = inputVec.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveVec);
        }
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive) return;
        anim.SetFloat("Speed", inputVec.magnitude);
        
        if (inputVec.x != 0)
        {
            sr.flipX = inputVec.x < 0;
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameManager.instance.GetExp(); 
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive) return;

        if (isDashing) return;

        GameManager.instance.hp -= Time.deltaTime * 10;

        if (GameManager.instance.hp <= 0)
        {
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
}