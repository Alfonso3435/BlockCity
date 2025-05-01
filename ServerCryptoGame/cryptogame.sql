
DROP DATABASE IF EXISTS Cryptogame;
CREATE DATABASE Cryptogame;
USE CryptoGame;

CREATE TABLE UsuariosAdmin (
    id_usuario_admin INT AUTO_INCREMENT PRIMARY KEY,
    contrasena_hash  VARCHAR(255) NOT NULL,
    fecha_registro   TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    email            VARCHAR(321) NOT NULL UNIQUE,
    username         VARCHAR(100) NOT NULL UNIQUE,
    rol              ENUM('Admin', 'Analytical') NOT NULL,
    avatar_url       VARCHAR(255),
    twitter          VARCHAR(255),
    linkedin         VARCHAR(255),
    github           VARCHAR(255)
);


CREATE TABLE Usuarios(
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nacionalidad VARCHAR(50) NOT NULL,
    fecha_nacimiento DATE NOT NULL,
    contrasena VARCHAR(30) NOT NULL,
    correo VARCHAR (321) NOT NULL UNIQUE, /*Máxima longitud permitida de un correo*/
    coins INT NOT NULL DEFAULT 0 CHECK(coins >= 0),
    nombre_user VARCHAR(30) NOT NULL,
    genero VARCHAR(10) NOT NULL,
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP(),
    nombre_comp VARCHAR(150) NOT NULL,
    analyst BOOL DEFAULT FALSE,
    administrator BOOL DEFAULT FALSE,
    CHECK (CHAR_LENGTH(contrasena) >= 8 AND contrasena REGEXP '[A-Z]' AND contrasena REGEXP '[a-z]')
);

