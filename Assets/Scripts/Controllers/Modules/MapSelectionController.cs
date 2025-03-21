using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSelectionController : MonoBehaviour
{
    public bool isUnlocked = false;
    public GameObject lockGo;
    public GameObject unclockGo;

    public int mapIndex;
    public int coinsRequired;

    public GameObject starsText; // GameObject que contiene el componente TextMeshPro para mostrar las estrellas
    public GameObject amountText; // GameObject que contiene el componente TextMeshPro para mostrar el costo de desbloqueo

    public string levelSelectionSceneName; // Nombre de la escena de Selector de Niveles asociada a este módulo

    private void Start()
    {
        int starsObtained = 3; // Por ejemplo, el usuario obtuvo 3 estrellas en el nivel 1
        string sceneName = "LevelSelection1"; // Nombre de la escena de Selector de Niveles
        PlayerPrefs.SetInt(sceneName + "_Lv1", starsObtained);
        PlayerPrefs.Save(); // Guarda los cambios // Llama a UpdateMapStatus al inicio para asegurarte de que el texto se actualice correctamente
    }

    private void Update()
    {
        UpdateMapStatus();
    }

    private void UpdateMapStatus()
    {
        if (isUnlocked)
        {
            unclockGo.gameObject.SetActive(true);
            lockGo.gameObject.SetActive(false);

            // Accede al componente TextMeshPro del GameObject starsText (dentro de UnlockedPanel)
            if (starsText != null)
            {
                TextMeshProUGUI starsTextComponent = starsText.GetComponent<TextMeshProUGUI>();
                if (starsTextComponent != null)
                {
                    // Obtiene el total de estrellas obtenidas en la escena de Selector de Niveles asociada
                    int totalStars = LevelSelectionController.GetTotalStars(levelSelectionSceneName);
                    // Actualiza el texto para mostrar el conteo de estrellas (ejemplo: "15/30")
                    starsTextComponent.text = totalStars.ToString() + "/30";
                }
                else
                {
                    Debug.LogError("El objeto Stars Text no tiene un componente TextMeshProUGUI.");
                }
            }
            else
            {
                Debug.LogError("El objeto Stars Text no está asignado en el Inspector.");
            }
        }
        else
        {
            unclockGo.gameObject.SetActive(false);
            lockGo.gameObject.SetActive(true);
        }

        // Accede al componente TextMeshPro del GameObject amountText (para mostrar el costo de desbloqueo)
        if (amountText != null)
        {
            TextMeshProUGUI amountTextComponent = amountText.GetComponent<TextMeshProUGUI>();
            if (amountTextComponent != null)
            {
                amountTextComponent.text = "Unlock for " + coinsRequired.ToString();
            }
            else
            {
                Debug.LogError("El objeto Amount no tiene un componente TextMeshProUGUI.");
            }
        }
        else
        {
            Debug.LogError("El objeto Amount Text no está asignado en el Inspector.");
        }
    }

    // Método para navegar a la escena de Selector de Niveles asociada
    public void OnModuleClicked()
    {
        if (isUnlocked && !string.IsNullOrEmpty(levelSelectionSceneName))
        {
            SceneManager.LoadScene(levelSelectionSceneName);
        }
        else
        {
            Debug.Log("El módulo está bloqueado o no tiene una escena asociada.");
        }
    }
}