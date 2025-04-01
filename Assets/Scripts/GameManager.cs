using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Question Settings")]
    private QuestionData selectedCategory;
    private int currentQuestionIndex;
    private int wrongAnswersCount = 0;
    private const int MAX_WRONG_ANSWERS = 3;
    private bool hasShownTipForCurrentQuestion = false;

    [Header("UI References")]
    public GameObject triviaGameObject;
    public TMP_Text questionText;
    public Button[] replyButtons;

    [Header("Feedback Panel")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackTitle;
    public TMP_Text feedbackExplanation;
    public Button continueButton;

    [Header("Tip UI")]
    public GameObject tipPanel;
    public TMP_Text tipText;

    [Header("Scoring")]
    public int pointsReductionPerMistake = 200;
    private int totalPointsEarned = 0;
    private int currentQuestionMaxPoints = 0;
    private int totalQuestions = 0;
    private int currentLevelNumber;
    private string currentLevelName;

    [Header("Feedback Colors")]
    public Color normalButtonColor = Color.white;
    public Color wrongAnswerColor = Color.red;
    public Color wrongAnswerTextColor = Color.white;
    public Color normalTextColor = Color.black; // Color normal del texto
    public float flashDuration = 1f;

    void Start()
    {
        currentLevelNumber = QuizDataHolder.Instance.GetLevelNumber();
        currentLevelName = PlayerPrefs.GetString("CurrentLevelName");
        selectedCategory = QuizDataHolder.Instance.GetQuizData();
        totalQuestions = selectedCategory.questions.Length;
        currentQuestionMaxPoints = selectedCategory.maxPoints / totalQuestions;
        
        InitializeGame();
    }

    void InitializeGame()
    {
        for (int i = 0; i < replyButtons.Length; i++)
        {
            int index = i;
            replyButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }

        continueButton.onClick.AddListener(ContinueAfterCorrectAnswer);
        feedbackPanel.SetActive(false);
        tipPanel.SetActive(false);
        triviaGameObject.SetActive(true);
        
        DisplayCurrentQuestion();
    }

    void DisplayCurrentQuestion()
    {
        ResetQuestionState();

        if (currentQuestionIndex >= selectedCategory.questions.Length)
        {
            CompleteQuiz();
            return;
        }

        Question currentQuestion = selectedCategory.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;

        for (int i = 0; i < replyButtons.Length; i++)
        {
            bool shouldActivate = i < currentQuestion.replies.Length;
            replyButtons[i].gameObject.SetActive(shouldActivate);
            
            if (shouldActivate)
            {
                replyButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.replies[i];
                // Restablecer colores iniciales
                replyButtons[i].GetComponent<Image>().color = normalButtonColor;
                replyButtons[i].GetComponentInChildren<TMP_Text>().color = normalTextColor;
            }
        }
    }

    void ResetQuestionState()
    {
        hasShownTipForCurrentQuestion = false;
        tipPanel.SetActive(false);
        wrongAnswersCount = 0;
    }

    void CheckAnswer(int answerIndex)
    {
        Question currentQuestion = selectedCategory.questions[currentQuestionIndex];
        bool isCorrect = answerIndex == currentQuestion.correctReplyIndex;

        if (isCorrect)
        {
            int pointsEarned = Mathf.Max(0, currentQuestionMaxPoints - (pointsReductionPerMistake * wrongAnswersCount));
            totalPointsEarned += pointsEarned;
            ShowFeedbackForCorrectAnswer(currentQuestion, pointsEarned);
        }
        else
        {
            StartCoroutine(FlashWrongAnswerButton(answerIndex));
            HandleWrongAnswer(currentQuestion);
        }
    }

    IEnumerator FlashWrongAnswerButton(int buttonIndex)
    {
        Button wrongButton = replyButtons[buttonIndex];
        Image buttonImage = wrongButton.GetComponent<Image>();
        TMP_Text buttonText = wrongButton.GetComponentInChildren<TMP_Text>();
        
        // Guardar los colores originales
        Color originalButtonColor = buttonImage.color;
        Color originalTextColor = buttonText.color;
        
        // Cambiar a colores de feedback
        buttonImage.color = wrongAnswerColor;
        buttonText.color = wrongAnswerTextColor;
        
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(flashDuration);
        
        // Restaurar colores originales
        buttonImage.color = originalButtonColor;
        buttonText.color = originalTextColor;
    }

    void ShowFeedbackForCorrectAnswer(Question question, int pointsEarned)
    {
        triviaGameObject.SetActive(false);
        feedbackPanel.SetActive(true);
        tipPanel.SetActive(false);

        GameObject.Find("CorrectEffect").GetComponent<AudioSource>().Play();
        
        feedbackTitle.text = $"Correct! +{pointsEarned} points";
        feedbackExplanation.text = question.explanation;
    }

    void HandleWrongAnswer(Question currentQuestion)
    {
        GameObject.Find("IncorrectEffect").GetComponent<AudioSource>().Play();
        wrongAnswersCount++;
        
        if (wrongAnswersCount == 1 && !hasShownTipForCurrentQuestion && !string.IsNullOrEmpty(currentQuestion.tip))
        {
            ShowTip(currentQuestion.tip);
            hasShownTipForCurrentQuestion = true;
        }
        
        if (wrongAnswersCount >= MAX_WRONG_ANSWERS)
        {
            SceneManager.LoadScene("FailedQuiz");
        }
    }

    void ShowTip(string tip)
    {
        tipText.text = tip;
        tipPanel.SetActive(true);
    }

    void ContinueAfterCorrectAnswer()
    {
        feedbackPanel.SetActive(false);
        triviaGameObject.SetActive(true);
        
        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    void CompleteQuiz()
    {
        float percentage = (float)totalPointsEarned / selectedCategory.maxPoints * 100f;
        
        int starsEarned = 0;
        if (percentage >= 90f) starsEarned = 3;
        else if (percentage >= 50f) starsEarned = 2;
        else if (percentage >= 20f) starsEarned = 1;
        
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        PlayerPrefs.SetInt(currentModule + "_Lv" + currentLevelName, starsEarned);
        
        PlayerPrefs.SetInt("TempStars", starsEarned);
        PlayerPrefs.SetInt("TempPoints", totalPointsEarned);
        PlayerPrefs.SetInt("TempCoins", starsEarned * 500);
        
        SceneManager.LoadScene("StageClear");
    }

    public void RestartQuiz()
    {
        currentQuestionIndex = 0;
        totalPointsEarned = 0;
        wrongAnswersCount = 0;
        ResetQuestionState();
        DisplayCurrentQuestion();
    }
}