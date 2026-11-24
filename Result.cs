using UnityEngine;

public class Result : MonoBehaviour
{
    public GameObject[] titles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Lose()
    {
        titles[0].SetActive(true);
    }

    // Update is called once per frame
    public void Win()
    {
        titles[1].SetActive(true);
    }
}
