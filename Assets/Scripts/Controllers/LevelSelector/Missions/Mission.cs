using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Mission : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text missionTitle;
    [SerializeField] private TMP_Text missionDescription;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardText;
    
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonText; // Nueva referencia para el texto del botón

    private UserQuest currentQuest;

    private void Awake()
    {
        // Obtener la referencia al texto del botón si no está asignada
        if (claimButton != null && claimButtonText == null)
        {
            claimButtonText = claimButton.GetComponentInChildren<TMP_Text>();
        }
    }

    public void Setup(UserQuest quest)
    {
        if (quest == null)
        {
            Debug.LogError("Se intentó configurar una misión nula");
            return;
        }

        currentQuest = quest;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Verificar componentes antes de usarlos
        if (missionTitle != null)
            missionTitle.text = currentQuest.nombre ?? "Sin nombre";

        if (missionDescription != null)
            missionDescription.text = currentQuest.descrip_quest ?? "Sin descripción";

        if (progressSlider != null)
        {
            progressSlider.maxValue = currentQuest.TargetProgress;
            progressSlider.value = currentQuest.userProgress;
        }

        if (rewardText != null)
        {
            rewardText.text = $"{currentQuest.RewardCoins}";
        }


        if (progressText != null)
            progressText.text = $"{currentQuest.userProgress}/{currentQuest.TargetProgress}";

        // Configurar estado del botón
        if (claimButton != null && claimButtonText != null)
        {
            bool canClaim = currentQuest.completado == 1 && currentQuest.userProgress >= currentQuest.TargetProgress;
            bool alreadyClaimed = currentQuest.completado == 2;
            
            claimButton.interactable = canClaim && !alreadyClaimed;
            
            // Cambiar texto según estado
            if (alreadyClaimed)
            {
                claimButtonText.text = "Claimed";
            }
            else if (canClaim)
            {
                claimButtonText.text = "Claim Reward";
            }
            else
            {
                claimButtonText.text = "In Progress";
            }
        }
    }

    public void OnClaimButton()
    {
        Debug.Log("Claim button clicked!");
        if (claimButton != null)
            claimButton.interactable = false;
        
            
        StartCoroutine(ClaimReward());
    }

    private IEnumerator ClaimReward()
{
    if (currentQuest == null || MissionManager.Instance == null)
    {
        Debug.LogError("No se puede reclamar recompensa - referencia nula");
        yield break;
    }

    // Desactivar el botón inmediatamente
    if (claimButton != null)
    {
        claimButton.interactable = false;
        claimButtonText.text = "Processing...";
    }

    // Llamar al manager para reclamar
    yield return MissionManager.Instance.ClaimQuestReward(currentQuest.id_quest);

    // Forzar actualización de datos
    yield return MissionManager.Instance.InitializeMissions();

    // Actualizar UI con los nuevos datos
    var updatedQuest = MissionManager.Instance.GetMissionById(currentQuest.id_quest);
    if (updatedQuest != null)
    {
        currentQuest = updatedQuest;
        UpdateUI();
    }
    else
    {
        Debug.LogError("No se pudo obtener la misión actualizada del manager");
    }

    // Verificación adicional
    if (claimButton != null && currentQuest.completado == 2)
    {
        claimButtonText.text = "Claimed";
    }
}

    public void UpdateFromServerData(UserQuest questData)
    {
        if (questData == null) return;
        currentQuest = questData;
        UpdateUI();
    }
}