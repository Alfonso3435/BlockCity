using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LectorDeCapitulos : MonoBehaviour
{
    public int numeroLibro;
    public int numeroCapitulo;
    public TMP_Text textoLectura;
    public ScrollRect scrollRect;

    void Start()
    {
        // Obtener el libro y capítulo seleccionados
        numeroLibro = PlayerPrefs.GetInt("CurrentBook", 1); // Valor por defecto: libro 1
        numeroCapitulo = PlayerPrefs.GetInt("CurrentChapter", 1); // Valor por defecto: capítulo 1

        MostrarCapitulo(numeroLibro, numeroCapitulo);
    }


    public void MostrarCapitulo(int libro, int capitulo)
    {
        string nombreArchivo = $"libro{libro}.txt";
        string path = Path.Combine(Application.streamingAssetsPath, nombreArchivo);

        if (File.Exists(path))
        {
            string contenido = File.ReadAllText(path);
            string[] capitulos = contenido.Split(new string[] { "#Capítulo" }, System.StringSplitOptions.RemoveEmptyEntries);

            if (capitulo - 1 < capitulos.Length)
            {
                textoLectura.text = capitulos[capitulo - 1].Trim();
                // Forzar el scroll hacia arriba
                Canvas.ForceUpdateCanvases(); // asegura que se actualice el layout antes
                scrollRect.verticalNormalizedPosition = 1f;
                Canvas.ForceUpdateCanvases(); // a veces ayuda repetirlo justo después
            }
            else
            {
                textoLectura.text = "Capítulo no encontrado.";
            }
        }
        else
        {
            textoLectura.text = $"Archivo no encontrado: {nombreArchivo}";
        }
    }
}
