using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Descripción: Este archivo controla el botón de regreso en la escena del juego de memoria, permitiendo al jugador volver al selector de niveles al interactuar con el botón correspondiente.
// Autor: Estrella Lolbeth

public class MemoryButton : MonoBehaviour
{
    [SerializeField]
    private Button regreso;

    void Start()
    {

        regreso.onClick.AddListener(() => LoadScene("LevelSelection1"));
    }


    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
