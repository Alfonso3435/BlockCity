using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Descripción: Este archivo gestiona la lógica principal del juego de memoria, incluyendo la inicialización del juego, el manejo de intentos, la verificación de coincidencias y la finalización del nivel con la asignación de recompensas.
// Autor: Alfonso Vega e Israel González
public class MemoryGameManager : MonoBehaviour, ICardGameManager
{
    public static MemoryGameManager Instance;
    public Card cardPrefab;
    public Sprite cardBack;
    public Transform cardHolder;
    public TMP_Text triesText; 

    private CardData cardData;
    private List<Card> cards = new List<Card>();
    private List<string> cardContents = new List<string>();

    public Card firstCard, secondCard;
    private int pairsMatched;
    private int totalPairs;
    private int triesLeft = 16;
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
        StartCoroutine(DBQuizReqHolder.Instance.GetMemoryData(DBQuizReqHolder.Instance.GetLevelNumber())); 
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
        triesLeft = 16; 
        pointsEarned = cardData.maxPoints;
        totalPairs = cardData.cardPairs.Length;
        UpdateTriesUI(); 
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

        /*
        foreach (CardData.CardPair pair in cardData.cardPairs)
        {
            cardContents.Add(pair.concept);
            cardContents.Add(pair.definition);
        }
        */
        
        /*
        for (int i = 0; i < 6; i++)
        {
            cardContents.Add("Concepto: " + i);
            cardContents.Add("Definición: " + i);
        }
        */
        for (int i = 0; i < 6; i++)
        {

            cardContents.Add(DBQuizReqHolder.Instance.GetMemoryDataArray()[i].concepto);
            cardContents.Add(DBQuizReqHolder.Instance.GetMemoryDataArray()[i].definicion);
        }

        int k = 0;
        foreach (CardData.CardPair pair in cardData.cardPairs)
        {
            pair.concept = cardContents[k];
            pair.definition = cardContents[k + 1];
            k += 2;
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
        //Debug.Log("firstContent: " + firstContent);
        //Debug.Log("secondContent: " + secondContent);

        
        foreach (CardData.CardPair pair in cardData.cardPairs)
        {
            /*
            Debug.Log("----------------------------------------");
            Debug.Log("firstcontent: " + firstContent);
            Debug.Log("pair.concept: " + pair.concept);
            Debug.Log("secondcontent: " + secondContent);
            Debug.Log("pair.definition: " + pair.definition);
            */
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
            triesLeft--; 
            UpdateTriesUI(); 
            
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

        yield return new WaitForSeconds(0.3f);
    
 
        firstCard.HideCard();
        secondCard.HideCard();
    

        yield return new WaitForSeconds(0.6f);
    
        firstCard = null;
        secondCard = null;
    }

        void CompleteLevel()
    {
        int triviaMissionId = 2; 
        DBQuizReqHolder.Instance.StartCoroutine(
        DBQuizReqHolder.Instance.IncrementQuestProgress(triviaMissionId)
        );
        //Debug.Log("Incrementing trivia mission progress");
        
        string missionID = "2";
        int currentProgress = PlayerPrefs.GetInt($"Mission_{missionID}_Progress", 0);
        PlayerPrefs.SetInt($"Mission_{missionID}_Progress", currentProgress + 1);

        int starsEarned = CalculateStars();
        
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        string currentLevelKey = currentModule + "_Lv" + currentLevel;


        PlayerPrefs.SetInt(currentLevelKey, starsEarned);
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", pointsEarned);
        PlayerPrefs.SetInt("TempCoins", starsEarned * 200);

        StartCoroutine(
            DBQuizReqHolder.Instance.UpdateQuizUsuario(
                DBQuizReqHolder.Instance.GetLevelNumber(), 
                DBQuizReqHolder.Instance.GetUserID(), 
                1,
                starsEarned,
                pointsEarned, 
                starsEarned * 200 
            ));

        Debug.Log("=============================");
        StartCoroutine(DBQuizReqHolder.Instance.UpdateCoins(
            DBQuizReqHolder.Instance.GetUserID(),
            starsEarned * 200)); 

 
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


        PlayerPrefs.SetInt(currentLevelKey, starsEarned);
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", pointsEarned);
        PlayerPrefs.SetInt("TempCoins", starsEarned * 500);


        int nextLevel = currentLevel + 1;
        PlayerPrefs.SetInt("UnlockedLevel", nextLevel); 


        SceneManager.LoadScene("LevelSelection");
    }
*/
    int CalculateStars()
    {
        int remainingTries = triesLeft;
        
        if (remainingTries >= 10) 
            return 3;
        else if (remainingTries >= 5) 
            return 2;
        else if (remainingTries >= 1) 
            return 1;
        else
            return 0; 
    }

    public void RestartGame()
    {
        InitializeGame();
    }
}