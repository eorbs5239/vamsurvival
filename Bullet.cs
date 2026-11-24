using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;
    private List<SkillEffectSO> effectsToApply;

    public void Init(float damage, int per, Vector3 dir, List<SkillEffectSO> effects)
    {
        this.damage = damage;
        this.per = per;
        this.effectsToApply = effects;

        if (per > -1)
        {
            GetComponent<Rigidbody2D>().linearVelocity = dir * 15f; 
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        if (effectsToApply != null)
        {
            foreach (SkillEffectSO effect in effectsToApply)
            {
                effect.Apply(collision, damage);
            }
        }

        // -1은 무한 관통이므로 로직 스킵
        if (per != -1)
        {
            per--;
            if (per == -1)
            {
                GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                gameObject.SetActive(false);
            }
        }
    }
}