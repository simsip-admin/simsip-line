.open ../linerunner.db
DELETE FROM RandomObstaclesEntity;
GO
.mode csv
.import Import-RandomObstaclesEntity.csv RandomObstaclesEntity
DELETE FROM RandomObstaclesEntity WHERE RandomObstaclesSet='RandomObstaclesSet';
.exit
