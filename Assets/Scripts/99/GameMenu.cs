using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Button is pressed");

        SceneManager.LoadScene("GameScene");
    }
}
