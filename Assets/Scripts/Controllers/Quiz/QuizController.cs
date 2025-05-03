using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Descripción: Este archivo controla la lógica básica de la escena del cuestionario, incluyendo la navegación de regreso al selector de niveles o la transición a otras escenas según las acciones del jugador.
// Autor: Alfonso Vega

public class QuizController : MonoBehaviour
{
   private UIDocument quiz;
    private Button regreso;

    //private Button fallar;


    void OnEnable(){
        quiz = GetComponent<UIDocument>();
        var root = quiz.rootVisualElement;
        regreso = root.Q<Button>("Regreso");
        //fallar = root.Q<Button>("Fallar");

        regreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "LevelSelection1");
        //fallar.RegisterCallback<ClickEvent, String>(IniciarJuego, "FailedQuiz");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
