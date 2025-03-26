using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LectureController : MonoBehaviour
{
    private UIDocument lectura;
    private Button regreso;
    private Button siguiente;

    void OnEnable(){
        lectura = GetComponent<UIDocument>();
        var root = lectura.rootVisualElement;
        regreso = root.Q<Button>("Regreso");
        siguiente = root.Q<Button>("Siguiente");

        regreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "LevelSelection1");
        siguiente.RegisterCallback<ClickEvent, String>(IniciarJuego, "Quiz");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
