CREATE DATABASE quarter;
CREATE DATABASE quarter_test;
CREATE USER sa01 WITH PASSWORD 'local';
GRANT ALL PRIVILEGES ON DATABASE "quarter" to sa01;
GRANT ALL PRIVILEGES ON DATABASE "quarter_test" to sa01;