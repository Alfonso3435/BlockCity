using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LogoutController : MonoBehaviour
{
    private Button botonLogout;
    private UIDocument menu;

    [Serializable]
    public class DatosSesion
    {
        public string correo;
        public string hora;
        public string tipo;
        public int id_usuario;
    }

    void OnEnable()
    {
        menu = GetComponent<UIDocument>();
        var root = menu.rootVisualElement;
        botonLogout = root.Q<Button>("Logout");

        botonLogout.clicked += () =>
        {
            string correo = PlayerPrefs.GetString("usuario", "unknown");
            int id_usuario = PlayerPrefs.GetInt("id_usuario", 0);
            DateTime horaFin = DateTime.Now;

            StartCoroutine(EnviarDatosLogout("logout", correo, horaFin, id_usuario));
        };
    }

    private IEnumerator EnviarDatosLogout(string tipo, string correo, DateTime hora, int id_usuario)
    {
        DatosSesion datos = new DatosSesion
        {
            correo = correo,
            hora = hora.ToString("yyyy-MM-dd HH:mm:ss"),
            tipo = tipo,
            id_usuario = id_usuario
        };

        string datosJSON = JsonUtility.ToJson(datos);
        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/logout", "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(datosJSON);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Limpiar PlayerPrefs
        PlayerPrefs.DeleteKey("usuario");
        PlayerPrefs.DeleteKey("horaInicio");
        PlayerPrefs.DeleteKey("id_usuario");
        PlayerPrefs.DeleteKey("nombre_user");
        PlayerPrefs.Save();

        // Limpiar estado en memoria
        DBQuizReqHolder.Instance.SetIsLoggedIn(false);
        DBQuizReqHolder.Instance.SetUserID(0);
        
        // Redirigir a la escena de login
        SceneManager.LoadScene("Login");
    }
}