using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class QuizRequest : MonoBehaviour
{
    public void GetQuizData(int idQuiz)
    {
        StartCoroutine(GetQuizCoroutine(idQuiz));
    }

    private IEnumerator GetQuizCoroutine(int idQuiz)
    {
        // Use urlBD from DBQuizReqHolder
        string url = $"{DBQuizReqHolder.Instance.urlBD}quiz/{idQuiz}"; // Construct the URL dynamically
        Debug.Log("Realizando solicitud a: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Send the request
            yield return request.SendWebRequest();

            // Handle errors
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en la solicitud: " + request.error);
            }
            else
            {
                // Process the response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta recibida: " + jsonResponse);

                // Deserialize the JSON if necessary
                // Example: QuizData[] quizData = JsonUtility.FromJson<QuizData[]>(jsonResponse);
            }
        }
    }
}
