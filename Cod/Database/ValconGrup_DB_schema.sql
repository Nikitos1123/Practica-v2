-- Schema pentru aplicația Valcongrup Desktop + MySQL
-- Rulează standalone (conține CREATE IF NOT EXISTS).

CREATE DATABASE IF NOT EXISTS ValconGrup_MySQL80;
USE ValconGrup_MySQL80;

CREATE TABLE IF NOT EXISTS Echipe_Constructii (
    id_echipa INT PRIMARY KEY AUTO_INCREMENT,
    nume_echipa VARCHAR(100) NOT NULL,
    numar_membri INT
);

CREATE TABLE IF NOT EXISTS Proiecte (
    id_proiect INT PRIMARY KEY AUTO_INCREMENT,
    nume_proiect VARCHAR(100) NOT NULL,
    adresa VARCHAR(255),
    data_incepere DATE,
    id_echipa INT,
    FOREIGN KEY (id_echipa) REFERENCES Echipe_Constructii(id_echipa)
);

CREATE TABLE IF NOT EXISTS Unitati_Imobiliare (
    id_unitate INT PRIMARY KEY AUTO_INCREMENT,
    nr_apartament VARCHAR(10),
    suprafata_mp DECIMAL(10, 2),
    pret_vanzare DECIMAL(15, 2),
    status_unitate ENUM('In constructie', 'Disponibil', 'Vandut') DEFAULT 'In constructie',
    id_proiect INT,
    FOREIGN KEY (id_proiect) REFERENCES Proiecte(id_proiect)
);

CREATE TABLE IF NOT EXISTS Clienti (
    id_client INT PRIMARY KEY AUTO_INCREMENT,
    nume VARCHAR(50),
    prenume VARCHAR(50),
    telefon VARCHAR(20) UNIQUE,
    email VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS Contracte (
    id_contract INT PRIMARY KEY AUTO_INCREMENT,
    data_finalizare DATE,
    id_client INT,
    id_unitate INT UNIQUE,
    FOREIGN KEY (id_client) REFERENCES Clienti(id_client),
    FOREIGN KEY (id_unitate) REFERENCES Unitati_Imobiliare(id_unitate)
);

-- Conturi pentru aplicația desktop (BCrypt în parola_hash). Nu face parte din tabelele cursului, dar este necesar pentru login/register în C#.
CREATE TABLE IF NOT EXISTS Utilizatori_App (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(150) NOT NULL UNIQUE,
    parola_hash VARCHAR(255) NOT NULL,
    nume VARCHAR(50) NOT NULL,
    prenume VARCHAR(50) NOT NULL,
    activ TINYINT(1) NOT NULL DEFAULT 1,
    is_approved TINYINT(1) NOT NULL DEFAULT 0,
    creat_la DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ultima_logare DATETIME NULL
);

-- Approval workflow pentru tabelele folosite de aplicatia desktop curenta.
-- Conturile create public pornesc neaprobate; administratorii trebuie aprobati pentru a nu bloca accesul initial.
CREATE TABLE IF NOT EXISTS roluri (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nume VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS utilizatori (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nume VARCHAR(50) NOT NULL,
    prenume VARCHAR(50) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    telefon VARCHAR(30),
    id_rol INT NOT NULL,
    parola_hash VARCHAR(255) NOT NULL,
    activ TINYINT(1) NOT NULL DEFAULT 1,
    is_approved TINYINT(1) NOT NULL DEFAULT 0,
    creat_la DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ultima_logare DATETIME NULL,
    FOREIGN KEY (id_rol) REFERENCES roluri(id)
);

ALTER TABLE utilizatori
    ADD COLUMN IF NOT EXISTS is_approved TINYINT(1) NOT NULL DEFAULT 1;

ALTER TABLE utilizatori
    ALTER COLUMN is_approved SET DEFAULT 0;

UPDATE utilizatori u
LEFT JOIN roluri r ON r.id = u.id_rol
SET u.is_approved = 1
WHERE LOWER(COALESCE(r.nume, '')) = 'admin';

INSERT IGNORE INTO Echipe_Constructii (nume_echipa, numar_membri) VALUES
('Construct Master', 12), ('Elite Builders', 8), ('Steel Frameworks', 15),
('Rapid Finish', 6), ('Pro Fundament', 20);

INSERT IGNORE INTO Clienti (nume, prenume, telefon, email) VALUES
('Ionescu', 'Mihai', '068123456', 'mihai.i@gmail.com'),
('Elon', 'Musk', '079111222', '321n.p@mail.md'),
('Chistrea', 'Marius-Iustin', '060333444', 'mariuswarboss.a@yahoo.com'),
('Ceban', 'Elena', '069555666', 'elena.c@outlook.com'),
('Dragan', 'Victor', '078777888', 'victor.v@gmail.com');

INSERT IGNORE INTO Proiecte (nume_proiect, adresa, data_incepere, id_echipa) VALUES
('Valcon Tower', 'Str. Albișoara 10', '2025-01-10', 1),
('Green Park Residance', 'Str. Florilor 5', '2025-02-15', 2),
('Skyline Apartment', 'Bd. Moscova 21', '2025-03-01', 3),
('Iris Gardens', 'Str. Grenoble 120', '2025-03-20', 4),
('Oasis Plaza', 'Str. Bogdan Voievod 1', '2025-04-05', 5),
('Sunrise Village', 'Com. Stăuceni', '2025-05-10', 1),
('Corner Office', 'Str. Pușkin 32', '2025-06-01', 2),
('Eco Home', 'Str. Nicolae Sulac 19', '2025-06-15', 3),
('Legacy Estate', 'Bd. Mircea cel Bătrân', '2025-07-01', 4),
('Modern Loft', 'Str. Columna 44', '2025-08-20', 5);

INSERT IGNORE INTO Unitati_Imobiliare (nr_apartament, suprafata_mp, pret_vanzare, status_unitate, id_proiect)
VALUES ('Ap. 10', 65.50, 85000, 'Disponibil', 1);

INSERT IGNORE INTO Unitati_Imobiliare (nr_apartament, suprafata_mp, id_proiect)
VALUES ('Ap. 22', 45.00, 2);

CREATE OR REPLACE VIEW Unitati_Disponibile AS
SELECT nr_apartament, suprafata_mp, pret_vanzare
FROM Unitati_Imobiliare
WHERE status_unitate = 'Disponibil';

CREATE OR REPLACE VIEW Proiecte_Recente AS
SELECT nume_proiect, data_incepere
FROM Proiecte
WHERE data_incepere < '2025-06-01';

CREATE OR REPLACE VIEW Detalii_Contracte AS
SELECT c.id_contract, cl.nume, cl.prenume, c.data_finalizare, u.nr_apartament
FROM Contracte c
JOIN Clienti cl ON c.id_client = cl.id_client
JOIN Unitati_Imobiliare u ON c.id_unitate = u.id_unitate;

CREATE INDEX IF NOT EXISTS idx_nume_proiect ON Proiecte(nume_proiect);
CREATE INDEX IF NOT EXISTS idx_nume_prenume_client ON Clienti(nume, prenume);
