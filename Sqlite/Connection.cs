using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Sqlite
{
    public class Connection
    {

        public async static Task InitializeDatabase()
        {
            //nombre base de datos sqlite
            using (SqliteConnection db =
               new SqliteConnection("Filename=data.db"))
            {
                //llamas al metodo que nos devolvera un listado de querys que tendran las tablas temporales
                List<string> tablesQuery = ListTables();

                //recorremos la lista de querys que contienen todos los scripts para crear las tablas
                for (int i = 0; i < tablesQuery.Count; i++)
                {
                    await db.OpenAsync();
                    SqliteCommand createTable = new SqliteCommand(tablesQuery[i], db);
                    await createTable.ExecuteReaderAsync();
                    db.Close();
                }

            }
        }


        //funcion con scripts para la creacion de las tablas de sqlite
        private static List<string> ListTables()
        {
            List<string> tables = new List<string>();

            tables.Add("CREATE TABLE IF NOT EXISTS DBAccess (" +
                         "Server_Name NVARCHAR(2048) NOT NULL, " +
                         "Username NVARCHAR(2048) NOT NULL, " +
                         "Password NVARCHAR(2048) NOT NULL, " +
                         "Db_Name NVARCHAR(2048) NOT NULL" +
                       ")");

            tables.Add("CREATE TABLE IF NOT EXISTS StoreDataTransferExecutions (" +
                         "Id INT IDENTITY(1,1) PRIMARY KEY, "+
                         "ExecutionTime INT, " +
                         "TransferType INT DEFAULT 0, " +
                         "Status INT DEFAULT 0, " +
                         "ErrorMessage NVARCHAR(2048) DEFAULT NULL" +
                       ");");

            return tables;
        }
    }
}
