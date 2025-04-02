using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Collections;

public class GameManagerCard : MonoBehaviour
{
    public static GameManagerCard Instance;

    public Card cardPrefab;
    public Sprite cardBack;
    public Transform cardHolder;

    [System.Serializable]
    public class CardPair
    {
        public string concept;
        public string definition;
    }

    public List<CardPair> cardPairs = new List<CardPair>();
    private List<Card> cards = new List<Card>();
    private List<string> cardContents = new List<string>();

    public Card firstCard, secondCard;
    private int pairsMatched;
    private int totalPairs;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        pairsMatched = 0;
        totalPairs = cardPairs.Count;
        CreateCards();
    }

    void CreateCards()
    {
        // Limpiar cartas existentes
        foreach (Transform child in cardHolder)
        {
            Destroy(child.gameObject);
        }
        cards.Clear();
        cardContents.Clear();

        // Crear lista de contenidos (todos los conceptos y definiciones)
        foreach (CardPair pair in cardPairs)
        {
            cardContents.Add(pair.concept);
            cardContents.Add(pair.definition);
        }

        // Verificar que tenemos pares completos
        if (cardContents.Count % 2 != 0)
        {
            Debug.LogError("Número impar de cartas. Verifica tus pares concepto-definición.");
            return;
        }

        // Mezclar las cartas
        ShuffleCards();

        // Crear las cartas físicas
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
        // Mezcla Fisher-Yates mejorada
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
        
        // Buscar si forman un par válido
        foreach (CardPair pair in cardPairs)
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
            pairsMatched++;
            if (pairsMatched == totalPairs)
            {
                LevelFinished();
            }
            // Deshabilitar cartas emparejadas
            firstCard.GetComponent<UnityEngine.UI.Button>().interactable = false;
            secondCard.GetComponent<UnityEngine.UI.Button>().interactable = false;
            firstCard = null;
            secondCard = null;
        }
        else
        {
            StartCoroutine(FlipBackCards());
        }
    }

    IEnumerator FlipBackCards()
    {
        yield return new WaitForSeconds(1f);
        firstCard.HideCard();
        secondCard.HideCard();
        firstCard = null;
        secondCard = null;
    }

    void LevelFinished()
    {
        Debug.Log("¡Nivel completado!");
    }

    public void RestartGame()
    {
        pairsMatched = 0;
        InitializeGame();
    }
}