using UnityEngine;
using UnityEngine.SceneManagement;


public class GameLoader : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}

