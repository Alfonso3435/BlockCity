using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// Descripción: Este archivo controla la lógica de cargar escenas
// Autor: Alfonso Vega

public class SliderController : MonoBehaviour
{
    public void PressSelection(string _LevelName)
    {
        Debug.Log("Cargando escena: " + _LevelName);
        SceneManager.LoadScene(_LevelName);
    }
}
