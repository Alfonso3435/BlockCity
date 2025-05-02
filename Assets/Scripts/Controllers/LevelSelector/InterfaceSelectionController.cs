using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class InterfaceSelectionController : MonoBehaviour
{
    // IDs y precios de los items
    private const int SHIELD_ID = 1;
    private const int POTION_ID = 2;
    private const int SHIELD_PRICE = 500;
    private const int POTION_PRICE = 350;

    // Referencias UI
    [SerializeField] private GameObject successPopup;
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private GameObject storePopUp;
    [SerializeField] private GameObject noTouchPanel;
    [SerializeField] private Button shieldButton;
    [SerializeField] private Button potionButton;
    [SerializeField] private TMP_Text shieldCountText;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private TMP_Text coinsText;

    private int userId;

    private void Start()
    {
        userId = DBQuizReqHolder.Instance.GetUserID();
        RefreshUserData();
        
        shieldButton.onClick.AddListener(() => TryBuyItem(SHIELD_ID, SHIELD_PRICE));
        potionButton.onClick.AddListener(() => TryBuyItem(POTION_ID, POTION_PRICE));
        
        successPopup.SetActive(false);
        errorPopup.SetActive(false);
    }

    private void RefreshUserData()
    {
        StartCoroutine(DBQuizReqHolder.Instance.GetCoinsData(userId));
        StartCoroutine(DBQuizReqHolder.Instance.GetItemsData(userId));
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        shieldCountText.text = "Currently in your inventory: " + DBQuizReqHolder.Instance.GetShieldCount();
        potionCountText.text = "Currently in your inventory: " + DBQuizReqHolder.Instance.GetPotionCount();
        coinsText.text = DBQuizReqHolder.Instance.GetCoins().ToString();
    }

    private void TryBuyItem(int itemId, int price)
    {
        if (DBQuizReqHolder.Instance.GetCoins() >= price)
        {
            StartCoroutine(ProcessPurchase(itemId, price));
        }
        else
        {
            StartCoroutine(ShowError());
        }
    }

    private IEnumerator ProcessPurchase(int itemId, int price)
    {
        // Primero actualizamos las monedas
        yield return StartCoroutine(UpdateUserCoins(userId, -price, (coinsSuccess) => {
            if (coinsSuccess)
            {
                // Luego actualizamos el item
                StartCoroutine(UpdateItemQuantity(userId, itemId, 1, (itemSuccess) => {
                    if (itemSuccess)
                    {
                        // Actualizamos los datos locales y mostramos éxito
                        RefreshUserData();
                        StartCoroutine(ShowSuccess());
                    }
                    else
                    {
                        StartCoroutine(ShowError());
                    }
                }));
            }
            else
            {
                StartCoroutine(ShowError());
            }
        }));
    }

    private IEnumerator UpdateUserCoins(int userId, int amount, System.Action<bool> callback)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "coins/modify";

        var data = new CoinsUpdateRequest
        {
            id_usuario = userId,
            coins = amount
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<CoinsUpdateResponse>(request.downloadHandler.text);
            if (response.success)
            {
                DBQuizReqHolder.Instance.SetCoins(response.newCoins);
                callback(true);
            }
            else
            {
                Debug.LogError("Error en la respuesta del servidor: " + request.downloadHandler.text);
                callback(false);
            }
        }
        else
        {
            Debug.LogError("Error en la petición: " + request.error);
            callback(false);
        }
    }

    private IEnumerator UpdateItemQuantity(int userId, int itemId, int quantityChange, System.Action<bool> callback)
    {
        int currentQuantity = itemId == SHIELD_ID ? 
            DBQuizReqHolder.Instance.GetShieldCount() : 
            DBQuizReqHolder.Instance.GetPotionCount();
        
        int newQuantity = currentQuantity + quantityChange;

        string url = DBQuizReqHolder.Instance.urlBD + "items/update";

        var data = new ItemUpdateRequest
        {
            id_usuario = userId,
            id_item = itemId,
            cantidad = newQuantity
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Actualizamos el valor local
            if (itemId == SHIELD_ID)
            {
                DBQuizReqHolder.Instance.SetShieldCount(newQuantity);
            }
            else
            {
                DBQuizReqHolder.Instance.SetPotionCount(newQuantity);
            }
            callback(true);
        }
        else
        {
            Debug.LogError("Error actualizando item: " + request.error);
            callback(false);
        }
    }

    private IEnumerator ShowSuccess()
    {
        successPopup.SetActive(true);
        yield return new WaitForSeconds(2f);
        successPopup.SetActive(false);
        storePopUp.SetActive(false);
        noTouchPanel.SetActive(false);
    }

    private IEnumerator ShowError()
    {
        errorPopup.SetActive(true);
        yield return new WaitForSeconds(2f);
        errorPopup.SetActive(false);
    }
}

[System.Serializable]
public class CoinsUpdateRequest
{
    public int id_usuario;
    public int coins;
}

[System.Serializable]
public class CoinsUpdateResponse
{
    public bool success;
    public int id_usuario;
    public int newCoins;
    public int amountChanged;
}

[System.Serializable]
public class ItemUpdateRequest
{
    public int id_usuario;
    public int id_item;
    public int cantidad;
}