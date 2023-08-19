// Needed for SpeechConfig
using Microsoft.CognitiveServices.Speech;
// Needed for AudioConfig
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace azureaisolution
{
    // Responsible for handling Speech to Text Conversion
    static class SpeechServicePractice
    {
        // Speech to Text needs SpeechConfig and AudioConfig
        static SpeechConfig? speechConfig;
        static AudioConfig? audioConfig;
        static SpeechRecognizer? speechRecognizer;
        static SpeechRecognitionResult? speechRecognitionResult;
        static SpeechSynthesizer? speechSynthesizer;

        // Method Convert Speech to Text
        public static async Task ConvertWavFileSpeechToText(string cogSvcKey, string cogSvcRegion)
        {
            try
            {
                // SpeechConfig is Required to connect to your Speech Resource
                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);

                // Source File
                string audioFileInput = @"assets\speechtotext\time.wav";

                // Input Source 
                audioConfig = AudioConfig.FromWavFileInput(audioFileInput);

                // This is not required and only for Demo purpose
                //SoundPlayer wavPlayer = new(audioFileInput);
                //wavPlayer.Play();

                speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

                speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                if (speechRecognitionResult.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine("Text : " + speechRecognitionResult.Text);
                    Console.WriteLine("Duration : " + speechRecognitionResult.Duration);
                    Console.WriteLine("OffsetInTicks : " + speechRecognitionResult.OffsetInTicks);
                }
                else
                {
                    Console.WriteLine(speechRecognitionResult.Reason);
                    if (speechRecognitionResult.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                        Console.WriteLine(cancellation.Reason);
                        Console.WriteLine(cancellation.ErrorDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        // Method Convert Text to Speech
        public static async Task ConvertTextToSpeech(string cogSvcKey, string cogSvcRegion)
        {
            try
            {
                // SpeechConfig is Required to connect to your Speech Resource
                speechConfig = SpeechConfig.FromSubscription(cogSvcKey, cogSvcRegion);
                speechConfig.SpeechSynthesisLanguage = "en-IN";
                speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

                string outputFile = "speechsyn.wav";
                audioConfig = AudioConfig.FromWavFileOutput(outputFile);

                speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
                //await speechSynthesizer.SpeakTextAsync("I'm excited to try text to speech");
                await speechSynthesizer.SpeakTextAsync("You plan to manually build headers based on factors like bit depth, sample rate, and number of channels.");
                Console.WriteLine("Speech Synthesization Completed");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
