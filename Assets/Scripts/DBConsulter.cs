using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Descripción: Este archivo realiza solicitudes al servidor para obtener datos de cuestionarios, gestionando las respuestas y errores de las solicitudes HTTP.
// Autor: Israel González

public class QuizRequest : MonoBehaviour
{
    public void GetQuizData(int idQuiz)
    {
        StartCoroutine(GetQuizCoroutine(idQuiz));
    }

    private IEnumerator GetQuizCoroutine(int idQuiz)
    {

        string url = $"{DBQuizReqHolder.Instance.urlBD}quiz/{idQuiz}"; // Construct the URL dynamically
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

            }
        }
    }
}
