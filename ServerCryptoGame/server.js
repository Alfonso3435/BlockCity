const express = require("express");
const mysql = require("mysql");
const bodyParser = require("body-parser");
const cors = require("cors");

const app = express();
const puerto = 3000;

app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true })); // Añade esta línea

/*
const db = mysql.createConnection({
    host: "localhost",
    // host: "10.48.108.185",
    user: "root",
    password: "",
    port: 3306,
    database: "CryptoGame",
});
*/

const db = mysql.createConnection({
    host: "bd-cryptochicks.cmirgwrejba3.us-east-1.rds.amazonaws.com",
    user: "admin", // Cambia esto por tu usuario real de RDS
    password: "Cryptonenas", // Cambia esto por tu contraseña real de RDS
    port: 3306,
    database: "CryptoGame"
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
                INSERT INTO Usuarios (correo, contrasena, nombre_user, nombre_comp, nacionalidad, fecha_nacimiento)
                VALUES (?, ?, ?, ?, ?, ?)
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
            p.explicacion, 
            p.tip, 
            r.indice, 
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
                    explicacion: row.explicacion, // Agregar explicacion
                    tip: row.tip,                 // Agregar tip
                    respuestas: []
                };
            }

            preguntasMap[row.id_pregunta].respuestas.push({
                indice: row.indice,
                respuesta: row.respuesta
            });
        });

        const preguntas = Object.values(preguntasMap);
        res.json(preguntas);
    });
});

