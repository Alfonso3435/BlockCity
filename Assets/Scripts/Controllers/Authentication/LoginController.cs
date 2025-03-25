using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginController : MonoBehaviour
{
    private UIDocument login;
    private Button botonOlvido;
    private Button botonInicioSesion;
    private Button botonRegistro;
    private Button botonRegreso;

    void OnEnable(){
        login = GetComponent<UIDocument>();
        var root = login.rootVisualElement;
        botonOlvido = root.Q<Button>("Olvido");
        botonInicioSesion = root.Q<Button>("Login");
        botonRegistro = root.Q<Button>("Registro");
        botonRegreso = root.Q<Button>("Regreso");

        
        botonRegistro.RegisterCallback<ClickEvent, String>(IniciarJuego, "Register");
        botonRegreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "Menu");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }


}
