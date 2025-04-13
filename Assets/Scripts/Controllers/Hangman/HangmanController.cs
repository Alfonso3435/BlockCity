using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class HangmanController : MonoBehaviour
{
    [SerializeField]
    private HangmanData hangmanData; // Reference to HangmanData
    
    [SerializeField]
    private GameObject[] hangmanStages;

    private Label palabraOcultaLabel;
    private TextField letraInput;
    private Button enviarLetraBtn;
    private Label letrasFalladasLabel;

    private string palabra;
    private char[] palabraMostrada;
    private List<char> letrasFallidas = new List<char>();

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        palabraOcultaLabel = root.Q<Label>("palabraOculta");
        letraInput = root.Q<TextField>("letraInput");
        enviarLetraBtn = root.Q<Button>("enviarLetraBtn");
        letrasFalladasLabel = root.Q<Label>("letrasFalladas");

        enviarLetraBtn.clicked += VerificarLetra;

        IniciarJuego();
    }

    void IniciarJuego()
    {
        // Usar el banco de palabras de HangmanData
        palabra = hangmanData.palabras[Random.Range(0, hangmanData.palabras.Length)].ToUpper();
        palabraMostrada = new char[palabra.Length];

        for (int i = 0; i < palabraMostrada.Length; i++)
        {
            palabraMostrada[i] = '_';
        }

        foreach (GameObject stage in hangmanStages)
        {
            stage.SetActive(false);
        }

        letrasFallidas.Clear();
        ActualizarUI();
    }

    void VerificarLetra()
    {
        string input = letraInput.value.ToUpper();
        letraInput.value = ""; // limpiar input

        if (string.IsNullOrEmpty(input) || input.Length != 1) return;

        char letra = input[0];
        bool acierto = false;

        for (int i = 0; i < palabra.Length; i++)
        {
            if (palabra[i] == letra)
            {
                palabraMostrada[i] = letra;
                acierto = true;
            }
        }

        if (!acierto && !letrasFallidas.Contains(letra))
        {
            letrasFallidas.Add(letra);
            hangmanStages[letrasFallidas.Count - 1].SetActive(true);
        }

        ActualizarUI();

        if (new string(palabraMostrada) == palabra)
        {
            palabraOcultaLabel.text = "¡Ganaste! Era: " + palabra;
        }
        else if (letrasFallidas.Count >= hangmanData.maxFallos) // Usar maxFallos de HangmanData
        {
            palabraOcultaLabel.text = "¡Perdiste! Era: " + palabra;
        }
    }

    void ActualizarUI()
    {
        palabraOcultaLabel.text = string.Join(" ", palabraMostrada);
        letrasFalladasLabel.text = "Fallos: " + string.Join(", ", letrasFallidas);
    }
}
