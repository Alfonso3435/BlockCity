using UnityEngine;
using System;

[Serializable]
public class CardData
{
    public string levelName;
    public CardPair[] cardPairs;
    public int maxPoints = 1000; // Puntos máximos posibles
    
    [Serializable]
    public class CardPair
    {
        public string concept;
        public string definition;
    }
}