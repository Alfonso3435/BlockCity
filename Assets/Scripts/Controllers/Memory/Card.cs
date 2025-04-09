using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public interface ICardGameManager
{
    void CardFlipped(Card flippedCard);
}

public class Card : MonoBehaviour
{
    public int cardID;
    public ICardGameManager gameManager;
    private bool isFlipped;
    public TextMeshProUGUI contentText;
    public GameObject cardFront; // Parte frontal de la carta
    public GameObject cardBack;  // Parte trasera de la carta
    
    // Variables para animación
    private bool isAnimating = false;
    private float flipSpeed = 8f; // Velocidad de la animación
    private float flipProgress = 0f;
    private bool flippingToFront = false;

    void Start()
    {
        isFlipped = false;
        cardFront.SetActive(false);
        cardBack.SetActive(true);
    }

    void Update()
    {
        if (isAnimating)
        {
            // Actualizar progreso de animación
            flipProgress = Mathf.Clamp01(flipProgress + Time.deltaTime * flipSpeed);
            
            // Calcular rotación y escala
            float rotation = flippingToFront ? 
                Mathf.Lerp(0, 180, flipProgress) : 
                Mathf.Lerp(180, 360, flipProgress);
                
            float scale = 1f + Mathf.Sin(flipProgress * Mathf.PI) * 0.1f;
            
            // Aplicar transformación
            transform.localEulerAngles = new Vector3(0, rotation, 0);
            transform.localScale = new Vector3(scale, scale, scale);
            
            // Corrección del texto (cambio brusco a la mitad)
            if (contentText != null)
            {
                contentText.transform.localEulerAngles = new Vector3(
                    0, 
                    flipProgress > 0.5f ? 180 : 0, 
                    0);
            }
            
            // Cambiar visibilidad a mitad de la animación
            if (flipProgress >= 0.5f)
            {
                cardFront.SetActive(flippingToFront);
                cardBack.SetActive(!flippingToFront);
            }
            
            // Finalizar animación
            if (flipProgress >= 1f)
            {
                isAnimating = false;
                flipProgress = 0f;
                
                // Notificar al manager cuando se completa el volteo hacia adelante
                if (flippingToFront)
                {
                    gameManager.CardFlipped(this);
                }
            }
        }
    }

    public void FlipCard()
    {
        GameObject.Find("FlipCardEffect").GetComponent<AudioSource>().Play();
        if (!isFlipped && !isAnimating && gameManager != null)
        {
            StartFlip(true);
        }
    }

    public void HideCard()
    {
        if (isFlipped && !isAnimating)
        {
            StartFlip(false);
        }
    }

    private void StartFlip(bool toFront)
    {
        isAnimating = true;
        flippingToFront = toFront;
        flipProgress = 0f;
        isFlipped = toFront;
    }

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }
}