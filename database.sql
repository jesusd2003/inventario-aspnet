-- ============================================================
-- SISTEMA DE INVENTARIO PRO - Base de datos completa
-- Ejecutar en HeidiSQL ANTES de correr la aplicacion
-- ============================================================

DROP DATABASE IF EXISTS inventario_db;
CREATE DATABASE inventario_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE inventario_db;

CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre   VARCHAR(100) NOT NULL,
    correo   VARCHAR(150) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE categorias (
    id     INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE productos (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    nombre       VARCHAR(150)  NOT NULL,
    descripcion  VARCHAR(500),
    precio       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    stock        INT           NOT NULL DEFAULT 0,
    categoria_id INT           NOT NULL,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE clientes (
    id        INT AUTO_INCREMENT PRIMARY KEY,
    nombre    VARCHAR(150) NOT NULL,
    telefono  VARCHAR(100),
    direccion VARCHAR(200)
) ENGINE=InnoDB;

CREATE TABLE ventas (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    fecha       DATETIME      NOT NULL DEFAULT NOW(),
    total       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    tipo_venta  VARCHAR(20)   NOT NULL DEFAULT "POS",
    tipo_pago   VARCHAR(20)   NOT NULL DEFAULT "Contado",
    notas       VARCHAR(300),
    cliente_id  INT           NOT NULL,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE detalle_ventas (
    id               INT AUTO_INCREMENT PRIMARY KEY,
    venta_id         INT           NOT NULL,
    producto_id      INT           NOT NULL,
    cantidad         INT           NOT NULL,
    precio_unitario  DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (venta_id)    REFERENCES ventas(id)    ON DELETE CASCADE,
    FOREIGN KEY (producto_id) REFERENCES productos(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE compras (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    fecha      DATETIME      NOT NULL DEFAULT NOW(),
    total      DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    proveedor  VARCHAR(150),
    notas      VARCHAR(300)
) ENGINE=InnoDB;

CREATE TABLE detalle_compras (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    compra_id       INT           NOT NULL,
    producto_id     INT           NOT NULL,
    cantidad        INT           NOT NULL,
    costo_unitario  DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (compra_id)   REFERENCES compras(id)   ON DELETE CASCADE,
    FOREIGN KEY (producto_id) REFERENCES productos(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- Datos iniciales
INSERT INTO usuarios   VALUES (1,"Administrador","admin@inventario.com","admin123");
INSERT INTO categorias VALUES (1,"Electronica"),(2,"Ropa"),(3,"Alimentos");
INSERT INTO clientes   VALUES (1,"Cliente General","0000-0000",NULL),(2,"Maria Lopez","3001234567",NULL),(3,"Carlos Perez","3109876543",NULL);
INSERT INTO productos  VALUES
  (1,"Laptop Dell",   "Procesador i5 8GB RAM",12500.00,10,1),
  (2,"Teclado Mecanico","RGB switches azules",  850.00, 3,1),
  (3,"Camiseta Basica","Algodon 100%",          150.00, 4,2),
  (4,"Arroz 5kg",     "Grano largo",             85.00, 2,3),
  (5,"Aceite Oliva 1L","Extra virgen",          180.00, 1,3);

SELECT "Base de datos creada correctamente!" AS Resultado;
SHOW TABLES;