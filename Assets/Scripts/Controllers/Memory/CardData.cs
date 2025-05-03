using UnityEngine;
using System;

// Descripción: Este archivo define los datos de las tarjetas para el juego de memoria, incluyendo los pares de conceptos y definiciones, así como la configuración del nivel y los puntos máximos.
// Autor: Alfonso Vega e Israel González

[CreateAssetMenu(fileName = "NewCardData", menuName = "Game Data/Card Data")]
public class CardData : ScriptableObject
{
    public string levelName;
    public CardPair[] cardPairs;
    public int maxPoints = 1000; 
    
    [Serializable]
    public class CardPair
    {
        public string concept;
        public string definition;
    }
}