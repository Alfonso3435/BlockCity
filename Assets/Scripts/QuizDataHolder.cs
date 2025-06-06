using UnityEngine;

// Descripción: Este archivo actúa como un contenedor de datos global para el juego, almacenando y gestionando los datos actuales de cuestionarios, tarjetas, ahorcado, tipo de juego y nivel, asegurando su persistencia entre escenas.
// Autor: Alfonso Vega y Estrella Lolbeth

public class QuizDataHolder : MonoBehaviour
{
    public static QuizDataHolder Instance;
    
    private QuestionData currentQuizData;
    private CardData currentCardData;
    private HangmanData currentHangmanData;
    private int currentLevelNumber;
    private string currentGameType; // "quiz" o "memory"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetQuizData(QuestionData data)
    {
        currentQuizData = data;
        currentCardData = null;
        currentGameType = "quiz";
    }

    public void SetCardData(CardData data)
    {
        currentCardData = data;
        currentQuizData = null;
        currentGameType = "memory";
    }

    public void SetHangmanData(HangmanData data)
    {
        currentHangmanData = data;
        currentQuizData = null;
        currentGameType = "hangman";
    }

    public QuestionData GetQuizData()
    {
        return currentQuizData;
    }

    public CardData GetCardData()
    {
        return currentCardData;
    }

    public HangmanData GetHangmanData()
    {
        return currentHangmanData;
    }


    public string GetCurrentGameType()
    {
        return currentGameType;
    }

    public void SetLevelNumber(int levelNum)
    {
        currentLevelNumber = levelNum;
    }

    public int GetLevelNumber()
    {
        return currentLevelNumber;
    }

    public void ResetData()
    {
        currentQuizData = null;
        currentCardData = null;
        currentLevelNumber = 0;
        currentGameType = "";
    }
}