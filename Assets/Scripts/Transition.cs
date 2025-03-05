using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
public class Transition : MonoBehaviour
{

    public void Menu(){
        print("Menu");
        SceneManager.LoadSceneAsync(0);
    }
    public void Login(){
        print("Login");
        SceneManager.LoadSceneAsync(1);
    }

    public void Register(){
        print("Register");
        SceneManager.LoadSceneAsync(2);
    }
}
