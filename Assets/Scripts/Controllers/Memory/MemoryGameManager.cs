using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour, ICardGameManager
{
    public static MemoryGameManager Instance;

    public Card cardPrefab;
    public Sprite cardBack;
    public Transform cardHolder;
    public TMP_Text triesText; // Referencia al texto de intentos

    private CardData cardData;
    private List<Card> cards = new List<Card>();
    private List<string> cardContents = new List<string>();

    public Card firstCard, secondCard;
    private int pairsMatched;
    private int totalPairs;
    private int triesLeft = 16; // Inicializamos con 16 intentos
    private int pointsEarned = 0;

    [Header("UI")]
    public Button backButton;

    private void Awake()
    {
        Instance = this;
        cardData = QuizDataHolder.Instance.GetCardData();
        if (backButton != null)
        {
            backButton.onClick.AddListener(ReturnToModuleSelection);
        }
    }

    public void ReturnToModuleSelection()
    {        
        SceneManager.LoadScene("ModuleSelection");
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        pairsMatched = 0;
        triesLeft = 16; // Resetear intentos al iniciar
        pointsEarned = cardData.maxPoints;
        totalPairs = cardData.cardPairs.Length;
        UpdateTriesUI(); // Actualizar UI al inicio
        CreateCards();
    }

    void UpdateTriesUI()
    {
        if (triesText != null)
        {
            triesText.text = $"{triesLeft}/16";
        }
    }

    void CreateCards()
    {
        foreach (Transform child in cardHolder)
        {
            Destroy(child.gameObject);
        }
        cards.Clear();
        cardContents.Clear();

        foreach (CardData.CardPair pair in cardData.cardPairs)
        {
            cardContents.Add(pair.concept);
            cardContents.Add(pair.definition);
        }

        if (cardContents.Count % 2 != 0)
        {
            Debug.LogError("Número impar de cartas. Verifica tus pares concepto-definición.");
            return;
        }

        ShuffleCards();

        for (int i = 0; i < cardContents.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, cardHolder);
            newCard.gameManager = this;
            newCard.SetContent(cardContents[i]);
            cards.Add(newCard);
        }
    }

    void ShuffleCards()
    {
        for (int i = cardContents.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = cardContents[i];
            cardContents[i] = cardContents[randomIndex];
            cardContents[randomIndex] = temp;
        }
    }

    public void CardFlipped(Card flippedCard)
    {
        if (firstCard == null)
        {
            firstCard = flippedCard;
        }
        else if (secondCard == null && flippedCard != firstCard)
        {
            secondCard = flippedCard;
            CheckMatch();
        }
    }

    void CheckMatch()
    {
        bool isMatch = false;
        string firstContent = firstCard.GetComponentInChildren<TextMeshProUGUI>().text;
        string secondContent = secondCard.GetComponentInChildren<TextMeshProUGUI>().text;
        
        foreach (CardData.CardPair pair in cardData.cardPairs)
        {
            if ((firstContent == pair.concept && secondContent == pair.definition) ||
                (firstContent == pair.definition && secondContent == pair.concept))
            {
                isMatch = true;
                break;
            }
        }

        if (isMatch)
        {
            GameObject.Find("CorrectEffect").GetComponent<AudioSource>().Play();
            pairsMatched++;
            if (pairsMatched == totalPairs)
            {
                CompleteLevel();
            }
            firstCard.GetComponent<UnityEngine.UI.Button>().interactable = false;
            secondCard.GetComponent<UnityEngine.UI.Button>().interactable = false;
            firstCard = null;
            secondCard = null;
        }
        else
        {
            triesLeft--; // Reducir intentos
            UpdateTriesUI(); // Actualizar UI
            
            if (triesLeft <= 0)
            {
                SceneManager.LoadScene("FailedQuiz");
            }
            else
            {
                StartCoroutine(FlipBackCards());
            }
        }
    }

    IEnumerator FlipBackCards()
    {
    // Esperar antes de empezar a voltear
        yield return new WaitForSeconds(0.3f);
    
    // Voltear ambas cartas
        firstCard.HideCard();
        secondCard.HideCard();
    
    // Esperar a que termine la animación (aproximadamente 0.6s)
        yield return new WaitForSeconds(0.6f);
    
        firstCard = null;
        secondCard = null;
    }

        void CompleteLevel()
    {
        int starsEarned = CalculateStars();
        
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

        SceneManager.LoadScene("StageClear");

        /*
        // Cargar la escena del siguiente nivel (nivel 3)
        if (nextLevel == 3)
        {
            SceneManager.LoadScene("Lecture"); // Cargar la escena Lecture antes del nivel 3
        }
        else
        {
            SceneManager.LoadScene("StageClear"); // Escena de nivel completado
        }
        */
    }
    /*Solo es temporal
    void CompleteLevel()
    {
        int starsEarned = CalculateStars();
        
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        string currentLevelKey = currentModule + "_Lv" + currentLevel;

        // Guardar progreso del nivel actual
        PlayerPrefs.SetInt(currentLevelKey, starsEarned);
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", pointsEarned);
        PlayerPrefs.SetInt("TempCoins", starsEarned * 500);

        // Desbloquear el siguiente nivel
        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt("UnlockedLevel", nextLevel); // Guardar el nivel desbloqueado

        // Regresar a la pantalla de selección de niveles
        SceneManager.LoadScene("LevelSelection");
    }
*/
    int CalculateStars()
    {
        int remainingTries = triesLeft;
        
        if (remainingTries >= 10) // 3 estrellas si quedan 10 o más intentos
            return 3;
        else if (remainingTries >= 5) // 2 estrellas si quedan entre 5-9 intentos
            return 2;
        else if (remainingTries >= 1) // 1 estrella si quedan entre 1-4 intentos
            return 1;
        else
            return 0; // No debería llegar aquí porque se habría cargado FailedQuiz
    }

    public void RestartGame()
    {
        InitializeGame();
    }
}