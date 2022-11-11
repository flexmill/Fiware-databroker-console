using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Diagnostics.CodeAnalysis;

namespace Databroker.Helper
{
    public static class DBHelper
    {
        private static string connectionString = "";
        private static MySqlConnection connection = null;

        public static void GetConnectionString()
        {
            string databaseServer  = "185.131.183.61";
            string databasePort     = "53306";
            string databaseUsername = "flexmill";
            string databasePassword = "5t39PDfTeV7JWyaS";
           // connectionString = "server=192.168.5.146;port=3306;user=" + databaseUsername + ";password=" + databasePassword + ";database=flexmill"; //Containerized MariaDB
            connectionString = "server=" + databaseServer + ";port=" + databasePort + ";user=" + databaseUsername + ";password=" + databasePassword + ";database=flexmill";

         //   Console.WriteLine(connectionString);
        }

        public static void ConnectToDatabase()
        {
            if (connectionString == "")
            {
                GetConnectionString();
            }
            System.Diagnostics.Debug.WriteLine("Connection-string: " + connectionString);
            connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                System.Diagnostics.Debug.WriteLine("Connected to database");
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Connection-exception: " + ex.Message);
            }
        }
  
        static public DataTable Query(string sql, Dictionary<string, string> query_params = null, bool ShowDebug = true)
        {
            DataTable dt = new DataTable();

            query_params = query_params ?? new Dictionary<string, string>();

            if (connection == null || connection.State != ConnectionState.Open)
            {
                ConnectToDatabase();
            }

       //     System.Diagnostics.Debug.WriteLine("SQL: " + sql);
            var command = new MySqlCommand(sql, connection);
            foreach (var KVP in query_params)
            {
                command.Parameters.AddWithValue(KVP.Key, KVP.Value);
            //    System.Diagnostics.Debug.WriteLine(KVP.Key + ": " + KVP.Value);
            }

            try
            {
                dt.Load(command.ExecuteReader());
            } catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("Query-exception: " + ex.Message);
            }
            command.Dispose();
            //System.Diagnostics.Debug.WriteLine("Query finished");

            return dt;
        }

        static public string Exec(string sql, Dictionary<string, string> query_params = null, bool ShowDebug = true)
        {
            string exec_result = "";
            query_params = query_params ?? new Dictionary<string, string>();

            if (connection == null || connection.State != ConnectionState.Open)
            {
                ConnectToDatabase();
            }
          //  Console.WriteLine("Exec-Hit: " + sql);

            var command = new MySqlCommand(sql, connection);
            foreach (var KVP in query_params)
            {
                command.Parameters.AddWithValue(KVP.Key, KVP.Value);
               // Console.WriteLine(KVP.Key + ": " + KVP.Value);
            }

            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                exec_result = ex.Message;
                Console.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("Exec-Exception: " + ex.Message);
            }
            command.Dispose();
    
            return exec_result;
        }
    }
}