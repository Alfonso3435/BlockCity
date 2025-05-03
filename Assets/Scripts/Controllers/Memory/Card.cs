using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
// Descripción: Este archivo controla la lógica de las tarjetas en el juego de memoria, incluyendo el volteo de tarjetas, la animación de volteo y la interacción con el administrador del juego.
// Autor: Alfonso Vega e Israel González
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
    public GameObject cardFront; 
    public GameObject cardBack;  
    

    private bool isAnimating = false;
    private float flipSpeed = 8f; 
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
            
            flipProgress = Mathf.Clamp01(flipProgress + Time.deltaTime * flipSpeed);
            
            
            float rotation = flippingToFront ? 
                Mathf.Lerp(0, 180, flipProgress) : 
                Mathf.Lerp(180, 360, flipProgress);
                
            float scale = 1f + Mathf.Sin(flipProgress * Mathf.PI) * 0.1f;
            
            
            transform.localEulerAngles = new Vector3(0, rotation, 0);
            transform.localScale = new Vector3(scale, scale, scale);
            
            
            if (contentText != null)
            {
                contentText.transform.localEulerAngles = new Vector3(
                    0, 
                    flipProgress > 0.5f ? 180 : 0, 
                    0);
            }
            
            
            if (flipProgress >= 0.5f)
            {
                cardFront.SetActive(flippingToFront);
                cardBack.SetActive(!flippingToFront);
            }
            
            
            if (flipProgress >= 1f)
            {
                isAnimating = false;
                flipProgress = 0f;
                
                
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