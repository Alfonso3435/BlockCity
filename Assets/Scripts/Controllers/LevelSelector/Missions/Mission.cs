using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text progressText;
    public TMP_Text rewardText;
    public Slider progressSlider;
    public Image statusBackground;
    public TMP_Text currentStatusText;
    public Button claimButton;

    [Header("Colors")]
    public Color inProgressColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Azul
    public Color claimableColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Verde
    public Color claimedColor = new Color(0.2f, 0.5f, 0.2f, 1f); // Verde oscuro

    private MissionData data;

    public void Setup(MissionData missionData)
{
    data = missionData;
    titleText.text = data.missionTitle;
    descriptionText.text = data.missionDescription;
    rewardText.text = data.rewardCoins.ToString();
    
    // Configurar el slider como no interactuable
    if (progressSlider != null)
    {
        progressSlider.interactable = false; // Esto desactiva la interacción
        progressSlider.maxValue = data.targetProgress;
        progressSlider.value = PlayerPrefs.GetInt($"Mission_{data.missionID}_Progress", 0);
    }
    
    claimButton.onClick.RemoveAllListeners();
    claimButton.onClick.AddListener(OnClaimButtonClicked);
    
    UpdateFromPlayerPrefs();
}

    public void UpdateFromPlayerPrefs()
    {
        int progress = PlayerPrefs.GetInt($"Mission_{data.missionID}_Progress", 0);
        bool isClaimed = PlayerPrefs.GetInt($"Mission_{data.missionID}_Claimed", 0) == 1;

        progressText.text = $"{progress}/{data.targetProgress}";
        
        if (progressSlider != null)
        {
            progressSlider.maxValue = data.targetProgress;
            progressSlider.value = progress;
        }

        UpdateStatusUI(progress, isClaimed);
    }

    private void UpdateStatusUI(int progress, bool isClaimed)
    {
        if (isClaimed)
        {
            SetStatus("Claimed", claimedColor);
            claimButton.interactable = false;
        }
        else if (progress >= data.targetProgress)
        {
            SetStatus("Claim", claimableColor);
            claimButton.interactable = true;
        }
        else
        {
            SetStatus("In Progress", inProgressColor);
            claimButton.interactable = false;
        }
    }

    private void SetStatus(string text, Color color)
    {
        currentStatusText.text = text;
        statusBackground.color = color;
    }

    private void OnClaimButtonClicked()
    {
        int progress = PlayerPrefs.GetInt($"Mission_{data.missionID}_Progress", 0);
        bool isClaimed = PlayerPrefs.GetInt($"Mission_{data.missionID}_Claimed", 0) == 1;
        
        if (!isClaimed && progress >= data.targetProgress)
        {
            // Añadir recompensa
            int totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
            totalCoins += data.rewardCoins;
            PlayerPrefs.SetInt("TotalCoins", totalCoins);
            
            // Marcar como reclamada
            PlayerPrefs.SetInt($"Mission_{data.missionID}_Claimed", 1);
            UpdateFromPlayerPrefs();
            
            Debug.Log($"Reclamadas {data.rewardCoins} monedas. Total: {totalCoins}");
        }
    }
}