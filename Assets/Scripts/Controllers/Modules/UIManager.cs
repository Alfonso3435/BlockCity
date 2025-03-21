using UnityEngine;
using UnityEngine.UI;

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
