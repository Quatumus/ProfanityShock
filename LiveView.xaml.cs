using ProfanityShock.Services;

namespace ProfanityShock;

public partial class LiveView : ContentPage
{
	public LiveView()
	{
		InitializeComponent();
	}

    bool isListening = false;

    private void OnToggleRecognitionButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Start Recognition" ? "Stop Recognition" : "Start Recognition";

        if (button.Text == "Start Recognition")
        {
            //VoiceRecognition.Recognition();
        }

	}
}