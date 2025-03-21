using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ModulesController : MonoBehaviour
{
   private UIDocument modulos;
    private Button moduloA;
    private Button moduloB;

    void OnEnable(){
        modulos = GetComponent<UIDocument>();
        var root = modulos.rootVisualElement;
        moduloA = root.Q<Button>("ModuloA");
        moduloB = root.Q<Button>("ModuloB");

        moduloA.RegisterCallback<ClickEvent, String>(IniciarJuego, "City");
        moduloB.RegisterCallback<ClickEvent, String>(IniciarJuego, "City");
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }
}
