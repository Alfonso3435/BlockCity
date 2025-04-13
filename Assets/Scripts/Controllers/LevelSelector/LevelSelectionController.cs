using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public HangmanData hangmanData; // Added for Hangman
    public string hangmanSceneName = "Hangman"; // Added for Hangman
    
    public int levelNum;
    public string quizSceneName = "Quiz";
    public string memorySceneName = "Memory";

    private void Start()
    {
        UpdateLevelState();
    }

    private void Update()
    {
        UpdateLevelState();
    }

    private void UpdateLevelState()
    {
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        int prevLevelNum = levelNum - 1;
        
        unlocked = prevLevelNum == 0 || 
                  PlayerPrefs.GetInt(currentModule + "_Lv" + prevLevelNum, 0) > 0;

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
            
            for (int i = 0; i < starsObtained; i++)
            {
                stars[i].GetComponent<Image>().sprite = starSprite;
            }
        }
    }

    public void PressSelection()
    {
        if (unlocked)
        {
            PlayerPrefs.SetInt("CurrentLevel", levelNum);
            PlayerPrefs.SetString("CurrentLevelName", gameObject.name);
            
            // Guardar el capítulo correspondiente al nivel actual
            int capitulo = levelNum;
            PlayerPrefs.SetInt("CurrentChapter", capitulo);

            // Guardar la siguiente escena que se debe cargar después de la lectura
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
            else if (levelNum == 3 && hangmanData != null) // Lógica para Hangman, es para que aparezca por ahora
            {
                QuizDataHolder.Instance.SetHangmanData(hangmanData);
                QuizDataHolder.Instance.SetLevelNumber(levelNum);
                PlayerPrefs.SetString("NextScene", hangmanSceneName);
            }

            // cargar la escena Lecture
            SceneManager.LoadScene("Lecture");
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