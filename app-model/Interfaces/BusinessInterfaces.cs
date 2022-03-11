using System.Collections.Generic;

namespace App.Models
{
    public interface ICustomerInfo
    {
        List<Field> Fields { get; set; }
        public void AddField(Field field);
        public void DeleteField(Field field);
    }

    public interface IField
    {
        Plants Plant { get; set; }
        string Location { get; set; }
        string Name { get; set; }
        long PlantingDate { get; set; }
    }

    public interface IRecommendation
    {
        string Type { get; init; }
        string Value { get; init; }
    }
}