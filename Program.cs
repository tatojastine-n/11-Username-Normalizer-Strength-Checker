using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Account
{
    private static readonly List<string> CommonWords = new List<string>
    {
        "password", "qwerty", "123456", "letmein", "welcome",
        "admin", "login", "master", "hello", "sunshine"
    };

    public string NormalizedUsername { get; }
    public string Password { get; }

    public Account(string rawUsername, string password, HashSet<string> existingUsernames)
    {
        NormalizedUsername = NormalizeUsername(rawUsername, existingUsernames);
        Password = password;

        if (!IsStrongPassword(Password))
        {
            throw new ArgumentException("Password does not meet strength requirements");
        }
    }

    private string NormalizeUsername(string rawUsername, HashSet<string> existingUsernames)
    {
        // 1. Convert to lowercase
        string normalized = rawUsername.ToLower();

        // 2. Replace spaces and special characters with hyphens
        normalized = Regex.Replace(normalized, @"[^\w]", "-");

        // 3. Remove consecutive hyphens
        normalized = Regex.Replace(normalized, @"-+", "-");

        // 4. Trim hyphens from start/end
        normalized = normalized.Trim('-');

        // 5. Handle duplicates
        string finalUsername = normalized;
        int suffix = 1;
        while (existingUsernames.Contains(finalUsername))
        {
            finalUsername = $"{normalized}{suffix++}";
        }

        return finalUsername;
    }

    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        if (CommonWords.Any(w => password.ToLower().Contains(w)))
            return false;

        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
        }
        
        int characterClasses = 0;
        if (hasUpper) characterClasses++;
        if (hasLower) characterClasses++;
        if (hasDigit) characterClasses++;
        if (hasSpecial) characterClasses++;

        return characterClasses >= 3;
    }

    public static string SuggestAlternatives(string baseUsername, HashSet<string> existingUsernames, int maxSuggestions = 3)
    {
        var suggestions = new List<string>();
        int suffix = 1;

        while (suggestions.Count < maxSuggestions)
        {
            string suggestion = $"{baseUsername}{suffix++}";
            if (!existingUsernames.Contains(suggestion))
            {
                suggestions.Add(suggestion);
            }
        }

        return string.Join(", ", suggestions);
    }

    public override string ToString()
    {
        return $"Username: {NormalizedUsername} | Password: {new string('*', Password.Length)} (meets strength requirements)";
    }
}
public class AccountSystem
{
    private HashSet<string> _existingUsernames = new HashSet<string>();
    private List<Account> _accounts = new List<Account>();

    public Account CreateAccount(string rawUsername, string password)
    {
        try
        {
            var account = new Account(rawUsername, password, _existingUsernames);
            _existingUsernames.Add(account.NormalizedUsername);
            _accounts.Add(account);
            return account;
        }
        catch (ArgumentException ex)
        {
            if (ex.Message.Contains("Password"))
            {
                Console.WriteLine($"Weak password for '{rawUsername}'. " +
                    "Password must be at least 8 characters with multiple character types.");
            }

            if (_existingUsernames.Contains(rawUsername.ToLower()))
            {
                string baseName = Regex.Replace(rawUsername.ToLower(), @"[^\w]", "-").Trim('-');
                Console.WriteLine($"Username conflict for '{rawUsername}'. " +
                    $"Suggestions: {Account.SuggestAlternatives(baseName, _existingUsernames)}");
            }

            return null;
        }
    }
}


namespace Username_Normalizer___Strength_Checker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var system = new AccountSystem();

            var acc1 = system.CreateAccount("Jastine ", "S3cur3Pa$$");
            var acc2 = system.CreateAccount("Nicole", "J@ne1234");

            if (acc1 != null) Console.WriteLine(acc1);
            if (acc2 != null) Console.WriteLine(acc2);

            Console.WriteLine("\nAttempting duplicate username:");
            var acc3 = system.CreateAccount("jastine", "An0therPa$$");

            Console.WriteLine("\nAttempting weak password:");
            var acc4 = system.CreateAccount("mochi", "weak");

            Console.WriteLine("\nCreating account with similar name:");
            var acc5 = system.CreateAccount("JastineNicole", "V3ryS3cure!");
            if (acc5 != null) Console.WriteLine(acc5);
        }
    }
}
