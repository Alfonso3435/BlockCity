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
        Debug.Log("WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW");
        Debug.Log("levelNum: " + DBQuizReqHolder.Instance.GetLevelNumber());
        StartCoroutine(DBQuizReqHolder.Instance.GetQuizData(DBQuizReqHolder.Instance.GetLevelNumber()));
        // Verificar si el nivel actual es 3 o 5
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1); // Nivel actual, por defecto 1
        if (currentLevel == 3 || currentLevel == 5)
        {
            // Cargar directamente la siguiente escena
            string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
            LoadScene(nextScene);
            return; // Salir del método para evitar ejecutar el resto del código
        }

        // Configurar el botón "Inicio"
        homeButton.onClick.AddListener(() => LoadScene("LevelSelection1"));

        // Configurar el botón "Siguiente" para cargar la escena guardada en PlayerPrefs
        nextButton.onClick.AddListener(() =>
        {
            string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
            LoadScene("Quiz");
        });
    }

    // Carga de la escena
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
