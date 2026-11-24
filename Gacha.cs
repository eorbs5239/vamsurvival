using UnityEngine;
using UnityEngine.SceneManagement;

public class Gacha : MonoBehaviour
{
    public void OnClickGachaButton()
    {

        LoadingUIManager.nextSceneName = "GachaScene";

        SceneManager.LoadScene("Loading Scene");
    }
}