using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Descripción: Este archivo controla la navegación dentro de la ciudad del selector de niveles, incluyendo la transición al siguiente nivel seleccionado por el jugador.
// Autor: Alfonso Vega
public class CityController : MonoBehaviour
{
    private UIDocument ciudad;
    private Button siguienteNivel;
    void OnEnable(){
        ciudad = GetComponent<UIDocument>();
        var root = ciudad.rootVisualElement;
        siguienteNivel = root.Q<Button>("Siguiente");


        siguienteNivel.RegisterCallback<ClickEvent, String>(IniciarJuego, "Lecture");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
