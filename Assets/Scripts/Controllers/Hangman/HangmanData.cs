using UnityEngine;
using System;

[Serializable]
public class HangmanData
{
    public string[] palabras = { "wallet", "bitcoin", "crypto", "w3" }; // Banco de palabras por defecto
    public int maxFallos = 6; // Número máximo de fallos
}
