-- SCHEMA
CREATE SCHEMA lending;
GO

-- 1) อ้างอิง/รหัสมาตรฐาน
CREATE TABLE lending.RefProvince (
    province_code  varchar(2)  NOT NULL PRIMARY KEY, -- เช่น '10' = กทม.
    province_name  nvarchar(100) NOT NULL
);

CREATE TABLE lending.RefVehicleMake (
    make_id   int IDENTITY PRIMARY KEY,
    make_name nvarchar(100) NOT NULL UNIQUE
);
CREATE TABLE lending.RefVehicleModel (
    model_id  int IDENTITY PRIMARY KEY,
    make_id   int NOT NULL REFERENCES lending.RefVehicleMake(make_id),
    model_name nvarchar(100) NOT NULL,
    UNIQUE(make_id, model_name)
);

CREATE TABLE lending.RefProduct (
    product_id   int IDENTITY PRIMARY KEY,
    product_code varchar(30) NOT NULL UNIQUE,
    product_name nvarchar(200) NOT NULL,
    purpose      varchar(30) NOT NULL CHECK (purpose IN ('NEW','USED','REFINANCE','REGIS_LOAN')),
    is_active    bit NOT NULL DEFAULT 1
);

-- แผนดอกเบี้ยแม่แบบ (สำหรับคำนวณก่อนสร้างสัญญา)
CREATE TABLE lending.RatePlanTemplate (
    template_id   int IDENTITY PRIMARY KEY,
    product_id    int NOT NULL REFERENCES lending.RefProduct(product_id),
    calc_method   varchar(20) NOT NULL CHECK (calc_method IN ('FLAT','EFF','RULE78')),
    base_rate_pa  decimal(9,6) NOT NULL,   -- อัตราพื้นฐาน (ต่อปี)
    tenor_min_m   int NOT NULL,
    tenor_max_m   int NOT NULL,
    created_at    datetime2 NOT NULL DEFAULT sysdatetime()
);

-- 2) ลูกค้า/ผู้เกี่ยวข้อง
CREATE TABLE lending.Customer (
    customer_id       bigint IDENTITY PRIMARY KEY,
    national_id       varchar(13) NULL, -- เลขบัตร ปชช. (อาจเป็นนิติบุคคลก็ได้)
    customer_type     varchar(10) NOT NULL CHECK (customer_type IN ('PERSON','CORP')),
    title             nvarchar(30) NULL,
    first_name        nvarchar(120) NULL,
    last_name         nvarchar(120) NULL,
    corp_name         nvarchar(200) NULL,
    mobile            varchar(20) NULL,
    email             varchar(200) NULL,
    date_of_birth     date NULL,
    address_line1     nvarchar(200) NULL,
    address_line2     nvarchar(200) NULL,
    district          nvarchar(100) NULL,
    amphur            nvarchar(100) NULL,
    province_code     varchar(2) NULL REFERENCES lending.RefProvince(province_code),
    postcode          varchar(10) NULL,
    created_at        datetime2 NOT NULL DEFAULT sysdatetime(),
    UNIQUE(national_id)
);

-- 3) สัญญา
CREATE TABLE lending.LoanContract (
    loan_id            bigint IDENTITY PRIMARY KEY,
    contract_no        varchar(40) NOT NULL UNIQUE,
    product_id         int NOT NULL REFERENCES lending.RefProduct(product_id),
    application_no     varchar(40) NULL,
    status             varchar(20) NOT NULL CHECK (status IN ('PENDING','ACTIVE','CLOSED','NPL','REPO','CANCELLED')),
    principal_amount   decimal(18,2) NOT NULL,
    approved_amount    decimal(18,2) NOT NULL,
    tenor_months       int NOT NULL,
    rate_plan_method   varchar(20) NOT NULL CHECK (rate_plan_method IN ('FLAT','EFF','RULE78')),
    rate_pa            decimal(9,6) NOT NULL,     -- อัตราต่อปี ณ วันที่ทำสัญญา
    first_due_date     date NOT NULL,
    contract_date      date NOT NULL,
    branch_code        varchar(10) NULL,
    province_code      varchar(2) NULL REFERENCES lending.RefProvince(province_code),
    created_at         datetime2 NOT NULL DEFAULT sysdatetime()
);

