using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string questionText;
    public string[] replies;
    public int correctReplyIndex;
}

[CreateAssetMenu(fileName = "New Category", menuName = "Quiz/Question Data")]
public class QuestionData : ScriptableObject
{
    public string category;
    public Question[] questions;

}
