using System.Collections.Generic;

namespace App.Models
{
    public interface ICustomerInfo
    {
        List<Field> Fields { get; set; }
        string Username { get; set; }

        bool FieldAddingIsPossible();
        /* static */ bool UsernameIsCorrect(string username);
    }

    public interface IField
    {
        string CultivatedPlant { get; set; }
        string Location { get; set; }
        string Name { get; set; }

        /* static */ bool NameIsCorrect(string name);
    }

    public interface IRecommendation
    {
        string Type { get; init; }
        string Value { get; init; }
    }
}