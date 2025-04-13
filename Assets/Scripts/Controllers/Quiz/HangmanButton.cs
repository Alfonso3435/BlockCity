using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

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
        homeButton.RegisterCallback<ClickEvent>((evt) => Regresar(evt, "LevelSelection"));
    }

    private void Regresar(ClickEvent evt, string escena)
    {
        SceneManager.LoadScene(escena);
    }
}