app.get("/items/:id_usuario", (req, res) => {
    const id_usuario = req.params.id_usuario;

    const query = `
        SELECT 
            id_item,
            cantidad 
        FROM Item_Usuario
        WHERE id_usuario = ?;`;

    db.query(query, [id_usuario], (err, results) => {
        if (err) {
            console.error("Error al obtener los items:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(404).json({ error: "No se encontraron items para este usuario." });
        }

        res.json(results);
    });
});

//Misiones
app.get("/quests", (req, res) => {
    const query = `
       SELECT 
            id_quest,
            nombre,
            descrip_quest,
            TargetProgress,
            RewardCoins
        FROM Quests;`;

    db.query(query, (err, results) => {
        if (err) {
            console.error("Error al obtener los quests:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(404).json({ error: "No se encontraron quests" });
        }

        res.json(results);
    });
});

// Añade estos endpoints al servidor (app.js)

// Obtener misiones con progreso del usuario
// Endpoint para incrementar progreso de misiones
app.post("/increment-quest-progress", (req, res) => {
    console.log("[MISIONES] Petición recibida en /increment-quest-progress");
    console.log("[MISIONES] Body recibido:", req.body);

    const { userId, questId, increment } = req.body;
    
    if (!userId || !questId || increment === undefined) {
        console.error("[MISIONES] Faltan parámetros requeridos");
        return res.status(400).json({ 
            success: false,
            error: "Se requieren userId, questId e increment",
            received: req.body
        });
    }

    const query = `
        INSERT INTO Usuario_quest (id_usuario, id_quest, Progress)
        VALUES (?, ?, ?)
        ON DUPLICATE KEY UPDATE 
            Progress = LEAST(Progress + ?, (SELECT TargetProgress FROM Quests WHERE id_quest = ?)),
            completado = CASE WHEN Progress + ? >= (SELECT TargetProgress FROM Quests WHERE id_quest = ?) THEN 1 ELSE completado END
    `;
    
    const params = [userId, questId, increment, increment, questId, increment, questId];
    
    console.log("[MISIONES] Ejecutando query:", query);
    console.log("[MISIONES] Parámetros:", params);

    // Ejecutar la primera query
    db.query(query, params, (err, result) => {
        if (err) {
            console.error("[MISIONES] Error en la query:", err);
            return res.status(500).json({ 
                success: false,
                error: "Error en la base de datos",
                details: err.message
            });
        }

        // Obtener el nuevo estado
        const statusQuery = `
            SELECT uq.Progress, uq.completado, q.TargetProgress 
            FROM Usuario_quest uq
            JOIN Quests q ON uq.id_quest = q.id_quest
            WHERE uq.id_usuario = ? AND uq.id_quest = ?
        `;
        
        db.query(statusQuery, [userId, questId], (err, missionStatus) => {
            if (err) {
                console.error("[MISIONES] Error al verificar estado:", err);
                return res.status(500).json({ 
                    success: false,
                    error: "Error al verificar estado",
                    details: err.message
                });
            }

            if (missionStatus.length === 0) {
                console.error("[MISIONES] No se encontró la misión actualizada");
                return res.status(404).json({ 
                    success: false,
                    error: "No se encontró la misión actualizada"
                });
            }

            const responseData = {
                success: true,
                newProgress: missionStatus[0].Progress,
                isCompleted: missionStatus[0].completado === 1,
                targetProgress: missionStatus[0].TargetProgress
            };

            console.log("[MISIONES] Respuesta exitosa:", responseData);
            res.json(responseData);
        });
    });
});

// Endpoint para reclamar recompensa (modificado)
app.post("/claim-quest-reward", (req, res) => {
    const { userId, questId } = req.body;

    if (!userId || !questId) {
        return res.status(400).json({ error: "Faltan parámetros requeridos" });
    }

    // Verificar que la misión está completada pero no reclamada
    const checkQuery = `
        SELECT q.RewardCoins, uq.completado, uq.Progress, q.TargetProgress
        FROM Usuario_quest uq
        JOIN Quests q ON uq.id_quest = q.id_quest
        WHERE uq.id_usuario = ? AND uq.id_quest = ? AND uq.completado = 1 AND uq.reclamado = FALSE
    `;

    db.query(checkQuery, [userId, questId], (err, results) => {
        if (err) {
            console.error("Error al verificar misión:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(400).json({ error: "La misión no está lista para reclamar" });
        }

        const mission = results[0];
        
        // Actualizar monedas y marcar como reclamada
        const updateQuery = `
            START TRANSACTION;
            
            UPDATE Usuarios 
            SET coins = coins + ?
            WHERE id_usuario = ?;
            
            UPDATE Usuario_quest
            SET completado = 2, reclamado = TRUE
            WHERE id_usuario = ? AND id_quest = ?;
            
            COMMIT;
        `;

        db.query(updateQuery, [mission.RewardCoins, userId, userId, questId], (err, results) => {
            if (err) {
                console.error("Error al reclamar recompensa:", err);
                return res.status(500).json({ error: "Error en la base de datos" });
            }

            res.json({ 
                success: true, 
                coinsAwarded: mission.RewardCoins,
                newStatus: 2
            });
        });
    });
});

// Endpoint para obtener misiones del usuario (modificado)
app.get("/user-quests/:userId", (req, res) => {
    const userId = req.params.userId;

    const query = `
        SELECT 
            q.id_quest,
            q.nombre,
            q.descrip_quest,
            q.TargetProgress,
            q.RewardCoins,
            IFNULL(uq.Progress, 0) AS userProgress,
            IFNULL(uq.completado, 0) AS completado,
            IFNULL(uq.reclamado, FALSE) AS reclamado
        FROM Quests q
        LEFT JOIN Usuario_quest uq ON q.id_quest = uq.id_quest AND uq.id_usuario = ?
        ORDER BY q.id_quest;
    `;

    db.query(query, [userId], (err, results) => {
        if (err) {
            console.error("Error al obtener misiones:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }
        res.json(results);
    });
});

// Actualizar progreso de misión
app.post("/update-quest-progress", (req, res) => {
    const { userId, questId, progress } = req.body;

    if (!userId || !questId || progress === undefined) {
        return res.status(400).json({ error: "Faltan parámetros requeridos" });
    }

    const query = `
        INSERT INTO Usuario_quest (id_usuario, id_quest, Progress, completado)
        VALUES (?, ?, ?, ?)
        ON DUPLICATE KEY UPDATE 
            Progress = VALUES(Progress),
            completado = CASE WHEN VALUES(Progress) >= (SELECT TargetProgress FROM Quests WHERE id_quest = ?) THEN 1 ELSE completado END
    `;

    db.query(query, [userId, questId, progress, progress, questId], (err, result) => {
        if (err) {
            console.error("Error al actualizar progreso:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        res.json({ success: true, affectedRows: result.affectedRows });
    });
});





app.listen(puerto, () => {
    console.log(`Servidor escuchando en http://localhost:${puerto}`);
});