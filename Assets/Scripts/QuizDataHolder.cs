using UnityEngine;

public class QuizDataHolder : MonoBehaviour
{
    public static QuizDataHolder Instance;
    
    private QuestionData currentQuizData;
    private int currentLevelNumber;

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
    }

    public QuestionData GetQuizData()
    {
        return currentQuizData;
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
        currentLevelNumber = 0;
    }
}