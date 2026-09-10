using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Program;

public class Expenses
{
    private static int _nextId = 1;

    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public Expenses() { }

    public static Expenses Create(string description, decimal amount)
    {
        return new Expenses
        {
            Id = _nextId++,
            Description = description,
            Amount = amount
        };
    }

    public static void UpdateNextId(int currentMaxId)
    {
        _nextId = Math.Max(_nextId, currentMaxId + 1);
    }
}

public class Program
{
    public static List<Expenses> expensesList = new List<Expenses>();
    private const string FilePath = "expenses.json";

    public static void Main(string[] args)
    {
        LoadDataFromFile();

        Console.WriteLine("""
            Commands:
            add <description> <amount> - Add a new expense
            list - List all expenses
            summary - Show total expenses
            summary <month> - Show total expenses for a specific month (1-12)
            delete <id> - Delete an expense by ID
            """
            );

        while (true)
        {
            Console.Write("\nEnter command: ");

            string? input = Console.ReadLine()?.Trim().ToLower();
            if (string.IsNullOrEmpty(input)) continue;

            string[] commandArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = commandArgs[0];

            switch (command)
            {
                case "add":
                    if (commandArgs.Length < 3)
                    {
                        Console.WriteLine("Error: Invalid command syntax! Use: add <description> <amount>");
                        break;
                    }

                    string description = commandArgs[1];
                    if (decimal.TryParse(commandArgs[2], out decimal amount))
                    {
                        add(description, amount);
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid amount! Please enter a valid number.");
                    }
                    break;

                case "list":
                    if (expensesList.Count == 0)
                    {
                        Console.WriteLine("No expenses recorded.");
                    }
                    else
                    {
                        list();
                    }
                    break;

                case "summary":
                    if (expensesList.Count == 0)
                    {
                        Console.WriteLine("No expenses recorded.");
                    }
                    else if (commandArgs.Length > 1 && int.TryParse(commandArgs[1], out int month))
                    {
                        if (month >= 1 && month <= 12)
                        {
                            summary_ByMonth(month);
                        }
                        else
                        {
                            Console.WriteLine("Invalid month. Please enter a number between 1 and 12.");
                        }
                    }
                    else
                    {
                        summary();
                    }
                    break;

                case "delete":
                    if (commandArgs.Length < 2)
                    {
                        Console.WriteLine("Error: Invalid command syntax! Use: delete <id>");
                        break;
                    }

                    if (int.TryParse(commandArgs[1], out int id))
                    {
                        delete(id);
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid ID! Please enter a valid number.");
                    }
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
    }

    private static void SaveDataToFile()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(expensesList, options);
        File.WriteAllText(FilePath, json);
    }

    private static void LoadDataFromFile()
    {
        if (!File.Exists(FilePath)) return;

        string json = File.ReadAllText(FilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            expensesList = new List<Expenses>();
            return;
        }

        expensesList = JsonSerializer.Deserialize<List<Expenses>>(json) ?? new List<Expenses>();

        int maxId = 0;
        foreach (var item in expensesList)
        {
            if (item.Id > maxId) maxId = item.Id;
        }
        Expenses.UpdateNextId(maxId);
    }
    public static void add(string description, decimal amount)
    {
        var expense = Expenses.Create(description, amount);
        expensesList.Add(expense);

        SaveDataToFile();

        Console.WriteLine($"ID: {expense.Id} Date: {expense.Date} Description: {description}, Amount: {amount}");
    }

    public static void list()
    {
        foreach (var item in expensesList)
        {
            Console.WriteLine($"ID: {item.Id} Date: {item.Date} Description: {item.Description} Amount: {item.Amount}");
        }
    }

    public static void summary()
    {
        decimal totalAmount = 0;
        foreach (var item in expensesList)
        {
            totalAmount += item.Amount;
        }
        Console.WriteLine($"Total Expenses: {totalAmount}");
    }

    public static void summary_ByMonth(int month)
    {
        decimal totalAmount = 0;
        int currentYear = DateTime.Now.Year;

        foreach (var item in expensesList)
        {
    
            if (item.Date.Month == month && item.Date.Year == currentYear)
            {
                totalAmount += item.Amount;
            }
        }
        Console.WriteLine($"Total expenses for month {month}: ${totalAmount}");
    }
    public static void delete(int id)
    {
        var expenseToRemove = expensesList.Find(expense => expense.Id == id);
        if (expenseToRemove != null)
        {
            expensesList.Remove(expenseToRemove);

            SaveDataToFile();

            Console.WriteLine($"Expense with ID {id} has been deleted.");
        }
        else
        {
            Console.WriteLine($"Expense with ID {id} not found.");
        }
    }
}