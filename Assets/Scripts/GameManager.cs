using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public QuestionData[] categories;
    private QuestionData selectedCategory;
    private int currentQuestionIndex;

    public TMP_Text questionText;
    public Button[] replyButtons;

    void Start()
    {
        Debug.Log("Iniciando juego...");
        SelectCategory(0);
        Debug.Log($"Categoría seleccionada: {selectedCategory?.category}");
    }

    public void SelectCategory(int categoryIndex)
{
    if (categories == null || categories.Length == 0)
    {
        Debug.LogError("No hay categorías disponibles en 'categories'. Asigna al menos una en el Inspector.");
        return;
    }
    
    if (categoryIndex < 0 || categoryIndex >= categories.Length)
    {
        Debug.LogError("Índice de categoría fuera de rango. Verifica que 'categoryIndex' es válido.");
        return;
    }

    selectedCategory = categories[categoryIndex];

    if (selectedCategory.questions == null || selectedCategory.questions.Length == 0)
    {
        Debug.LogError($"La categoría '{selectedCategory.category}' no tiene preguntas asignadas.");
        return;
    }

    currentQuestionIndex = 0;
    DisplayQuestion();
}

    public void DisplayQuestion()
    {
        if (selectedCategory == null)
        {
            return;
        }
        var question = selectedCategory.questions[currentQuestionIndex];
        questionText.text = question.questionText;
        
        for (int i =0; i < replyButtons.Length; i++)
        {
            TMP_Text buttonText = replyButtons[i].GetComponentInChildren<TMP_Text>();
            buttonText.text = question.replies[i];
        }
    }

    public void OnReplySelected(int replyIndex){
        if (replyIndex == selectedCategory.questions[currentQuestionIndex].correctReplyIndex)
        {
            Debug.Log("Correct!");
        }
        else
        {
            Debug.Log("Incorrect!");
        }

        currentQuestionIndex++;
        if(currentQuestionIndex < selectedCategory.questions.Length)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Category completed!");
        }
    } 
}
