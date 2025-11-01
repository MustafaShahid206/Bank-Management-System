using System;
using BankManagementSystem.Models;
using BankManagementSystem.Services;

namespace BankManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Bank Management System";

            var userService = new UserService();
            var transactionService = new TransactionService();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== Bank Management System =====");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine();

                switch(choice)
                {
                    case "1":
                        Register(userService);
                        break;
                    case "2":
                        Login(userService, transactionService);
                        break;
                    case "3":
                        Console.WriteLine("Exiting...");
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        static void Register(UserService userService)
        {
            Console.Clear();
            Console.WriteLine("=== Register New Account ===");
            var user = new User();

            Console.Write("Full Name: ");
            user.FullName = Console.ReadLine();

            Console.Write("Email: ");
            user.Email = Console.ReadLine();

            Console.Write("Password: ");
            user.Password = Console.ReadLine();

            Console.Write("Account Type (Saving/Current): ");
            user.AccountType = Console.ReadLine();

            bool success = userService.Register(user);
            Console.WriteLine(success ? "Account created successfully!" : "Registration failed.");
        }

        static void Login(UserService userService, TransactionService transactionService)
        {
            Console.Clear();
            Console.WriteLine("=== Login ===");

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Password: ");
            string password = Console.ReadLine();

            var user = userService.Login(email, password);

            if (user != null)
            {
                Console.WriteLine($"\nWelcome {user.FullName}!");
                UserMenu(user, transactionService);
            }
            else
            {
                Console.WriteLine("Invalid credentials.");
            }
        }

        static void UserMenu(User user, TransactionService transactionService)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== Welcome, {user.FullName} ===");
                Console.WriteLine("1. View Balance");
                Console.WriteLine("2. Deposit");
                Console.WriteLine("3. Withdraw");
                Console.WriteLine("4. Transfer");
                Console.WriteLine("5. Transaction History");
                Console.WriteLine("6. Logout");
                Console.Write("Select option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine($"Your balance: {user.Balance}");
                        break;
                    case "2":
                        transactionService.Deposit(user);
                        break;
                    case "3":
                        //transactionService.Withdraw(user);
                        break;
                    case "4":
                        //transactionService.Transfer(user);
                        break;
                    case "5":
                        transactionService.ShowHistory(user);
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
