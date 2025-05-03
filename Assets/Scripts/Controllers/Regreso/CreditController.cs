using UnityEngine;
using UnityEngine.UIElements;

// Descripción: Este archivo controla la pantalla de créditos, permitiendo al jugador regresar al menú principal al interactuar con el botón correspondiente.
// Autor: Alfonso Vega

public class CreditController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonRegreso;

    void OnEnable()
    {
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;

        botonRegreso = root.Q<Button>("Cerrar");


        botonRegreso.RegisterCallback<ClickEvent>(evt => CambiarEscena("Menu"));
    }

    private void CambiarEscena(string escena)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(escena);
    }

}