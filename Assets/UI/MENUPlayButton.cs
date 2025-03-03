using UnityEngine;
using UnityEngine.SceneManagement;

public class MENUPlayButton : MonoBehaviour
{
    public void SwitchToGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
