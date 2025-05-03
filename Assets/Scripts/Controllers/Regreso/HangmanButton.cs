using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

// Descripción: Este archivo controla el botón de regreso en la escena del ahorcado, permitiendo al jugador volver al selector de niveles al interactuar con el botón correspondiente.
// Autor: Estrella Lolbeth

public class HangmanButton : MonoBehaviour
{
    private UIDocument Hangman;
    private Button homeButton;

    void OnEnable()
    {
        Hangman = GetComponent<UIDocument>();
        var root = Hangman.rootVisualElement;
        homeButton = root.Q<Button>("home");
        
        // Cambiar la escena a "LevelSelection"
        homeButton.RegisterCallback<ClickEvent>((evt) => Regresar(evt, "LevelSelection1"));
    }

    private void Regresar(ClickEvent evt, string escena)
    {
        SceneManager.LoadScene(escena);
    }
}
