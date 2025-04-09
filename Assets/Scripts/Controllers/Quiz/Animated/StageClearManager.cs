using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageClearManager : MonoBehaviour
{
    [Header("Stars Configuration")]
    [SerializeField] private Star[] stars;
    [SerializeField] private float starAnimationDuration = 0.5f;
    [SerializeField] private float delayBetweenStars = 0.2f;

    [Header("Counters Configuration")]
    [SerializeField] private ScoreCoinsManager countersManager;
    [SerializeField] private float delayBeforeCounters = 0.5f;

    [Header("UI Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button retryButton;

    private void Start()
    {
        int stars = PlayerPrefs.GetInt("TempStars", 0);
        int points = PlayerPrefs.GetInt("TempPoints", 0);
        int coins = PlayerPrefs.GetInt("TempCoins", 0);
        
        continueButton.onClick.AddListener(ContinueToNextLevel);
        retryButton.onClick.AddListener(RetryLevel);
        
        ShowStageClear(stars, points, coins);
    }

    public void ShowStageClear(int starsEarned, int finalScore, int finalCoins)
    {
        gameObject.SetActive(true);
        StartCoroutine(StageClearAnimation(starsEarned, finalScore, finalCoins));
        
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", currentCoins + finalCoins);
    }

    private IEnumerator StageClearAnimation(int starsEarned, int finalScore, int finalCoins)
    {
        yield return StartCoroutine(AnimateStars(starsEarned));
        yield return new WaitForSeconds(delayBeforeCounters);
        countersManager.SetTargetValues(finalScore, finalCoins);
        countersManager.StartCountersAnimation();
    }

    private IEnumerator AnimateStars(int starsToShow)
    {
        foreach (Star star in stars) star.ResetStar();

        for (int i = 0; i < Mathf.Min(starsToShow, stars.Length); i++)
        {
            yield return StartCoroutine(AnimateSingleStar(stars[i]));
            yield return new WaitForSeconds(delayBetweenStars);
        }
    }

    private IEnumerator AnimateSingleStar(Star star)
    {
        star.ShowStar();
        float timer = 0;
        Vector3 startScale = Vector3.zero;
        
        while (timer < starAnimationDuration)
        {
            star.SetScale(Vector3.Lerp(startScale, Vector3.one, timer / starAnimationDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        
        star.SetScale(Vector3.one);
    }

    private void ContinueToNextLevel()
    {
        string currentModule = PlayerPrefs.GetString("CurrentModule", "LevelSelection1");
        SceneManager.LoadScene(currentModule);
    }

    private void RetryLevel()
    {
        string gameType = QuizDataHolder.Instance.GetCurrentGameType();
        SceneManager.LoadScene(gameType == "quiz" ? "Quiz" : "MemoryGame");
    }
}