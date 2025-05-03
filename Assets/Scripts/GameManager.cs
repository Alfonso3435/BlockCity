using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Data.Common;

// Descripción: Este archivo gestiona la lógica principal del juego de trivia, incluyendo la inicialización del juego, manejo de preguntas, uso de potenciadores, cálculo de puntuaciones y transición entre escenas según el progreso del jugador.
// Autor: Alfonso Vega e Israel González

public class GameManager : MonoBehaviour
{

    private int shieldCount = DBQuizReqHolder.Instance.GetShieldCount();
    private int potionCount = DBQuizReqHolder.Instance.GetPotionCount();

    public GameObject PotionPopUp;
    public Button confirmPotionButton;
    public TMP_Text rejectPotionText;


    public GameObject ShieldPopUp;
    public Button confirmShieldButton;
    public TMP_Text rejectShieldText;


    private bool isShieldActive = false;
    private bool isPotionActive = false;

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
    public Color normalTextColor = Color.black;
    public float flashDuration = 1f;

    void Start()
    {
        StartCoroutine(InitializeGameWithItems());
    }

    private IEnumerator InitializeGameWithItems()
    {
        currentLevelNumber = DBQuizReqHolder.Instance.GetLevelNumber();
        Debug.Log("Current Level Number: " + currentLevelNumber);
        currentLevelName = PlayerPrefs.GetString("CurrentLevelName");
        selectedCategory = QuizDataHolder.Instance.GetQuizData();
        currentQuestionMaxPoints = 3000;
        rejectPotionText.gameObject.SetActive(false);
        rejectShieldText.gameObject.SetActive(false);

        confirmPotionButton.onClick.AddListener(UsePotion);
        confirmShieldButton.onClick.AddListener(UseShield);


        yield return StartCoroutine(DBQuizReqHolder.Instance.GetItemsData(DBQuizReqHolder.Instance.GetUserID()));
        yield return StartCoroutine(DBQuizReqHolder.Instance.GetQuizData(currentLevelNumber));


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

        StartCoroutine(DBQuizReqHolder.Instance.GetItemsData(DBQuizReqHolder.Instance.GetUserID()));
        DisplayCurrentQuestion();
        
        ShieldText.text = DBQuizReqHolder.Instance.GetShieldCount().ToString();
        PotionText.text = DBQuizReqHolder.Instance.GetPotionCount().ToString();
        //Debug.Log("Shield count: " + DBQuizReqHolder.Instance.GetShieldCount());
        //Debug.Log("Potion count: " + DBQuizReqHolder.Instance.GetPotionCount());
        
    }

    void UsePotion()
    {
        if (isPotionActive)
        {

            rejectPotionText.text = "You currently have an active booster";
            rejectPotionText.gameObject.SetActive(true);
            return;
        }

        if (potionCount > 0)
        {
            potionCount--;
            StartCoroutine(UpdateItemQuantity(DBQuizReqHolder.Instance.GetUserID(), 2, potionCount));
            DBQuizReqHolder.Instance.SetPotionCount(potionCount);
            PotionText.text = potionCount.ToString();
            currentQuestionMaxPoints *= 2;
            isPotionActive = true;
            PotionPopUp.gameObject.SetActive(false);
            GameObject.Find("ActivateEffect").GetComponent<AudioSource>().Play();


            foreach (Button button in replyButtons)
            {
                button.GetComponent<Image>().color = Color.magenta;
                button.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
        }
        else
        {
            rejectPotionText.text = "No potions available";
            rejectPotionText.gameObject.SetActive(true);
        }
    }

    void UseShield()
    {
        if (isShieldActive)
        {

            rejectShieldText.text = "You currently have an active shield";
            rejectShieldText.gameObject.SetActive(true);
            return;
        }

        if (shieldCount > 0)
        {
            shieldCount--;
            StartCoroutine(UpdateItemQuantity(DBQuizReqHolder.Instance.GetUserID(), 1, shieldCount));
            DBQuizReqHolder.Instance.SetPotionCount(potionCount);
            ShieldText.text = shieldCount.ToString();
            wrongAnswersCount = -1;
            isShieldActive = true;
            ShieldPopUp.gameObject.SetActive(false);
            GameObject.Find("ActivateEffect").GetComponent<AudioSource>().Play();
            
            int triviaMissionId = 3;
            DBQuizReqHolder.Instance.StartCoroutine(
            DBQuizReqHolder.Instance.IncrementQuestProgress(triviaMissionId)
            );
            Debug.Log("Incrementing trivia mission progress");
            
            string missionID = "3";
            int currentProgress = PlayerPrefs.GetInt($"Mission_{missionID}_Progress", 0);
            PlayerPrefs.SetInt($"Mission_{missionID}_Progress", currentProgress + 1);
        }
        else
        {
            rejectShieldText.text = "No shields available";
            rejectShieldText.gameObject.SetActive(true);
        }
    }

    void DisplayCurrentQuestion()
    {
        ResetQuestionState();

        if (currentQuestionIndex >= GetNumberOfQuestions())
        {
            CompleteQuiz();
            return;
        }


        if (isPotionActive)
        {
            foreach (Button button in replyButtons)
            {
                button.GetComponent<Image>().color = Color.magenta;
                button.GetComponentInChildren<TMP_Text>().color = Color.white;
            }
        }


        currentQuestionText.text = (currentQuestionIndex + 1) + "/" + GetNumberOfQuestions();
        HeartText.text = (lives - quizErrors).ToString();

        //Question currentQuestion = selectedCategory.questions[currentQuestionIndex];
        questionText.text = DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].pregunta;

        for (int i = 0; i < replyButtons.Length; i++)
        {
            bool shouldActivate = i < 7;
            replyButtons[i].gameObject.SetActive(shouldActivate);

            if (shouldActivate)
            {
                replyButtons[i].GetComponentInChildren<TMP_Text>().text = DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].respuestas[i].respuesta;


                if (!isPotionActive)
                {
                    replyButtons[i].GetComponent<Image>().color = normalButtonColor;
                    replyButtons[i].GetComponentInChildren<TMP_Text>().color = normalTextColor;
                }
            }
        }


