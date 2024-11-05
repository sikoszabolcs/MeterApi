COPY accounts(id, first_name, last_name)
FROM '/docker-entrypoint-initdb.d/Accounts.csv'
DELIMITER ','
CSV HEADER;