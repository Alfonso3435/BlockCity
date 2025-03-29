using UnityEngine;

[System.Serializable]
public class Question
{
    [TextArea(3, 10)]
    public string questionText;
    
    [TextArea(1, 3)]
    public string[] replies;
    
    public int correctReplyIndex;
    
    [TextArea(3, 10)]
    public string explanation;
    
    [TextArea(2, 5)]
    public string tip;
}

[CreateAssetMenu(fileName = "New Category", menuName = "Quiz/Question Data")]
public class QuestionData : ScriptableObject
{
    public string category;
    public int maxPoints = 1000; // Puntos máximos para este conjunto de preguntas
    public Question[] questions;
}