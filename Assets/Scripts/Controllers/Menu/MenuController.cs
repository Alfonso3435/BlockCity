using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonPlay;
    private Button botonLogin;

    void OnEnable(){
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;
        botonPlay = root.Q<Button>("Play");
        botonLogin = root.Q<Button>("InicioSesion");

        botonPlay.RegisterCallback<ClickEvent, String>(IniciarJuego, "ModuleSelection");
        botonLogin.RegisterCallback<ClickEvent, String>(IniciarJuego, "Login");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }


}