-- ผู้กู้หลัก/ร่วม/ผู้ค้ำ
CREATE TABLE lending.LoanParty (
    loan_id     bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    customer_id bigint NOT NULL REFERENCES lending.Customer(customer_id),
    role_code   varchar(10) NOT NULL CHECK (role_code IN ('MAIN','CO_BORR','GUAR')),
    PRIMARY KEY (loan_id, customer_id, role_code)
);

-- 4) หลักประกัน: ยานพาหนะ (แกนกลาง + subtype)
CREATE TABLE lending.CollateralVehicle (
    collateral_id   bigint IDENTITY PRIMARY KEY,
    loan_id         bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    vehicle_type    varchar(15) NOT NULL CHECK (vehicle_type IN ('CAR','PICKUP','MOTORCYCLE')),
    registration_no nvarchar(20) NULL,  -- ทะเบียน
    province_code   varchar(2) NULL REFERENCES lending.RefProvince(province_code),
    chassis_no      nvarchar(50) NOT NULL,
    engine_no       nvarchar(50) NULL,
    make_id         int NOT NULL REFERENCES lending.RefVehicleMake(make_id),
    model_id        int NULL REFERENCES lending.RefVehicleModel(model_id),
    model_year      smallint NULL,
    color           nvarchar(30) NULL,
    odo_km          int NULL,
    ownership_type  varchar(20) NULL CHECK (ownership_type IN ('BORROWER','THIRDPARTY','COMPANY')),
    UNIQUE(chassis_no)
);

-- รายละเอียดเฉพาะประเภทรถ
CREATE TABLE lending.VehicleCar (
    collateral_id bigint PRIMARY KEY REFERENCES lending.CollateralVehicle(collateral_id),
    body_type     varchar(20) NULL,   -- SEDAN/HATCHBACK/MPV/SUV
    doors         tinyint NULL
);
CREATE TABLE lending.VehiclePickup (
    collateral_id bigint PRIMARY KEY REFERENCES lending.CollateralVehicle(collateral_id),
    cab_type      varchar(20) NULL,   -- SINGLE/EXTRA/DOUBLE
    drive_type    varchar(10) NULL    -- 2WD/4WD
);
CREATE TABLE lending.VehicleMotorcycle (
    collateral_id bigint PRIMARY KEY REFERENCES lending.CollateralVehicle(collateral_id),
    cc            int NULL,
    moto_type     varchar(20) NULL    -- AUTO/SPORT/UNDERBONE/etc.
);

-- 5) การประเมินราคา/ตรวจสภาพ
CREATE TABLE lending.Valuation (
    valuation_id   bigint IDENTITY PRIMARY KEY,
    collateral_id  bigint NOT NULL REFERENCES lending.CollateralVehicle(collateral_id),
    valuation_date date NOT NULL,
    method         varchar(20) NOT NULL CHECK (method IN ('BOOK','MARKET','INSPECT')),
    appraised_value decimal(18,2) NOT NULL,
    appraiser      nvarchar(120) NULL,
    notes          nvarchar(500) NULL
);

-- 6) ค่าธรรมเนียม ณ สัญญา
CREATE TABLE lending.Fee (
    fee_id       bigint IDENTITY PRIMARY KEY,
    loan_id      bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    fee_code     varchar(30) NOT NULL,  -- e.g. PROC, STAMP, GPS, INS_ADDON
    description  nvarchar(200) NULL,
    amount       decimal(18,2) NOT NULL,
    is_financed  bit NOT NULL DEFAULT 1  -- ทบในยอดกู้?
);

