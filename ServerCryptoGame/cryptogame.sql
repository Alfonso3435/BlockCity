
DROP DATABASE IF EXISTS Cryptogame;
CREATE DATABASE Cryptogame;
USE CryptoGame;

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

CREATE TABLE Quests(
    id_quest INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(100) NOT NULL,
    descrip_quest VARCHAR(255),
    recompensa INT
);

CREATE TABLE Usuario_quest(
    id_usuario INT,
    id_quest INT,
    completado BOOL DEFAULT FALSE,
    PRIMARY KEY (id_usuario, id_quest),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE,
    FOREIGN KEY (id_quest) REFERENCES Quests(id_quest) ON DELETE CASCADE
);

CREATE TABLE Items(
	id_item INT AUTO_INCREMENT PRIMARY KEY,
    nombre_item VARCHAR(100),
    descrip_item VARCHAR(255)
);

CREATE TABLE Item_Usuario(
	id_item INT,
    id_usuario INT,
    cantidad INT DEFAULT 0,
    PRIMARY KEY (id_item, id_usuario),	
    FOREIGN KEY (id_item) REFERENCES Items(id_item) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);

CREATE TABLE Modulos(
    id_modulo INT AUTO_INCREMENT PRIMARY KEY,
    descrip_mod VARCHAR(200) NOT NULL,
    nombre_mod VARCHAR(200) NOT NULL,
    costo INT NOT NULL CHECK(costo > 0)
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

CREATE TABLE Quizzes(
    id_quiz INT AUTO_INCREMENT PRIMARY KEY,
    id_modulo INT,
    nombre_quiz VARCHAR(255) NOT NULL,
    FOREIGN KEY (id_modulo) REFERENCES Modulos(id_modulo) ON DELETE CASCADE
);

CREATE TABLE Quiz_Usuario(
	id_quiz INT,
    id_usuario INT,
    desbloqueado BOOL DEFAULT False,
    estrellas INT DEFAULT 0,
    puntos INT DEFAULT 0,
    PRIMARY KEY (id_quiz, id_usuario),
    FOREIGN KEY (id_quiz) REFERENCES Quizzes(id_quiz) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
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

 /*Es una entidad debil*/
CREATE TABLE Respuestas_Quiz(
    id_pregunta INT,
    respuesta VARCHAR(150) NOT NULL,
    indice INT,
    PRIMARY KEY (id_pregunta, indice),
    FOREIGN KEY (id_pregunta) REFERENCES Preguntas(id_pregunta) ON DELETE CASCADE
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

CREATE TABLE Usuario_Memorama(
	id_memorama INT,
    id_usuario INT,
    intentos INT DEFAULT 0,
    resuelta BOOL DEFAULT FALSE,
    PRIMARY KEY(id_memorama, id_usuario),
    FOREIGN KEY (id_memorama) REFERENCES Memoramas(id_memorama) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
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

CREATE TABLE Usuario_Ahorcado(
	id_ahorcado INT,
    id_usuario INT,
    intentos INT DEFAULT 0,
    resuelta BOOL DEFAULT FALSE,
    PRIMARY KEY(id_ahorcado, id_usuario),
    FOREIGN KEY (id_ahorcado) REFERENCES Ahorcados(id_ahorcado) ON DELETE CASCADE,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario) ON DELETE CASCADE
);
