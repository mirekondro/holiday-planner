-- Weekly work schedules with full history. A schedule applies from valid_from to valid_to
-- (null = still valid). The API closes the previous period when a new one starts, so periods
-- of one employee never overlap.
CREATE TABLE work_schedule (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id uuid NOT NULL REFERENCES employee (id),
    valid_from  date NOT NULL,
    valid_to    date NULL,
    CONSTRAINT ck_work_schedule_period CHECK (valid_to IS NULL OR valid_to >= valid_from),
    CONSTRAINT ux_work_schedule_start UNIQUE (employee_id, valid_from)
);

-- At most one open (still valid) schedule per employee.
CREATE UNIQUE INDEX ux_work_schedule_open ON work_schedule (employee_id) WHERE valid_to IS NULL;

-- One row per weekday Monday (1) – Friday (5). No times = day off.
CREATE TABLE work_schedule_day (
    schedule_id uuid NOT NULL REFERENCES work_schedule (id) ON DELETE CASCADE,
    weekday     int  NOT NULL CHECK (weekday BETWEEN 1 AND 5),
    start_time  time NULL,
    end_time    time NULL,
    PRIMARY KEY (schedule_id, weekday),
    CONSTRAINT ck_work_schedule_day_times CHECK (
        (start_time IS NULL AND end_time IS NULL) OR (start_time IS NOT NULL AND end_time > start_time))
);
