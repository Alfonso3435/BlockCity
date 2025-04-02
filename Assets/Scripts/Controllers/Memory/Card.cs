using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    public int cardID; // ID único para esta carta
    public GameManagerCard gameManager; // Referencia al GameManager
    private bool isFlipped; // Flag para verificar si la carta está volteada
    public GameObject cardFront; // Objeto que muestra el frente de la carta
    public GameObject cardBack; // Objeto que muestra el reverso de la carta
    public TextMeshProUGUI contentText; // Texto que muestra el contenido

    void Start()
    {
        isFlipped = false;
        cardFront.SetActive(false);
        cardBack.SetActive(true);
    }

    public void FlipCard()
    {
        if (!isFlipped && (gameManager.firstCard == null || gameManager.secondCard == null))
        {
            isFlipped = true;
            cardFront.SetActive(true);
            cardBack.SetActive(false);
            gameManager.CardFlipped(this);
        }
    }

    public void HideCard()
    {
        isFlipped = false;
        cardFront.SetActive(false);
        cardBack.SetActive(true);
    }

    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }
}