using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemoryButton : MonoBehaviour
{
    [SerializeField]
    private Button regreso;

    void Start()
    {
        // Click
        regreso.onClick.AddListener(() => LoadScene("LevelSelection1"));
    }

    // Carga de la escena 
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
