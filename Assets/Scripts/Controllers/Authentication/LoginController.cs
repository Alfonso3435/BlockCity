using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;

public class LoginController : MonoBehaviour
{
    private UIDocument login;
    private Button botonOlvido;
    private Button botonInicioSesion;
    private Button botonRegistro;
    private Button botonRegreso;

    private TextField campoCorreo;
    private TextField campoPassword;
    private Label mensaje;

    void OnEnable()
    {
        login = GetComponent<UIDocument>();
        var root = login.rootVisualElement;
        botonOlvido = root.Q<Button>("Olvido");
        botonInicioSesion = root.Q<Button>("Login");
        botonRegistro = root.Q<Button>("Registro");
        botonRegreso = root.Q<Button>("Regreso");

        campoCorreo = root.Q<TextField>("Correo");
        campoPassword = root.Q<TextField>("Contrasena");
        mensaje = root.Q<Label>("Mensaje");

        if (mensaje == null)
        {
            mensaje = new Label();
            root.Add(mensaje);
        }

        botonInicioSesion.clicked += () =>
        {
            string correo = campoCorreo.text.Trim();
            string contrasena = campoPassword.text.Trim();

            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                mensaje.text = "Completa ambos campos.";
                return;
            }

            mensaje.text = "Conectando...";
            StartCoroutine(IniciarSesion(correo, contrasena));
        };

        botonRegistro.RegisterCallback<ClickEvent, String>(IniciarJuego, "Register");
        botonRegreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "Menu");
    }

    private IEnumerator IniciarSesion(string correo, string contrasena)
    {
        Credenciales datos = new Credenciales
        {
            correo = correo,
            contrasena = contrasena
        };

        string json = JsonUtility.ToJson(datos);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        //string url = $"http://localhost:3000/quiz/{NivelID}";

        //UnityWebRequest request = new UnityWebRequest("http://192.168.100.143:3000/login", "POST");
        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string respuesta = request.downloadHandler.text;
            Debug.Log("Respuesta del servidor: " + respuesta);

            if (respuesta.Contains("LOGIN_OK"))
            {
                mensaje.text = "Inicio de sesión exitoso.";
                yield return new WaitForSeconds(1f);
                DBQuizReqHolder.Instance.SetIsLoggedIn(true);
                SceneManager.LoadScene("Menu");
            }
            else
            {
                mensaje.text = "Correo o contraseña incorrectos.";
            }
        }
        else if (request.responseCode == 401)
        {
            mensaje.text = "Correo o contraseña incorrectos.";
        }
        else
        {
            mensaje.text = "Error de conexión: " + request.responseCode;
        }
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }

    [Serializable]
    public class Credenciales
    {
        public string correo;
        public string contrasena;
    }
}
