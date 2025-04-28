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

    //Error
    private Button contenedorError;
    private Button cerrarError;
    private Label mensaje;

    private TextField campoCorreo;
    private TextField campoNombre;
    private TextField campoUsuario;
    private TextField campoPassword;
    private TextField campoPasswordRepetida;
    private DropdownField campoNacionalidad;
    private DropdownField campoGenero;
    private TextField campoDia;
    private TextField campoMes;
    private TextField campoAno;
    

    void OnEnable()
    {
        register = GetComponent<UIDocument>();
        var root = register.rootVisualElement;

        botonRegreso = root.Q<Button>("Regreso");
        botonRegreso.RegisterCallback<ClickEvent, String>(IniciarJuego, "Login");

        //Error
        contenedorError = root.Q<Button>("ErrorPopUp");
        mensaje = root.Q<Label>("Mensaje");
        cerrarError = root.Q<Button>("CerrarPopUp");

        cerrarError.RegisterCallback<ClickEvent>(CerrarError);
        contenedorError.RegisterCallback<ClickEvent>(CerrarError);
        contenedorError.style.display = DisplayStyle.None;

        // Obtener campos
        campoCorreo = root.Q<TextField>("Correo");
        campoNombre = root.Q<TextField>("Nombre");
        campoUsuario = root.Q<TextField>("NombreUsuario");
        campoPassword = root.Q<TextField>("InputContrasena");
        campoPasswordRepetida = root.Q<TextField>("RepetirContrasena");
        campoNacionalidad = root.Q<DropdownField>("Nacionalidad");
        campoGenero = root.Q<DropdownField>("Genero");
        campoDia = root.Q<TextField>("Dia");
        campoMes = root.Q<TextField>("Mes");
        campoAno = root.Q<TextField>("Ano");
        botonSubir = root.Q<Button>("Subir");

        botonSubir.clicked += () =>
        {
            string correo = campoCorreo.text.Trim();
            string nombre = campoNombre.text.Trim();
            string usuario = campoUsuario.text.Trim();
            string password = campoPassword.text.Trim();
            string password2 = campoPasswordRepetida.text.Trim();
            string nacionalidad = campoNacionalidad.value;
            string genero = campoGenero.value;
            string fechaNacimiento = $"{campoAno.text}-{campoMes.text.PadLeft(2, '0')}-{campoDia.text.PadLeft(2, '0')}";
            //Aquí

            if (string.IsNullOrEmpty(correo) ||string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(usuario) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password2) ||
                string.IsNullOrEmpty(nacionalidad) ||string.IsNullOrEmpty(genero) || string.IsNullOrEmpty(campoDia.text) ||
                string.IsNullOrEmpty(campoMes.text) || string.IsNullOrEmpty(campoAno.text))
            {
                contenedorError.style.display = DisplayStyle.Flex;
                mensaje.text = "Please \nfill all \ntext fields.";
                return;
            }

            if (password.Length < 8)
            {
                contenedorError.style.display = DisplayStyle.Flex;
                mensaje.text = "The password \nmust have \nat least \n8 characters.";
                return;
            }

            if (password != password2)
            {
                contenedorError.style.display = DisplayStyle.Flex;
                mensaje.text = "The \npasswords \ndo not match.";
                return;
            }

            UsuarioRegistro datos = new UsuarioRegistro
            {
                correo = correo,
                nombre_completo = nombre,
                genero = genero,
                contrasena = password,
                nombre_user = usuario,
                nombre_comp = usuario,
                nacionalidad = nacionalidad,
                fecha_nacimiento = fechaNacimiento
            };

            StartCoroutine(EnviarRegistro(datos));
        };
    }

    private void CerrarError(ClickEvent evt)
    {
        if (contenedorError.style.display == DisplayStyle.Flex)
        {
            contenedorError.style.display = DisplayStyle.None;
            return;
        }
        
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
                mensaje.text = "This mail \nis already \nregistered.";
            else if (respuesta.Contains("EXISTE_USUARIO"))
                mensaje.text = "This user name \nis already \nregistered.";
            else
                mensaje.text = "Unexpected error \nplease try again.";
        }
        else if (request.result == UnityWebRequest.Result.Success)
        {
            string respuesta = request.downloadHandler.text;
            if (respuesta.Contains("REGISTRO_OK"))
            {
                mensaje.text = "User registered\n Redirecting...";
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Login");
            }
            else
            {
                mensaje.text = "Unexpected error \nplease try again.";
            }
        }
        else
        {
            mensaje.text = "Connection error \nor invalid data.";
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

        public string nombre_completo;
        public string contrasena;
        public string nombre_user;
        public string nombre_comp;
        public string nacionalidad;
        public string genero;
        public string fecha_nacimiento;
    }
}
