using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            HandleWrongAnswer(currentQuestion);
        }
    }

    void ShowFeedbackForCorrectAnswer(Question question, int pointsEarned)
    {
        triviaGameObject.SetActive(false);
        feedbackPanel.SetActive(true);
        tipPanel.SetActive(false);
        
        feedbackTitle.text = $"Correct! +{pointsEarned} points";
        feedbackExplanation.text = question.explanation;
    }

    void HandleWrongAnswer(Question currentQuestion)
    {
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