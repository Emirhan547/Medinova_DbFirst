using System;

namespace Medinova.Helpers
{
    public static class Env
    {
        public static string Get(string key, bool required = true)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (required && string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{key}' tanımlı değil veya boş.");
            }

            return value;
        }

        public static int GetInt(string key)
        {
            return int.Parse(Get(key));
        }

   
        public static bool GetBool(string key, bool defaultValue = false)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public static bool Exists(string key)
        {
            return !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(key));
        }
    }
}
