using UnityEngine;

public class HideMapControls : MonoBehaviour
{
    private GameLogic gameLoader;

    private void Start()
    {
        gameLoader = GameObject.Find("GameLogic").GetComponent<GameLogic>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Hide() {

        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(gameLoader.isGuessing);
        }
    }
}
