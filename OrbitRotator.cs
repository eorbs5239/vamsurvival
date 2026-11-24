using UnityEngine;

public class OrbitRotator : MonoBehaviour
{
    public float speed;

    void Update()
    {
        if (!GameManager.instance.isLive) return;
        transform.Rotate(Vector3.back * speed * Time.deltaTime);
    }
}