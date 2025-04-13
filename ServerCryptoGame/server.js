const express = require("express");
const mysql = require("mysql");
const bodyParser = require("body-parser");
const cors = require("cors");

const app = express();
const puerto = 3000;

app.use(cors());
app.use(bodyParser.json());

const db = mysql.createConnection({
    host: "localhost",
    user: "root",
    password: "",
    port: 3306,
    database: "CryptoGame",
});

// Conecta con la base de datos
db.connect((err) => {
    if (err) {
        console.error("Error de conexión MySQL:", err);
        return;
    }
    console.log("Conectado a MySQL (XAMPP)");
});

// Ruta que recibe JSON desde Unity
app.post("/unity/recibeJSON", (req, res) => {
    const { usuario, hora, tipo } = req.body;
    
    if (tipo === "inicio") {
        const query = "INSERT INTO sesiones (usuario, hora_inicio) VALUES (?, ?)";
        db.query(query, [usuario, hora], (err, result) => {
            if (err) {
                console.error("Error al insertar:", err);
                return res.status(500).json({ error: "Error al insertar" });
            }
            res.json({ status: "Inicio guardado" });
        }); 
    } else if (tipo === "fin") {
        const query = "UPDATE sesiones SET hora_fin = ? WHERE usuario = ? ORDER BY id DESC LIMIT 1";
        db.query(query, [hora, usuario], (err, result) => {
            if (err) {
                console.error("Error al actualizar:", err);
                return res.status(500).json({ error: "Error al actualizar" });
            }
            res.json({ status: "Fin actualizado" });
        });
    } else {
        res.status(400).json({ error: "Tipo inválido" });
    }
});

app.post("/registro", (req, res) => {
    const {
        correo,
        contrasena,
        nombre_user,
        nombre_comp,
        nacionalidad,
        fecha_nacimiento
    } = req.body;

    if (
        !correo || !contrasena || !nombre_user || !nombre_comp ||
        !nacionalidad || !fecha_nacimiento
    ) {
        return res.status(400).json({ error: "Faltan campos requeridos" });
    }

    // Verificar si la contraseña tiene al menos 8 caracteres
    if (contrasena.length < 8) {
        return res.status(400).json({ error: "La contraseña debe tener al menos 8 caracteres." });
    }

    // Verificar si el correo ya está registrado
    const queryCorreo = "SELECT * FROM Usuarios WHERE correo = ?";
    const queryUsuario = "SELECT * FROM Usuarios WHERE nombre_user = ?";
    db.query(queryCorreo, [correo], (err, resultsCorreo) => {
        if (err) {
            console.error("Error en verificación de correo:", err);
            return res.status(500).json({ error: "Error del servidor" });
        }

        if (resultsCorreo.length > 0) {
            return res.status(409).json({ status: "EXISTE_CORREO" });
        }

        // Verificar si el nombre de usuario ya existe
        db.query(queryUsuario, [nombre_user], (err, resultsUsuario) => {
            if (err) {
                console.error("Error en verificación de usuario:", err);
                return res.status(500).json({ error: "Error del servidor" });
            }

            if (resultsUsuario.length > 0) {
                return res.status(409).json({ status: "EXISTE_USUARIO" });
            }

            // Insertar nuevo usuario
            const insertQuery = `
                INSERT INTO Usuarios (correo, contrasena, nombre_user, nombre_comp, nacionalidad, fecha_nacimiento, vidas)
                VALUES (?, ?, ?, ?, ?, ?, 3)
            `;
            db.query(insertQuery, [correo, contrasena, nombre_user, nombre_comp, nacionalidad, fecha_nacimiento], (err) => {
                if (err) {
                    console.error("Error al registrar:", err);
                    return res.status(500).json({ error: "Error al registrar" });
                }
                res.json({ status: "REGISTRO_OK" });
            });
        });
    });
});

// Ruta de inicio de sesión
app.post("/login", (req, res) => {
    const { correo, contrasena } = req.body;

    if (!correo || !contrasena) {
        return res.status(400).json({ error: "Faltan campos" });
    }

    const query = `
        SELECT id_usuario, nombre_user FROM Usuarios
        WHERE correo = ? AND contrasena = ?
    `;
    db.query(query, [correo, contrasena], (err, results) => {
        if (err) {
            console.error("Error en login:", err);
            return res.status(500).json({ error: "Error del servidor" });
        }

        if (results.length === 0) {
            return res.status(401).json({ status: "LOGIN_FAIL" });
        }

        const user = results[0];
        res.json({
            status: "LOGIN_OK",
            id_usuario: user.id_usuario,
            nombre_user: user.nombre_user
        });
    });
});

app.get("/quiz/:id_quiz", (req, res) => {
    const id_quiz = req.params.id_quiz;

    const query = `
        SELECT 
            p.id_pregunta, 
            p.pregunta, 
            p.indice_correcto, 
            r.indice AS respuesta_indice, 
            r.respuesta 
        FROM Preguntas p
        JOIN Respuestas_Quiz r ON p.id_pregunta = r.id_pregunta
        WHERE p.id_quiz = ?
        ORDER BY p.id_pregunta, r.indice
    `;

    db.query(query, [id_quiz], (err, results) => {
        if (err) {
            console.error("Error al obtener preguntas:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        const preguntasMap = {};

        results.forEach(row => {
            if (!preguntasMap[row.id_pregunta]) {
                preguntasMap[row.id_pregunta] = {
                    id_pregunta: row.id_pregunta,
                    pregunta: row.pregunta,
                    indice_correcto: row.indice_correcto,
                    respuestas: []
                };
            }

            preguntasMap[row.id_pregunta].respuestas.push({
                indice: row.respuesta_indice,
                respuesta: row.respuesta
            });
        });

        const preguntas = Object.values(preguntasMap);
        res.json(preguntas);
    });
});



app.listen(puerto, () => {
    console.log(`Servidor escuchando en http://localhost:${puerto}`);
});