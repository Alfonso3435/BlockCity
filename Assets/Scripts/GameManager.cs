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
    public TMP_Text currentQuestionText;
    public Button[] replyButtons;
    public TMP_Text ShieldText;
    public TMP_Text PotionText;
    public TMP_Text HeartText;

    [Header("Feedback Panel")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackTitle;
    public TMP_Text feedbackExplanation;
    public Button continueButton;

    [Header("Tip UI")]
    public GameObject tipPanel;
    public TMP_Text tipText;

    [Header("Scoring")]
    private int totalPointsEarned = 0;
    private int currentQuestionMaxPoints = 0;
    private int currentLevelNumber;
    private string currentLevelName;
    private int lives = 3;
    private int quizErrors = 0;

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
        currentQuestionMaxPoints = 3000;
        
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

        if (currentQuestionIndex >= GetNumberOfQuestions()/*selectedCategory.questions.Length*/)
        {
            CompleteQuiz();
            return;
        }
        // Conectar estos 3 a la base de datos
        ShieldText.text = PlayerPrefs.GetInt("Shield").ToString();
        PotionText.text = PlayerPrefs.GetInt("Potion").ToString();
        HeartText.text = PlayerPrefs.GetInt("Heart").ToString();

        currentQuestionText.text = (currentQuestionIndex + 1) + "/" + GetNumberOfQuestions();
        HeartText.text = (lives - quizErrors).ToString();
        
        Debug.Log(currentQuestionIndex);
        Question currentQuestion = selectedCategory.questions[currentQuestionIndex];
        //questionText.text = currentQuestion.questionText; // INFO: Este es el texto de la pregunta
        questionText.text = DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].pregunta;

        
        for (int i = 0; i < replyButtons.Length; i++)
        {
            bool shouldActivate = i < currentQuestion.replies.Length;
            replyButtons[i].gameObject.SetActive(shouldActivate);
            
            if (shouldActivate)
            {
                //replyButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.replies[i]; // INFO: Este es el texto de la respuesta
                replyButtons[i].GetComponentInChildren<TMP_Text>().text = DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].respuestas[i].respuesta;
                
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
        //bool isCorrect = answerIndex == currentQuestion.correctReplyIndex; // INFO: Este es el índice de la respuesta correcta
        bool isCorrect = answerIndex == DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].indice_correcto;
        Debug.Log("Correct answer index: " + DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].indice_correcto);
        if (isCorrect)
        {
            //int pointsEarned = currentQuestionMaxPoints - (pointsReductionPerMistake * wrongAnswersCount);
            int pointsEarned = (currentQuestionMaxPoints / GetNumberOfQuestions()) - 
            ((currentQuestionMaxPoints / GetNumberOfQuestions()) / 3) * wrongAnswersCount; // Elgi 3 pensando que es la cantidad de vidas, pero esto es arbitrario
            totalPointsEarned += pointsEarned;
            ShowFeedbackForCorrectAnswer(currentQuestion, pointsEarned);
        }
        else
        {
            //totalPointsEarned -= (currentQuestionMaxPoints / GetNumberOfQuestions()) / 3;
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
        //feedbackExplanation.text = question.explanation;
        feedbackExplanation.text = DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].explicacion;
    }

    void HandleWrongAnswer(Question currentQuestion)
    {
        GameObject.Find("IncorrectEffect").GetComponent<AudioSource>().Play();
        wrongAnswersCount++;
        quizErrors++;

        HeartText.text = (lives - quizErrors).ToString();
        
        if (wrongAnswersCount == 1 && !hasShownTipForCurrentQuestion && !string.IsNullOrEmpty(currentQuestion.tip))
        {
            //ShowTip(currentQuestion.tip);
            ShowTip(DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].tip);
            hasShownTipForCurrentQuestion = true;
        }
        
        if (quizErrors >= 3) // I chose 5 as it is the initial amount of lives, but this can be changed
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
        string missionID = "2";
        int currentProgress = PlayerPrefs.GetInt($"Mission_{missionID}_Progress", 0);
        PlayerPrefs.SetInt($"Mission_{missionID}_Progress", currentProgress + 1);

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

    int GetNumberOfQuestions()
    {
        if (DBQuizReqHolder.Instance.GetPreguntas() != null)
        {
            return DBQuizReqHolder.Instance.GetPreguntas().Length;
        }
        else
        {
            Debug.LogError("No questions available in DBQuizReqHolder.");
            return 0;
        }
    }
}