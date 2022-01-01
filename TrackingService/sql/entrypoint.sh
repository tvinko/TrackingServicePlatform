#!/bin/bash
database=Accounts
wait_time=25s
password=P@ssw0rd

# wait for SQL Server to come up
echo importing data will start in $wait_time...
sleep $wait_time
echo importing data...

# run the init script to create the DB and the tables in /table
/opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i ./init.sql

TABLE_FILES="table/*.sql"
for table_file in $TABLE_FILES
do 
  echo "Processing $table_file file..."
  /opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i $table_file
done

SP_FILES="stored_procedure/*.sql"
for sp_file in $SP_FILES 
do
  echo "Processing $sp_file file..."
  /opt/mssql-tools/bin/sqlcmd -S 0.0.0.0 -U sa -P $password -i $sp_file
done

#import the data from the csv files
DATA_FILES="data/*.csv"
for data_file in $DATA_FILES 
do
  # i.e: transform /data/MyTable.csv to MyTable
  shortname=$(echo $data_file | cut -f 1 -d '.' | cut -f 2 -d '/')
  tableName=$database.dbo.$shortname
  echo importing $tableName from $data_file
  /opt/mssql-tools/bin/bcp $tableName in $data_file -c -t',' -F 2 -S 0.0.0.0 -U sa -P $password
done