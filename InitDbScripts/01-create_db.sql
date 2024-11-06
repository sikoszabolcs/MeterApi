CREATE TABLE IF NOT EXISTS accounts (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS readings (
    id SERIAL PRIMARY KEY,
    account_id INTEGER REFERENCES accounts(id),
    meter_reading_date_time TIMESTAMP NOT NULL,
    meter_read_value INT NOT NULL CHECK (meter_read_value >= 0 AND meter_read_value <= 99999),
    CONSTRAINT unique_readings UNIQUE (account_id, meter_reading_date_time, meter_read_value)
);
