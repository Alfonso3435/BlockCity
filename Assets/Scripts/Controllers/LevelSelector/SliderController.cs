using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SliderController : MonoBehaviour
{
    public void PressSelection(string _LevelName)
    {

        SceneManager.LoadScene(_LevelName);
    }
}
