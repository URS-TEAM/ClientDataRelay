using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Sqlite
{
    public class DBData : Connection
    {
        private const string EncryptionKey = "rpaSPvIvVLlrcmtzPU9/c67Gkj7yL1S5";
        private string connString = "Filename=data.db";

        public event EventHandler<DatabaseChangedEventArgs> DatabaseChanged;

        protected virtual void OnDatabaseChangedEvent(DatabaseChangedEventArgs e)
        {
            DatabaseChanged?.Invoke(this, e);
        }

        public async void AddData(string serverName, string username, string password, string dbName)
        {
            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "DELETE FROM DBAccess;";
                        command.ExecuteReader();
                    }
                }
            }
            catch (SqliteException)
            {
                await Connection.InitializeDatabase();
            }

            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "INSERT INTO DBAccess (Server_Name, Username, Password, Db_Name)" +
                            " VALUES (@Entry1, @Entry2, @Entry3, @Entry4);";
                        command.Parameters.AddWithValue("@Entry1", Encrypt(serverName));
                        command.Parameters.AddWithValue("@Entry2", Encrypt(username));
                        command.Parameters.AddWithValue("@Entry3", Encrypt(password));
                        command.Parameters.AddWithValue("@Entry4", Encrypt(dbName));

                        command.ExecuteReader();
                        OnDatabaseChangedEvent(new DatabaseChangedEventArgs(true));
                    }
                }
            }
            catch (SqliteException)
            {
                await Connection.InitializeDatabase();
            }
        }



        public string GetServer()
        {
            string entries = "";
            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "SELECT Server_Name FROM DBAccess";

                        using (var query = command.ExecuteReader())
                        {
                            if (query.HasRows)
                            {
                                while (query.Read())
                                {
                                    entries = query.GetString(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqliteException)
            {
                Connection.InitializeDatabase();
            }

            return Decrypt(entries);
        }



        public string GetUser()
        {
            string entries = "";

            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "SELECT Username FROM DBAccess";

                        using (var query = command.ExecuteReader())
                        {
                            if (query.HasRows)
                            {
                                while (query.Read())
                                {
                                    entries = query.GetString(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqliteException)
            {
                Connection.InitializeDatabase();
            }

            return Decrypt(entries);
        }


        public string GetPass()
        {
            string entries = "";

            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "SELECT Password FROM DBAccess";

                        using (var query = command.ExecuteReader())
                        {
                            if (query.HasRows)
                            {
                                while (query.Read())
                                {
                                    entries = query.GetString(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqliteException)
            {
                Connection.InitializeDatabase();
            }

            return Decrypt(entries);
        }


        public string GetDb()
        {
            string entries = "";

            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "SELECT Db_Name FROM DBAccess";

                        using (var query = command.ExecuteReader())
                        {
                            if (query.HasRows)
                            {
                                while (query.Read())
                                {
                                    entries = query.GetString(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqliteException)
            {
                Connection.InitializeDatabase();
            }

            return Decrypt(entries);
        }

        public void Drop_Server()
        {
            try
            {
                using (var db = new SqliteConnection(connString))
                {
                    db.Open();

                    using (var command = new SqliteCommand())
                    {
                        command.Connection = db;
                        command.CommandText = "DELETE FROM DBAccess";
                        command.ExecuteReader();

                        OnDatabaseChangedEvent(new DatabaseChangedEventArgs(false));
                    }
                }
            }
            catch (SqliteException)
            {
                Connection.InitializeDatabase();
            }
        }

        public string Encrypt(string text)
        {
            try
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                    {
                        text += EncryptionKey;
                        tdes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                        tdes.Mode = CipherMode.ECB;
                        tdes.Padding = PaddingMode.PKCS7;
                        using (ICryptoTransform transform = tdes.CreateEncryptor())
                        {
                            byte[] textBytes = Encoding.UTF8.GetBytes(text);
                            byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                            return Convert.ToBase64String(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public string Decrypt(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            try
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
                    {
                        tdes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(EncryptionKey));
                        tdes.Mode = CipherMode.ECB;
                        tdes.Padding = PaddingMode.PKCS7;
                        using (ICryptoTransform transform = tdes.CreateDecryptor())
                        {
                            byte[] cipherBytes = Convert.FromBase64String(text);
                            byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                            string value = Encoding.UTF8.GetString(bytes);
                            value = value.Replace(EncryptionKey, string.Empty);
                            return value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }

    public class DatabaseChangedEventArgs : EventArgs
    {
        public bool NewStatus { get; }

        public DatabaseChangedEventArgs(bool newStatus)
        {
            NewStatus = newStatus;
        }
    }
}
