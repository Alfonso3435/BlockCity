using UnityEngine;
using UnityEngine.UI;

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