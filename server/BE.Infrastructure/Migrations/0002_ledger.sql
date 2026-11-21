-- Ledger (kniha pohybů, jen se přidává) všeho, co mění zůstatek dovolené – kromě měsíčního připisování, to se počítá.
-- request_id zatím nemá cizí klíč: tabulka request přijde s API pro žádosti (cizí klíč doplní pozdější migrace).
CREATE TABLE ledger_entry (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id  uuid NOT NULL REFERENCES employee (id),
    account      text NOT NULL CHECK (account IN ('Weeks1to5', 'Week6')),
    earning_year int  NOT NULL,
    entry_date   date NOT NULL,
    type         text NOT NULL CHECK (type IN ('Usage', 'CancellationReturn', 'ObstacleReturn', 'TransferOut',
                                               'TransferIn', 'ScheduleConversion', 'Expiry', 'PayoutFlag')),
    hours        numeric(8, 2) NOT NULL,
    request_id   uuid NULL,
    note         text NULL,
    created_at   timestamptz NOT NULL DEFAULT now()
);

-- Zůstatky se vždy čtou podle zaměstnance, účtu a roku získání.
CREATE INDEX ix_ledger_entry_employee_account_year ON ledger_entry (employee_id, account, earning_year);
