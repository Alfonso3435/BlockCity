using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class QuizRequest : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000/quiz"; // Cambia esto si tu servidor está en otro lugar

    public void GetQuizData(int idQuiz)
    {
        StartCoroutine(GetQuizCoroutine(idQuiz));
    }

    private IEnumerator GetQuizCoroutine(int idQuiz)
    {
        string url = $"{baseUrl}/{idQuiz}"; // Construir la URL con el ID del quiz
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Enviar la solicitud
            yield return request.SendWebRequest();

            // Manejar errores
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                // Procesar la respuesta
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta recibida: " + jsonResponse);

                // Aquí puedes deserializar el JSON si es necesario
                // Ejemplo: QuizData[] quizData = JsonUtility.FromJson<QuizData[]>(jsonResponse);
            }
        }
    }
}
