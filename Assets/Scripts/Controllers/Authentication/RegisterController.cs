using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RegisterController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private UIDocument register;
    private Button botonRegreso;

    void OnEnable(){
        register = GetComponent<UIDocument>();
        var root = register.rootVisualElement;
        botonRegreso = root.Q<Button>("Regreso");

        
        botonRegreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "Login");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }

}
