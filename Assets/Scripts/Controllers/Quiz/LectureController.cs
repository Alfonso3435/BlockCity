using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Descripción: Este archivo controla la lógica de la escena de lectura, incluyendo la navegación hacia el menú principal o la transición a la siguiente escena según el nivel actual del jugador.
// Autor: Estrella Lolbeth e Israel González
public class LectureController : MonoBehaviour
{
    [SerializeField]
    private Button homeButton;

    [SerializeField]
    private Button nextButton;

    void Start()
    {
        Debug.Log("WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW");
        Debug.Log("levelNum: " + DBQuizReqHolder.Instance.GetLevelNumber());
        StartCoroutine(DBQuizReqHolder.Instance.GetQuizData(DBQuizReqHolder.Instance.GetLevelNumber()));

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (currentLevel == 3 || currentLevel == 5)
        {

            string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
            LoadScene(nextScene);
            return; 
        }


        homeButton.onClick.AddListener(() => LoadScene("LevelSelection1"));


        nextButton.onClick.AddListener(() =>
        {
            string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
            LoadScene("Quiz");
        });
    }


    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
