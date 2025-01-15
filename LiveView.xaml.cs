using ProfanityShock.Services;
using ProfanityShock.Data;

namespace ProfanityShock;

public partial class LiveView : ContentPage
{
	public LiveView()
	{
		InitializeComponent();

    }

    private async void OnToggleRecognitionButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Start Listening" ? "Stop Listening" : "Start Listening";

        VoiceRecognition.Active = button.Text == "Stop Listening";

        if (button.Text == "Stop Listening")
        {
            var words = WordListManager.GetList();
            VoiceRecognition.Recognition(true, words);
        }
	}
}