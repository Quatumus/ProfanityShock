using System.Diagnostics;
using ProfanityShock.Data;
using System.Text.Json;
using System.Text;
using System.Speech.Recognition;
using System.Globalization;
using static ProfanityShock.Services.LiveViewInterface;


namespace ProfanityShock.Services

{
    internal class VoiceRecognition
    {
        // Declare an interface instance.
        private static ILiveViewInterface obj = new ImplementationClass();

        private static string language = SettingsRepository.LoadAsync().Result?.Language ?? "en-US";

        private static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(
            new System.Globalization.CultureInfo(language));

        public static void Recognition(bool Activate, List<string> words)
        {
            if (Activate)
            {

                Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);

                // Create a simple grammar that recognizes words from a list.
                Choices choices = new Choices();
                choices.Add(words.ToArray());
                // Create a GrammarBuilder object and append the Choices object.
                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(choices);
                // Create the Grammar instance and load it into the speech recognition engine.
                Grammar g = new Grammar(gb);
                recognizer.LoadGrammar(g);

                gb.Culture = Thread.CurrentThread.CurrentCulture;

                // Add a handler for the speech recognized event.  
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                // Configure input to the speech recognizer.  
                recognizer.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.  
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                recognizer.RecognizeAsyncStop();
            }
        }
        // Handle the SpeechRecognized event.  
        static async void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Debug.Print("Recognized text: " + e.Result.Text);

            obj.SetText(e.Result.Text, (int)(e.Result.Confidence * 100));

            if (WordListManager.GetList().Any(word => e.Result.Text.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                Debug.Print("Profanity detected");

                if ((int)(e.Result.Confidence * 100) >= SettingsRepository.LoadAsync().Result?.MinConfidence)
                {
                    var shockers = await ShockerRepository.ListAsync();
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
                    if (shockers[0].Delay > 0)
                    {
                        await Task.Delay(shockers[0].Delay + 300);
                    }
                    foreach (var shocker in shockers)
                    {
                        var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
                        var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
                        await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);
                    }
                }             
            }
        }
    }
}

