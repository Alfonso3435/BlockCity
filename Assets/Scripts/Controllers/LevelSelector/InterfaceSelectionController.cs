using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class InterfaceSelectionController : MonoBehaviour
{
    // IDs de los items
    private const int SHIELD_ID = 1;
    private const int POTION_ID = 2;
    
    // Precios de los items
    private const int SHIELD_PRICE = 500;
    private const int POTION_PRICE = 350;
    
    // Referencias a los GameObjects de la UI
    [SerializeField] private GameObject successPopup;
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private GameObject storePopUp;
    [SerializeField] private GameObject noTouchPanel;
    
    // Referencias a los botones
    [SerializeField] private Button shieldButton;
    [SerializeField] private Button potionButton;
    
    // Referencias a los textos de cantidad
    [SerializeField] private TMP_Text shieldCountText;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private TMP_Text coinsText;

    private int userId;
    private bool coinsUpdateSuccess = false;

    private void Start()
    {
        userId = DBQuizReqHolder.Instance.GetUserID();
        DBQuizReqHolder.Instance.StartCoroutine(DBQuizReqHolder.Instance.GetCoinsData(userId));
        
        // Asignar los listeners a los botones
        shieldButton.onClick.AddListener(() => TryBuyItem(SHIELD_ID, SHIELD_PRICE));
        potionButton.onClick.AddListener(() => TryBuyItem(POTION_ID, POTION_PRICE));
        
        successPopup.SetActive(false);
        errorPopup.SetActive(false);
        
        // Actualizar las cantidades iniciales
        UpdateItemCounts();
    }

    private void Update()
    {
        UpdateItemCounts();
    }

    private void TryBuyItem(int itemId, int price)
    {
        int currentCoins = DBQuizReqHolder.Instance.GetCoins();
        
        if (currentCoins >= price)
        {
            // Comprar el item
            StartCoroutine(ProcessPurchase(itemId, price));
        }
        else
        {
            // Mostrar error de fondos insuficientes
            StartCoroutine(ShowError());
        }
    }

    private IEnumerator ProcessPurchase(int itemId, int price)
    {
        // Resetear el estado de éxito antes de cada compra
        coinsUpdateSuccess = false;
        
        // 1. Actualizar las monedas en la base de datos
        yield return StartCoroutine(UpdateCoins(userId, -price));
        
        // Verificar si la actualización de monedas fue exitosa
        if (coinsUpdateSuccess)
        {
            // 2. Actualizar la cantidad del item en la base de datos
            int currentQuantity = itemId == SHIELD_ID ? 
                DBQuizReqHolder.Instance.GetShieldCount() : 
                DBQuizReqHolder.Instance.GetPotionCount();
            
            yield return StartCoroutine(UpdateItemQuantity(userId, itemId, currentQuantity + 1));
            
            // 3. Actualizar los valores locales
            if (itemId == SHIELD_ID)
            {
                DBQuizReqHolder.Instance.SetShieldCount(currentQuantity + 1);
            }
            else
            {
                DBQuizReqHolder.Instance.SetPotionCount(currentQuantity + 1);
            }
            
            // 4. Actualizar la UI
            UpdateItemCounts();
            
            // 5. Mostrar éxito
            StartCoroutine(ShowSuccess());
        }
        else
        {
            // Mostrar error si la actualización de monedas falló
            StartCoroutine(ShowError());
        }
    }

    private IEnumerator UpdateCoins(int userId, int amount)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "coins/update";
        
        var data = new CoinsUpdateRequest
        {
            id_usuario = userId,
            amount = amount
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
            Debug.Log("Respuesta del servidor: " + request.downloadHandler.text);
            
            try
            {
                // Parsear la respuesta JSON
                var response = JsonUtility.FromJson<CoinsUpdateResponse>(request.downloadHandler.text);
                coinsUpdateSuccess = response.success;
                
                if (coinsUpdateSuccess)
                {
                    Debug.Log("Monedas actualizadas en BD. Nuevo total: " + response.newCoins);
                    // Actualizar el valor local solo si la BD se actualizó correctamente
                    DBQuizReqHolder.Instance.SetCoins(response.newCoins);
                }
                else
                {
                    Debug.LogError("Error en la respuesta del servidor: " + request.downloadHandler.text);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al parsear respuesta: " + e.Message);
                coinsUpdateSuccess = false;
            }
        }
        else
        {
            Debug.LogError("Error en la petición: " + request.error);
            coinsUpdateSuccess = false;
        }
    }

    private void UpdateItemCounts()
    {
        shieldCountText.text = "Currently in your inventory: " + DBQuizReqHolder.Instance.GetShieldCount().ToString();
        potionCountText.text = "Currently in your inventory: " + DBQuizReqHolder.Instance.GetPotionCount().ToString();
        coinsText.text = DBQuizReqHolder.Instance.GetCoins().ToString();
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

    private IEnumerator UpdateItemQuantity(int userID, int itemID, int quantity)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "items/update";

        ItemUpdateRequest data = new ItemUpdateRequest
        {
            id_usuario = userID,
            id_item = itemID,
            cantidad = quantity
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
            Debug.Log("Item quantity updated successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error updating item quantity: " + request.error);
        }
    }
}

[System.Serializable]
public class ItemUpdateRequest
{
    public int id_usuario;
    public int id_item;
    public int cantidad;
}

[System.Serializable]
public class CoinsUpdateRequest
{
    public int id_usuario;
    public int amount;
}

[System.Serializable]
public class CoinsUpdateResponse
{
    public bool success;
    public int id_usuario;
    public int newCoins;
    public int amountChanged;
}