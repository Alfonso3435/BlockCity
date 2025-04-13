using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LectureController : MonoBehaviour
{
    [SerializeField]
    private Button homeButton;

    [SerializeField]
    private Button nextButton;

    void Start()
    {
        // Click
        homeButton.onClick.AddListener(() => LoadScene("LevelSelection1"));

        // Escena guardada en PlayerPrefs para el botón "Siguiente"
        nextButton.onClick.AddListener(() =>
        {
            string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
            LoadScene(nextScene);
        });
    }

    // Carga de la escena 
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
