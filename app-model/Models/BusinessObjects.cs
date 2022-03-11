using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace App.Models
{
    public enum Plants
    {
        Default,
        Carrot,
        Potato,
        Wheat
    }

    public class CustomerInfo : ICustomerInfo
    {
        public List<Field> _fields = new();
        public List<Field> Fields
        {
            get => new(_fields);
            init => _fields = value;
        }

        /// <exception cref="InvalidOperationException"/>
        public void AddField(Field field)
        {
            if (!FieldAddingIsPossible)
            {
                throw new InvalidOperationException("List size limit exceeded.");
            }
            _fields.Add(field);
        }

        public void DeleteField(Field field)
        {
            _fields.Remove(field);
        }

        private bool FieldAddingIsPossible => Fields.Count < StaticSettings.ConfigVariables.FieldListLimitSize;
    }

    public class Field : IField
    {
        private string _name;

        public string Location { get; set; }

        [JsonIgnore]
        public Plants Plant { get; set; }
        public string PlantName
        {
            get => Enum.GetName(typeof(Plants), Plant);
            set
            {
                foreach (Plants plant in Enum.GetValues(typeof(Plants)))
                {
                    if (Enum.GetName(typeof(Plants), plant) == value)
                    {
                        Plant = plant;
                        return;
                    }
                }
                Plant = Plants.Default;
            }
        }

        public long PlantingDate { get; set; }

        /// <exception cref="ArgumentException"/>
        public string Name
        {
            get => _name;
            set
            {
                if (!NameIsCorrect(value))
                {
                    throw new ArgumentException("Invalid Name value.");
                }
                _name = value;
            }
        }

        private static bool NameIsCorrect(string name) => name == null
                || (name.Length <= 50 && !new Regex(@"[^a-zA-Z0-9]").IsMatch(name));
    }

    public class Recommendation : IRecommendation
    {
        public string Type { get; init; }    // TODO: enum ?
        public string Value { get; init; }
        public long RelevanceLimitTimestamp { get; init; }

        public bool IsRelevant => ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() <= RelevanceLimitTimestamp;
    }
}
