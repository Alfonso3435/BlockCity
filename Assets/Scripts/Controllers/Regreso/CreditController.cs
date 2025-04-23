using UnityEngine;
using UnityEngine.UIElements;


public class CreditController : MonoBehaviour
{
    private UIDocument menu;
    private Button botonRegreso;

    void OnEnable()
    {
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;

        botonRegreso = root.Q<Button>("Cerrar");

        // Callbacks
        botonRegreso.RegisterCallback<ClickEvent>(evt => CambiarEscena("Menu"));
    }

    private void CambiarEscena(string escena)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(escena);
    }

}