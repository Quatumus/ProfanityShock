using ProfanityShock.Services;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static ProfanityShock.Services.LiveViewInterface;

namespace ProfanityShock;


class ImplementationClass : ILiveViewInterface
{
    // Explicit interface member implementation:
    void ILiveViewInterface.SetText()
    {
        //get current content page from appshell
        var appShell = (AppShell)Application.Current.MainPage;
        var liveView = (LiveView)appShell.CurrentPage;
        liveView.UpdateTextBox();
    }
}
public partial class LiveView : ContentPage
{
    public LiveView()
	{
		InitializeComponent();
    }

    public void UpdateTextBox()
    {
        textBox.Text = VoiceRecognition.Text;
        Debug.Print("Updated text box!");
    }

    private void OnRecognitionModeButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Recognition mode: Azure + custom" ? "Recognition mode: custom" : "Recognition mode: Azure + custom";
    }
    private void OnToggleRecognitionButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Start Recognition" ? "Stop Recognition" : "Start Recognition";

        VoiceRecognition.Active = button.Text == "Stop Recognition";

        if (button.Text == "Stop Recognition")
        {
            var words = WordListManager.GetList();
            VoiceRecognition.Recognition(recognitionModeButton.Text == "Recognition mode: Azure + custom", words);
        }
	}
}