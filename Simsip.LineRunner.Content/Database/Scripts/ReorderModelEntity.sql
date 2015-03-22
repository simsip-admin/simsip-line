CREATE TABLE `ModelEntityOrdered` (
	`ModelName`	TEXT NOT NULL,
	`ModelAlias`	TEXT,
	`ModelWidth`	FLOAT NOT NULL,
	`ModelHeight`	FLOAT NOT NULL,
	`ModelDepth`	FLOAT NOT NULL,
	PRIMARY KEY(ModelName)
);

INSERT INTO ModelEntityOrdered (
	ModelName,
	ModelAlias,
	ModelWidth,
	ModelHeight,
	ModelDepth
) 
SELECT 
	ModelName,
	ModelAlias,
	ModelWidth,
	ModelHeight,
	ModelDepth
FROM ModelEntity 
ORDER BY ModelName;

DROP TABLE ModelEntity;

ALtER TABLE ModelEntityOrdered RENAME TO ModelEntity;