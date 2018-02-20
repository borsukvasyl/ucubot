USE ucubot;
CREATE TABLE lesson_signal (
id INT NOT NULL AUTO_INCREMENT,
timestamp TIMESTAMP,
signal_type INT,
user_id VARCHAR(255) NOT NULL,
PRIMARY KEY(id)
);