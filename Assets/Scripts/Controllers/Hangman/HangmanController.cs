using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HangmanController : MonoBehaviour
{
    [SerializeField]
    private GameObject wordContainer; // Contenedor de las letras de la palabra

    [SerializeField]
    private GameObject keyboardContainer; // Contenedor del teclado virtual

    [SerializeField]
    private GameObject letterContainer; // Prefab para las letras de la palabra

    [SerializeField]
    private GameObject[] hangmanStages; // Etapas del ahorcado (partes del cuerpo)

    [SerializeField]
    private GameObject letterButton; // Prefab para los botones del teclado

    [SerializeField]
    private Animator hangmanAnimator; // Referencia al Animator para animaciones

    [SerializeField]
    private AudioSource loseAudioSource; // Sonido de derrota

    [SerializeField]
    private AudioSource winAudioSource; // Sonido de victoria

    [SerializeField]
    private HangmanData hangmanData; // Referencia al ScriptableObject HangmanData
    
    public TMP_Text triesText; // Referencia al texto de intentos
    private string word; // Palabra actual del juego
    private int incorrectGuesses, correctGuesses; // Contadores de aciertos y errores
    private int triesLeft; // Intentos restantes
    private int pointsEarned; // Puntos ganados en el nivel

    void Start()
    {
        InitializeButtons(); // Inicializar los botones del teclado
        InitializeGame(); // Configurar el estado inicial del juego
    }

    private void InitializeButtons()
    {
        // Crear botones para las letras (A-Z)
        for (int i = 65; i <= 90; i++)
        {
            createButton(i);
        }
    }

    private void InitializeGame()
    {
        incorrectGuesses = 0;
        correctGuesses = 0;
        triesLeft = hangmanData.maxFallos; // Usar el número máximo de fallos desde HangmanData
        pointsEarned = hangmanData.maxPoints; // Usar los puntos máximos desde HangmanData
        UpdateTriesUI(); // Actualizar la UI de intentos

        // Habilitar todos los botones del teclado virtual
        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>())
        {
            child.interactable = true; // Hacer que los botones sean interactivos
        }

        // Eliminar todas las letras generadas previamente
        foreach (Transform child in wordContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Reiniciar las etapas del ahorcado
        foreach (GameObject stage in hangmanStages)
        {
            stage.SetActive(false); // Ocultar todas las partes del ahorcado
        }

        // Generar una nueva palabra
        word = GenerateWord().ToUpper();
        foreach (char letter in word)
        {
            var temp = Instantiate(letterContainer, wordContainer.transform); // Crear letras en la UI
        }
    }

    private string GenerateWord()
    {
        // Seleccionar una palabra aleatoria del banco de palabras en HangmanData
        string[] wordList = hangmanData.palabras;
        string line = wordList[Random.Range(0, wordList.Length)];
        return line.Trim().ToUpper(); // Eliminar espacios y convertir a mayúsculas
    }

    void UpdateTriesUI()
    {
        // Actualizar el texto de intentos restantes en la UI
        if (triesText != null)
        {
            triesText.text = $"{triesLeft}/{hangmanData.maxFallos}";
        }
    }

    private void createButton(int i)
    {
        // Crear un botón para cada letra del teclado
        GameObject temp = Instantiate(letterButton, keyboardContainer.transform);
        temp.GetComponentInChildren<TextMeshProUGUI>().text = ((char)i).ToString(); // Asignar la letra al botón
        temp.GetComponent<Button>().onClick.AddListener(delegate { checkLetter(((char)i).ToString()); }); // Asignar evento al botón
    }

    private void checkLetter(string inputLetter)
    {
        // Validar la entrada del jugador
        if (string.IsNullOrEmpty(inputLetter) || inputLetter.Length != 1 || !char.IsLetter(inputLetter[0]))
        {
            Debug.LogWarning("Entrada inválida: " + inputLetter);
            return;
        }

        bool letterInWord = false; // Bandera para verificar si la letra está en la palabra
        TextMeshProUGUI[] letters = wordContainer.GetComponentsInChildren<TextMeshProUGUI>();

        // Verificar si la letra está en la palabra
        for (int i = 0; i < word.Length; i++)
        {
            if (inputLetter == word[i].ToString())
            {
                letterInWord = true;
                correctGuesses++;
                letters[i].text = inputLetter; // Mostrar la letra en la UI
            }
        }

        if (!letterInWord)
        {
            incorrectGuesses++;
            if (incorrectGuesses <= hangmanStages.Length)
            {
                hangmanStages[incorrectGuesses - 1].SetActive(true); // Mostrar la siguiente etapa del ahorcado
                triesLeft--; // Reducir intentos
                UpdateTriesUI(); // Actualizar la UI de intentos

                if (triesLeft <= 0)
                {
                    SceneManager.LoadScene("FailedQuiz"); // Cargar la escena de derrota
                }
            }
        }

        checkOutcome(); // Verificar si el jugador ganó o perdió
    }

    private void checkOutcome()
    {
        TextMeshProUGUI[] letters = wordContainer.GetComponentsInChildren<TextMeshProUGUI>();

        if (correctGuesses == word.Length) // Ganar
        {
            for (int i = 0; i < word.Length; i++)
            {
                letters[i].color = Color.green; // Cambiar el color de las letras a verde
            }

            CompleteLevel(); // Completar el nivel
        }
        else if (incorrectGuesses == hangmanStages.Length) // Perder
        {
            for (int i = 0; i < word.Length; i++)
            {
                letters[i].color = Color.red; // Cambiar el color de las letras a rojo
                letters[i].text = word[i].ToString(); // Mostrar la palabra completa
            }

            if(hangmanAnimator != null)
            {
                hangmanAnimator.SetTrigger("flotar"); // Activar la animación de derrota
            }

            SceneManager.LoadScene("FailedQuiz"); // Cargar la escena de derrota con un retraso

        }
    }

    int CalculateStars()
    {
        // Calcular las estrellas basadas en los intentos restantes
        int remainingTries = hangmanStages.Length - incorrectGuesses;

        if (remainingTries >= 5) // 3 estrellas si quedan 5 o más intentos
            return 3;
        else if (remainingTries >= 3) // 2 estrellas si quedan entre 3-4 intentos
            return 2;
        else if (remainingTries >= 1) // 1 estrella si queda al menos 1 intento
            return 1;
        else
            return 0; // 0 estrellas si no quedan intentos
    }

    void CompleteLevel()
    {
        // Calcular las estrellas ganadas
        int starsEarned = CalculateStars();

        // Obtener el módulo y nivel actual
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        string currentLevelKey = currentModule + "_Lv" + currentLevel;

        // Guardar progreso del nivel actual
        PlayerPrefs.SetInt(currentLevelKey, starsEarned);
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", pointsEarned);
        PlayerPrefs.SetInt("TempCoins", starsEarned * 500);

        // Incrementar el nivel actual para pasar al siguiente nivel
        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt("CurrentLevel", nextLevel);

        // Cargar la escena del siguiente nivel
        if (nextLevel == 3 || nextLevel == 5)
        {
            SceneManager.LoadScene("StageClear"); // Cargar la escena StageClear antes del nivel 3 o 5
        }
        else
        {
            SceneManager.LoadScene("Lecture"); // Escena de nivel completado
        }
    }
}
