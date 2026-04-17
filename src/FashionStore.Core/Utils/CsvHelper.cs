using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FashionStore.Core.Utils
{
    /// <summary>
    /// Centralized utility for handling CSV operations across the system.
    /// Ensures consistent parsing of quoted fields and escaping of special characters.
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Parses a single CSV line into fields, correctly handling fields enclosed in double quotes.
        /// </summary>
        public static string[] ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<string>();

            var fields = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Handle escaped quotes (nested quotes represented as "")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields.ToArray();
        }

        /// <summary>
        /// Escapes a field for CSV storage. Encloses in quotes if necessary and escapes internal quotes.
        /// </summary>
        public static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            // If it contains a comma, quote, or newline, wrap it in quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
