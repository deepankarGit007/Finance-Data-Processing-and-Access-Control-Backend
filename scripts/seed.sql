-- ============================================================
-- Finance Backend — Seed Script
-- Run AFTER migrations: dotnet ef database update
-- ============================================================

-- Passwords are BCrypt hashes (work factor 12):
--   admin@finance.local   → Admin1234!
--   analyst@finance.local → Analyst1234!
--   viewer@finance.local  → Viewer1234!

INSERT INTO users (id, email, full_name, hashed_password, role, is_active, created_at, updated_at)
VALUES
  (gen_random_uuid(), 'admin@finance.local',
   'System Admin',
   '$2a$12$LszKMCj/gOasHXQXUPzBuuCrfwKWaUlhI5V0.dGHi/tSKCkFEuvze',
   'Admin', true, NOW(), NOW()),

  (gen_random_uuid(), 'analyst@finance.local',
   'Finance Analyst',
   '$2a$12$r4UDhLKh5TIoNnYqdCAEMuUfuOoRE3VB8u9mS8fcH7B4PPEbGFHAi',
   'Analyst', true, NOW(), NOW()),

  (gen_random_uuid(), 'viewer@finance.local',
   'Dashboard Viewer',
   '$2a$12$N7mR8GqGSlUMp9aZ.p4oH.LWwL2g7EkJ5Y/jPKMqjsQFiqDUGqJUC',
   'Viewer', true, NOW(), NOW());

-- Reference the admin user for transaction inserts
DO $$
DECLARE
  admin_id UUID;
BEGIN
  SELECT id INTO admin_id FROM users WHERE email = 'admin@finance.local';

  INSERT INTO transactions (id, created_by_user_id, amount, type, category, date, notes, is_deleted, created_at, updated_at)
  VALUES
    (gen_random_uuid(), admin_id, 5000.00,  'Income',  'Salary',       '2024-11-01', 'November salary',          false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1200.00,  'Expense', 'Rent',         '2024-11-02', 'Monthly rent',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  350.00,  'Expense', 'Food',         '2024-11-05', 'Groceries + dining',       false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 5000.00,  'Income',  'Salary',       '2024-12-01', 'December salary',          false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1200.00,  'Expense', 'Rent',         '2024-12-02', 'Monthly rent',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  420.00,  'Expense', 'Food',         '2024-12-10', 'End of year groceries',    false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  800.00,  'Expense', 'Travel',       '2024-12-20', 'Holiday flights',          false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 5000.00,  'Income',  'Salary',       '2025-01-01', 'January salary',           false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1200.00,  'Expense', 'Rent',         '2025-01-02', 'Monthly rent',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  125.00,  'Expense', 'Utilities',    '2025-01-07', 'Electricity bill',         false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 2500.00,  'Income',  'Freelance',    '2025-01-15', 'Consulting project',       false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  650.00,  'Expense', 'Shopping',     '2025-01-20', 'Electronics',              false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 5000.00,  'Income',  'Salary',       '2025-02-01', 'February salary',          false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1200.00,  'Expense', 'Rent',         '2025-02-02', 'Monthly rent',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  310.00,  'Expense', 'Food',         '2025-02-14', 'Valentines dinner',        false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1800.00,  'Income',  'Freelance',    '2025-02-20', 'Design project payment',   false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 5000.00,  'Income',  'Salary',       '2025-03-01', 'March salary',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id, 1200.00,  'Expense', 'Rent',         '2025-03-02', 'Monthly rent',             false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  200.00,  'Expense', 'Utilities',    '2025-03-08', 'Internet + electricity',   false, NOW(), NOW()),
    (gen_random_uuid(), admin_id,  950.00,  'Expense', 'Travel',       '2025-03-25', 'Weekend trip',             false, NOW(), NOW());
END;
$$;
