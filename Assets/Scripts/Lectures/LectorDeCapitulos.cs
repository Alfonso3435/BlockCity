using System.IO; // Namespace for file handling (not used in this script but included)
using UnityEngine; // Core Unity namespace
using UnityEngine.UI; // Namespace for UI components like ScrollRect
using UnityEngine.SceneManagement; // Namespace for scene management

// This class handles loading specific scenes (lectures) based on the chapter number stored in PlayerPrefs
public class LectorDeCapitulos : MonoBehaviour
{
    // Reference to the ScrollRect component to control scrolling behavior
    public ScrollRect scrollRect;

    // Variable to store the current chapter number
    private int numeroCapitulo;

    // Unity's Start method, called when the script is first executed
    void Start()
    {
        // Forzar la actualización del layout
        Canvas.ForceUpdateCanvases();

        // Ajustar la posición del scroll al inicio
        scrollRect.verticalNormalizedPosition = 1f;

        // Llamar al método para cargar la escena
        CargarEscenaLecture(numeroCapitulo);
    }

    // Method to load the appropriate lecture scene
    private void CargarEscenaLecture(int nivel)
    {
        // Loop to find the highest available lecture scene starting from the given chapter number
        while (nivel > 0)
        {
            // Construct the scene name dynamically (e.g., "Lecture1", "Lecture2", etc.)
            string nombreEscena = $"Lecture{nivel}";

            // Check if the scene can be loaded (exists in the build settings)
            if (Application.CanStreamedLevelBeLoaded(nombreEscena))
            {
                // Load the scene
                SceneManager.LoadScene(nombreEscena);

                // Force the scroll view to reset to the top
                Canvas.ForceUpdateCanvases(); // Ensures the layout is updated before scrolling
                scrollRect.verticalNormalizedPosition = 1f; // Set scroll position to the top
                Canvas.ForceUpdateCanvases(); // Sometimes helps to call it again immediately after

                // Exit the method after successfully loading the scene
                return;
            }
        }

        // Log a warning if no valid lecture scene is found
        Debug.LogWarning("No se encontró ninguna escena Lecture correspondiente.");
    }
}