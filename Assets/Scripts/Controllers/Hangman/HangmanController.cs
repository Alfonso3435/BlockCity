using UnityEngine;
using UnityEngine.UI; // Asegúrate de usar UnityEngine.UI para botones interactivos
using System.Collections.Generic;
using TMPro;

public class HangmanController : MonoBehaviour
{
    [SerializeField]
    private GameObject wordContainer;

    [SerializeField]
    private GameObject keyboardContainer; // Contenedor del teclado virtual

    [SerializeField]
    private GameObject letterContainer; // Contenedor de letras para la palabra

    [SerializeField]
    private GameObject[] hangmanStages; // Etapas  

    [SerializeField]
    private GameObject letterButton;

    [SerializeField]
    private TextAsset possibleWord;

    private string word;
    private int incorrectGuesses, correctGuesses;

    void Start()
    {
        InitializeButtons();
        InitializeGame();
    }

    private void InitializeButtons()
    {
        for (int i = 65; i <= 90; i++)
        {
            createButton(i);
        }
    }

    private void InitializeGame() // Reset data to original state
    {
        incorrectGuesses = 0;
        correctGuesses = 0;

        // Habilitar todos los botones del teclado virtual
        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>())
        {
            child.interactable = true; // Asegúrate de que interactable pertenece a UnityEngine.UI.Button
        }

        // Eliminar todas las letras generadas previamente
        foreach (Transform child in wordContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Reiniciar las etapas del ahorcado
        foreach (GameObject stage in hangmanStages)
        {
            stage.SetActive(false);
        }

        // Generar una nueva palabra
        word = generateWord().ToUpper();
        foreach (char letter in word)
        {
            var temp = Instantiate(letterContainer, wordContainer.transform);
        }
    }

    private void createButton(int i)
    {
        GameObject temp = Instantiate(letterButton, keyboardContainer.transform);
        temp.GetComponentInChildren<TextMeshProUGUI>().text = ((char)i).ToString();
        temp.GetComponent<Button>().onClick.AddListener(delegate { checkLetter(((char)i).ToString()); });
    }

    private string generateWord()
    {
        string[] wordList = possibleWord.text.Split("\n");
        string line = wordList[Random.Range(0, wordList.Length)];
        return line.Trim().ToUpper(); // Elimina espacios y convierte a mayúsculas
    }

    private void checkLetter(string inputLetter)
    {
        if (string.IsNullOrEmpty(inputLetter) || inputLetter.Length != 1 || !char.IsLetter(inputLetter[0]))
        {
            Debug.LogWarning("Entrada inválida: " + inputLetter);
            return;
        }

        bool letterInWord = false;
        TextMeshProUGUI[] letters = wordContainer.GetComponentsInChildren<TextMeshProUGUI>();

        for (int i = 0; i < word.Length; i++)
        {
            if (inputLetter == word[i].ToString())
            {
                letterInWord = true;
                correctGuesses++;
                letters[i].text = inputLetter;
            }
        }

        if (!letterInWord)
        {
            incorrectGuesses++;
            if (incorrectGuesses <= hangmanStages.Length)
            {
                hangmanStages[incorrectGuesses - 1].SetActive(true);
            }
        }

        checkOutcome();
    }

    private void checkOutcome()
    {
        TextMeshProUGUI[] letters = wordContainer.GetComponentsInChildren<TextMeshProUGUI>();

        if (correctGuesses == word.Length) // Ganar
        {
            for (int i = 0; i < word.Length; i++)
            {
                letters[i].color = Color.green;
            }
            Invoke("InitializeGame", 2f);
        }
        else if (incorrectGuesses == hangmanStages.Length) // Perder
        {
            for (int i = 0; i < word.Length; i++)
            {
                letters[i].color = Color.red;
                letters[i].text = word[i].ToString();
            }
            Invoke("InitializeGame", 2f);
        }
    }
}
