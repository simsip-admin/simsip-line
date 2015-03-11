BEGIN TRANSACTION;
DROP TABLE IF EXISTS PageObstaclesEntity;
CREATE TABLE "PageObstaclesEntity" (
	`PageNumber`	INTEGER NOT NULL,
	`LineNumber`	INTEGER NOT NULL,
	`ObstacleNumber`	INTEGER NOT NULL,
	`ModelName`	TEXT NOT NULL,
	`ObstacleType`	TEXT NOT NULL,
	`LogicalX`	INTEGER NOT NULL,
	`LogicalHeight`	INTEGER NOT NULL,
	`LogicalAngle`	INTEGER NOT NULL,
	`IsGoal`	BOOLEAN
);
COMMIT;
