.open ../linerunner.db
DELETE FROM PageObstaclesEntity;
GO
.mode csv
.import Import-PageObstaclesEntity.csv PageObstaclesEntity
DELETE FROM PageObstaclesEntity WHERE PageNumber='PageNumber';
.exit
