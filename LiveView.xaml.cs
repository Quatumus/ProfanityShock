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
    void ILiveViewInterface.SetText(string sentence)
    {
        //get current content page from appshell
        var appShell = (AppShell)Application.Current.MainPage;
        var liveView = (LiveView)appShell.CurrentPage;
        liveView.UpdateTextBox(sentence);
    }
}
public partial class LiveView : ContentPage
{
    public LiveView()
	{
		InitializeComponent();
        AccountManager.LoadSave().Wait();
    }

    public void UpdateTextBox(string text)
    {
        textBox.Text = text;
        Debug.Print("Updated text box!");
    }

    private void OnToggleRecognitionButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.Text = button.Text == "Start Recognition" ? "Stop Recognition" : "Start Recognition";

        var words = WordListManager.GetList();
        VoiceRecognition.Recognition(button.Text == "Stop Recognition", words);
	}
}