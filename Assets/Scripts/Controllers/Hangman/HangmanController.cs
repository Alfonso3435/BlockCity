using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class HangmanController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text ValTries;
    [SerializeField] private Transform WordContainer;
    [SerializeField] private TMP_Text Question;
    [SerializeField] private Transform KeyboardContainer;
    [SerializeField] private GameObject[] hangmanStages;

    [SerializeField] private Button Regreso;
    [SerializeField] private TMP_Text descConteo;
    [SerializeField] private TMP_Text conteoText;

    private int conteo = 1;

    [Header("Prefabs")]
    [SerializeField] private GameObject letterPrefab;
    [SerializeField] private GameObject keyboardButtonPrefab;

    [Header("Game Settings")]
    [SerializeField] private int quizId = 5;
    [SerializeField] private int basePoints = 1000;
    [SerializeField] private int attemptsPerGame = 6;
    [SerializeField] private int totalAttemptsAllowed = 42;

    private string targetWord;
    private string currentWordState;
    private int remainingTotalAttempts;
    private int remainingGameAttempts;
    private int currentErrors = 0;
    private bool gameActive = false;
    private List<HangmanItem> availableWords;
    private List<HangmanItem> completedWords = new List<HangmanItem>();

    private void Awake()
    {
        Regreso.onClick.AddListener(ReturnToModuleSelection);
    }

    private IEnumerator Start()
    {
        remainingTotalAttempts = totalAttemptsAllowed;
        yield return StartCoroutine(InitializeGame());
        conteo=1;
        conteoText.text =conteo.ToString();
    }

    private IEnumerator InitializeGame()
    {
        ClearGameUI();

        yield return StartCoroutine(DBQuizReqHolder.Instance.GetHangmanData(
            quizId,
            (success, hangmanData) => {
                if (success && hangmanData != null && hangmanData.Length > 0)
                {
                    availableWords = new List<HangmanItem>(hangmanData);
                    StartNextWord();
                }
                else
                {
                    Debug.LogError("Error loading hangman data");
                    // Mostrar mensaje de error
                }
            }
        ));
    }

    private void StartNextWord()
    {
        if (availableWords.Count == 0)
        {
            CompleteAllWords();
            return;
        }

        // Reiniciar contadores para el nuevo minijuego
        remainingGameAttempts = attemptsPerGame;
        currentErrors = 0;
        UpdateTriesDisplay();
        ClearGameUI();
        conteo++;
        conteoText.text =conteo.ToString();

        // Seleccionar palabra aleatoria de las disponibles
        int randomIndex = Random.Range(0, availableWords.Count);
        var selectedItem = availableWords[randomIndex];
        availableWords.RemoveAt(randomIndex);

        targetWord = selectedItem.word.ToUpper().Trim();
        Question.text = selectedItem.definition.Trim();
        currentWordState = new string('_', targetWord.Length);

        CreateWordDisplay();
        CreateKeyboard();

        gameActive = true;
        Debug.Log($"Current word: {targetWord}");
    }

    private void ClearGameUI()
    {
        // Limpiar letras y teclado
        foreach (Transform child in WordContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in KeyboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Reiniciar visualización del ahorcado
        foreach (var stage in hangmanStages)
        {
            stage.SetActive(false);
        }
    }

    private void CreateWordDisplay()
    {
        foreach (char _ in targetWord)
        {
            var letterObj = Instantiate(letterPrefab, WordContainer);
            letterObj.GetComponentInChildren<TMP_Text>().text = "_";
        }
    }

    private void CreateKeyboard()
    {
        for (char c = 'A'; c <= 'Z'; c++)
        {
            var buttonObj = Instantiate(keyboardButtonPrefab, KeyboardContainer);
            buttonObj.GetComponentInChildren<TMP_Text>().text = c.ToString();
            
            var button = buttonObj.GetComponent<Button>();
            char currentChar = c;
            button.onClick.AddListener(() => OnLetterGuessed(currentChar));
        }
    }

    private void OnLetterGuessed(char guessedLetter)
    {
        if (!gameActive) return;

        bool correctGuess = false;
        char[] wordState = currentWordState.ToCharArray();

        for (int i = 0; i < targetWord.Length; i++)
        {
            if (targetWord[i] == guessedLetter)
            {
                wordState[i] = guessedLetter;
                correctGuess = true;
                WordContainer.GetChild(i).GetComponentInChildren<TMP_Text>().text = guessedLetter.ToString();
            }
        }

        if (!correctGuess)
        {
            remainingGameAttempts--;
            remainingTotalAttempts--;
            currentErrors++;
            UpdateHangmanVisual();
        }

        currentWordState = new string(wordState);
        CheckGameState();
    }

    private void UpdateHangmanVisual()
    {
        // Mostrar solo las partes correspondientes a los errores actuales
        for (int i = 0; i < hangmanStages.Length; i++)
        {
            hangmanStages[i].SetActive(i < currentErrors);
        }

        UpdateTriesDisplay();
    }

    private void UpdateTriesDisplay()
    {
        ValTries.text = remainingGameAttempts.ToString();
    }

    private void CheckGameState()
    {
        if (!currentWordState.Contains('_'))
        {
            // Palabra completada
            GameObject.Find("CorrectEffect").GetComponent<AudioSource>().Play();
            StartCoroutine(WordCompleted());
        }
        else if (remainingGameAttempts <= 0)
        {
            // Sin intentos restantes para este minijuego
            SceneManager.LoadScene("FailedQuiz");
        }
    }

    private IEnumerator WordCompleted()
    {
        gameActive = false;
        yield return new WaitForSeconds(1f); // Breve pausa para el efecto de sonido
        
        // Pasar a la siguiente palabra
        StartNextWord();
    }

    private void CompleteAllWords()
    {
        // Calcular estrellas basadas en errores totales
        int starsEarned = CalculateStars();
        int coinsEarned = starsEarned * 150;

        // Guardar progreso
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        string levelKey = $"{currentModule}_Lv{currentLevel}";

        PlayerPrefs.SetInt(levelKey, starsEarned);
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", basePoints);
        PlayerPrefs.SetInt("TempCoins", coinsEarned);

        // Actualizar base de datos
        DBQuizReqHolder.Instance.StartCoroutine(
            DBQuizReqHolder.Instance.UpdateCoins(
                DBQuizReqHolder.Instance.GetUserID(),
                coinsEarned
            )
        );

        DBQuizReqHolder.Instance.StartCoroutine(
            DBQuizReqHolder.Instance.UpdateQuizUsuario(
                DBQuizReqHolder.Instance.GetLevelNumber(),
                DBQuizReqHolder.Instance.GetUserID(),
                1,
                starsEarned,
                3000,
                1
            )
        );


        SceneManager.LoadScene("StageClear");
    }

    private int CalculateStars()
    {
        float errorPercentage = (float)(totalAttemptsAllowed - remainingTotalAttempts) / totalAttemptsAllowed;
        
        if (errorPercentage <= 0.1f) return 3;  // 10% o menos errores
        if (errorPercentage <= 0.3f) return 2;  // 30% o menos errores
        if (errorPercentage <= 0.5f) return 1;  // 50% o menos errores
        
        return 0; // Más del 50% de errores
    }
    
    public void ReturnToModuleSelection()
    {
        SceneManager.LoadScene("LevelSelection1");
    }
}