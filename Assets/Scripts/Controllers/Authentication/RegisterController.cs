using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;

public class RegisterController : MonoBehaviour
{
    private UIDocument register;
    private Button botonRegreso;
    private Button botonSubir;

    private TextField campoCorreo;
    private TextField campoUsuario;
    private TextField campoPassword;
    private TextField campoPasswordRepetida;
    private DropdownField campoNacionalidad;
    private TextField campoDia;
    private TextField campoMes;
    private TextField campoAno;
    private Label mensaje;

    void OnEnable()
    {
        register = GetComponent<UIDocument>();
        var root = register.rootVisualElement;

        botonRegreso = root.Q<Button>("Regreso");
        botonRegreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "Login");

        // Obtener campos
        campoCorreo = root.Q<TextField>("Correo");
        campoUsuario = root.Q<TextField>("NombreUsuario");
        campoPassword = root.Q<TextField>("InputContrasena");
        campoPasswordRepetida = root.Q<TextField>("RepetirContrasena");
        campoNacionalidad = root.Q<DropdownField>("Nacionalidad");
        campoDia = root.Q<TextField>("Dia");
        campoMes = root.Q<TextField>("Mes");
        campoAno = root.Q<TextField>("Ano");
        botonSubir = root.Q<Button>("Subir");
        mensaje = root.Q<Label>("Mensaje");

        botonSubir.clicked += () =>
        {
            string correo = campoCorreo.text.Trim();
            string usuario = campoUsuario.text.Trim();
            string password = campoPassword.text.Trim();
            string password2 = campoPasswordRepetida.text.Trim();
            string nacionalidad = campoNacionalidad.value;
            string fechaNacimiento = $"{campoAno.text}-{campoMes.text.PadLeft(2, '0')}-{campoDia.text.PadLeft(2, '0')}";

            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(usuario) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password2) ||
                string.IsNullOrEmpty(nacionalidad) || string.IsNullOrEmpty(campoDia.text) ||
                string.IsNullOrEmpty(campoMes.text) || string.IsNullOrEmpty(campoAno.text))
            {
                mensaje.text = "Por favor llena todos los campos.";
                return;
            }

            if (password.Length < 8)
            {
                mensaje.text = "La contraseña debe tener al menos 8 caracteres.";
                return;
            }

            if (password != password2)
            {
                mensaje.text = "Las contraseñas no coinciden.";
                return;
            }

            UsuarioRegistro datos = new UsuarioRegistro
            {
                correo = correo,
                contrasena = password,
                nombre_user = usuario,
                nombre_comp = usuario,
                nacionalidad = nacionalidad,
                fecha_nacimiento = fechaNacimiento
            };

            StartCoroutine(EnviarRegistro(datos));
        };
    }

    private IEnumerator EnviarRegistro(UsuarioRegistro datos)
    {
        string json = JsonUtility.ToJson(datos);

        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/registro", "POST");
        
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.responseCode == 409)
        {
            string respuesta = request.downloadHandler.text;
            if (respuesta.Contains("EXISTE_CORREO"))
                mensaje.text = "Este correo ya está registrado.";
            else if (respuesta.Contains("EXISTE_USUARIO"))
                mensaje.text = "Este nombre de usuario ya está en uso.";
            else
                mensaje.text = "Conflicto al registrar.";
        }
        else if (request.result == UnityWebRequest.Result.Success)
        {
            string respuesta = request.downloadHandler.text;
            if (respuesta.Contains("REGISTRO_OK"))
            {
                mensaje.text = "Registro exitoso. Redirigiendo...";
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Login");
            }
            else
            {
                mensaje.text = "Error al registrar.";
            }
        }
        else
        {
            mensaje.text = "Error de conexión.";
        }
    }

    private void IniciarJuego(ClickEvent evt, String escena)
    {
        SceneManager.LoadScene(escena);
    }

    [Serializable]
    public class UsuarioRegistro
    {
        public string correo;
        public string contrasena;
        public string nombre_user;
        public string nombre_comp;
        public string nacionalidad;
        public string fecha_nacimiento;
    }
}
