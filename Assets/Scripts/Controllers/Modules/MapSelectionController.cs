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
        isUnlocked = PlayerPrefs.GetInt("Module_" + mapIndex + "_Unlocked", mapIndex == 1 ? 1 : 0) == 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        lockPanel.SetActive(!isUnlocked);
        unlockPanel.SetActive(isUnlocked);

        if (isUnlocked)
        {
            int totalStars = LevelSelectionController.GetTotalStars(levelSelectionSceneName);
            starsText.text = totalStars.ToString() + "/15";
        }
        
        amountText.text = "Unlock for " + coinsRequired.ToString();
    }

    public void OnModuleClicked()
    {
        if (isUnlocked)
        {
            //Guardar el libro correspondiente al modulo actual
            PlayerPrefs.SetInt("CurrentBook", mapIndex);
            PlayerPrefs.SetString("CurrentModule", levelSelectionSceneName);

            // Modificar el valor de ModuleID en el singleton
            DBQuizReqHolder.Instance.SetModuleID(mapIndex);

            SceneManager.LoadScene(levelSelectionSceneName);
            
            //Debug.Log(mapIndex); // Este es el número del módulo que seleccionó el usuario
            
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