        isPotionActive = false;
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
        bool isCorrect = answerIndex == DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].indice_correcto;
        //Debug.Log("Correct answer index: " + DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].indice_correcto);


        int originalQuestionMaxPoints = currentQuestionMaxPoints;

        if (isCorrect)
        {
            int pointsEarned = (currentQuestionMaxPoints / GetNumberOfQuestions()) - 
                ((currentQuestionMaxPoints / GetNumberOfQuestions()) / 3) * wrongAnswersCount;
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


        Color originalButtonColor = buttonImage.color;
        Color originalTextColor = buttonText.color;


        if (isShieldActive)
        {
            GameObject.Find("BlockEffect").GetComponent<AudioSource>().Play();
            buttonImage.color = Color.blue;
        }
        else
        {
            GameObject.Find("IncorrectEffect").GetComponent<AudioSource>().Play();
            buttonImage.color = wrongAnswerColor;
        }
        buttonText.color = wrongAnswerTextColor;


        yield return new WaitForSeconds(flashDuration);


        buttonImage.color = originalButtonColor;
        buttonText.color = originalTextColor;


        isShieldActive = false;
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
        if (wrongAnswersCount == -1)
        {
            wrongAnswersCount = 0;
            return;
        }

        wrongAnswersCount++;
        quizErrors++;

        HeartText.text = (lives - quizErrors).ToString();

        if (wrongAnswersCount == 1 && !hasShownTipForCurrentQuestion && !string.IsNullOrEmpty(currentQuestion.tip))
        {
            ShowTip(DBQuizReqHolder.Instance.GetPreguntas()[currentQuestionIndex].tip);
            hasShownTipForCurrentQuestion = true;
        }

        if (quizErrors >= 3)
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


        if (isPotionActive)
        {
            currentQuestionMaxPoints /= 2;
            isPotionActive = false;
        }

        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }

    void CompleteQuiz()
    {

        int triviaMissionId = 1;
        DBQuizReqHolder.Instance.StartCoroutine(
        DBQuizReqHolder.Instance.IncrementQuestProgress(triviaMissionId)
        );
        //Debug.Log("Incrementing trivia mission progress");
        
        string missionID = "1";
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
        PlayerPrefs.SetInt("TempCoins", starsEarned * 150);

        Debug.Log("=================================");
        Debug.Log("userID: " + DBQuizReqHolder.Instance.GetUserID());
        Debug.Log("levelNumber: " + DBQuizReqHolder.Instance.GetLevelNumber());
        StartCoroutine(
            DBQuizReqHolder.Instance.UpdateQuizUsuario(
                DBQuizReqHolder.Instance.GetLevelNumber(),
                DBQuizReqHolder.Instance.GetUserID(),
                1,
                starsEarned,
                totalPointsEarned,
                1
            ));

        StartCoroutine(
            DBQuizReqHolder.Instance.UpdateCoins(
                DBQuizReqHolder.Instance.GetUserID(),
                starsEarned * 150
                 ));
        
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

    private IEnumerator UpdateItemQuantity(int userID, int itemID, int quantity)
    {
        string url = DBQuizReqHolder.Instance.urlBD + "items/update";

        ItemUpdateRequest data = new ItemUpdateRequest
        {
            id_usuario = userID,
            id_item = itemID,
            cantidad = quantity
        };

        string json = JsonUtility.ToJson(data);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Item quantity updated successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error updating item quantity: " + request.error);
        }
    }


    [System.Serializable]
    public class ItemUpdateRequest
    {
        public int id_usuario;
        public int id_item;
        public int cantidad;
    }
}