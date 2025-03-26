using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class QuizController : MonoBehaviour
{
   private UIDocument quiz;
    private Button regreso;

    private Button fallar;


    void OnEnable(){
        quiz = GetComponent<UIDocument>();
        var root = quiz.rootVisualElement;
        regreso = root.Q<Button>("Regreso");
        fallar = root.Q<Button>("Fallar");

        regreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "LevelSelection1");
        fallar.RegisterCallback<ClickEvent, String>(IniciarJuego, "FailedQuiz");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
