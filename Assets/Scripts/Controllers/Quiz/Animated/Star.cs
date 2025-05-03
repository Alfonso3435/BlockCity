using UnityEngine;
using UnityEngine.UI;

// Descripción: Este archivo controla la animación y el estado visual de las estrellas en la interfaz de usuario, permitiendo su reinicio, activación y ajuste de escala para efectos visuales.
// Autor: Alfonso Vega

public class Star : MonoBehaviour
{
    [SerializeField] private Image activeStar;
    [SerializeField] private Image inactiveStar;

    public void ResetStar()
    {
        activeStar.gameObject.SetActive(false);
        inactiveStar.gameObject.SetActive(true);
        activeStar.transform.localScale = Vector3.zero;
    }

    public void ShowStar()
    {
        activeStar.gameObject.SetActive(true);
        inactiveStar.gameObject.SetActive(false);
    }

    public void SetScale(Vector3 scale)
    {
        activeStar.transform.localScale = scale;
    }
}