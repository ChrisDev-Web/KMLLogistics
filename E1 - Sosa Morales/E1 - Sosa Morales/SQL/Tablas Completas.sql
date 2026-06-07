-- ==========================================
-- Document Types
-- ==========================================
CREATE TABLE DocumentTypes (
    id_document_type INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Countries / Regions / Provinces / Districts
-- ==========================================
CREATE TABLE Countries (
    id_country INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

CREATE TABLE Regions (
    id_region INT IDENTITY(1,1) PRIMARY KEY,
    id_country INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_region_country FOREIGN KEY (id_country) REFERENCES Countries(id_country),
    CONSTRAINT uq_region UNIQUE (id_country, name)
);

CREATE TABLE Provinces (
    id_province INT IDENTITY(1,1) PRIMARY KEY,
    id_region INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_province_region FOREIGN KEY (id_region) REFERENCES Regions(id_region),
    CONSTRAINT uq_province UNIQUE (id_region, name)
);

CREATE TABLE Districts (
    id_district INT IDENTITY(1,1) PRIMARY KEY,
    id_province INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_district_province FOREIGN KEY (id_province) REFERENCES Provinces(id_province),
    CONSTRAINT uq_district UNIQUE (id_province, name)
);

-- ==========================================
-- Categories
-- ==========================================
CREATE TABLE Categories (
    id_category INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    photo VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Brands
-- ==========================================
CREATE TABLE Brands (
    id_brand INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Suppliers
-- ==========================================
CREATE TABLE Suppliers (
    id_supplier INT IDENTITY(1,1) PRIMARY KEY,
    id_document_type INT NOT NULL,
    document_number VARCHAR(20) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    phone VARCHAR(20) NULL,
    email VARCHAR(100) NULL UNIQUE,
    address VARCHAR(255) NULL,
    id_district INT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_supplier_document_type FOREIGN KEY (id_document_type) REFERENCES DocumentTypes(id_document_type),
    CONSTRAINT fk_supplier_district FOREIGN KEY (id_district) REFERENCES Districts(id_district)
);

-- ==========================================
-- Products
-- ==========================================
CREATE TABLE Products (
    id_product INT IDENTITY(1,1) PRIMARY KEY,
    id_category INT NOT NULL,
    id_brand INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NULL,
    cost DECIMAL(10,2) NOT NULL,
    profit_percentage DECIMAL(5,2) NOT NULL,
    sale_price AS (cost / (1 - (profit_percentage / 100))) PERSISTED,
	weight DECIMAL(10,2) NULL,
	height DECIMAL(10,2) NULL,
	width DECIMAL(10,2) NULL,
	length DECIMAL(10,2) NULL,
	volume AS (height * width * length) PERSISTED,
    photo VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_product_category FOREIGN KEY (id_category) REFERENCES Categories(id_category),
    CONSTRAINT fk_product_brand FOREIGN KEY (id_brand) REFERENCES Brands(id_brand),
    CONSTRAINT chk_product_profit CHECK (profit_percentage > 0 AND profit_percentage < 100),
    CONSTRAINT chk_product_cost CHECK (cost >= 0)
);

-- ==========================================
-- ProductSuppliers
-- ==========================================
CREATE TABLE ProductSuppliers (
    id_product_supplier INT IDENTITY(1,1) PRIMARY KEY,
    id_product INT NOT NULL,
    id_supplier INT NOT NULL,
    supplier_cost DECIMAL(10,2) NOT NULL,
    last_purchase_cost DECIMAL(10,2) NULL,
    is_main_supplier BIT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_product_supplier_product FOREIGN KEY (id_product) REFERENCES Products(id_product),
    CONSTRAINT fk_product_supplier_supplier FOREIGN KEY (id_supplier) REFERENCES Suppliers(id_supplier),
    CONSTRAINT uq_product_supplier UNIQUE (id_product, id_supplier),
    CONSTRAINT chk_product_supplier_cost CHECK (supplier_cost >= 0),
    CONSTRAINT chk_product_supplier_last_purchase_cost CHECK (last_purchase_cost IS NULL OR last_purchase_cost >= 0)
);

CREATE TABLE SupplierBrands (
    id_supplier INT NOT NULL,
    id_brand INT NOT NULL,
    CONSTRAINT pk_supplier_brand PRIMARY KEY (id_supplier, id_brand),
    CONSTRAINT fk_supplier_brand_supplier FOREIGN KEY (id_supplier) REFERENCES Suppliers(id_supplier),
    CONSTRAINT fk_supplier_brand_brand FOREIGN KEY (id_brand) REFERENCES Brands(id_brand)
);

-- ==========================================
-- Warehouses
-- ==========================================
CREATE TABLE Warehouses (
    id_warehouse INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    address VARCHAR(255) NOT NULL,
    id_district INT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_warehouse_district FOREIGN KEY (id_district) REFERENCES Districts(id_district)
);

-- ==========================================
-- WarehouseDetails
-- ==========================================
CREATE TABLE WarehouseDetails (
    id_warehouse_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_warehouse INT NOT NULL,
    id_product INT NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    location VARCHAR(100) NULL,
    CONSTRAINT fk_warehouse_detail_warehouse FOREIGN KEY (id_warehouse) REFERENCES Warehouses(id_warehouse),
    CONSTRAINT fk_warehouse_detail_product FOREIGN KEY (id_product) REFERENCES Products(id_product),
    CONSTRAINT uq_warehouse_product UNIQUE (id_warehouse, id_product),
    CONSTRAINT chk_warehouse_stock CHECK (stock >= 0)
);

-- ==========================================
-- Roles
-- ==========================================
CREATE TABLE Roles (
    id_role INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Users
-- ==========================================
CREATE TABLE Users (
    id_user INT IDENTITY(1,1) PRIMARY KEY,
    id_role INT NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_user_role FOREIGN KEY (id_role) REFERENCES Roles(id_role)
);

-- ==========================================
-- JobPositions
-- ==========================================
CREATE TABLE JobPositions (
    id_job_position INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Employees
-- ==========================================
CREATE TABLE Employees (
    id_employee INT IDENTITY(1,1) PRIMARY KEY,
    id_user INT NOT NULL UNIQUE,
    id_job_position INT NOT NULL,
    id_district INT NULL,
    name VARCHAR(80) NOT NULL,
    last_name_paternal VARCHAR(80) NOT NULL,
    last_name_maternal VARCHAR(80) NULL,
    phone VARCHAR(20) NULL,
    email VARCHAR(100) NULL UNIQUE,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_employee_user FOREIGN KEY (id_user) REFERENCES Users(id_user),
    CONSTRAINT fk_employee_job_position FOREIGN KEY (id_job_position) REFERENCES JobPositions(id_job_position),
    CONSTRAINT fk_employee_district FOREIGN KEY (id_district) REFERENCES Districts(id_district)
);

-- ==========================================
-- Clients
-- ==========================================
CREATE TABLE Clients (
    id_client INT IDENTITY(1,1) PRIMARY KEY,
    id_document_type INT NOT NULL,
    document_number VARCHAR(20) NOT NULL,
    name VARCHAR(80) NOT NULL,
    last_name_paternal VARCHAR(80) NOT NULL,
    last_name_maternal VARCHAR(80) NULL,
    phone VARCHAR(20) NULL,
    email VARCHAR(100) NULL UNIQUE,
    address VARCHAR(255) NULL,
    id_district INT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_client_document_type FOREIGN KEY (id_document_type) REFERENCES DocumentTypes(id_document_type),
    CONSTRAINT fk_client_district FOREIGN KEY (id_district) REFERENCES Districts(id_district),
    CONSTRAINT uq_client_document UNIQUE (id_document_type, document_number)
);

-- ==========================================
-- Purchase Status
-- ==========================================
CREATE TABLE PurchaseStatuses (
    id_purchase_status INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Purchases and Details (using ProductSuppliers)
-- ==========================================
CREATE TABLE Purchases (
    id_purchase INT IDENTITY(1,1) PRIMARY KEY,
    id_supplier INT NOT NULL,
    id_employee INT NOT NULL,
    id_purchase_status INT NOT NULL,
    fec_purchase DATETIME NOT NULL DEFAULT GETDATE(),
    subtotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    tax DECIMAL(10,2) NOT NULL DEFAULT 0,
    total DECIMAL(10,2) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_purchase_supplier FOREIGN KEY (id_supplier) REFERENCES Suppliers(id_supplier),
    CONSTRAINT fk_purchase_employee FOREIGN KEY (id_employee) REFERENCES Employees(id_employee),
    CONSTRAINT fk_purchase_status FOREIGN KEY (id_purchase_status) REFERENCES PurchaseStatuses(id_purchase_status)
);

CREATE TABLE PurchaseDetails (
    id_purchase_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_purchase INT NOT NULL,
    id_product_supplier INT NOT NULL,
    quantity INT NOT NULL,
    unit_cost DECIMAL(10,2) NOT NULL,
    subtotal AS (quantity * unit_cost) PERSISTED,
    CONSTRAINT fk_purchase_detail_purchase FOREIGN KEY (id_purchase) REFERENCES Purchases(id_purchase),
    CONSTRAINT fk_purchase_detail_product_supplier FOREIGN KEY (id_product_supplier) REFERENCES ProductSuppliers(id_product_supplier),
    CONSTRAINT chk_purchase_detail_quantity CHECK (quantity > 0),
    CONSTRAINT chk_purchase_detail_unit_cost CHECK (unit_cost >= 0)
);

CREATE TABLE PurchaseWarehouseDetails (
    id_purchase_warehouse_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_purchase_detail INT NOT NULL,
    id_warehouse INT NOT NULL,
    quantity INT NOT NULL,
    CONSTRAINT fk_purchase_warehouse_detail_purchase_detail FOREIGN KEY (id_purchase_detail) REFERENCES PurchaseDetails(id_purchase_detail),
    CONSTRAINT fk_purchase_warehouse_detail_warehouse FOREIGN KEY (id_warehouse) REFERENCES Warehouses(id_warehouse),
    CONSTRAINT chk_purchase_warehouse_quantity CHECK (quantity > 0)
);

-- ==========================================
-- Status Transfers
-- ==========================================
CREATE TABLE StatusTransfers (
    id_status_transfer INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Transfers (multi-product between warehouses)
-- ==========================================
CREATE TABLE Transfers (
    id_transfer INT IDENTITY(1,1) PRIMARY KEY,
    id_warehouse_origin INT NOT NULL,
    id_warehouse_destination INT NOT NULL,
    id_status_transfer INT NOT NULL,
    fec_transfer DATETIME NOT NULL,
    id_employee INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_transfer_origin FOREIGN KEY (id_warehouse_origin) REFERENCES Warehouses(id_warehouse),
    CONSTRAINT fk_transfer_destination FOREIGN KEY (id_warehouse_destination) REFERENCES Warehouses(id_warehouse),
    CONSTRAINT fk_transfer_status FOREIGN KEY (id_status_transfer) REFERENCES StatusTransfers(id_status_transfer),
    CONSTRAINT fk_transfer_employee FOREIGN KEY (id_employee) REFERENCES Employees(id_employee)
);

CREATE TABLE TransferDetails (
    id_transfer_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_transfer INT NOT NULL,
    id_product INT NOT NULL,
    quantity INT NOT NULL,
    CONSTRAINT fk_transfer_detail_transfer FOREIGN KEY (id_transfer) REFERENCES Transfers(id_transfer),
    CONSTRAINT fk_transfer_detail_product FOREIGN KEY (id_product) REFERENCES Products(id_product),
    CONSTRAINT chk_transfer_detail_quantity CHECK (quantity > 0)
);

-- ==========================================
-- Movement Types and Inventory Movements
-- ==========================================
CREATE TABLE MovementTypes (
    id_movement_type INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

CREATE TABLE InventoryMovements (
    id_inventory_movement INT IDENTITY(1,1) PRIMARY KEY,
    id_product INT NOT NULL,
    id_warehouse INT NOT NULL,
    id_movement_type INT NOT NULL,
    id_employee INT NOT NULL,
    quantity INT NOT NULL,
    reference VARCHAR(100) NULL,
    fec_movement DATETIME NOT NULL DEFAULT GETDATE(),
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,
    CONSTRAINT fk_inventory_movement_product FOREIGN KEY (id_product) REFERENCES Products(id_product),
    CONSTRAINT fk_inventory_movement_warehouse FOREIGN KEY (id_warehouse) REFERENCES Warehouses(id_warehouse),
    CONSTRAINT fk_inventory_movement_type FOREIGN KEY (id_movement_type) REFERENCES MovementTypes(id_movement_type),
    CONSTRAINT fk_inventory_movement_employee FOREIGN KEY (id_employee) REFERENCES Employees(id_employee)
);

-- ==========================================
-- SaleStatuses
-- ==========================================
CREATE TABLE SaleStatuses (
    id_sale_status INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

-- ==========================================
-- Sales
-- ==========================================
CREATE TABLE Sales (
    id_sale INT IDENTITY(1,1) PRIMARY KEY,
    id_client INT NOT NULL,
    id_employee INT NOT NULL,
    id_sale_status INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    tax DECIMAL(10,2) NOT NULL DEFAULT 0,
    total DECIMAL(10,2) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,

    CONSTRAINT fk_sale_client
        FOREIGN KEY (id_client)
        REFERENCES Clients(id_client),

    CONSTRAINT fk_sale_employee
        FOREIGN KEY (id_employee)
        REFERENCES Employees(id_employee),

    CONSTRAINT fk_sale_status
        FOREIGN KEY (id_sale_status)
        REFERENCES SaleStatuses(id_sale_status)
);

-- ==========================================
-- SaleDetails (multi-warehouse)
-- ==========================================
CREATE TABLE SaleDetails (
    id_sale_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_sale INT NOT NULL,
    id_product INT NOT NULL,
    id_warehouse INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    subtotal AS (quantity * unit_price) PERSISTED,

    CONSTRAINT fk_sale_detail_sale
        FOREIGN KEY (id_sale)
        REFERENCES Sales(id_sale),

    CONSTRAINT fk_sale_detail_product
        FOREIGN KEY (id_product)
        REFERENCES Products(id_product),

    CONSTRAINT fk_sale_detail_warehouse
        FOREIGN KEY (id_warehouse)
        REFERENCES Warehouses(id_warehouse),

    CONSTRAINT chk_sale_detail_quantity
        CHECK (quantity > 0),

    CONSTRAINT chk_sale_detail_unit_price
        CHECK (unit_price >= 0)
);

CREATE TABLE Boxes (
    id_box INT IDENTITY(1,1) PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    weight DECIMAL(10,2) NULL,
    height DECIMAL(10,2) NULL,
    width DECIMAL(10,2) NULL,
    length DECIMAL(10,2) NULL,
    volume AS (height * width * length) PERSISTED,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

CREATE TABLE BoxDetails (
    id_box_detail INT IDENTITY(1,1) PRIMARY KEY,
    id_box INT NOT NULL,
    id_sale_detail INT NOT NULL,
    quantity INT NOT NULL,

    CONSTRAINT fk_box_detail_box
        FOREIGN KEY (id_box)
        REFERENCES Boxes(id_box),

    CONSTRAINT fk_box_detail_sale_detail
        FOREIGN KEY (id_sale_detail)
        REFERENCES SaleDetails(id_sale_detail),

    CONSTRAINT chk_box_detail_quantity
        CHECK (quantity > 0)
);

CREATE TABLE VehicleTypes (
    id_vehicle_type INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

CREATE TABLE Vehicles (
    id_vehicle INT IDENTITY(1,1) PRIMARY KEY,
    id_vehicle_type INT NOT NULL,

    plate VARCHAR(20) NOT NULL UNIQUE,

    maximum_weight DECIMAL(10,2) NULL,
    maximum_volume DECIMAL(10,2) NULL,

    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,

    CONSTRAINT fk_vehicle_vehicle_type
        FOREIGN KEY (id_vehicle_type)
        REFERENCES VehicleTypes(id_vehicle_type)
);

CREATE TABLE ShipmentStatuses (
    id_shipment_status INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255) NULL,

    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL
);

CREATE TABLE Shipments (
    id_shipment INT IDENTITY(1,1) PRIMARY KEY,

    id_vehicle INT NOT NULL,
    id_employee INT NOT NULL,
    id_shipment_status INT NOT NULL,

    departure_date DATETIME NULL,
    arrival_date DATETIME NULL,

    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,

    CONSTRAINT fk_shipment_vehicle
        FOREIGN KEY (id_vehicle)
        REFERENCES Vehicles(id_vehicle),

    CONSTRAINT fk_shipment_employee
        FOREIGN KEY (id_employee)
        REFERENCES Employees(id_employee),

    CONSTRAINT fk_shipment_status
        FOREIGN KEY (id_shipment_status)
        REFERENCES ShipmentStatuses(id_shipment_status)
);

CREATE TABLE ShipmentBoxes (
    id_shipment_box INT IDENTITY(1,1) PRIMARY KEY,

    id_shipment INT NOT NULL,
    id_box INT NOT NULL,

    CONSTRAINT fk_shipment_box_shipment
        FOREIGN KEY (id_shipment)
        REFERENCES Shipments(id_shipment),

    CONSTRAINT fk_shipment_box_box
        FOREIGN KEY (id_box)
        REFERENCES Boxes(id_box),

    CONSTRAINT uq_shipment_box
        UNIQUE (id_shipment, id_box)
);

CREATE TABLE ShipmentSales (
    id_shipment_sale INT IDENTITY(1,1) PRIMARY KEY,

    id_shipment INT NOT NULL,
    id_sale INT NOT NULL,

    created_at DATETIME NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME NULL,
    deleted_at DATETIME NULL,

    CONSTRAINT fk_shipment_sale_shipment
        FOREIGN KEY (id_shipment)
        REFERENCES Shipments(id_shipment),

    CONSTRAINT fk_shipment_sale_sale
        FOREIGN KEY (id_sale)
        REFERENCES Sales(id_sale),

    CONSTRAINT uq_shipment_sale
        UNIQUE (id_shipment, id_sale)
);

INSERT INTO Roles (name, description)
VALUES
('Administrador', 'Acceso completo al sistema'),
('Supervisor', 'Supervisa operaciones y usuarios'),
('Operador', 'Realiza tareas operativas'),
('Consulta', 'Solo tiene permisos de lectura');