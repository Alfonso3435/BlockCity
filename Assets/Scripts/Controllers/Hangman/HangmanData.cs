using UnityEngine;

// Descripción: Este archivo es de la lógica de los datos del juego de ahorcado
// Autor: Alfonso Vega y Estrella Lolbeth

[CreateAssetMenu(fileName = "NewHangmanData", menuName = "Game Data/Hangman Data")]
public class HangmanData : ScriptableObject
{
    public string[] palabras = { "wallet", "bitcoin", "crypto", "w3" }; 
    public int maxFallos = 6;
    public int maxPoints = 1000; 
}
