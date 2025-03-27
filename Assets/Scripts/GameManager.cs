using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Question Settings")]
    public QuestionData[] categories;
    private QuestionData selectedCategory;
    private int currentQuestionIndex;
    private int wrongAnswersCount = 0;
    private const int MAX_WRONG_ANSWERS = 3;

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

    [Header("Scene Names")]
    public string stageClearScene = "StageClear";
    public string failedQuizScene = "FailedQuiz";

    private bool isShowingFeedback = false;
    private bool hasShownTipForCurrentQuestion = false;

    void Start()
    {
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

        if (categories.Length > 0)
        {
            SelectCategory(0);
        }

        feedbackPanel.SetActive(false);
        tipPanel.SetActive(false);
        triviaGameObject.SetActive(true);
    }

    public void SelectCategory(int categoryIndex)
    {
        selectedCategory = categories[categoryIndex];
        currentQuestionIndex = 0;
        wrongAnswersCount = 0;
        ResetQuestionState();
        DisplayCurrentQuestion();
    }

    void DisplayCurrentQuestion()
    {
        ResetQuestionState();

        if (currentQuestionIndex >= selectedCategory.questions.Length)
        {
            SceneManager.LoadScene(stageClearScene);
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
            ShowFeedbackForCorrectAnswer(currentQuestion);
        }
        else
        {
            HandleWrongAnswer(currentQuestion);
        }
    }

    void ShowFeedbackForCorrectAnswer(Question question)
    {
        isShowingFeedback = true;
        triviaGameObject.SetActive(false);
        feedbackPanel.SetActive(true);
        tipPanel.SetActive(false);
        
        feedbackExplanation.text = question.explanation;
    }

    void HandleWrongAnswer(Question currentQuestion)
    {
        wrongAnswersCount++;
        
        // Mostrar tip solo si es el primer error y no se ha mostrado aún
        if (wrongAnswersCount == 1 && !hasShownTipForCurrentQuestion)
        {
            ShowTip(currentQuestion.tip);
            hasShownTipForCurrentQuestion = true;
        }
        
        if (wrongAnswersCount >= MAX_WRONG_ANSWERS)
        {
            SceneManager.LoadScene(failedQuizScene);
        }
    }

    void ShowTip(string tip)
    {
        if (!string.IsNullOrEmpty(tip))
        {
            tipText.text = tip;
            tipPanel.SetActive(true);
        }
    }

    void ContinueAfterCorrectAnswer()
    {
        isShowingFeedback = false;
        feedbackPanel.SetActive(false);
        triviaGameObject.SetActive(true);
        
        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    public void RestartQuiz()
    {
        currentQuestionIndex = 0;
        ResetQuestionState();
        DisplayCurrentQuestion();
    }
}