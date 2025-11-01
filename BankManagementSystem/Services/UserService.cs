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
    public class UserService
    {
        public bool Register(User user)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO Users (FullName, Email, Password, AccountType, Balance) VALUES (@name,@email,@pass,@type,0)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", user.FullName);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@pass", user.Password);
                    cmd.Parameters.AddWithValue("@type", user.AccountType);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public User Login(string email, string password)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password AND IsActive = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // user mila, ab object return karte hain
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString(),
                                AccountType = reader["AccountType"].ToString(),
                                Balance = Convert.ToDecimal(reader["Balance"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                        else
                        {
                            // login failed
                            return null;
                        }
                    }
                }
            }
        }
    }
}
