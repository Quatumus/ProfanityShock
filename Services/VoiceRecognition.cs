using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Globalization;
using System.Diagnostics;


namespace ProfanityShock.Services

{
    internal class VoiceRecognition
    {

        public static void Recognition() // edited example taken from https://www.nuget.org/packages/System.Speech/#readme-body-tab
        {

            // Create a new SpeechRecognitionEngine instance.
            using SpeechRecognizer recognizer = new SpeechRecognizer();
            using ManualResetEvent exit = new ManualResetEvent(false);

            // Create a simple grammar that recognizes words from a list.
            Choices choices = new Choices();
            choices.Add(new string[] { "red", "green", "blue", "exit" });

            // Create a GrammarBuilder object and append the Choices object.
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(choices);

            // Create the Grammar instance and load it into the speech recognition engine.
            Grammar g = new Grammar(gb);
            recognizer.LoadGrammar(g);

            // Register a handler for the SpeechRecognized event.
            recognizer.SpeechRecognized += (s, e) =>
            {
                Debug.Print($"Recognized: {e.Result.Text}, Confidence: {e.Result.Confidence}");
                if (e.Result.Text == "exit")
                {
                    exit.Set();
                }
            };

            // Emulate
            Debug.Print("Emulating \"red\".");
            recognizer.EmulateRecognize("red");

            Debug.Print("Speak red, green, blue, or exit please...");

        }
    }
}
