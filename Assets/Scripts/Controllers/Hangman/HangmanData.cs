using UnityEngine;

[CreateAssetMenu(fileName = "NewHangmanData", menuName = "Game Data/Hangman Data")]
public class HangmanData : ScriptableObject
{
    public string[] palabras = { "wallet", "bitcoin", "crypto", "w3" }; // Banco de palabras por defecto
    public int maxFallos = 6; // Número máximo de fallos
}
