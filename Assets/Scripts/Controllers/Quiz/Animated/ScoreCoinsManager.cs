using UnityEngine;
using System.Collections;

// Descripción: Este archivo gestiona la animación de los contadores de puntuación y monedas, coordinando la secuencia de animaciones y permitiendo la configuración de valores objetivo para cada contador.
// Autor: [Tu Nombre]

public class ScoreCoinsManager : MonoBehaviour
{
    [System.Serializable]
    public class CounterSettings
    {
        public CounterAnimator animator;
        public int targetValue;
        public float delayAfter = 0.3f;
    }

    [Header("Counters")]
    [SerializeField] private CounterSettings scoreCounter;
    [SerializeField] private CounterSettings coinsCounter;

    [Header("Animation Sequence")]
    [SerializeField] private float initialDelay = 0.5f;

    public void SetTargetValues(int score, int coins)
    {
        scoreCounter.targetValue = score;
        coinsCounter.targetValue = coins;
    }

    public void StartCountersAnimation()
    {
        StartCoroutine(AnimateCountersSequence());
    }

    private IEnumerator AnimateCountersSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        
        // Animar Score
        scoreCounter.animator.AnimateToValue(scoreCounter.targetValue);
        yield return new WaitForSeconds(scoreCounter.delayAfter);
        
        // Animar Coins
        coinsCounter.animator.AnimateToValue(coinsCounter.targetValue);
    }

    // Método rápido para testing
    public void TestAnimation(int testScore = 1000, int testCoins = 50)
    {
        SetTargetValues(testScore, testCoins);
        StartCountersAnimation();
    }
}