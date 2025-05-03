using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

// Descripción: Este archivo controla la navegación y carga de capítulos en la escena de lectura, asegurando que el contenido se muestre desde el inicio y gestionando la transición a las escenas correspondientes según el capítulo seleccionado.
// Autor: Estrella Lolbeth
public class LectorDeCapitulos : MonoBehaviour
{
   
    public ScrollRect scrollRect;

   
    private int numeroCapitulo;

    void Start()
    {

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 1f;

        CargarEscenaLecture(numeroCapitulo);
    }


    private void CargarEscenaLecture(int nivel)
    {

        while (nivel > 0)
        {

            string nombreEscena = $"Lecture{nivel}";

            if (Application.CanStreamedLevelBeLoaded(nombreEscena))
            {

                SceneManager.LoadScene(nombreEscena);

                Canvas.ForceUpdateCanvases(); 
                scrollRect.verticalNormalizedPosition = 1f; 
                Canvas.ForceUpdateCanvases(); 

                return;
            }
        }


        Debug.LogWarning("No se encontró ninguna escena Lecture correspondiente.");
    }
}