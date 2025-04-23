using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class Pregunta
{
    public int id_pregunta;
    public string pregunta;
    public int indice_correcto;
    public string explicacion;
    public string tip;
    public Respuesta[] respuestas;
}

[System.Serializable]
public class Respuesta
{
    public int indice;
    public string respuesta;
}

[System.Serializable]
public class PreguntasWrapper
{
    public Pregunta[] preguntas;
}

[System.Serializable]
public class Item
{
    public int id_item;
    public string nombre_item;
    public int cantidad;
}

[System.Serializable]
public class ItemsWrapper
{
    public Item[] items;
}

public class DBQuizReqHolder : MonoBehaviour
{
    public static DBQuizReqHolder Instance;

    private int ModuleID;
    private int LevelNumber;
    private string GameType; // Quiz, Memory, Hangman
    private Pregunta[] preguntas;

    private bool isLoggedIn = false;

    private void Awake()
    {
        Debug.Log("DBQuizReqHolder Awake() llamado.");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DBQuizReqHolder inicializado.");
        }
        else
        {
            Destroy(gameObject); // Esto asegura que no haya duplicados
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento de cambio de escena
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Escena cargada: " + scene.name);

        Debug.Log($"ModuleID: {ModuleID}");
        Debug.Log($"LevelNumber: {LevelNumber}");
        Debug.Log($"GameType: {GameType}");

        StartCoroutine(GetQuizData(LevelNumber));
    }

    public IEnumerator GetQuizData(int NivelID)
    {
        string url = $"http://localhost:3000/quiz/{NivelID}";
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta recibida: " + jsonResponse);

                // Deserializar el JSON utilizando el wrapper
                PreguntasWrapper preguntasWrapper = JsonUtility.FromJson<PreguntasWrapper>($"{{\"preguntas\":{jsonResponse}}}");
                preguntas = preguntasWrapper.preguntas;

                // Acceder a los valores individuales
                foreach (Pregunta pregunta in preguntas)
                {
                    /*
                    Debug.Log($"Pregunta ID: {pregunta.id_pregunta}");
                    Debug.Log($"Texto de la pregunta: {pregunta.pregunta}");
                    Debug.Log($"Índice correcto: {pregunta.indice_correcto}");
                    Debug.Log($"Explicación: {pregunta.explicacion}"); // Mostrar la explicación
                    Debug.Log($"Tip: {pregunta.tip}");               // Mostrar el tip
                    */

                    foreach (Respuesta respuesta in pregunta.respuestas)
                    {
                        /*
                        Debug.Log($"Índice de respuesta: {respuesta.indice}");
                        Debug.Log($"Texto de respuesta: {respuesta.respuesta}");
                        */
                    }
                }
            }
        }
    }

    public IEnumerator GetItemsData(int userID)
    {
        string url = $"http://localhost:3000/items/{userID}";
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta recibida: " + jsonResponse);

                // Deserializar el JSON en una lista de ítems
                Item[] items = JsonUtility.FromJson<ItemsWrapper>($"{{\"items\":{jsonResponse}}}").items;

                // Acceder a los valores individuales
                foreach (Item item in items)
                {
                    Debug.Log($"ID del ítem: {item.id_item}");
                    Debug.Log($"Nombre del ítem: {item.nombre_item}");
                    Debug.Log($"Cantidad: {item.cantidad}");
                }
            }
        }
    }

    // Métodos para ModuleID
    public int GetModuleID()
    {
        return ModuleID;
    }

    public void SetModuleID(int moduleID)
    {
        ModuleID = moduleID;
    }

    // Métodos para LevelNumber
    public int GetLevelNumber()
    {
        return LevelNumber;
    }

    public void SetLevelNumber(int levelNumber)
    {
        LevelNumber = levelNumber;
    }

    // Métodos para GameType
    public string GetGameType()
    {
        return GameType;
    }

    public void SetGameType(string gameType)
    {
        GameType = gameType;
    }

    // Métodos para Preguntas
    public Pregunta[] GetPreguntas()
    {
        return preguntas;
    }

    public void SetPreguntas(Pregunta[] preguntasArray)
    {
        preguntas = preguntasArray;
    }

    // Getter and Setter for IsLoggedIn
    public bool GetIsLoggedIn()
    {
        return isLoggedIn;
    }

    public void SetIsLoggedIn(bool loggedIn)
    {
        isLoggedIn = loggedIn;
    }
}
