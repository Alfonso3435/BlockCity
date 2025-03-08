using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
