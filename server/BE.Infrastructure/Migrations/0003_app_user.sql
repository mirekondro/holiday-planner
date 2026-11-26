-- Login accounts. Employees log in with an account linked to their employee row;
-- the accountant and the system owner have separate accounts (no employee link).
CREATE TABLE app_user (
    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email         text        NOT NULL,
    password_hash text        NOT NULL,
    role          text        NOT NULL CHECK (role IN ('Employee', 'Accountant', 'SystemOwner')),
    employee_id   uuid        NULL REFERENCES employee (id),
    created_at    timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_app_user_email_domain CHECK (email LIKE '%@businessesbjerg.com'),
    CONSTRAINT ck_app_user_employee_link CHECK ((role = 'Employee') = (employee_id IS NOT NULL))
);

-- Emails are stored in lower case; the index makes sure they are unique regardless.
CREATE UNIQUE INDEX ux_app_user_email ON app_user (lower(email));

-- There is exactly one accountant account and one system owner account.
CREATE UNIQUE INDEX ux_app_user_single_role ON app_user (role) WHERE role IN ('Accountant', 'SystemOwner');

-- An employee has at most one login.
CREATE UNIQUE INDEX ux_app_user_employee ON app_user (employee_id) WHERE employee_id IS NOT NULL;
