
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

        // Configuration for Clasify Image
        private static string? classifyImagePredictionEndpoint;
        private static string? classifyImagePredictionKey;
        private static string? classifyImageModelName;
        private static Guid classifyImageProjectID;

        // Configuration for Detect Object
        private static string? detectObjectPredictionEndpoint;
        private static string? detectObjectPredictionKey;
        private static string? detectObjectModelName;
        private static Guid detectObjectProjectID;

        // Configuration for Face API
        private static string? faceAPIEndpoint;
        private static string? faceAPIKey;



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
                    Console.WriteLine("2. Computer Vision Service");
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

                        case 2:
                            Console.WriteLine("You Selected : Computer Vision Service...");
                            Console.WriteLine();

                            Console.WriteLine("Select Computer Vision Capability - ");
                            Console.WriteLine("1. Analyze Images");
                            Console.WriteLine("2. Classify Image");
                            Console.WriteLine("3. Detect Object");
                            Console.WriteLine("4. Face Detection");
                            Console.WriteLine();


                            int computerVisionCapability = Convert.ToInt32(Console.ReadLine());

                            switch (computerVisionCapability)
                            {
                                case 1:
                                    Console.WriteLine("You Selected : Analyzing Images");
                                    await ComputerVisionPractice.AnalyseImage(cogSvcKey, cogSvcEndpoint);
                                    break;
                                case 2:
                                    Console.WriteLine("You Selected : Classify Image");
                                    await ComputerVisionPractice.GetPredictionsForImageAsync(classifyImagePredictionEndpoint,
                                        classifyImagePredictionKey, classifyImageProjectID, classifyImageModelName);
                                    break;
                                case 3:
                                    Console.WriteLine("You Selected : Detect Object");
                                    await ComputerVisionPractice.DetectObjectsForImageAsync(detectObjectPredictionEndpoint,
                                        detectObjectPredictionKey, detectObjectProjectID, detectObjectModelName);
                                    break;
                                case 4:
                                    Console.WriteLine("You Selected : Face Detection");
                                    await ComputerVisionPractice.DetectFacePredictionAsync(faceAPIEndpoint,faceAPIKey);
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

            // General Configuration to connect to Cognitive Service
            cogSvcEndpoint = configuration["CognitiveServiceEndpoint"];
            cogSvcKey = configuration["CognitiveServiceKey"];
            region = configuration["CognitiveServiceRegion"];

            // Configuration of Custom Vision for Classify Images
            classifyImagePredictionEndpoint = configuration["ClassifyImagePredictionEndpoint"];
            classifyImagePredictionKey = configuration["ClassifyImagePredictionKey"];
            classifyImageProjectID = Guid.Parse(configuration["ClassifyImageProjectId"]);
            classifyImageModelName = configuration["ClassifyImageModelName"];

            // Configuration of Custom Vision for Detecting Object within Image
            detectObjectPredictionEndpoint = configuration["DetectObjectPredictionEndpoint"];
            detectObjectPredictionKey = configuration["DetectObjectPredictionKey"];
            detectObjectProjectID = Guid.Parse(configuration["DetectObjectProjectId"]);
            detectObjectModelName = configuration["DetectObjectModelName"];

            // Configuration for Face API
            faceAPIEndpoint =  configuration["FaceAPIEndpoint"];
            faceAPIKey = configuration["FaceAPIKey"];


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
