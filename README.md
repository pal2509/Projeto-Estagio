# API 4Meteo


## How to run

1. Install .Net Core 5.0
2. Run in CLI with `dotnet run`

### Instaled NuGetPackages

AspNetCore SwashBuckle 5.6.3

### [Timescaledb](https://docs.timescale.com/latest/main)

TimescaleDB is an open-source time-series database optimized for fast ingest and complex queries.

[Creation of a database in timescale.](https://docs.timescale.com/latest/getting-started/setup)

1. Create a new database in pgsql
> `CREATE DATABASE fourmeteo;`
2. Connect to the database
>`\c fourmeteo;`
3. Extend the database with TimescaleDB
>`CREATE EXTENSION IF NOT EXISTS timescaledb;`
4. Create a normal table

```
CREATE TABLE station (
  stationId int PRIMARY KEY NOT NULL,
  latitude DOUBLE PRECISION,
  longitude DOUBLE PRECISION,
  name TEXT,
  createdBy TEXT,
  createdDate TIMESTAMP,
  updatedBy TEXT,
  updatedDate TIMESTAMP
);
 CREATE TABLE stationRecords (
  time TIMESTAMPTZ NOT  NULL,
  stationId INTEGER NOT NULL,
  temperature DOUBLE PRECISION NULL,
  humidity DOUBLE PRECISION NULL,
  windSpeed DOUBLE PRECISION NULL,
  windDir TEXT NULL,
  pressure DOUBLE PRECISION NULL,
  precipitation DOUBLE PRECISION NULL,
  radiation DOUBLE PRECISION NULL,
  leafWetness DOUBLE PRECISION NULL,
  soilMoisture1 DOUBLE PRECISION NULL,
  soilMoisture2 DOUBLE PRECISION NULL,
  soilMoisture3 DOUBLE PRECISION NULL,
  soilTemperature1 DOUBLE PRECISION NULL, 
  soilTemperature2 DOUBLE PRECISION NULL,
  soilTemperature3 DOUBLE PRECISION NULL,
  customD1 DOUBLE PRECISION NULL,
  customD2 DOUBLE PRECISION NULL,
  customD3 DOUBLE PRECISION NULL,
  customD4 DOUBLE PRECISION NULL,
  customD5 DOUBLE PRECISION NULL,
  customT1 TEXT NULL,
  customT2 TEXT NULL,
  customT3 TEXT NULL,
  customT4 TEXT NULL,
  customT5 TEXT NULL
);
ALTER TABLE stationRecords ADD FOREIGN KEY (stationId) REFERENCES station (stationId);
```

