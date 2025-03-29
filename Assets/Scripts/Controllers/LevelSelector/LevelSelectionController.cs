using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*

Autor: Juan Alfonso Vega Sol
Referencia: https://www.youtube.com/watch?v=uVVyBp0yLqQ&t=16s
*/


public class LevelSelectionController : MonoBehaviour
{
    [SerializeField] private bool unlocked;
    public Image unlockImage;
    public GameObject[] stars;

    public Sprite starSprite;

    private void Update()
    {
        UpdateLevelImage();
        UpdateLevelStatus();
    }

    private void UpdateLevelStatus(){
        int previousLevelNum = int.Parse(gameObject.name) - 1;
        if (PlayerPrefs.GetInt("Lv" + previousLevelNum.ToString()) > 0)
        {
            unlocked = true;
        }
    }

    private void UpdateLevelImage()
    {
        if (!unlocked)
        {
            unlockImage.gameObject.SetActive(true);
            foreach (GameObject star in stars)
            {
                star.SetActive(false);
            }
        }
        else
        {
            unlockImage.gameObject.SetActive(false);
            foreach (GameObject star in stars)
            {
                star.SetActive(true);
            }

            for (int i = 0; i < PlayerPrefs.GetInt("Lv" + gameObject.name); i++)
            {
                stars[i].GetComponent<Image>().sprite = starSprite;
            }
        }
    }

    public void PressSelection(string _LevelName)
    {
        if (unlocked)
        {
            SceneManager.LoadScene(_LevelName);
        }
    }

    // Método para contar las estrellas obtenidas en todos los niveles
    public static int GetTotalStars(string sceneName)
    {
        int totalStars = 0;
        for (int i = 1; i <= 10; i++) // Suponiendo que hay 10 niveles por escena
        {
            // Usamos el nombre de la escena como prefijo para las claves de PlayerPrefs
            totalStars += PlayerPrefs.GetInt(sceneName + "_Lv" + i.ToString(), 0);
        }
        return totalStars;
    }
}