using UnityEngine;

// Descripción: Este archivo define la estructura de los datos de las preguntas para el juego de trivia, incluyendo el texto de la pregunta, respuestas posibles, índice de la respuesta correcta, explicación y consejos. También permite la creación de categorías de preguntas como objetos ScriptableObject.
// Autor: Alfonso Vega

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
    public int maxPoints = 1000;
    public Question[] questions;
}