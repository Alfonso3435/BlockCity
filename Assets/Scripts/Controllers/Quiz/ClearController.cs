using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ClearController : MonoBehaviour
{
    private UIDocument completado;
    private Button reinicio;
    private Button siguiente;

    void OnEnable(){
        completado = GetComponent<UIDocument>();
        var root = completado.rootVisualElement;
        reinicio = root.Q<Button>("Reiniciar");
        siguiente = root.Q<Button>("Siguiente");

        reinicio.RegisterCallback<ClickEvent, String>(IniciarJuego, "Quiz");
        siguiente.RegisterCallback<ClickEvent, String>(IniciarJuego, "LevelSelection1");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