-- 7) แผนดอกเบี้ยจริงที่ใช้กับสัญญา
CREATE TABLE lending.RatePlan (
    rateplan_id  bigint IDENTITY PRIMARY KEY,
    loan_id      bigint NOT NULL UNIQUE REFERENCES lending.LoanContract(loan_id),
    calc_method  varchar(20) NOT NULL CHECK (calc_method IN ('FLAT','EFF','RULE78')),
    rate_pa      decimal(9,6) NOT NULL,
    installment_amt decimal(18,2) NOT NULL
);

-- 8) ตารางงวด (Schedule) และการรับชำระ
CREATE TABLE lending.Schedule (
    schedule_id   bigint IDENTITY PRIMARY KEY,
    loan_id       bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    seq_no        int NOT NULL,               -- งวดที่
    due_date      date NOT NULL,
    principal_due decimal(18,2) NOT NULL,
    interest_due  decimal(18,2) NOT NULL,
    fee_due       decimal(18,2) NOT NULL DEFAULT 0,
    status        varchar(10) NOT NULL DEFAULT 'DUE' CHECK (status IN ('DUE','PAID','PART')),
    UNIQUE(loan_id, seq_no)
);

CREATE TABLE lending.Receipt (
    receipt_id     bigint IDENTITY PRIMARY KEY,
    receipt_no     varchar(40) NOT NULL UNIQUE,
    loan_id        bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    pay_date       datetime2 NOT NULL DEFAULT sysdatetime(),
    channel        varchar(20) NULL,   -- CASH/TRANSFER/AUTO-DEBIT/APP
    amount_total   decimal(18,2) NOT NULL
);

CREATE TABLE lending.Repayment (
    repayment_id  bigint IDENTITY PRIMARY KEY,
    receipt_id    bigint NOT NULL REFERENCES lending.Receipt(receipt_id),
    schedule_id   bigint NULL REFERENCES lending.Schedule(schedule_id),
    principal_pay decimal(18,2) NOT NULL DEFAULT 0,
    interest_pay  decimal(18,2) NOT NULL DEFAULT 0,
    fee_pay       decimal(18,2) NOT NULL DEFAULT 0,
    penalty_pay   decimal(18,2) NOT NULL DEFAULT 0
);

-- 9) ประกันภัย/คุ้มครอง
CREATE TABLE lending.Insurance (
    insurance_id   bigint IDENTITY PRIMARY KEY,
    loan_id        bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    provider       nvarchar(120) NOT NULL,
    policy_no      nvarchar(60) NOT NULL,
    coverage_type  varchar(20) NOT NULL CHECK (coverage_type IN ('COMPULSORY','VOLUNTARY','GAP')),
    start_date     date NOT NULL,
    end_date       date NOT NULL,
    premium        decimal(18,2) NOT NULL,
    is_financed    bit NOT NULL DEFAULT 1
);

-- 10) เอกสารและการติดตามหนี้
CREATE TABLE lending.Document (
    document_id   bigint IDENTITY PRIMARY KEY,
    loan_id       bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    doc_type      varchar(30) NOT NULL, -- APP_FORM, ID_COPY, CONTRACT, REG_BOOK, etc.
    file_uri      nvarchar(500) NOT NULL,
    uploaded_at   datetime2 NOT NULL DEFAULT sysdatetime()
);

CREATE TABLE lending.CollectionEvent (
    event_id     bigint IDENTITY PRIMARY KEY,
    loan_id      bigint NOT NULL REFERENCES lending.LoanContract(loan_id),
    event_date   datetime2 NOT NULL DEFAULT sysdatetime(),
    event_type   varchar(20) NOT NULL CHECK (event_type IN ('REMIND','FIELD','PROMISE','BREACH','REPO','LEGAL')),
    note         nvarchar(500) NULL,
    created_by   nvarchar(120) NULL
);

-- ดัชนีสำคัญ
CREATE INDEX IX_Customer_NationalId ON lending.Customer(national_id);
CREATE INDEX IX_Contract_Status ON lending.LoanContract(status, province_code);
CREATE INDEX IX_Schedule_Due ON lending.Schedule(loan_id, status, due_date);
CREATE INDEX IX_Collateral_Reg ON lending.CollateralVehicle(registration_no, province_code);
