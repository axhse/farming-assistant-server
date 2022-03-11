using System.Text.Json;
using System.IO;

namespace App.Models
{
    public static class StaticSettings
    {
        public static readonly string ConfigFilePath = "config.json";
        public static readonly string AccountDataPath = "AccountData.json";

        public static ConfigVariables ConfigVariables = new()
        {
            ServerUrl = "farming-assistant.eastus.cloudapp.azure.com",
            ServerPort = 80,
            DefaultToken = "0000000000000000000000000000000000000000000000000000000000000000",
            SendingTimeout = 5000,
            ReceivingTimeout = 5000,
            FieldListLimitSize = 100
        };
        // public static ConfigVariables ConfigVariables { get; private set; } = ConfigVariables.LoadFromFile();
    }

    public class ConfigVariables
    {
        public string ServerUrl { get; init; }
        public int ServerPort { get; init; }
        public int SendingTimeout { get; init; }
        public int ReceivingTimeout { get; init; }
        public string DefaultToken { get; init; }
        public int FieldListLimitSize { get; init; }

        public static ConfigVariables LoadFromFile() =>
            JsonSerializer.Deserialize<ConfigVariables>(File.ReadAllText(StaticSettings.ConfigFilePath));
    }
}
