using System;
using System.Data;
using System.Data.Common;
using System.Net.Mail;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmailGateway
{
    class program
    {
        static void Main()
        {
            var config = GetConfigraion();
            //Console.WriteLine(config);

            SqlConnection connEmail = CreateConnection(config, "ConnStringEmail");
            //Console.WriteLine(connEmail);
            Console.WriteLine("ConnStringEmail connected successfully!");

            var accountRepo = new AccountRepo(connEmail);

            long ProfileID = long.Parse(GetValue("Enter the Profile ID: "));
            string AccountName = GetValue("Enter the Account Name: ");
            string EmailAddress = GetValue("Enter the Email Address: ");
            string DisplayName = GetValue("Enter the Display Name: ");
            string MailServerName = GetValue("Enter the Mail Server Name: ");
            string MailServerType = GetValue("Enter the Mail Server Type: ");
            int Port = int.Parse(GetValue("Enter the Port Number: "));
            string UserName = GetValue("Enter the UserName: ");
            string Password = GetValue("Enter the Password: ");
            bool Status = bool.Parse(GetValue("Is the Account Active? (true/false)"));
            string enableSSL = GetValue("Enter Email Address");
            string ReplayToAddress = GetValue("Enter Email Address");
            string Description = GetValue("Enter Email Address");

            Account newAccount = new Account()
            {
                    long ProfileID = GetintValue("Enter Profile ID"),
                    string AccountName
                    string EmailAddress
                    string DisplayName
                    string MailServerName
                    string MailServerType
                    int Port
                    string UserName
                    string Password
                    bool Status
                    string enableSSL
                    string ReplayToAddress
                    string Description
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
        private static string GetValue(string text)
        {
            Console.WriteLine(text);
            return Console.ReadLine();
        }

    }
    class Account
    {
        public long BankAccountID { get; set; }
        public long ProfileID { get; set; }
        public string AccountName { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string MailServerName { get; set; }
        public string MailServerType { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
        public string enableSSL { get; set; }
        public string ReplayToAddress { get; set; }
        public string Description { get; set; }
    }

    class AccountRepo
    {
        private SqlConnection _connection;

        //SqlConnection connEmail = CreateConnection(config, "ConnStringEmail");

        public AccountRepo(SqlConnection connection) //constructer
        {
            _connection = connection;
        }

        public bool CreateAccount(Account newAccount)
        {

            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                _connection.Open();

                try
                {
                    string Query = @"insert int Account " +
                                    "(ProfileID, EmailAddress, DisplayName, MailServerName, MailServerType, Port, UserName, Password, Status, enableSSL, ReplayToAddress, Description)" +
                                    "values" +
                                    "(@ProfileID, @EmailAddress, @DisplayName, @MailServerName, @MailServerType, @Port, @UserName, @Password, @Status, @enableSSL, @ReplayToAddress, @Description)";

                    using (SqlCommand Command = new SqlCommand(Query, _connection))
                    {
                        Command.Parameters.AddWithValue("@ProfileID", newAccount.ProfileID);
                        Command.Parameters.AddWithValue("@EmailAddress", newAccount.EmailAddress);
                        Command.Parameters.AddWithValue("@DisplayName", newAccount.DisplayName);
                        Command.Parameters.AddWithValue("@MailServerName", newAccount.MailServerName);
                        Command.Parameters.AddWithValue("@MailServerType", newAccount.MailServerType);
                        Command.Parameters.AddWithValue("@Port", newAccount.Port);
                        Command.Parameters.AddWithValue("@UserName", newAccount.UserName);
                        Command.Parameters.AddWithValue("@Password", newAccount.Password);
                        Command.Parameters.AddWithValue("@Status", newAccount.Status);
                        Command.Parameters.AddWithValue("@enableSSL", newAccount.enableSSL);
                        Command.Parameters.AddWithValue("@ReplayToAddress", newAccount.ReplayToAddress ?? (object)DBNull.Value);
                        Command.Parameters.AddWithValue("@Description", newAccount.Description);

                        Command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        
    }
}
