using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azureaisolution
{
    internal class FormRecognizerPractice
    {
        public static async Task TrainModelFormRecognize(string formEndpoint, string formKey, string trainingStorageUri)
        {
            try
            {
                // Authenticate Form Training Client 
                var credential = new AzureKeyCredential(formKey);
                var trainingClient = new FormTrainingClient(new Uri(formEndpoint), credential);

                // Train model 
                CustomFormModel model = await trainingClient
                .StartTrainingAsync(new Uri(trainingStorageUri), useTrainingLabels: true)
                .WaitForCompletionAsync();
                // Get model info
                Console.WriteLine($"Custom Model Info:");
                Console.WriteLine($"    Model Id: {model.ModelId}");
                Console.WriteLine($"    Model Status: {model.Status}");
                Console.WriteLine($"    Training model started on: {model.TrainingStartedOn}");
                Console.WriteLine($"    Training model completed on: {model.TrainingCompletedOn}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public static async Task TestModelFormRecognize(string formEndpoint, string formKey, string modelId)
        {
            try
            {
                // Authenticate Form Training Client 
                var credential = new AzureKeyCredential(formKey);
                var recognizerClient = new FormRecognizerClient(new Uri(formEndpoint), credential);
                // Get form url for testing   
                string image_file = "test1.jpg";
                using (var image_data = File.OpenRead(image_file))
                {
                    // Use trained model with new form 
                    RecognizedFormCollection forms = await recognizerClient
                    .StartRecognizeCustomForms(modelId, image_data)
                    .WaitForCompletionAsync();

                    foreach (RecognizedForm form in forms)
                    {
                        Console.WriteLine($"Form of type: {form.FormType}");
                        foreach (FormField field in form.Fields.Values)
                        {
                            Console.WriteLine($"Field '{field.Name}: ");

                            if (field.LabelData != null)
                            {
                                Console.WriteLine($"    Label: '{field.LabelData.Text}");
                            }

                            Console.WriteLine($"    Value: '{field.ValueData.Text}");
                            Console.WriteLine($"    Confidence: '{field.Confidence}");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