CREATE TABLE Item_Usuario(
	id_item INT,
    id_usuario INT,
    cantidad INT DEFAULT 0,
    PRIMARY KEY (id_item, id_usuario),	
    FOREIGN KEY (id_item) REFERENCES Items(id_item) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Usuario_quest (
    id_usuario INT,
    id_quest INT,
    Progress INT DEFAULT 0,
    completado TINYINT DEFAULT 0 COMMENT '0=no completada, 1=completada, 2=reclamada',
    reclamado BOOLEAN DEFAULT FALSE,
    PRIMARY KEY (id_usuario, id_quest),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
    FOREIGN KEY (id_quest) REFERENCES Quests(id_quest) ON DELETE CASCADE
);

CREATE TABLE Usuario_Modulos (
    id_usuario INT NOT NULL,
    id_modulo INT NOT NULL,
    desbloqueado BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (id_usuario, id_modulo),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Usuario_Pregunta(
    id_pregunta INT,
    id_usuario INT,
    intentos INT DEFAULT 0,
    resuelta BOOL DEFAULT FALSE,
    PRIMARY KEY(id_pregunta, id_usuario),
    FOREIGN KEY (id_pregunta) REFERENCES Preguntas(id_pregunta) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Usuario_Ahorcado(
	id_ahorcado INT,
    id_usuario INT,
    intentos INT DEFAULT 0,
    resuelta BOOL DEFAULT FALSE,
    PRIMARY KEY(id_ahorcado, id_usuario),
    FOREIGN KEY (id_ahorcado) REFERENCES Ahorcados(id_ahorcado) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Usuario_Memorama(
	id_memorama INT,
    id_usuario INT,
    intentos INT DEFAULT 0,
    resuelta BOOL DEFAULT FALSE,
    PRIMARY KEY(id_memorama, id_usuario),
    FOREIGN KEY (id_memorama) REFERENCES Memoramas(id_memorama) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Progreso(
    id_usuario INT,
    id_modulo INT,
    porce_complet DECIMAL(5,2) DEFAULT 0.00 NOT NULL CHECK(porce_complet BETWEEN 0.00 AND 1.00),
    desbloqueado BOOL DEFAULT FALSE NOT NULL,
    PRIMARY KEY (id_usuario, id_modulo),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
    FOREIGN KEY (id_modulo) REFERENCES Modulos(id_modulo) ON DELETE CASCADE
);

CREATE TABLE Quiz_Usuario(
    id_quiz INT,
    id_usuario INT,
    desbloqueado BOOL DEFAULT FALSE,
    estrellas INT DEFAULT 0,
    puntos INT DEFAULT 0,
    completado BOOLEAN DEFAULT FALSE,
    PRIMARY KEY (id_quiz, id_usuario),
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Quests(
    id_quest INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    descrip_quest VARCHAR(255),
    recompensa INT
);

CREATE TABLE Items(
	id_item INT AUTO_INCREMENT PRIMARY KEY,
    nombre_item VARCHAR(100),
    descrip_item VARCHAR(255)
);
CREATE TABLE Modulos(
    id_modulo INT AUTO_INCREMENT PRIMARY KEY,
    descrip_mod VARCHAR(200) NOT NULL,
    nombre_mod VARCHAR(200) NOT NULL,
    costo INT NOT NULL CHECK(costo > 0)
);
CREATE TABLE Quizzes(
    id_quiz INT AUTO_INCREMENT PRIMARY KEY,
    id_modulo INT,
    nombre_quiz VARCHAR(255) NOT NULL,
    FOREIGN KEY (id_modulo) REFERENCES Modulos(id_modulo) ON DELETE CASCADE
);
CREATE TABLE Contenido(
    id_contenido INT PRIMARY KEY AUTO_INCREMENT,
    id_quiz INT,
    duracion INT NOT NULL CHECK(duracion > 0),
    url VARCHAR(1024) NOT NULL, /*Las urls pueden ser muy largas y no se si las vamos a acortar*/
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE
);
CREATE TABLE Preguntas(
    id_pregunta INT PRIMARY KEY AUTO_INCREMENT,
    id_quiz INT,
    explicacion VARCHAR(255) NOT NULL,
    tip VARCHAR(255) NOT NULL,
    indice_correcto INT NOT NULL,
    pregunta VARCHAR(255) NOT NULL,
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE
);
CREATE TABLE Respuestas_Quiz(
    id_pregunta INT,
    respuesta VARCHAR(150) NOT NULL,
    indice INT,
    PRIMARY KEY (id_pregunta, indice),
    FOREIGN KEY (id_pregunta) REFERENCES Preguntas(id_pregunta) ON DELETE CASCADE
);
CREATE TABLE Memoramas(
	id_memorama INT PRIMARY KEY AUTO_INCREMENT,
    id_quiz INT,
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE
);
CREATE TABLE Respuestas_Memorama(
	id_respuesta INT AUTO_INCREMENT,
    id_memorama INT,
    concepto VARCHAR(50),
    definicion VARCHAR(255),
    PRIMARY KEY (id_respuesta, id_memorama),
    FOREIGN KEY (id_memorama) REFERENCES Memoramas(id_memorama) ON DELETE CASCADE
);
CREATE TABLE Ahorcados(
	id_ahorcado INT PRIMARY KEY AUTO_INCREMENT,
    id_quiz INT,
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE
);
CREATE TABLE Respuestas_Ahorcado(
	id_respuesta INT AUTO_INCREMENT,
    id_ahorcado INT,
    concepto VARCHAR(50),
    definicion VARCHAR(255),
    PRIMARY KEY (id_respuesta, id_ahorcado),
    FOREIGN KEY (id_ahorcado) REFERENCES Ahorcados(id_ahorcado) ON DELETE CASCADE
);

DROP TRIGGER IF EXISTS after_insert_quest;
DROP TRIGGER IF EXISTS after_insert_usuario;
DROP TRIGGER IF EXISTS after_progress_update;
DROP TRIGGER IF EXISTS insertar_items_usuario;
DROP TRIGGER IF EXISTS after_usuario_modulo;
DROP TRIGGER IF EXISTS evitar_progreso_excedido;

DELIMITER //
-- Trigger: Después de insertar una nueva quest
CREATE TRIGGER after_insert_quest
AFTER INSERT ON Quests
FOR EACH ROW
BEGIN
    INSERT INTO Usuario_quest (id_usuario, id_quest, completado, Progress)
    SELECT id_usuario, NEW.id_quest, FALSE, 0
    FROM Usuarios;
END;
//

-- Trigger: Después de insertar un nuevo usuario
CREATE TRIGGER after_insert_usuario
AFTER INSERT ON Usuarios
FOR EACH ROW
BEGIN
    INSERT INTO Usuario_quest (id_usuario, id_quest, completado, Progress)
    SELECT NEW.id_usuario, id_quest, FALSE, 0
    FROM Quests;
END;
//

DELIMITER ;

Use CryptoGame;
DELIMITER //
CREATE TRIGGER after_progress_update
AFTER UPDATE ON Usuario_quest
FOR EACH ROW
BEGIN
    DECLARE target_prog INT;
    
    IF NEW.Progress <> OLD.Progress THEN
        SELECT TargetProgress INTO target_prog FROM Quests WHERE id_quest = NEW.id_quest;
        
        IF NEW.Progress >= target_prog AND NEW.completado = 0 THEN
            UPDATE Usuario_quest 
            SET completado = 1 
            WHERE id_usuario = NEW.id_usuario AND id_quest = NEW.id_quest;
        END IF;
    END IF;
END//
DELIMITER ;


Use CryptoGame;
DELIMITER $$
CREATE TRIGGER insertar_items_usuario
AFTER INSERT ON Usuarios
FOR EACH ROW
BEGIN
    -- Inserta una relación para cada ítem existente
    INSERT INTO Item_Usuario (id_item, id_usuario, cantidad)
    SELECT id_item, NEW.id_usuario, 0
    FROM Items;
END$$

DELIMITER ;

Use CryptoGame;
DELIMITER //
CREATE TRIGGER after_usuario_modulo
AFTER INSERT ON Usuarios
FOR EACH ROW
BEGIN
    -- Insertar los 4 módulos para el nuevo usuario
    -- Módulo 1 siempre desbloqueado
    INSERT INTO Usuario_Modulos (id_usuario, id_modulo, desbloqueado) 
    VALUES (NEW.id_usuario, 1, TRUE);
    
    -- Módulos 2-4 bloqueados por defecto
    INSERT INTO Usuario_Modulos (id_usuario, id_modulo, desbloqueado) 
    VALUES (NEW.id_usuario, 2, FALSE);
    
    INSERT INTO Usuario_Modulos (id_usuario, id_modulo, desbloqueado) 
    VALUES (NEW.id_usuario, 3, FALSE);
    
    INSERT INTO Usuario_Modulos (id_usuario, id_modulo, desbloqueado) 
    VALUES (NEW.id_usuario, 4, FALSE);
END//
DELIMITER ;


Use CryptoGame;
DELIMITER //

CREATE TRIGGER evitar_progreso_excedido
BEFORE UPDATE ON Usuario_quest
FOR EACH ROW
BEGIN
    DECLARE objetivo INT;

    -- Obtener el valor de TargetProgress de la quest correspondiente
    SELECT TargetProgress INTO objetivo
    FROM Quests
    WHERE id_quest = NEW.id_quest;

    -- Si el nuevo progreso es mayor al objetivo, lo ajustamos al objetivo
    IF NEW.Progress > objetivo THEN
        SET NEW.Progress = objetivo;
    END IF;
END;
//

DELIMITER ;

USE CryptoGame;

-- Asegúrate de cerrar bien los delimitadores anteriores
DELIMITER //

DROP TRIGGER IF EXISTS after_quiz_insert //
CREATE TRIGGER after_quiz_insert
AFTER INSERT ON Quizzes
FOR EACH ROW
BEGIN
    INSERT INTO Quiz_Usuario (id_quiz, id_usuario, desbloqueado, estrellas, puntos, Completado)
    SELECT NEW.id_quiz, id_usuario, FALSE, 0, 0, FALSE
    FROM Usuarios;
END;
//

DROP TRIGGER IF EXISTS after_usuario_insert //
CREATE TRIGGER after_usuario_insert
AFTER INSERT ON Usuarios
FOR EACH ROW
BEGIN
    INSERT INTO Quiz_Usuario (id_quiz, id_usuario, desbloqueado, estrellas, puntos, Completado)
    SELECT id_quiz, NEW.id_usuario, FALSE, 0, 0, FALSE
    FROM Quizzes;
END;
//

DELIMITER ;

