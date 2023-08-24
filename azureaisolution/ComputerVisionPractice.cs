using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;

// Analyse/Classify Image
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

// Face Detection
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace azureaisolution
{
    internal static class ComputerVisionPractice
    {
        private static Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials? credentials;
        private static ComputerVisionClient? computerVisionClient;
        private static FaceClient? facePredictionClient;
        private static CustomVisionPredictionClient predictionClient;
        private static int thumbNailCount = 1;


        public static async Task AnalyseImage(string cogSvcKey, string endPoint)
        {
            // Authenticate Client
            credentials = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(cogSvcKey);
            computerVisionClient = new ComputerVisionClient(credentials)
            {
                Endpoint = endPoint
            };

            // Specifiy features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags,
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Adult
            };

            string folderPath = @"assets\computervision";
            string[] files = Directory.GetFiles(folderPath);
            foreach (string filePath in files)
            {
                Console.WriteLine($"\n\n \t FilePath - {filePath}");

                using (var imageData = File.OpenRead(filePath))
                {
                    var analysis = await computerVisionClient.AnalyzeImageInStreamAsync(imageData, features);

                    // get image captions
                    foreach (var caption in analysis.Description.Captions)
                    {
                        Console.WriteLine($"\n \t Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                        Console.WriteLine();
                    }

                    // Get Image Tags
                    Console.WriteLine("\n \t Image Tags");
                    foreach (var tag in analysis.Tags)
                    {
                        Console.WriteLine($"\t \t -{tag.Name} (confidence: {tag.Confidence.ToString("P")})");
                    }

                    // Get brands in the image
                    if (analysis.Brands.Count > 0)
                    {
                        Console.WriteLine("\n \t Brands:");
                        foreach (var brand in analysis.Brands)
                        {
                            Console.WriteLine($"\t \t -{brand.Name} (confidence: {brand.Confidence.ToString("P")})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n \t \t No Brands Detected");
                    }

                    // Get Categories
                    Console.WriteLine("\n \t Image Category");
                    List<LandmarksModel> landmarks = new List<LandmarksModel> { };
                    foreach (var category in analysis.Categories)
                    {
                        // Print the category
                        Console.WriteLine($"\t \t -{category.Name} (confidence: {category.Score.ToString("P")})");
                        // Get landmarks in this category
                        if (category.Detail?.Landmarks != null)
                        {
                            foreach (LandmarksModel landmark in category.Detail.Landmarks)
                            {
                                if (!landmarks.Any(item => item.Name == landmark.Name))
                                {
                                    landmarks.Add(landmark);
                                }
                            }
                        }
                    }

                    // If there were landmarks, list them
                    if (landmarks.Count > 0)
                    {
                        Console.WriteLine("\n \t Landmarks:");
                        foreach (LandmarksModel landmark in landmarks)
                        {
                            Console.WriteLine($"\t \t -{landmark.Name} (confidence: {landmark.Confidence.ToString("P")})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n \t No LandMark Detected");
                    }



                    // Get objects in the image
                    if (analysis.Objects.Count > 0)
                    {
                        Console.WriteLine("\n \t Objects in image:");
                        DrawImage(filePath, analysis);
                    }

                    // Get moderation ratings
                    string ratings = $"\n \t Ratings:\n \t \t  -Adult: {analysis.Adult.IsAdultContent}\n \t \t  -Racy: {analysis.Adult.IsRacyContent}\n \t \t  -Gore: {analysis.Adult.IsGoryContent}";
                    Console.WriteLine(ratings);

                    Console.WriteLine();
                }

                await GenerateThumbnail(filePath, computerVisionClient);
            }



        }

        // Method that will Draw Image
        public static void DrawImage(string imageFile, ImageAnalysis analysis)
        {
            // Prepare image for drawing
            Image image = Image.FromFile(imageFile);
            Graphics graphics = Graphics.FromImage(image);
            Pen pen = new Pen(Color.Cyan, 3);
            Font font = new Font("Arial", 16);
            SolidBrush brush = new SolidBrush(Color.Black);

            foreach (var detectedObject in analysis.Objects)
            {
                // Print object name
                Console.WriteLine($"\t \t  -{detectedObject.ObjectProperty} (confidence: {detectedObject.Confidence.ToString("P")})");

                // Draw object bounding box
                var r = detectedObject.Rectangle;
                Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
                graphics.DrawRectangle(pen, rect);
                graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);

            }
            // Save annotated image
            String output_file = "objects.jpg";
            image.Save(output_file);
            Console.WriteLine(" \n \t Results saved in " + output_file);
        }

        public static async Task GenerateThumbnail(string imageFile, ComputerVisionClient cvClient)
        {

            // Generate a thumbnail
            using (var imageData = File.OpenRead(imageFile))
            {
                // Get thumbnail data
                var thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(100, 100, imageData, true);

                // Save thumbnail image
                string thumbnailFileName = $"{thumbNailCount}thumbnail.png";
                using (Stream thumbnailFile = File.Create(thumbnailFileName))
                {
                    thumbnailStream.CopyTo(thumbnailFile);
                }

                Console.WriteLine($"\n \tThumbnail saved in {thumbnailFileName}");
                thumbNailCount++;
            }
        }

        public static async Task GetPredictionsForImageAsync(string predictionEndPoint,
            string predictionKey, Guid projectId,
            string modelName)
        {
            // Authenticate a client for the prediction API
            predictionClient = new CustomVisionPredictionClient(new
                Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction
                .ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = predictionEndPoint
            };

            // Classify test images
            String[] images = Directory.GetFiles(@"assets\computervision\");
            foreach (var image in images)
            {
                Console.Write(image + ": ");
                MemoryStream image_data = new MemoryStream(File.ReadAllBytes(image));
                var result = await predictionClient.ClassifyImageAsync(projectId, modelName, image_data);


                // Loop over each label prediction and print any with probability > 50%
                foreach (var prediction in result.Predictions)
                {
                    if (prediction.Probability > 0.5)
                    {
                        Console.WriteLine($"\n \t \t{prediction.TagName} ({prediction.Probability:P1})");
                    }
                }
            }
        }

        public static async Task DetectObjectsForImageAsync(string predictionEndPoint, string predictionKey,
            Guid projectId, string modelName)
        {
            // Authenticate a client for the prediction API
            predictionClient = new CustomVisionPredictionClient(new
                Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction
                .ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = predictionEndPoint
            };

            string imageFile = @"assets\detectobject\produce.jpg";
            Image image = Image.FromFile(imageFile);
            int h = image.Height;
            int w = image.Width;
            Graphics graphics = Graphics.FromImage(image);
            Pen pen = new Pen(Color.Magenta, 3);
            Font font = new Font("Arial", 16);
            SolidBrush brush = new SolidBrush(Color.Black);
            try
            {
                using (var imageData = File.OpenRead(imageFile))
                {
                    Console.WriteLine("\n \t Detecting object in : " + imageFile);
                    var result = await predictionClient.DetectImageAsync(projectId, modelName, imageData);

                    // Loop over each Prediction
                    foreach (var prediction in result.Predictions)
                    {
                        if (prediction.Probability > 0.5)
                        {
                            // The bounding box sizes are proportional - convert to absolute
                            int left = Convert.ToInt32(prediction.BoundingBox.Left * w);
                            int top = Convert.ToInt32(prediction.BoundingBox.Top * h);
                            int height = Convert.ToInt32(prediction.BoundingBox.Height * h);
                            int width = Convert.ToInt32(prediction.BoundingBox.Width * w);
                            // Draw the bounding box
                            Rectangle rect = new Rectangle(left, top, width, height);
                            graphics.DrawRectangle(pen, rect);
                            // Annotate with the predicted label
                            graphics.DrawString(prediction.TagName, font, brush, left, top);

                        }

                    }
                    // Save the annotated image
                    String output_file = "output.jpg";
                    image.Save(output_file);
                    Console.WriteLine("\n \t\tResults saved in " + output_file);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task DetectFacePredictionAsync(string predictionEndPoint, string predictionKey)
        {
            // Authenticate a client for the prediction API
            facePredictionClient = new FaceClient(new
                Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction
                .ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = predictionEndPoint
            };

            // Specify facial features to be retrieved
            List<FaceAttributeType?> features = new List<FaceAttributeType?>
            {
                FaceAttributeType.Occlusion,
                FaceAttributeType.Blur,
                FaceAttributeType.Glasses,
                //FaceAttributeType.Emotion,
                //FaceAttributeType.HeadPose,
                //FaceAttributeType.Age,
                //FaceAttributeType.Gender,
                //FaceAttributeType.Smile
            };


            string folderPath = @"assets\facedetection";
            string[] files = Directory.GetFiles(folderPath);
            foreach (var imageFile in files)
            {
                try
                {
                    // Get faces
                    using (var imageData = File.OpenRead(imageFile))
                    {
                        var detected_faces = await facePredictionClient.Face.DetectWithStreamAsync(imageData, returnFaceAttributes: features, returnFaceId: false);

                        if (detected_faces.Count > 0)
                        {
                            Console.WriteLine($"{detected_faces.Count} faces detected.");

                            // Prepare image for drawing
                            Image image = Image.FromFile(imageFile);
                            Graphics graphics = Graphics.FromImage(image);
                            Pen pen = new Pen(Color.LightGreen, 3);
                            Font font = new Font("Arial", 4);
                            SolidBrush brush = new SolidBrush(Color.Black);
                            int faceCount = 0;

                            // Draw and annotate each face
                            foreach (var face in detected_faces)
                            {
                                faceCount++;
                                Console.WriteLine($"\n \tFace number {faceCount}");

                                // Get face properties
                                Console.WriteLine($"\n \t\t - Mouth Occluded: {face.FaceAttributes.Occlusion.MouthOccluded}");
                                Console.WriteLine($"\n \t\t - Eye Occluded: {face.FaceAttributes.Occlusion.EyeOccluded}");
                                Console.WriteLine($"\n \t\t - Blur: {face.FaceAttributes.Blur.BlurLevel}");
                                Console.WriteLine($"\n \t\t - Glasses: {face.FaceAttributes.Glasses}");

                                // Draw and annotate face
                                var r = face.FaceRectangle;
                                Rectangle rect = new Rectangle(r.Left, r.Top, r.Width, r.Height);
                                graphics.DrawRectangle(pen, rect);
                                string annotation = $"Face ID: {face.FaceId}";
                                graphics.DrawString(annotation, font, brush, r.Left, r.Top);
                            }

                            // Save annotated image
                            String output_file = $"{faceCount}detected_faces.jpg";
                            image.Save(output_file);
                            Console.WriteLine("\n \t\tResults saved in " + output_file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }


        public static async Task ReadTextOCR(string cogSvcKey, string endPoint)
        {
            // Authenticate Client
            credentials = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(cogSvcKey);
            computerVisionClient = new ComputerVisionClient(credentials)
            {
                Endpoint = endPoint
            };
 
            try
            {
                string folderPath = @"assets\ocr";
                string[] files = Directory.GetFiles(folderPath);
                foreach (string filePath in files)
                {
                    Console.WriteLine($"\n\n \t FilePath - {filePath}");

                    using (var imageData = File.OpenRead(filePath))
                    {
                        var textHeaders = await computerVisionClient.ReadInStreamAsync(imageData, null, null, "2022-04-30");


                        // Get the Operation ID
                        // After the request, get the operation location (operation ID)
                        string operationLocation = textHeaders.OperationLocation;
                        Thread.Sleep(2000);

                        // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
                        // We only need the ID and not the full URL
                        const int numberOfCharsInOperationId = 36;
                        string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);
                        // Extract the text
                        ReadOperationResult results;

                        Console.WriteLine();
                        do
                        {
                            results = await computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                        }
                        while ((results.Status == OperationStatusCodes.Running ||
                            results.Status == OperationStatusCodes.NotStarted));

                        // Display the found text.
                        Console.WriteLine();
                        var textUrlFileResults = results.AnalyzeResult.ReadResults;
                        foreach (ReadResult page in textUrlFileResults)
                        {
                            foreach (Line line in page.Lines)
                            {
                                Console.WriteLine(line.Text);
                            }
                        }
                        Console.WriteLine();


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
