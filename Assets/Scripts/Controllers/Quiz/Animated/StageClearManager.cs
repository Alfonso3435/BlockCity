using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    [Header("Stars Configuration")]
    [SerializeField] private Star[] stars;
    [SerializeField] private float starAnimationDuration = 0.5f;
    [SerializeField] private float delayBetweenStars = 0.2f;

    [Header("Counters Configuration")]
    [SerializeField] private ScoreCoinsManager countersManager;
    [SerializeField] private float delayBeforeCounters = 0.5f;

    private void OnEnable()
    {
        // Guardar las estrellas obtenidas
        int starsEarned = 3;
        int level = 2; // Valor de ejemplo
        SaveLevelProgress(starsEarned, level);
        
        // Mostrar la pantalla de Stage Clear
        ShowStageClear(starsEarned, 3500, 1000);
    }

    private void SaveLevelProgress(int stars, int levelNum)
    {
        // Guardar para el nivel actual
        PlayerPrefs.SetInt("_Lv" + levelNum, stars);
        
        // Desbloquear el siguiente nivel si es necesario
        if(stars > 0) // Asume 10 niveles máximo
        {
            PlayerPrefs.SetInt("Lv" + levelNum, stars); // Desbloquea el nivel actual
        }
        
        PlayerPrefs.Save();
    }

    public void ShowStageClear(int starsEarned, int finalScore, int finalCoins)
    {
        StartCoroutine(StageClearAnimation(starsEarned, finalScore, finalCoins));
    }

    private IEnumerator StageClearAnimation(int starsEarned, int finalScore, int finalCoins)
    {
        // Animar estrellas primero
        yield return StartCoroutine(AnimateStars(starsEarned));
        
        // Pequeña pausa antes de los contadores
        yield return new WaitForSeconds(delayBeforeCounters);
        
        // Configurar y animar contadores
        countersManager.SetTargetValues(finalScore, finalCoins);
        countersManager.StartCountersAnimation();
    }

    private IEnumerator AnimateStars(int starsToShow)
    {
        // Resetear todas las estrellas
        foreach (Star star in stars)
        {
            star.ResetStar();
        }

        // Animar las estrellas ganadas
        for (int i = 0; i < Mathf.Min(starsToShow, stars.Length); i++)
        {
            yield return StartCoroutine(AnimateSingleStar(stars[i]));
            yield return new WaitForSeconds(delayBetweenStars);
        }
    }

    private IEnumerator AnimateSingleStar(Star star)
    {
        star.ShowStar();
        
        // Animación simple de escala
        float timer = 0;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        
        while (timer < starAnimationDuration)
        {
            timer += Time.deltaTime;
            star.SetScale(Vector3.Lerp(startScale, endScale, timer / starAnimationDuration));
            yield return null;
        }
        
        star.SetScale(endScale);
    }
}