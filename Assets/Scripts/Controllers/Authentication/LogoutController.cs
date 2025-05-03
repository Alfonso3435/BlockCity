using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

// Descripción: Este archivo controla el proceso de cierre de sesión del usuario, enviando los datos de la sesión al servidor, limpiando el estado local y redirigiendo a la escena de inicio de sesión.
// Autor: Mike Argumedo

public class LogoutController : MonoBehaviour
{
    [SerializeField] private Button logoutButton;

    [Serializable]
    public class DatosSesion
    {
        public string correo;
        public string hora;
        public string tipo;
        public int id_usuario;
    }

    void Start()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        }
        else
        {
            Debug.LogError("LogoutButton no asignado en el Inspector");
        }
    }

    private void OnLogoutButtonClicked()
    {
        string correo = PlayerPrefs.GetString("usuario", "unknown");
        int id_usuario = PlayerPrefs.GetInt("id_usuario", 0);
        DateTime horaFin = DateTime.Now;

        StartCoroutine(EnviarDatosLogout("logout", correo, horaFin, id_usuario));
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
        string url = DBQuizReqHolder.Instance.urlBD + (DBQuizReqHolder.Instance.urlBD.EndsWith("/") ? "logout" : "/logout");
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(datosJSON);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error en logout: " + request.error);
        }

        PlayerPrefs.DeleteKey("usuario");
        PlayerPrefs.DeleteKey("horaInicio");
        PlayerPrefs.DeleteKey("id_usuario");
        PlayerPrefs.DeleteKey("nombre_user");
        PlayerPrefs.Save();

        DBQuizReqHolder.Instance?.SetIsLoggedIn(false);
        DBQuizReqHolder.Instance?.SetUserID(0);
        
        SceneManager.LoadScene("Login");
    }

    void OnDestroy()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(OnLogoutButtonClicked);
        }
    }
}