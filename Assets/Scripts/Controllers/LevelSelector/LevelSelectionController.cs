using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using NUnit.Framework.Constraints;
// Descripción: Este archivo controla la selección de niveles dentro del módulo, gestionando el estado de desbloqueo, la visualización de estrellas obtenidas y la transición a las escenas correspondientes según el nivel seleccionado.
// Autor: Alfonso Vega
public class LevelSelectionController : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private bool unlocked;
    public Image unlockImage;
    public GameObject[] stars;
    public Sprite starSprite;
    
    [Header("Game Settings")]
    public QuestionData quizData;
    public CardData cardData;
    public HangmanData hangmanData; 
    

    public int levelNum;
    public string quizSceneName = "Quiz";
    public string memorySceneName = "Memory";
    public string hangmanSceneName = "Hangman"; 
    public bool[] desbloqueos = {false, false, false, false, false, false};
    public int[] starobtained = {0, 0, 0, 0, 0, 0};

    private void Start()
    {
        StartCoroutine(Starter());
    }
    private IEnumerator Starter()
    {
        UpdateLevelState();
        yield return StartCoroutine(DBQuizReqHolder.Instance.GetModuleQuizStars(
            DBQuizReqHolder.Instance.GetModuleID(),
            DBQuizReqHolder.Instance.GetUserID()));

        var quizStars = DBQuizReqHolder.Instance.GetModuleQuizStarsData();

        int j = 0;
        if (quizStars != null)
        {
            Debug.Log("Quiz Stars Data:");
            foreach (var quiz in quizStars)
            {
                Debug.Log($"Quiz ID: {quiz.id_quiz}, Name: {quiz.nombre_quiz}, Stars: {quiz.estrellas}");
                starobtained[j] = quiz.estrellas;
                j++;
            }
        }
        else
        {
            Debug.LogError("No quiz stars data available.");
        }

        yield return StartCoroutine(DBQuizReqHolder.Instance.GetModuleLevels(idModulo: 1,
            idUsuario: DBQuizReqHolder.Instance.GetUserID(),
            onSuccess: (levels) =>
            {
                Debug.Log("GetModuleLevels Success:");
                foreach (var level in levels)
                {
                    //Debug.Log($"Level ID: {level.id_quiz}, Name: {level.nombre_quiz}, Unlocked: {level.desbloqueado}");
                    desbloqueos[level.id_quiz - 1] = level.desbloqueado;
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"GetModuleLevels Error: {error}");
            }
            ));

        
        for (int i = 0; i < desbloqueos.Length; i++)
            {
                //Debug.Log($"desbloqueos[{i}]: {desbloqueos[i]}");
            }
    }

    private void Update()
    {
        UpdateLevelState();
    }
    private void UpdateLevelState()
    {
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int prevLevelNum = levelNum - 1;
        
        //Debug.Log("LevelNum: " + levelNum);


        unlocked = prevLevelNum == 0 || 
                  PlayerPrefs.GetInt(currentModule + "_Lv" + prevLevelNum, 0) > 0
                  ;
        unlocked = desbloqueos[levelNum - 1];

        if (levelNum == 1)
        {
            unlocked = true;
        }

        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        unlockImage.gameObject.SetActive(!unlocked);
        foreach (GameObject star in stars)
        {
            star.SetActive(unlocked);
        }

        if (unlocked)
        {
            string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
            int starsObtained = PlayerPrefs.GetInt(currentModule + "_Lv" + levelNum, 0);
            
            for (int i = 0; i < starobtained[levelNum - 1]; i++)
            {
                stars[i].GetComponent<Image>().sprite = starSprite;
            }
        }
    }

    public void PressSelection()
    {
        if (unlocked)
        {
            //Debug.Log("Nivel " + levelNum + " seleccionado.");
            PlayerPrefs.SetInt("CurrentLevel", levelNum);
            PlayerPrefs.SetString("CurrentLevelName", gameObject.name);
            
           
            int capitulo = levelNum;
            PlayerPrefs.SetInt("CurrentChapter", capitulo);

            
            DBQuizReqHolder.Instance.SetLevelNumber(levelNum);

            
            if (quizData != null) 
            {
                QuizDataHolder.Instance.SetQuizData(quizData);
                QuizDataHolder.Instance.SetLevelNumber(levelNum);
                PlayerPrefs.SetString("NextScene", quizSceneName);
            }
            else if (cardData != null) 
            {
                QuizDataHolder.Instance.SetCardData(cardData);
                QuizDataHolder.Instance.SetLevelNumber(levelNum);
                PlayerPrefs.SetString("NextScene", memorySceneName);
            }
            else if (hangmanData != null) 
            {
                QuizDataHolder.Instance.SetHangmanData(hangmanData);
                QuizDataHolder.Instance.SetLevelNumber(levelNum);
                PlayerPrefs.SetString("NextScene", hangmanSceneName);
            }

            
            if (cardData != null) 
            {
                SceneManager.LoadScene("Memory"); 
            }
            else
            {
                if (levelNum == 5){
                     SceneManager.LoadScene("Hangman"); 
                }
                else{SceneManager.LoadScene($"Lecture{levelNum}");
            }
        }
    }
    }

    public static int GetTotalStars(string moduleName)
    {
        int totalStars = 0;
        for (int i = 1; i <= 10; i++)
        {
            totalStars += PlayerPrefs.GetInt(moduleName + "_Lv" + i, 0);
        }
        return totalStars;
    }
}