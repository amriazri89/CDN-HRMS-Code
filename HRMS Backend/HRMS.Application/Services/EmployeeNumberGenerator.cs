using System;
using System.Globalization;

namespace HRMS.Application.Services
{
    public static class EmployeeNumberGenerator
    {
        public static string Generate(string name, DateTime dateOfBirth)
        {
            // First 3 letters of name (uppercase)
            var prefix = name.Length >= 3
                ? name.Substring(0, 3).ToUpper()
                : name.ToUpper().PadRight(3, 'X');

            // 5-digit random number (padded with zeros)
            var random = new Random();
            var randomNumber = random.Next(0, 100000).ToString("D5");

            // Date of birth formatted as ddMMMyyyy
            var dobString = dateOfBirth.ToString("ddMMMyyyy", CultureInfo.InvariantCulture).ToUpper();

            // Format: ABC-12345-10JAN1994
            return $"{prefix}-{randomNumber}-{dobString}";
        }
    }
}