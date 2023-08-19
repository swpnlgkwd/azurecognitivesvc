
// Added for IConfigurationBuilder 
using Microsoft.Extensions.Configuration;
using System;

namespace azureaisolution
{
    internal class Program
    {
        // Configuration required to connect to Cognitive Service
        private static string? cogSvcEndpoint;
        private static string? cogSvcKey;
        private static string? region;

        static async Task Main(string[] args)
        {
            // Load and Display Configuration
            LoadConfiguration();
            DisplayConfigurationValues();

            if (region != null && cogSvcEndpoint != null && cogSvcKey != null)
            {
                Console.WriteLine("Speech to Text Service");
                await SpeechToTextPractice.ConvertWavFileSpeechToText(cogSvcKey, region);
            }

        }

        public static void LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            cogSvcEndpoint = configuration["CognitiveServiceEndpoint"];
            cogSvcKey = configuration["CognitiveServiceKey"];
            region = configuration["CognitiveServiceRegion"];
        }

        private static void DisplayConfigurationValues()
        {
            Console.WriteLine("Cognitive Service Configuration:");
            Console.WriteLine($"ServiceEndPoint: {cogSvcEndpoint}");
            Console.WriteLine($"ServiceKey: {cogSvcKey}");
            Console.WriteLine($"ServiceRegion: {region}");
            Console.WriteLine();
        }
    }
}
