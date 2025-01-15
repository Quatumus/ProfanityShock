using ProfanityShock.Services;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace ProfanityShock;

public partial class LiveView : ContentPage
{
	public LiveView()
	{
		InitializeComponent();

    }

    public void UpdateTextBox()
    {
        textBox.Text = null;
        textBox.Text = VoiceRecognition.Text;
    }

    private void OnRecognitionModeButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Recognition mode: Azure + custom" ? "Recognition mode: custom" : "Recognition mode: Azure + custom";
    }
    private void OnToggleRecognitionButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Start Listening" ? "Stop Listening" : "Start Listening";

        VoiceRecognition.Active = button.Text == "Stop Listening";

        if (button.Text == "Stop Listening")
        {
            var words = WordListManager.GetList();
            VoiceRecognition.Recognition(recognitionModeButton.Text == "Recognition mode: Azure + custom", words);
        }
	}
}