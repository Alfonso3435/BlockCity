using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class InterfaceModulesController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text nombreUsuario;
    
    private int userId;
    
    void Start()
    {
        // Verificación de referencias
        if (nombreUsuario == null)
        {
            Debug.LogError("Error: No se ha asignado el campo 'nombreUsuario' en el Inspector");
            nombreUsuario = GetComponentInChildren<TMP_Text>();
        }

        userId = DBQuizReqHolder.Instance.GetUserID();
        
        // Cargar datos
        StartCoroutine(InitializeData());
    }

    private IEnumerator InitializeData()
    {
        // Cargar monedas
        yield return DBQuizReqHolder.Instance.StartCoroutine(
            DBQuizReqHolder.Instance.GetCoinsData(userId));
        
        // Cargar nombre de usuario
        yield return StartCoroutine(LoadUserName());
        
        UpdateItemCounts();
    }

    private void Update()
    {
        UpdateItemCounts();
    }

    private void UpdateItemCounts()
    {
        if (coinsText != null)
            coinsText.text = DBQuizReqHolder.Instance.GetCoins().ToString();
    }

    private IEnumerator LoadUserName()
    {
        // Construir URL correctamente
        string url = $"{DBQuizReqHolder.Instance.urlBD}user/name?id={userId}";
        //Debug.Log($"Solicitando nombre de usuario a: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Configurar timeout
            request.timeout = 10;
            
            yield return request.SendWebRequest();

            // Verificar resultados
            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log($"Respuesta del servidor: {request.downloadHandler.text}");
                
                try
                {
                    var response = JsonUtility.FromJson<UserNameResponse>(request.downloadHandler.text);
                    
                    if (response != null && response.success && response.data != null)
                    {
                        nombreUsuario.text = $"Welcome Back {response.data.nombre_user}!";
                        //Debug.Log($"Nombre mostrado: {response.data.nombre_user}");
                    }
                    else
                    {
                        SetDefaultName();
                        Debug.LogWarning("Respuesta del servidor incompleta");
                    }
                }
                catch (System.Exception e)
                {
                    SetDefaultName();
                    Debug.LogError($"Error al parsear JSON: {e.Message}");
                }
            }
            else
            {
                SetDefaultName();
                Debug.LogError($"Error en la petición: {request.error}");
            }
        }
    }

    private void SetDefaultName()
    {
        if (nombreUsuario != null)
            nombreUsuario.text = "Welcome Back!";
    }
}

// Clases para manejar la respuesta JSON
[System.Serializable]
public class UserNameResponse
{
    public bool success;
    public UserNameData data;
    public string error;
}

[System.Serializable]
public class UserNameData
{
    public int id_usuario;
    public string nombre_user;
}