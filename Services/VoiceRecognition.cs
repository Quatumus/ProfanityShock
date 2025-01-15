using System.Diagnostics;
using ProfanityShock.Data;
using ProfanityShock;
using System.Text.Json;
using System.Text;
using System.Speech.Recognition;
using static ProfanityShock.Services.LiveViewInterface;


namespace ProfanityShock.Services

{
    internal class VoiceRecognition
    {
        // Declare an interface instance.
        private static ILiveViewInterface obj = new ImplementationClass();

        public static bool Active = false;
        public static async void Recognition(bool yeah, List<string> words)
        {
            await DoRecognition();

            async Task DoRecognition()
            {
                // Create a new SpeechRecognitionEngine instance.
                using SpeechRecognizer recognizer = new SpeechRecognizer();
                using ManualResetEvent exit = new ManualResetEvent(false);
                // Create a simple grammar that recognizes words from a list.
                Choices choices = new Choices();
                choices.Add(new string[] { "red", "green", "blue", "exit" });
                choices.Add(words.ToArray());
                // Create a GrammarBuilder object and append the Choices object.
                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = new System.Globalization.CultureInfo("en-GB");
                gb.Append(choices);
                // Create the Grammar instance and load it into the speech recognition engine.
                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);
                // Register a handler for the SpeechRecognized event.
                recognizer.SpeechRecognized += async (s, e) =>
                {
                    Debug.Print($"Recognized: {e.Result.Text}, Confidence: {e.Result.Confidence}");
                    if (words.Any(word => e.Result.Text.Contains(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        Debug.Print("Profanity detected");
                        obj.SetText(e.Result.Text);

                        var shockers = await SettingsRepository.ListAsync();
                        foreach (var shocker in shockers)
                        {
                            if (shocker.Intensity > 0)
                            {
                                if (shocker.Warning != ControlType.Stop)
                                {
                                    var shockersJsonwarning = new { shocks = new[] { new { id = shocker.ID, type = shocker.Warning.ToString(), intensity = shocker.Intensity, duration = shocker.Delay, exclusive = true } }, customName = "ProfanityShock API call" };
                                    var contentwarning = new StringContent(JsonSerializer.Serialize(shockersJsonwarning), Encoding.UTF8, "application/json");
                                    await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", contentwarning);
                                }
                            }
                        }
                        await Task.Delay(shockers[0].Delay + 100);
                        foreach (var shocker in shockers)
                        {
                            var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
                            var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
                            await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);
                        }

                        Debug.Print(e.Result.Text);

                        obj.SetText(e.Result.Text);

                        if (!Active)
                        exit.Set();
                    }
                };
            }
        }
    }
}

