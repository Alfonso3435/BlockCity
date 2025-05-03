const express = require("express");
const mysql = require("mysql");
const bodyParser = require("body-parser");
const cors = require("cors");

const app = express();
const puerto = 3000;

app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true })); 

// Descripción: Este archivo implementa un servidor backend utilizando Node.js y Express, gestionando las solicitudes relacionadas con usuarios, cuestionarios, misiones, módulos, monedas y otros datos del juego. Proporciona endpoints para operaciones CRUD y sincronización con la base de datos MySQL.
// Autor: Mike Argumedo, Israel González y Alfonso Vega

const db = mysql.createConnection({
    host: "bd-cryptochicks.cmirgwrejba3.us-east-1.rds.amazonaws.com",
    user: "admin",
    password: "Cryptonenas",
    port: 3306,
    database: "CryptoGame"
});


db.connect((err) => {
    if (err) {
        console.error("Error de conexión MySQL:", err);
        return;
    }
    console.log("Conectado a DB MySQL RDS");
});



app.post("/unity/recibeJSON", (req, res) => {
    const { usuario, hora, tipo } = req.body;
    
    if (tipo === "inicio") {
        const query = "INSERT INTO Sesiones (usuario, hora_inicio) VALUES (?, ?)";
        db.query(query, [usuario, hora], (err, result) => {
            if (err) {
                console.error("Error al insertar:", err);
                return res.status(500).json({ error: "Error al insertar" });
            }
            res.json({ status: "Inicio guardado" });
        }); 
    } else if (tipo === "fin") {
        const query = "UPDATE Sesiones SET hora_fin = ? WHERE usuario = ? ORDER BY id DESC LIMIT 1";
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


    if (contrasena.length < 8) {
        return res.status(400).json({ error: "La contraseña debe tener al menos 8 caracteres." });
    }

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

        db.query(queryUsuario, [nombre_user], (err, resultsUsuario) => {
            if (err) {
                console.error("Error en verificación de usuario:", err);
                return res.status(500).json({ error: "Error del servidor" });
            }

            if (resultsUsuario.length > 0) {
                return res.status(409).json({ status: "EXISTE_USUARIO" });
            }


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
                    explicacion: row.explicacion,
                    tip: row.tip,
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



app.put("/items/update", (req, res) => {
    const { id_usuario, id_item, cantidad } = req.body;


    if (!id_usuario || !id_item || cantidad === undefined) {
        return res.status(400).json({ error: "Faltan campos requeridos" });
    }

    const query = `
        UPDATE Item_Usuario
        SET cantidad = ?
        WHERE id_usuario = ? AND id_item = ?
    `;

    db.query(query, [cantidad, id_usuario, id_item], (err, result) => {
        if (err) {
            console.error("Error al actualizar el item:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (result.affectedRows === 0) {
            return res.status(404).json({ error: "No se encontró el item para este usuario." });
        }

        res.json({ status: "UPDATE_OK", id_usuario, id_item, cantidad });
    });
});

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


app.post("/increment-quest-progress", (req, res) => {
    console.log("[MISIONES] Petición recibida en /increment-quest-progress");
    console.log("[MISIONES] Body recibido:", req.body);

    const { userId, questId, increment } = req.body;
    
    if (!userId || !questId || increment === undefined) {
        return res.status(400).json({ 
            success: false,
            error: "Se requieren userId, questId e increment"
        });
    }


    const checkQuery = `
        SELECT completado, reclamado FROM Usuario_quest 
        WHERE id_usuario = ? AND id_quest = ?
    `;
    
    db.query(checkQuery, [userId, questId], (err, checkResults) => {
        if (err) {
            console.error("[MISIONES] Error al verificar estado:", err);
            return res.status(500).json({ 
                success: false,
                error: "Error en la base de datos"
            });
        }

        if (checkResults.length > 0 && (checkResults[0].completado === 2 || checkResults[0].reclamado)) {
            return res.json({ 
                success: true,
                message: "Misión ya reclamada, no se actualiza"
            });
        }

        const updateQuery = `
            INSERT INTO Usuario_quest (id_usuario, id_quest, Progress)
            VALUES (?, ?, ?)
            ON DUPLICATE KEY UPDATE 
                Progress = LEAST(Progress + ?, (SELECT TargetProgress FROM Quests WHERE id_quest = ?)),
                completado = CASE 
                    WHEN reclamado = FALSE AND Progress + ? >= (SELECT TargetProgress FROM Quests WHERE id_quest = ?) 
                    THEN 1 
                    ELSE completado 
                END
        `;
        
        const params = [userId, questId, increment, increment, questId, increment, questId];
        
        db.query(updateQuery, params, (err, result) => {
            if (err) {
                console.error("[MISIONES] Error en la query:", err);
                return res.status(500).json({ 
                    success: false,
                    error: "Error en la base de datos"
                });
            }

            const statusQuery = `
                SELECT uq.Progress, uq.completado, uq.reclamado, q.TargetProgress 
                FROM Usuario_quest uq
                JOIN Quests q ON uq.id_quest = q.id_quest
                WHERE uq.id_usuario = ? AND uq.id_quest = ?
            `;
            
            db.query(statusQuery, [userId, questId], (err, missionStatus) => {
                if (err) {
                    console.error("[MISIONES] Error al verificar estado:", err);
                    return res.status(500).json({ 
                        success: false,
                        error: "Error al verificar estado"
                    });
                }

                if (missionStatus.length === 0) {
                    return res.status(404).json({ 
                        success: false,
                        error: "No se encontró la misión"
                    });
                }

                res.json({
                    success: true,
                    newProgress: missionStatus[0].Progress,
                    isCompleted: missionStatus[0].completado === 1,
                    isClaimed: missionStatus[0].reclamado === 1,
                    targetProgress: missionStatus[0].TargetProgress
                });
            });
        });
    });
});

app.post("/claim-quest-reward", (req, res) => {
    console.log("Solicitud para reclamar recompensa recibida:", req.body);
    
    const { userId, questId } = req.body;
    
    if (!userId || !questId) {
        console.error("Faltan parámetros requeridos");
        return res.status(400).json({ 
            success: false,
            error: "Se requieren userId y questId" 
        });
    }

    const checkQuery = `
        SELECT q.RewardCoins, uq.completado, uq.reclamado
        FROM Usuario_quest uq
        JOIN Quests q ON uq.id_quest = q.id_quest
        WHERE uq.id_usuario = ? AND uq.id_quest = ?
    `;

    db.query(checkQuery, [userId, questId], (err, results) => {
        if (err) {
            console.error("Error al verificar misión:", err);
            return res.status(500).json({ 
                success: false,
                error: "Error en la base de datos" 
            });
        }

        if (results.length === 0) {
            console.error("Misión no encontrada para el usuario");
            return res.status(404).json({ 
                success: false,
                error: "Misión no encontrada" 
            });
        }

        const mission = results[0];
        
        if (mission.completado !== 1 || mission.reclamado) {
            console.error("La misión no está lista para reclamar", {
                completado: mission.completado,
                reclamado: mission.reclamado
            });
            return res.status(400).json({ 
                success: false,
                error: "La misión no está lista para reclamar" 
            });
        }

        db.query("START TRANSACTION", (transactionErr) => {
            if (transactionErr) {
                console.error("Error en transacción:", transactionErr);
                return res.status(500).json({ 
                    success: false,
                    error: "Error al iniciar transacción" 
                });
            }

            const updateCoinsQuery = `
                UPDATE Usuarios 
                SET coins = coins + ?
                WHERE id_usuario = ?
            `;
            
            db.query(updateCoinsQuery, [mission.RewardCoins, userId], (coinsErr, coinsResult) => {
                if (coinsErr) {
                    return db.query("ROLLBACK", () => {
                        console.error("Error al actualizar monedas:", coinsErr);
                        res.status(500).json({ 
                            success: false,
                            error: "Error al actualizar monedas" 
                        });
                    });
                }

                const updateQuestQuery = `
                    UPDATE Usuario_quest
                    SET completado = 2, reclamado = TRUE
                    WHERE id_usuario = ? AND id_quest = ?
                `;
                
                db.query(updateQuestQuery, [userId, questId], (questErr, questResult) => {
                    if (questErr) {
                        return db.query("ROLLBACK", () => {
                            console.error("Error al actualizar misión:", questErr);
                            res.status(500).json({ 
                                success: false,
                                error: "Error al actualizar misión" 
                            });
                        });
                    }

                    db.query("COMMIT", (commitErr) => {
                        if (commitErr) {
                            console.error("Error al confirmar transacción:", commitErr);
                            return res.status(500).json({ 
                                success: false,
                                error: "Error al confirmar transacción" 
                            });
                        }

                        console.log("Recompensa reclamada con éxito para usuario:", userId, "misión:", questId);
                        res.json({ 
                            success: true,
                            coinsAwarded: mission.RewardCoins,
                            newStatus: 2
                        });
                    });
                });
            });
        });
    });
});

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

app.get("/coins/:id_usuario", (req, res) => {
    const id_usuario = req.params.id_usuario;

    if (!id_usuario) {
        return res.status(400).json({ error: "Faltan campos requeridos" });
    }

    const query = `
        SELECT coins
        FROM Usuarios
        WHERE id_usuario = ?;
    `;

    db.query(query, [id_usuario], (err, results) => {
        if (err) {
            console.error("Error al obtener las monedas:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(404).json({ error: "No se encontraron monedas para este usuario." });
        }

        res.json({ id_usuario, coins: results[0].coins });
    });
});

app.put("/coins/update", (req, res) => {
    const { id_usuario, coins } = req.body;

    if (!id_usuario || coins === undefined) {
        console.error("Faltan campos requeridos");
        return res.status(400).json({ 
            success: false,
            error: "Se requieren id_usuario y coins" 
        });
    }

    db.query("START TRANSACTION", (transactionErr) => {
        if (transactionErr) {
            console.error("Error al iniciar transacción:", transactionErr);
            return res.status(500).json({ 
                success: false,
                error: "Error al iniciar transacción" 
            });
        }

        const getQuery = "SELECT coins FROM Usuarios WHERE id_usuario = ?";
        db.query(getQuery, [id_usuario], (getErr, getResults) => {
            if (getErr) {
                return db.query("ROLLBACK", () => {
                    console.error("Error al obtener monedas:", getErr);
                    res.status(500).json({ 
                        success: false,
                        error: "Error al obtener monedas" 
                    });
                });
            }

            if (getResults.length === 0) {
                return db.query("ROLLBACK", () => {
                    console.error("Usuario no encontrado");
                    res.status(404).json({ 
                        success: false,
                        error: "Usuario no encontrado" 
                    });
                });
            }

            const currentCoins = getResults[0].coins;
            const newCoins = currentCoins + coins;

            if (newCoins < 0) {
                return db.query("ROLLBACK", () => {
                    console.error("Monedas insuficientes");
                    res.status(400).json({ 
                        success: false,
                        error: "No hay suficientes monedas" 
                    });
                });
            }

            const updateQuery = "UPDATE Usuarios SET coins = ? WHERE id_usuario = ?";
            db.query(updateQuery, [newCoins, id_usuario], (updateErr, updateResult) => {
                if (updateErr) {
                    return db.query("ROLLBACK", () => {
                        console.error("Error al actualizar monedas:", updateErr);
                        res.status(500).json({ 
                            success: false,
                            error: "Error al actualizar monedas" 
                        });
                    });
                }

                db.query("COMMIT", (commitErr) => {
                    if (commitErr) {
                        return db.query("ROLLBACK", () => {
                            console.error("Error al confirmar transacción:", commitErr);
                            res.status(500).json({ 
                                success: false,
                                error: "Error al confirmar transacción" 
                            });
                        });
                    }

                    console.log("Monedas actualizadas correctamente");
                    res.json({ 
                        success: true,
                        id_usuario,
                        newCoins,
                        amountChanged: coins
                    });
                });
            });
        });
    });
});

app.put("/coins/modify", (req, res) => {
    const { id_usuario, coins } = req.body;

    if (!id_usuario || coins === undefined) {
        console.error("Missing required fields");
        return res.status(400).json({
            success: false,
            error: "id_usuario and coins are required"
        });
    }

    db.query("START TRANSACTION", (transactionErr) => {
        if (transactionErr) {
            console.error("Error starting transaction:", transactionErr);
            return res.status(500).json({
                success: false,
                error: "Error starting transaction"
            });
        }

        const getQuery = "SELECT coins FROM Usuarios WHERE id_usuario = ?";
        db.query(getQuery, [id_usuario], (getErr, getResults) => {
            if (getErr) {
                return db.query("ROLLBACK", () => {
                    console.error("Error fetching coins:", getErr);
                    res.status(500).json({
                        success: false,
                        error: "Error fetching coins"
                    });
                });
            }

            if (getResults.length === 0) {
                return db.query("ROLLBACK", () => {
                    console.error("User not found");
                    res.status(404).json({
                        success: false,
                        error: "User not found"
                    });
                });
            }

            const currentCoins = getResults[0].coins;
            const newCoins = currentCoins + coins;

            if (newCoins < 0) {
                return db.query("ROLLBACK", () => {
                    console.error("Insufficient coins");
                    res.status(400).json({
                        success: false,
                        error: "Insufficient coins"
                    });
                });
            }

            const updateQuery = "UPDATE Usuarios SET coins = ? WHERE id_usuario = ?";
            db.query(updateQuery, [newCoins, id_usuario], (updateErr, updateResult) => {
                if (updateErr) {
                    return db.query("ROLLBACK", () => {
                        console.error("Error updating coins:", updateErr);
                        res.status(500).json({
                            success: false,
                            error: "Error updating coins"
                        });
                    });
                }

                db.query("COMMIT", (commitErr) => {
                    if (commitErr) {
                        return db.query("ROLLBACK", () => {
                            console.error("Error committing transaction:", commitErr);
                            res.status(500).json({
                                success: false,
                                error: "Error committing transaction"
                            });
                        });
                    }

                    console.log("Coins updated successfully");
                    res.json({
                        success: true,
                        id_usuario,
                        newCoins,
                        amountChanged: coins
                    });
                });
            });
        });
    });
});

app.get("/module/status", (req, res) => {
    const userId = req.query.userId;
    const moduleId = req.query.moduleId;

    if (!userId || !moduleId) {
        return res.status(400).json({ error: "Faltan parámetros requeridos" });
    }

    const query = `
        SELECT desbloqueado 
        FROM Usuario_Modulos 
        WHERE id_usuario = ? AND id_modulo = ?
    `;

    db.query(query, [userId, moduleId], (err, results) => {
        if (err) {
            console.error("Error al consultar módulo:", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(404).json({ error: "Módulo no encontrado para este usuario" });
        }

        res.json({ desbloqueado: results[0].desbloqueado });
    });
});

app.post("/module/unlock", (req, res) => {
    const { id_usuario, id_modulo, precio } = req.body;

    if (!id_usuario || !id_modulo || precio === undefined) {
        return res.status(400).json({ error: "Faltan campos requeridos" });
    }

    db.query("START TRANSACTION", (transactionErr) => {
        if (transactionErr) {
            return res.status(500).json({ error: "Error al iniciar transacción" });
        }

        const checkCoinsQuery = "SELECT coins FROM Usuarios WHERE id_usuario = ?";
        db.query(checkCoinsQuery, [id_usuario], (coinsErr, coinsResults) => {
            if (coinsErr) {
                return db.query("ROLLBACK", () => {
                    res.status(500).json({ error: "Error al verificar monedas" });
                });
            }

            if (coinsResults.length === 0) {
                return db.query("ROLLBACK", () => {
                    res.status(404).json({ error: "Usuario no encontrado" });
                });
            }

            const currentCoins = coinsResults[0].coins;
            if (currentCoins < precio) {
                return db.query("ROLLBACK", () => {
                    res.status(400).json({ success: false, error: "Monedas insuficientes" });
                });
            }

            const updateCoinsQuery = "UPDATE Usuarios SET coins = coins - ? WHERE id_usuario = ?";
            db.query(updateCoinsQuery, [precio, id_usuario], (updateErr, updateResult) => {
                if (updateErr) {
                    return db.query("ROLLBACK", () => {
                        res.status(500).json({ error: "Error al actualizar monedas" });
                    });
                }

                const unlockQuery = `
                    UPDATE Usuario_Modulos 
                    SET desbloqueado = TRUE 
                    WHERE id_usuario = ? AND id_modulo = ?
                `;
                db.query(unlockQuery, [id_usuario, id_modulo], (unlockErr, unlockResult) => {
                    if (unlockErr) {
                        return db.query("ROLLBACK", () => {
                            res.status(500).json({ error: "Error al desbloquear módulo" });
                        });
                    }

                    db.query("COMMIT", (commitErr) => {
                        if (commitErr) {
                            return db.query("ROLLBACK", () => {
                                res.status(500).json({ error: "Error al confirmar transacción" });
                            });
                        }


                        db.query(checkCoinsQuery, [id_usuario], (err, finalCoins) => {
                            if (err) {
                                console.error("Error al obtener monedas finales", err);
                                
                                return res.json({ 
                                    success: true, 
                                    nuevas_monedas: currentCoins - precio 
                                });
                            }

                            res.json({ 
                                success: true, 
                                nuevas_monedas: finalCoins[0].coins 
                            });
                        });
                    });
                });
            });
        });
    });
});


app.get("/user/name", (req, res) => {
    const userId = req.query.id;

    if (!userId) {
        return res.status(400).json({ 
            success: false,
            error: "Se requiere el ID de usuario" 
        });
    }

    const query = "SELECT nombre_user FROM Usuarios WHERE id_usuario = ?";
    
    db.query(query, [userId], (err, results) => {
        if (err) {
            console.error("Error al obtener nombre:", err);
            return res.status(500).json({ 
                success: false,
                error: "Error en la base de datos",
                details: err.message
            });
        }

        if (results.length === 0) {
            return res.status(404).json({ 
                success: false,
                error: "Usuario no encontrado" 
            });
        }

        res.json({ 
            success: true,
            data: {
                id_usuario: userId,
                nombre_user: results[0].nombre_user
            }
        });
    });
});

app.get("/memory/:id_memorama", (req, res) => {
    const id_memorama = req.params.id_memorama;
    
    const query = `
        SELECT concepto,
        definicion
        FROM Respuestas_Memorama
        WHERE id_memorama = ?
    `;

    db.query(query, [id_memorama], (err, results) => {
        if (err) {
            console.error("Error al obtener las respuestas del memorama", err);
            return res.status(500).json({ error: "Error en la base de datos" });
        }

        if (results.length === 0) {
            return res.status(404).json({ error: "No se encontraron elementos en este memorama" });
        }

        //res.json({ id_usuario, coins: results[0].coins });
        res.json(results);
    });
});

app.put("/quiz-usuario/update", (req, res) => {
    const { id_quiz, id_usuario, desbloqueado, estrellas, puntos, completado } = req.body;

    if (!id_quiz || !id_usuario || desbloqueado === undefined || estrellas === undefined || puntos === undefined || completado === undefined) {
        return res.status(400).json({
            success: false,
            error: "All fields (id_quiz, id_usuario, desbloqueado, estrellas, puntos, completado) are required"
        });
    }

    const query = `
        UPDATE Quiz_Usuario
        SET 
            desbloqueado = COALESCE(?, desbloqueado),
            estrellas = COALESCE(?, estrellas),
            puntos = COALESCE(?, puntos),
            completado = COALESCE(?, completado)
        WHERE 
            id_quiz = ? AND 
            id_usuario = ?
    `;

    const params = [desbloqueado, estrellas, puntos, completado, id_quiz, id_usuario];

    db.query(query, params, (err, result) => {
        if (err) {
            console.error("Error updating Quiz_Usuario:", err);
            return res.status(500).json({
                success: false,
                error: "Error updating the database"
            });
        }

        if (result.affectedRows === 0) {
            return res.status(404).json({
                success: false,
                error: "No matching record found for the given id_quiz and id_usuario"
            });
        }

        res.json({
            success: true,
            message: "Quiz_Usuario updated successfully",
            affectedRows: result.affectedRows
        });
    });
});

app.put("/quiz-usuario/unlock", (req, res) => {
    const { id_quiz, id_usuario, desbloqueado } = req.body;

    if (!id_quiz || !id_usuario || desbloqueado === undefined) {
        return res.status(400).json({
            success: false,
            error: "id_quiz, id_usuario, and desbloqueado are required"
        });
    }


    const query = `
        UPDATE Quiz_Usuario
        SET desbloqueado = ?
        WHERE id_quiz = ? AND id_usuario = ?
    `;

    const params = [desbloqueado, id_quiz, id_usuario];

    db.query(query, params, (err, result) => {
        if (err) {
            console.error("Error updating desbloqueado in Quiz_Usuario:", err);
            return res.status(500).json({
                success: false,
                error: "Error updating the database"
            });
        }

        if (result.affectedRows === 0) {
            return res.status(404).json({
                success: false,
                error: "No matching record found for the given id_quiz and id_usuario"
            });
        }

        res.json({
            success: true,
            message: "Quiz_Usuario desbloqueado updated successfully",
            affectedRows: result.affectedRows
        });
    });
});

app.get("/module/quiz-stars", (req, res) => {
    const { id_modulo, id_usuario } = req.query;

    if (!id_modulo || !id_usuario) {
        return res.status(400).json({
            success: false,
            error: "Both id_modulo and id_usuario are required"
        });
    }

    const query = `
        SELECT 
            q.id_quiz, 
            q.nombre_quiz, 
            IFNULL(qu.estrellas, 0) AS estrellas
        FROM Quizzes q
        LEFT JOIN Quiz_Usuario qu 
            ON q.id_quiz = qu.id_quiz AND qu.id_usuario = ?
        WHERE q.id_modulo = ?
    `;

    db.query(query, [id_usuario, id_modulo], (err, results) => {
        if (err) {
            console.error("Error fetching quiz stars:", err);
            return res.status(500).json({
                success: false,
                error: "Error in the database"
            });
        }

        if (results.length === 0) {
            return res.status(404).json({
                success: false,
                error: "No quizzes found for the given module and user"
            });
        }

        res.json({
            success: true,
            quizzes: results
        });
    });
});

app.get("/module/stars", (req, res) => {
    const { id_modulo, id_usuario } = req.query;

    if (!id_modulo || !id_usuario) {
        return res.status(400).json({
            success: false,
            error: "Both id_modulo and id_usuario are required"
        });
    }

    const query = `
        SELECT SUM(qu.estrellas) AS total_stars
        FROM Quiz_Usuario qu
        JOIN Quizzes q ON qu.id_quiz = q.id_quiz
        WHERE q.id_modulo = ? AND qu.id_usuario = ?
    `;

    db.query(query, [id_modulo, id_usuario], (err, results) => {
        if (err) {
            console.error("Error calculating total stars:", err);
            return res.status(500).json({
                success: false,
                error: "Error in the database"
            });
        }

        const totalStars = results[0]?.total_stars || 0;

        res.json({
            success: true,
            id_modulo,
            id_usuario,
            total_stars: totalStars
        });
    });
});

app.get("/module/levels", (req, res) => {
    const { id_modulo, id_usuario } = req.query;

    if (!id_modulo || !id_usuario) {
        return res.status(400).json({
            success: false,
            error: "Both id_modulo and id_usuario are required"
        });
    }

    const query = `
        SELECT 
            q.id_quiz, 
            q.nombre_quiz, 
            IFNULL(qu.desbloqueado, FALSE) AS desbloqueado
        FROM Quizzes q
        LEFT JOIN Quiz_Usuario qu 
            ON q.id_quiz = qu.id_quiz AND qu.id_usuario = ?
        WHERE q.id_modulo = ?
    `;

    db.query(query, [id_usuario, id_modulo], (err, results) => {
        if (err) {
            console.error("Error fetching levels:", err);
            return res.status(500).json({
                success: false,
                error: "Error in the database"
            });
        }

        if (results.length === 0) {
            return res.status(404).json({
                success: false,
                error: "No levels found for the given module and user"
            });
        }

        res.json({
            success: true,
            levels: results
        });
    });
});

app.post("/logout", (req, res) => {
    const { correo, hora, tipo, id_usuario } = req.body;

    if (!correo || !hora || !tipo || !id_usuario) {
        console.error("Faltan datos requeridos para el logout");
        return res.status(400).json({ 
            success: false,
            error: "Faltan datos requeridos" 
        });
    }

    const query = `
        INSERT INTO Sesiones (id_usuario, correo, hora_fin, tipo) 
        VALUES (?, ?, ?, ?)
    `;

    db.query(query, [id_usuario, correo, hora, tipo], (err, result) => {
        if (err) {
            console.error("Error al registrar logout:", err);
            return res.status(500).json({ 
                success: false,
                error: "Error al registrar logout" 
            });
        }

        res.json({ 
            success: true,
            message: "Logout registrado correctamente"
        });
    });
});

app.get("/hangman/:id_quiz", (req, res) => {
    const id_quiz = req.params.id_quiz;

    const query = `
        SELECT 
            r.id_respuesta as id,
            r.id_ahorcado,
            r.concepto as word,
            r.definicion as definition
        FROM Respuestas_Ahorcado r
        INNER JOIN Ahorcados a ON r.id_ahorcado = a.id_ahorcado
        WHERE a.id_quiz = ?
        ORDER BY r.id_respuesta
    `;

    db.query(query, [id_quiz], (err, results) => {
        if (err) {
            console.error("Database error:", err);
            return res.status(500).json({
                success: false,
                error: "Database query failed"
            });
        }

        if (!results || results.length === 0) {
            return res.status(404).json({
                success: false,
                error: "No data found"
            });
        }

        res.json({
            success: true,
            items: results
        });
    });
});

app.listen(puerto, () => {
    console.log(`Servidor escuchando`);
});