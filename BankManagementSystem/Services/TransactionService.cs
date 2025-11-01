using BankManagementSystem.Database;
using BankManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankManagementSystem.Services
{
    public class TransactionService
    {
        public void Deposit(User user)
        {
            Console.Write("Enter amount to deposit: ");
            string input = Console.ReadLine();

            if (!decimal.TryParse(input, out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount. Amount must be a positive number.");
                return;
            }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Update user's balance
                        string updateQuery = "UPDATE Users SET Balance = Balance + @Amount WHERE Id = @UserId AND IsActive = 1";
                        using (var updateCmd = new SqlCommand(updateQuery, conn, tran))
                        {
                            updateCmd.Parameters.AddWithValue("@Amount", amount);
                            updateCmd.Parameters.AddWithValue("@UserId", user.Id);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                // user not found or inactive
                                Console.WriteLine("Deposit failed. User not found or account inactive.");
                                tran.Rollback();
                                return;
                            }
                        }

                        // 2) Insert transaction record
                        string insertQuery = @"
                        INSERT INTO Transactions (UserId, Type, Amount, Date, TargetAccountId)
                        VALUES (@UserId, @Type, @Amount, GETDATE(), NULL)";
                        using (var insertCmd = new SqlCommand(insertQuery, conn, tran))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", user.Id);
                            insertCmd.Parameters.AddWithValue("@Type", "Deposit");
                            insertCmd.Parameters.AddWithValue("@Amount", amount);

                            insertCmd.ExecuteNonQuery();
                        }

                        // 3) Read new balance and update in-memory user object
                        string selectQuery = "SELECT Balance FROM Users WHERE Id = @UserId";
                        using (var selectCmd = new SqlCommand(selectQuery, conn, tran))
                        {
                            selectCmd.Parameters.AddWithValue("@UserId", user.Id);
                            object result = selectCmd.ExecuteScalar();
                            if (result != null && decimal.TryParse(result.ToString(), out decimal newBalance))
                            {
                                user.Balance = newBalance;
                            }
                        }

                        tran.Commit();
                        Console.WriteLine($"Deposit successful. New balance: {user.Balance:C}");
                    }
                    catch (Exception ex)
                    {
                        try { tran.Rollback(); } catch { /* ignore rollback errors */ }
                        Console.WriteLine("An error occurred while performing deposit: " + ex.Message);
                    }
                }
            }
        }

        public void ShowHistory(User user)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT TOP 20 Id, Type, Amount, Date, TargetAccountId 
                    FROM Transactions 
                    WHERE UserId = @UserId 
                    ORDER BY Date DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", user.Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.Clear();
                        Console.WriteLine("=== Transaction History ===\n");

                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No transactions found.");
                            return;
                        }

                        Console.WriteLine("ID\tType\t\tAmount\t\tDate\t\t\tTargetAccount");
                        Console.WriteLine("---------------------------------------------------------------");

                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["Id"]);
                            string type = reader["Type"].ToString();
                            decimal amount = Convert.ToDecimal(reader["Amount"]);
                            DateTime date = Convert.ToDateTime(reader["Date"]);
                            string target = reader["TargetAccountId"] == DBNull.Value ? "-" : reader["TargetAccountId"].ToString();

                            Console.WriteLine($"{id}\t{type}\t\t{amount}\t\t{date}\t{target}");
                        }
                    }
                }
            }
        }
    }
}
