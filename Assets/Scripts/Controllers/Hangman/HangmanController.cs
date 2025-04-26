using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class HangmanController : MonoBehaviour
{
    [SerializeField]
    private HangmanData hangmanData; // Referencia al ScriptableObject que contiene el banco de palabras y el número máximo de fallos
    
    [SerializeField]
    private GameObject[] hangmanStages; // Array de GameObjects que representan las etapas del ahorcado (por ejemplo, partes del cuerpo)

    private Label palabraOcultaLabel; // Etiqueta para mostrar la palabra oculta
    private TextField letraInput; // Campo de texto para que el jugador ingrese una letra
    private Button enviarLetraBtn; // Botón para enviar la letra ingresada
    private Label letrasFalladasLabel; // Etiqueta para mostrar las letras falladas

    private string palabra; // Palabra seleccionada para el juego
    private char[] palabraMostrada; // Array que representa la palabra oculta con letras descubiertas
    private List<char> letrasFallidas = new List<char>(); // Lista de letras incorrectas ingresadas por el jugador

    void OnEnable()
    {
        // Obtener la raíz del documento de UI Toolkit
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Asignar los elementos de la UI a las variables correspondientes
        palabraOcultaLabel = root.Q<Label>("palabraOculta");
        letraInput = root.Q<TextField>("letraInput");
        enviarLetraBtn = root.Q<Button>("enviarLetraBtn");
        letrasFalladasLabel = root.Q<Label>("letrasFalladas");

        // Agregar un evento al botón para verificar la letra ingresada
        enviarLetraBtn.clicked += VerificarLetra;

        // Restringir la entrada del campo de texto a una sola letra
        letraInput.RegisterValueChangedCallback(evt =>
        {
            string input = evt.newValue.ToUpper(); // Convertir a mayúsculas
            if (input.Length > 1) // Si hay más de una letra, tomar solo la primera
            {
                letraInput.value = input[0].ToString();
            }
            else if (!string.IsNullOrEmpty(input) && !char.IsLetter(input[0])) // Si no es una letra, limpiar el campo
            {
                letraInput.value = "";
            }
        });

        // Iniciar el juego
        IniciarJuego();
    }

    void IniciarJuego()
    {
        // Seleccionar una palabra aleatoria del banco de palabras y convertirla a mayúsculas
        palabra = hangmanData.palabras[Random.Range(0, hangmanData.palabras.Length)].ToUpper();

        // Inicializar el array de la palabra mostrada con guiones bajos
        palabraMostrada = new char[palabra.Length];
        for (int i = 0; i < palabraMostrada.Length; i++)
        {
            palabraMostrada[i] = '_';
        }

        // Desactivar todas las etapas del ahorcado
        foreach (GameObject stage in hangmanStages)
        {
            stage.SetActive(false);
        }

        // Limpiar la lista de letras fallidas
        letrasFallidas.Clear();

        // Actualizar la UI para reflejar el estado inicial del juego
        ActualizarUI();
    }

    void VerificarLetra()
    {
        // Obtener la letra ingresada por el jugador y convertirla a mayúsculas
        string input = letraInput.value.ToUpper();
        letraInput.value = ""; // Limpiar el campo de texto

        // Validar que la entrada no esté vacía y que sea una sola letra
        if (string.IsNullOrEmpty(input) || input.Length != 1) return;

        char letra = input[0];
        bool acierto = false;

        // Verificar si la letra ingresada está en la palabra
        for (int i = 0; i < palabra.Length; i++)
        {
            if (palabra[i] == letra)
            {
                palabraMostrada[i] = letra; // Descubrir la letra en la palabra mostrada
                acierto = true;
            }
        }

        // Si la letra no está en la palabra y no ha sido ingresada antes, agregarla a las letras fallidas
        if (!acierto && !letrasFallidas.Contains(letra))
        {
            letrasFallidas.Add(letra);
            hangmanStages[letrasFallidas.Count - 1].SetActive(true); // Activar la siguiente etapa del ahorcado
        }

        // Actualizar la UI para reflejar el estado actual del juego
        ActualizarUI();

        // Verificar si el jugador ha ganado o perdido
        if (new string(palabraMostrada) == palabra)
        {
            palabraOcultaLabel.text = "¡Ganaste! Era: " + palabra; // Mostrar mensaje de victoria
        }
        else if (letrasFallidas.Count >= hangmanData.maxFallos) // Usar maxFallos de HangmanData
        {
            palabraOcultaLabel.text = "¡Perdiste! Era: " + palabra; // Mostrar mensaje de derrota
        }
    }

    void ActualizarUI()
    {
        // Actualizar la palabra mostrada en la etiqueta
        palabraOcultaLabel.text = string.Join(" ", palabraMostrada);

        // Actualizar las letras falladas en la etiqueta
        letrasFalladasLabel.text = "Fallos: " + string.Join(", ", letrasFallidas);
    }
}
