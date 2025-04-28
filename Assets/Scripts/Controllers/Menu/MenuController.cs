using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonPlay;
    private Button botonLogin;

    //Error
    private Button contenedorError;
    private Button cerrarError;
    private Label mensaje;

    private Button infoButton;
    private Label infoText;
    private Button creditosButton;
    private Button infoContainer;
    private Button cerrarJuegoButton;

    private void Start()
    {
        ResetAllGameProgress();
    }

    private void ResetAllGameProgress()
    {
        // 1. Reiniciar progreso de niveles para TODOS los módulos
        string[] modules = { "LevelSelection1", "LevelSelection2", "LevelSelection3" }; // Añade todos tus módulos
        
        foreach (string module in modules)
        {
            for (int i = 1; i <= 10; i++) // Para cada nivel
            {
                PlayerPrefs.DeleteKey(module + "_Lv" + i);
            }
            PlayerPrefs.DeleteKey("Module_" + Array.IndexOf(modules, module) + "_Unlocked");
        }

        // 2. Reiniciar datos de módulos (excepto el primero que suele estar desbloqueado por defecto)
        for (int i = 2; i <= 5; i++) // Ajusta según tus módulos
        {
            PlayerPrefs.DeleteKey("Module_" + i + "_Unlocked");
        }

        // 3. Reiniciar datos temporales y globales
        PlayerPrefs.DeleteKey("MaxLevelUnlocked");
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("CurrentLevelName");
        PlayerPrefs.DeleteKey("CurrentModule");
        PlayerPrefs.DeleteKey("TempStars");
        PlayerPrefs.DeleteKey("TempPoints");
        PlayerPrefs.DeleteKey("TempCoins");

        // 4. Valores por defecto (opcional)
        PlayerPrefs.SetInt("Module_1_Unlocked", 1); // Desbloquear primer módulo
        PlayerPrefs.SetInt("TotalCoins", 1000); // Opcional: Dar monedas iniciales

        PlayerPrefs.Save();
        //Debug.Log("Todos los datos del juego han sido reiniciados");
    }

    void OnEnable()
    {
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;
        botonPlay = root.Q<Button>("Play");

        botonLogin = root.Q<Button>("InicioSesion");
        creditosButton = root.Q<Button>("Creditos");
        infoButton = root.Q<Button>("InfoButton");
        infoText = root.Q<Label>("InfoText");
        infoContainer = root.Q<Button>("InfoContainer");
        cerrarJuegoButton = root.Q<Button>("Cerrar");

        //Error
        contenedorError = root.Q<Button>("ErrorPopUp");
        mensaje = root.Q<Label>("Mensaje");
        cerrarError = root.Q<Button>("CerrarPopUp");

        cerrarError.RegisterCallback<ClickEvent>(CerrarError);
        contenedorError.RegisterCallback<ClickEvent>(CerrarError);
        contenedorError.style.display = DisplayStyle.None;

        infoText.style.display = DisplayStyle.None;
        infoContainer.style.display = DisplayStyle.None;

        botonPlay.RegisterCallback<ClickEvent>(CheckLogin);
        botonLogin.RegisterCallback<ClickEvent, String>(IniciarJuego, "Login");
        creditosButton.RegisterCallback<ClickEvent, String>(IniciarJuego, "Credits");
        infoButton.RegisterCallback<ClickEvent>(MostrarInfo);
        cerrarJuegoButton.RegisterCallback<ClickEvent>(CerrarJuego);


    }

    

    private void CheckLogin(ClickEvent evt)
    {
        DBQuizReqHolder.Instance.SetIsLoggedIn(true);
        if (DBQuizReqHolder.Instance.GetIsLoggedIn() == false)
        {
            contenedorError.style.display = DisplayStyle.Flex;
            mensaje.text = "You must \nlogin first.";
            return;
        }
        SceneManager.LoadScene("ModuleSelection");
    }

    private void CerrarError(ClickEvent evt)
    {
        if (contenedorError.style.display == DisplayStyle.Flex)
        {
            contenedorError.style.display = DisplayStyle.None;
            return;
        }
        
    }
    private void MostrarInfo(ClickEvent evt)
    {
        if (infoText.style.display == DisplayStyle.Flex)
        {
            infoText.style.display = DisplayStyle.None;
            infoContainer.style.display = DisplayStyle.None;
            return;
        }
        infoText.style.display = DisplayStyle.Flex;
        infoContainer.style.display = DisplayStyle.Flex;
    }
    /*
    private void CerrarJuego(ClickEvent evt)
    {
        UnityEditor.EditorApplication.isPlaying = false; 
        Application.Quit(); 
    }
    */
    private void CerrarJuego(ClickEvent evt)
    {
        Application.Quit(); 
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}