using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace App.Models
{
    public class AccountInfo
    {
        public string AuthToken { get; set; }
        public CustomerInfo CustomerInfo { get; set; }
    }

    public class CustomerInfo
    {
        public string Username { get; set; } = string.Empty;
        public List<Field> Fields { get; set; } = new();

        public bool FieldAddingIsPossible() => Fields.Count < StaticSettings.ConfigVariables.FieldListLimitSize;
        public static bool UsernameIsCorrect(string username) => username != null && 6 <= username.Length
                && username.Length <= 20 && !new Regex(@"\W").IsMatch(username);
    }

    public class Field
    {
        public string Location { get; set; }
        public string CultivatedPlant { get; set; }
        //public long PlantingDate { get; set; }
        public string Name { get; set; }

        public static bool NameIsCorrect(string name) => name != null && name.Length <= 30;
    }

    public class Recommendation
    {
        public string Type { get; init; }
        public string Value { get; init; }
        public long RelevanceLimitTimestamp { get; init; }

        public bool IsRelevant() => ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() <= RelevanceLimitTimestamp;
    }
}
