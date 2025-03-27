using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    
    [TextArea(2, 5)] // TextArea más pequeña para el tip
    public string tip; // Nuevo campo para el tip
}

[CreateAssetMenu(fileName = "New Category", menuName = "Quiz/Question Data")]
public class QuestionData : ScriptableObject
{
    public string category;
    public Question[] questions;
}