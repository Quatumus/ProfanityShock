using Microsoft.CognitiveServices.Speech;
using System.Diagnostics;
using ProfanityShock.Data;
using ProfanityShock;
using System.Text.Json;
using System.Text;


namespace ProfanityShock.Services

{
    internal class VoiceRecognition
    {

        public static bool Active;
        public static string? Text; //spoken text
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
                            Text = result.Text.Trim().ToLower();

                            var containsProfanity = words.Any(word => Text.Contains(word, StringComparison.OrdinalIgnoreCase));
                            if (containsProfanity || Text.Contains('*'))
                            {
                                Debug.Print("Profanity detected");

                                var shockers = await SettingsRepository.ListAsync();
                                foreach (var shocker in shockers)
                                {
                                    if (shocker.Intensity > 0)
                                    {
                                        if (shocker.Warning != ControlType.Stop)
                                        {
                                            var shockersJsonwarning = new { shocks = new [] { new { id = shocker.ID, type = shocker.Warning.ToString(), intensity = shocker.Intensity, duration = shocker.Delay, exclusive = true } }, customName = "ProfanityShock API call" };
                                            var contentwarning = new StringContent(JsonSerializer.Serialize(shockersJsonwarning), Encoding.UTF8, "application/json");
                                            await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", contentwarning);
                                        }
                                        await Task.Delay(shocker.Delay);
                                        var shockersJson = new { shocks = new [] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
                                        var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
                                        await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);
                                    }
                                }
                            }

                            Debug.Print(Text);

                            var liveView = App.Current.Windows[0].Page.Navigation.NavigationStack.FirstOrDefault(x => x is LiveView) as LiveView;
                            if (liveView != null)
                            {
                                liveView.UpdateTextBox();
                            }
                            
                            speechRecognized = true;
                        }
                    }
                }
            }
        }
    }
}

