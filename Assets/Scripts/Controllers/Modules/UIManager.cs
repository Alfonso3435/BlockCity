using UnityEngine;
using UnityEngine.UI;
// Descripción: Este archivo gestiona la interfaz de usuario relacionada con la selección de mapas, incluyendo la actualización de los textos de monedas requeridas y estrellas obtenidas para cada mapa.
// Autor: Alfonso Vega
public class UIManager : MonoBehaviour
{
    public MapSelectionController[] mapSelections;
    public Text[] requieredCoinsTexts;
    public Text[] starsTexts;

    private void Update()
    {
        
    }

    private void UpdateRequieredCoinsUI()
    {
        for (int i = 0; i < mapSelections.Length; i++)
        {
            requieredCoinsTexts[i].text = mapSelections[i].coinsRequired.ToString();
        }
    }
}
