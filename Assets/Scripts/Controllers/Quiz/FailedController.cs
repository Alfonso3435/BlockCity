using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Descripción: Este archivo controla la pantalla de nivel fallido, permitiendo al jugador regresar al selector de niveles tras no completar un nivel exitosamente.
// Autor: Alfonso Vega

public class FailedController : MonoBehaviour
{
     private UIDocument pagina;
    private Button regreso;
    void OnEnable(){
        pagina = GetComponent<UIDocument>();
        var root = pagina.rootVisualElement;
        regreso = root.Q<Button>("Regreso");


        regreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "LevelSelection1");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
