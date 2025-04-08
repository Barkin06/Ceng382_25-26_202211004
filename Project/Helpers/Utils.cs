using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Week2.Helpers
{
    public sealed class Utils
    {
        private static readonly Lazy<Utils> lazy = new Lazy<Utils>(() => new Utils());

        public static Utils Instance { get { return lazy.Value; } }

        private Utils() { }

        public string ExportToJson<T>(List<T> data, List<string> selectedProperties = null)
        {
            if (selectedProperties == null || selectedProperties.Count == 0)
            {
                return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            }

            var filteredData = new List<Dictionary<string, object>>();

            foreach (var item in data)
            {
                var itemDict = new Dictionary<string, object>();
                var properties = item.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedProperties.Contains(prop.Name))
                    {
                        itemDict[prop.Name] = prop.GetValue(item);
                    }
                }
                filteredData.Add(itemDict);
            }

            return JsonSerializer.Serialize(filteredData, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

