using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour
{
    [SerializeField] private bool unlocked;
    public Image unlockImage;
    public GameObject[] stars;
    public Sprite starSprite;
    
    public QuestionData quizData;
    public int levelNum;
    public string quizSceneName = "Quiz";

    private void Update()
    {
        UpdateLevelImage();
        UpdateLevelStatus();
    }

    private void UpdateLevelStatus()
    {
        int previousLevelNum = int.Parse(gameObject.name) - 1;
        if (PlayerPrefs.GetInt("Lv" + previousLevelNum.ToString()) > 0)
        {
            unlocked = true;
        }
    }

    private void UpdateLevelImage()
    {
        if (!unlocked)
        {
            unlockImage.gameObject.SetActive(true);
            foreach (GameObject star in stars)
            {
                star.SetActive(false);
            }
        }
        else
        {
            unlockImage.gameObject.SetActive(false);
            foreach (GameObject star in stars)
            {
                star.SetActive(true);
            }

            for (int i = 0; i < PlayerPrefs.GetInt("Lv" + gameObject.name); i++)
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
            
            QuizDataHolder.Instance.SetQuizData(quizData);
            QuizDataHolder.Instance.SetLevelNumber(levelNum);
            
            SceneManager.LoadScene(quizSceneName);
        }
    }

    public static int GetTotalStars(string sceneName)
    {
        int totalStars = 0;
        for (int i = 1; i <= 10; i++)
        {
            totalStars += PlayerPrefs.GetInt(sceneName + "_Lv" + i.ToString(), 0);
        }
        return totalStars;
    }
}