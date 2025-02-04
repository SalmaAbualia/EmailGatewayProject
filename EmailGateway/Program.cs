using System;
using System.Data;
using System.Data.Common;
using System.Net.Mail;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmailGateway
{
    class Program
    {
        static void Main()
        {
            var config = GetConfigraion();

            SqlConnection connEmail = CreateConnection(config, "ConnStringEmail");
            Console.WriteLine("ConnStringEmail connected successfully!");

            SqlConnection connMarketing = CreateConnection(config, "ConnStringMarketing");
            Console.WriteLine("ConnStringMarketing connected successfully!");

            OpenConnection(connEmail, "ConnStringEmail");
            OpenConnection(connMarketing, "ConnStringMarketing");

            ExecuteCommand(connEmail, "select * from Profile");
            ExecuteCommand(connMarketing, "select * from A2A_iMessaging.dbo.Table_UserSMSProfile");

            int CustID = GetCustomerID();

            int isCustomeExists = CheckCustomerExists(connMarketing, CustID);

            if (isCustomeExists > 0)
            {
                string profileName = GetInfo("Enter Profile Name: ");
                CreateProfile(connEmail, CustID, profileName);
                Console.WriteLine("Profile created successfully.");
            }
            else
            {
                Console.WriteLine($"Customer with the specified CustID : {CustID} does not exist.");
            }
        }

        private static IConfiguration GetConfigraion()
        {
            return new ConfigurationBuilder().AddJsonFile("AppSetting.json").Build();
        }

        private static SqlConnection CreateConnection(IConfiguration config, string ConnectionKey)
        {
            string connectionString = config.GetSection(ConnectionKey).Value;
            return new SqlConnection(connectionString);
        }

        private static void OpenConnection(SqlConnection connection, string connectionName)
        {
            connection.Open();
            Console.WriteLine($"{connectionName} connection opened successfully!");
        }

        private static SqlCommand CreateCommand(SqlConnection connection, string Query)
        {
            return new SqlCommand(Query, connection);
        }

        private static void ExecuteCommand(SqlConnection connection, string Query)
        {
            using (SqlCommand Command = new SqlCommand(Query, connection))
            {
                Command.ExecuteNonQuery();
                Console.WriteLine("Command executed successfully!");
            }
        }

        private static int GetCustomerID()
        {
            Console.WriteLine("Enter Customer ID: ");
            return int.Parse(Console.ReadLine());
        }

        private static string GetInfo(string stm)

        {
            Console.WriteLine(stm);
            return Console.ReadLine();
        }

        private static int CheckCustomerExists(SqlConnection connection, int custID)
        {
            string Query = "select count(*) from A2A_iMessaging.dbo.Table_UserSMSProfile where CustID = @CustID";

            using (SqlCommand command = new SqlCommand(Query, connection))
            {
                command.Parameters.AddWithValue("@CustID", custID);
                int matchRecord = (int)command.ExecuteScalar();
                return matchRecord;
            }
        }

        //private static void CreateProfile(SqlConnection connection, int CustID, string ProfileName)
        //{
        //    using (SqlCommand Command = new SqlCommand("PR_CreateEmailProfile" ,connection))
        //    {
        //        Command.CommandType = System.Data.CommandType.StoredProcedure;
        //        Command.Parameters.AddWithValue("@ProfileName", ProfileName);
        //        Command.ExecuteNonQuery();
        //    }

        //    string Query = "insert into Profile (ProfileName, CustID) VALUES (@ProfileName, @CustID)";
        //    using (SqlCommand InsertCommand = new SqlCommand(Query, connection))
        //    {
        //        InsertCommand.CommandType = System.Data.CommandType.StoredProcedure;
        //        InsertCommand.Parameters.AddWithValue("@ProfileName", ProfileName);
        //        InsertCommand.Parameters.AddWithValue("@CustID", CustID);
        //        InsertCommand.ExecuteNonQuery();
        //    }
        //}

        private static void CreateProfile(SqlConnection connection, int custID, string profileName)
        {
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    Console.WriteLine(custID + 1);
                    ExecuteCreateEmailProfile(connection, transaction, profileName);
                    InsertProfile(connection, transaction, profileName, custID);

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        private static void ExecuteCreateEmailProfile(SqlConnection connection, SqlTransaction transaction, string profileName)
        {
            using (SqlCommand Command = new SqlCommand("PR_CreateEmailProfile", connection, transaction))
            {
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.AddWithValue("@ProfileName", profileName);
                Command.ExecuteNonQuery();
            }
        }

        private static void InsertProfile(SqlConnection connection, SqlTransaction transaction, string profileName, int custID)
        {
            using (SqlCommand InsertCommand = new SqlCommand("PR_InsertProfile", connection, transaction))
            {
                Console.WriteLine(custID + 1);

                InsertCommand.CommandType = CommandType.StoredProcedure;
                InsertCommand.Parameters.AddWithValue("@ProfileName", profileName);
                InsertCommand.Parameters.AddWithValue("@CustID", custID);
                InsertCommand.ExecuteNonQuery();
            }
        }

        private static void CreateEmailAccount(SqlConnection connection, int custID)
        {
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string AccountName = GetInfo("Enter Account Name: ");
                    string EmailAddress = GetInfo("Enter Email Address: ");
                    string AccountDisplayName = GetInfo("Enter Account Display Name: ");
                    string ReplayToAddress = GetInfo("Enter ReplayTo Address(Optional): ");
                    string Description = GetInfo("Enter Description: ");
                    string MailServerName = GetInfo("Enter Mail Server Name (e.g., smtp.gmail.com): ");
                    string MailServerType = GetInfo("Enter Mail Server Type: ");
                    int Port = int.Parse(GetInfo("Enter Port Number (e.g., 587 for TLS, 465 for SSL): "));
                    string UserName = GetInfo("Enter User Name: ");
                    string Password = GetInfo("Enter Password: ");
                    bool enableSSL = GetSSLInput(GetInfo("Enable SSL? (yes/no): "));

                    CreateAccount(connection, transaction, AccountName, EmailAddress, AccountDisplayName, ReplayToAddress, Description, MailServerName, MailServerType, Port,
                               UserName, Password, enableSSL);

                    //LinkAccountToProfile()

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void CreateAccount(SqlConnection connection, SqlTransaction transaction, string AccountName, string EmailAddress,
                                       string AccountDisplayName, string ReplayToAddress, string Description, string MailServerName,
                                       string MailServerType, int Port, string UserName, string Password, bool enableSSL)
        {
            using (SqlCommand Command = new SqlCommand("PR_CreateAccount", connection, transaction))
            {
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.AddWithValue("@account_name", AccountName);
                Command.Parameters.AddWithValue("@email_address", EmailAddress);
                Command.Parameters.AddWithValue("@display_name", AccountDisplayName);
                Command.Parameters.AddWithValue("@replyto_address", ReplayToAddress);
                Command.Parameters.AddWithValue("@description", Description);
                Command.Parameters.AddWithValue("@mailserver_name", MailServerName);
                Command.Parameters.AddWithValue("@mailserver_type", MailServerType);
                Command.Parameters.AddWithValue("@port", Port);
                Command.Parameters.AddWithValue("@username", UserName);
                Command.Parameters.AddWithValue("@password", Password);
                Command.Parameters.AddWithValue("@enable_ssl", enableSSL ? 1 : 0);
                Command.ExecuteNonQuery();
            }
        }

        private static bool GetSSLInput(string stm)
        {
            if (stm == "yes" || stm == "Yes" || stm == "Y" || stm == "y")
                return true;
            if (stm == "no" || stm == "No" || stm == "N" || stm == "n")
                return false;
            return true;
        }
    }
}
