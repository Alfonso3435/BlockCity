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
        nextButton.onClick.AddListener(() => LoadScene("Quiz"));
    }
    // Método para cargar la escena
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
