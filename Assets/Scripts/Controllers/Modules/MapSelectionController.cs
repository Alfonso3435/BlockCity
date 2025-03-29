using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapSelectionController : MonoBehaviour
{
    [Header("Module Settings")]
    public bool isUnlocked = false;
    public int mapIndex;
    public int coinsRequired;
    public string levelSelectionSceneName;

    [Header("UI References")]
    public GameObject lockPanel;
    public GameObject unlockPanel;
    public TextMeshProUGUI starsText;
    public TextMeshProUGUI amountText;

    private void Start()
    {
        // Verificar si el módulo está desbloqueado
        isUnlocked = PlayerPrefs.GetInt("Module_" + mapIndex + "_Unlocked", mapIndex == 1 ? 1 : 0) == 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        lockPanel.SetActive(!isUnlocked);
        unlockPanel.SetActive(isUnlocked);

        if (isUnlocked)
        {
            // Mostrar estrellas totales del módulo
            int totalStars = LevelSelectionController.GetTotalStars(levelSelectionSceneName);
            starsText.text = totalStars.ToString() + "/15";
        }
        
        amountText.text = "Unlock for " + coinsRequired.ToString();
    }

    public void OnModuleClicked()
    {
        if (isUnlocked)
        {
            PlayerPrefs.SetString("CurrentModule", levelSelectionSceneName);
            SceneManager.LoadScene(levelSelectionSceneName);
        }
        else
        {
            TryUnlockModule();
        }
    }

    private void TryUnlockModule()
    {
        int playerCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        if (playerCoins >= coinsRequired)
        {
            PlayerPrefs.SetInt("TotalCoins", playerCoins - coinsRequired);
            PlayerPrefs.SetInt("Module_" + mapIndex + "_Unlocked", 1);
            isUnlocked = true;
            UpdateUI();
        }
    }
}