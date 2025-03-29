using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonPlay;
    private Button botonLogin;

    private void Start()
    {
        // Reiniciar progreso de niveles
        for (int i = 1; i <= 10; i++) // Ajusta el número máximo de niveles
        {
            PlayerPrefs.DeleteKey("Lv" + i.ToString());
        }

        // Reiniciar otros datos
        PlayerPrefs.DeleteKey("MaxLevelUnlocked");
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("CurrentLevelName");

        // Forzar guardado
        PlayerPrefs.Save();   
    }

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
