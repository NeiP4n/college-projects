using System;
using System.Data;
using System.Data.OleDb;
using Npgsql;

namespace MyDatabaseLibrary
{
    public class MyDatabaseLibrary
    {


        private string accessConnectionString;
        private string postgreConnectionString;

        public MyDatabaseLibrary(string accessConnectionString, string postgreConnectionString)
        {
            this.accessConnectionString = accessConnectionString;
            this.postgreConnectionString = postgreConnectionString;
        }

        public DataTable LoadTables(string selectedDatabase)
        {
            DataTable tables = new DataTable();
            try
            {
                using (var connection = GetConnection(selectedDatabase))
                {
                    connection.Open();
                    if (selectedDatabase == "Access")
                    {
                        OleDbConnection oleDbConnection = (OleDbConnection)connection;
                        DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                        tables = schemaTable;
                    }
                    else if (selectedDatabase == "PostgreSQL")
                    {
                        NpgsqlConnection npgsqlConnection = (NpgsqlConnection)connection;
                        DataTable schemaTable = npgsqlConnection.GetSchema("Tables");
                        tables = schemaTable;
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при загрузке таблиц: " + ex.Message);
            }
            return tables;
        }

        public void UpdateData(string selectedDatabase, string selectedTable)
        {
            try
            {
                using (var connection = GetConnection(selectedDatabase))
                {
                    connection.Open();
                    if (selectedDatabase == "Access")
                    {
                        // Обновление данных для Access
                    }
                    else if (selectedDatabase == "PostgreSQL")
                    {
                        // Обновление данных для PostgreSQL
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine("Ошибка при обновлении данных: " + ex.Message);
            }
        }

        private IDbConnection GetConnection(string selectedDatabase)
        {
            if (selectedDatabase == "Access")
            {
                return new OleDbConnection(accessConnectionString);
            }
            else if (selectedDatabase == "PostgreSQL")
            {
                return new NpgsqlConnection(postgreConnectionString);
            }
            else
            {
                throw new ArgumentException("Неверно выбрана база данных");
            }
        }
    }
}
