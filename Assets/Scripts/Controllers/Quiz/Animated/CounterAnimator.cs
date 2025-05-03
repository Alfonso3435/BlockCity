using UnityEngine;
using TMPro;
using System.Collections;

// Descripción: Este archivo anima un contador numérico en la interfaz de usuario, mostrando una transición suave entre valores y aplicando efectos visuales como pulsaciones para resaltar cambios.
// Autor: Alfonso Vega

[RequireComponent(typeof(TMP_Text))]
public class CounterAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private string numberFormat = "0000";
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Effects")]
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseScale = 1.1f;
    
    private TMP_Text counterText;
    private Vector3 originalScale;
    private Coroutine animationRoutine;

    private void Awake()
    {
        counterText = GetComponent<TMP_Text>();
        originalScale = transform.localScale;
        counterText.text = "0".PadLeft(numberFormat.Length, '0');
    }

    public void AnimateToValue(int targetValue)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);
        
        animationRoutine = StartCoroutine(AnimateCounter(targetValue));
    }

    private IEnumerator AnimateCounter(int targetValue)
    {
        float elapsedTime = 0;
        int currentValue = 0;
        int startValue = 0;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            currentValue = (int)Mathf.Lerp(startValue, targetValue, progress);
            counterText.text = currentValue.ToString(numberFormat);
            yield return null;
        }

        counterText.text = targetValue.ToString(numberFormat);
        
        if (pulseEffect)
            yield return StartCoroutine(PlayPulseEffect());
    }

    private IEnumerator PlayPulseEffect()
    {
        float pulseTime = 0.2f;
        float timer = 0;
        
        while (timer < pulseTime)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(1f, pulseScale, Mathf.PingPong(timer / pulseTime * 2, 1));
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}