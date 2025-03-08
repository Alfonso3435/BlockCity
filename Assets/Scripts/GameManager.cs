using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Importa el namespace para manejar escenas

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
        for (int i = 0; i < replyButtons.Length; i++)
        {
            int index = i; // Captura el índice en una variable local
            replyButtons[i].onClick.AddListener(() => OnReplySelected(index));
        }
        SelectCategory(0);
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

        if (selectedCategory == null)
        {
            Debug.LogError("La categoría seleccionada es nula.");
            return;
        }

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
        Debug.Log("Displaying question...");

        if (selectedCategory == null)
        {
            Debug.LogError("No category selected.");
            return;
        }

        if (currentQuestionIndex < 0 || currentQuestionIndex >= selectedCategory.questions.Length)
        {
            Debug.LogError("Índice de pregunta fuera de rango.");
            return;
        }

        var question = selectedCategory.questions[currentQuestionIndex];

        if (question == null)
        {
            Debug.LogError("La pregunta actual es nula.");
            return;
        }

        if (questionText == null)
        {
            Debug.LogError("El texto de la pregunta no está asignado.");
            return;
        }

        questionText.text = question.questionText;

        if (replyButtons == null || replyButtons.Length == 0)
        {
            Debug.LogError("No hay botones de respuesta asignados.");
            return;
        }

        for (int i = 0; i < replyButtons.Length; i++)
        {
            if (replyButtons[i] == null)
            {
                Debug.LogError($"El botón de respuesta {i} no está asignado.");
                continue;
            }

            TMP_Text buttonText = replyButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText == null)
            {
                Debug.LogError($"El texto del botón de respuesta {i} no está asignado.");
                continue;
            }

            if (i < question.replies.Length)
            {
                buttonText.text = question.replies[i];
            }
            else
            {
                Debug.LogError($"No hay suficientes respuestas para el botón {i}.");
            }
        }
    }

    public void OnReplySelected(int replyIndex)
    {
        Debug.Log("Reply selected: " + replyIndex);

        if (selectedCategory == null || selectedCategory.questions == null || currentQuestionIndex < 0 || currentQuestionIndex >= selectedCategory.questions.Length)
        {
            Debug.LogError("Error en la selección de respuesta: categoría o pregunta no válida.");
            return;
        }

        var question = selectedCategory.questions[currentQuestionIndex];

        if (question == null)
        {
            Debug.LogError("La pregunta actual es nula.");
            return;
        }

        if (replyIndex == question.correctReplyIndex)
        {
            Debug.Log("Correct!");
            currentQuestionIndex++;
            Debug.Log("Current Question Index: " + currentQuestionIndex);

            if (currentQuestionIndex < selectedCategory.questions.Length)
            {
                DisplayQuestion();
            }
            else
            {
                Debug.Log("Category completed!");
                // Cargar la escena "StageClear"
                SceneManager.LoadScene("StageClear");
            }
        }
        else
        {
            Debug.Log("Incorrect!");
            // Aquí podrías agregar lógica para manejar una respuesta incorrecta
        }
    }
}