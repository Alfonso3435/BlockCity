using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class QuizController : MonoBehaviour
{
   private UIDocument quiz;
    private Button regreso;


    void OnEnable(){
        quiz = GetComponent<UIDocument>();
        var root = quiz.rootVisualElement;
        regreso = root.Q<Button>("Regreso");

        regreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "City");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
