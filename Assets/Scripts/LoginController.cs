using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonOlvido;
    private Button botonInicioSesion;
    private Button botonRegistro;

    void OnEnable(){
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;
        botonOlvido = root.Q<Button>("Olvido");
        botonInicioSesion = root.Q<Button>("IniciarSesion");
        botonRegistro = root.Q<Button>("CrearCuenta");

        
        botonRegistro.RegisterCallback<ClickEvent, String>(IniciarJuego, "Register");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }


}
