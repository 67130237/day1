-- ==========================================
-- REF DATA
-- ==========================================
INSERT INTO dbo.RefProvince (province_code, province_name) VALUES
('10', N'กรุงเทพมหานคร'),
('20', N'ชลบุรี'),
('30', N'นครราชสีมา');

INSERT INTO dbo.RefVehicleMake (make_name) VALUES
(N'Toyota'), (N'Honda'), (N'Isuzu'), (N'Yamaha');

INSERT INTO dbo.RefVehicleModel (make_id, model_name) VALUES
(1, N'Corolla Altis'),
(1, N'Hilux Revo'),
(2, N'City'),
(3, N'D-Max'),
(4, N'Filano');

INSERT INTO dbo.RefProduct (product_code, product_name, purpose)
VALUES
('AUTO_NEW', N'สินเชื่อรถใหม่', 'NEW'),
('AUTO_USED', N'สินเชื่อรถมือสอง', 'USED'),
('MOTO_USED', N'สินเชื่อมอเตอร์ไซค์มือสอง', 'USED');

-- ==========================================
-- CUSTOMER
-- ==========================================
INSERT INTO dbo.Customer (national_id, customer_type, title, first_name, last_name, mobile, province_code)
VALUES
('1234567890123', 'PERSON', N'นาย', N'สมชาย', N'ใจดี', '0811111111', '10'),
('2345678901234', 'PERSON', N'นางสาว', N'สมศรี', N'ขยัน', '0822222222', '20'),
('3456789012345', 'PERSON', N'นาย', N'อนุชา', N'ขับรถ', '0833333333', '30');

-- ==========================================
-- LOAN CONTRACT
-- ==========================================
INSERT INTO dbo.LoanContract (
    contract_no, product_id, status, principal_amount, approved_amount,
    tenor_months, rate_plan_method, rate_pa, first_due_date, contract_date, province_code
) VALUES
('LC2025-0001', 1, 'ACTIVE', 500000, 500000, 48, 'FLAT', 0.055, '2025-10-01', '2025-09-01', '10'),
('LC2025-0002', 2, 'ACTIVE', 350000, 300000, 36, 'EFF', 0.065, '2025-10-05', '2025-09-05', '20'),
('LC2025-0003', 3, 'ACTIVE', 80000, 70000, 24, 'FLAT', 0.12,  '2025-10-10', '2025-09-05', '30');

-- ==========================================
-- LOAN PARTY (ผู้กู้/ผู้ค้ำ)
-- ==========================================
INSERT INTO dbo.LoanParty (loan_id, customer_id, role_code) VALUES
(1, 1, 'MAIN'),
(2, 2, 'MAIN'),
(2, 3, 'GUAR'),
(3, 3, 'MAIN');

-- ==========================================
-- COLLATERAL VEHICLE
-- ==========================================
-- รถยนต์ (Altis)
INSERT INTO dbo.CollateralVehicle (
    loan_id, vehicle_type, registration_no, province_code,
    chassis_no, engine_no, make_id, model_id, model_year, color, odo_km, ownership_type
) VALUES
(1, 'CAR', 'กข1234', '10', 'CHASSIS123ALTIS', 'ENG123ALTIS', 1, 1, 2023, N'ขาว', 15000, 'BORROWER');

INSERT INTO dbo.VehicleCar (collateral_id, body_type, doors)
VALUES (SCOPE_IDENTITY(), 'SEDAN', 4);

-- รถกระบะ (Revo)
INSERT INTO dbo.CollateralVehicle (
    loan_id, vehicle_type, registration_no, province_code,
    chassis_no, engine_no, make_id, model_id, model_year, color, odo_km, ownership_type
) VALUES
(2, 'PICKUP', 'ขค5678', '20', 'CHASSIS567REVO', 'ENG567REVO', 1, 2, 2021, N'ดำ', 50000, 'BORROWER');

INSERT INTO dbo.VehiclePickup (collateral_id, cab_type, drive_type)
VALUES (SCOPE_IDENTITY(), 'DOUBLE', '4WD');

-- มอเตอร์ไซค์ (Yamaha Filano)
INSERT INTO dbo.CollateralVehicle (
    loan_id, vehicle_type, registration_no, province_code,
    chassis_no, engine_no, make_id, model_id, model_year, color, odo_km, ownership_type
) VALUES
(3, 'MOTORCYCLE', 'มจ1122', '30', 'CHASSIS1122FILA', 'ENG1122FILA', 4, 5, 2022, N'น้ำเงิน', 8000, 'BORROWER');

INSERT INTO dbo.VehicleMotorcycle (collateral_id, cc, moto_type)
VALUES (SCOPE_IDENTITY(), 125, 'AUTO');

-- ==========================================
-- RATE PLAN & SCHEDULE
-- ==========================================
INSERT INTO dbo.RatePlan (loan_id, calc_method, rate_pa, installment_amt)
VALUES
(1, 'FLAT', 0.055, 12500),
(2, 'EFF', 0.065, 9800),
(3, 'FLAT', 0.12, 3500);

-- ตัวอย่างงวด (เฉพาะ 2 งวดแรก)
INSERT INTO dbo.Schedule (loan_id, seq_no, due_date, principal_due, interest_due, fee_due)
VALUES
(1, 1, '2025-10-01', 10000, 2500, 0),
(1, 2, '2025-11-01', 10000, 2500, 0),
(2, 1, '2025-10-05', 8500, 1300, 0),
(2, 2, '2025-11-05', 8500, 1300, 0),
(3, 1, '2025-10-10', 3000, 500, 0),
(3, 2, '2025-11-10', 3000, 500, 0);

-- ==========================================
-- RECEIPT & REPAYMENT (การชำระ)
-- ==========================================
INSERT INTO dbo.Receipt (receipt_no, loan_id, pay_date, channel, amount_total)
VALUES
('RC2025-0001', 1, '2025-10-01', 'TRANSFER', 12500),
('RC2025-0002', 3, '2025-10-10', 'CASH', 3500);

INSERT INTO dbo.Repayment (receipt_id, schedule_id, principal_pay, interest_pay)
VALUES
(1, 1, 10000, 2500),
(2, 5, 3000, 500);
