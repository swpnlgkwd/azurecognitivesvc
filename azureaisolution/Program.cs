
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
                
                while (true)
                {

                    Console.WriteLine("Select Service to use - ");
                    Console.WriteLine("1. Speech Service");
                    int serviceType = Convert.ToInt32(Console.ReadLine());
                    switch (serviceType)
                    {
                        case 1:
                            Console.WriteLine("You Selected Speech Service...");
                            Console.WriteLine();

                            Console.WriteLine("Select Speech Service type - ");
                            Console.WriteLine("1. Speech to Text Service");
                            Console.WriteLine("2. Text to Speech Service");

                            int speechServiceType = Convert.ToInt32(Console.ReadLine());

                            switch (speechServiceType)
                            {
                                case 1:
                                    Console.WriteLine("You Selected Speech to Text");
                                    await SpeechServicePractice.ConvertWavFileSpeechToText(cogSvcKey, region);
                                    break;
                                case 2:
                                    Console.WriteLine("You Selected Text to Speech");
                                    await SpeechServicePractice.ConvertTextToSpeech(cogSvcKey, region);
                                    break;
                                default:
                                    Console.WriteLine("Please enter valid number");
                                    break;
                            }
                            break;

                        default:
                            Console.WriteLine("Please enter valid Service");
                            break;
                    }

                    Console.Write("Do you want to continue? (yes/no): ");
                    string userInput = Console.ReadLine();
                    if (userInput.Equals("no", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

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
