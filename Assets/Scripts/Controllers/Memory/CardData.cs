using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewCardData", menuName = "Game Data/Card Data")]
public class CardData : ScriptableObject
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