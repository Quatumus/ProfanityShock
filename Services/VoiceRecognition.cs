using Microsoft.CognitiveServices.Speech;
using System.Diagnostics;



namespace ProfanityShock.Services

{
    internal class VoiceRecognition
    {

        public static bool Active;
        public static async void Recognition(bool UseAzureList, List<string> words)
        {
            await DoRecognition();

            async Task DoRecognition()
            {   // if you plan to fork this project, please make your own subscription on azure (it's free)
                var config = SpeechConfig.FromSubscription("8spUbABMG6NQUfeAwIHhNhOUpt7XZAuYk0GRq1ep3Nxl2A2zME4hJQQJ99BAACi5YpzXJ3w3AAAYACOGJbZt", "northeurope"); 
                if (!UseAzureList)
                {
                    config.SetProfanity(ProfanityOption.Raw);
                }
                using var recognizer = new SpeechRecognizer(config);

                while (Active)
                {
                    bool speechRecognized = false;

                    while (!speechRecognized)
                    {
                        var result = await recognizer.RecognizeOnceAsync();

                        if (result.Reason == ResultReason.RecognizedSpeech)
                        {
                            string userChoice = result.Text.Trim().ToLower();

                            var containsProfanity = words.Any(word => userChoice.Contains(word, StringComparison.OrdinalIgnoreCase));
                            if (containsProfanity || userChoice.Contains('*'))
                            {
                                Debug.Print("Profanity detected or contains '*'");

                            }

                            Debug.Print(userChoice);
                            
                            speechRecognized = true;
                        }
                    }
                }
            }
        }
    }
}

