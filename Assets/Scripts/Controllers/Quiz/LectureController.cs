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
    // Click en los botones
    homeButton.onClick.AddListener(() => LoadScene("LevelSelection1"));

    // Usamos la escena guardada en PlayerPrefs para el botón "Siguiente"
    nextButton.onClick.AddListener(() =>
    {
        string nextScene = PlayerPrefs.GetString("NextScene", "Quiz");
        LoadScene(nextScene);
    });
}

    // Método para cargar la escena
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
