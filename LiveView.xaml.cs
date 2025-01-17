using ProfanityShock.Services;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using static ProfanityShock.Services.LiveViewInterface;
using ProfanityShock.Config;

namespace ProfanityShock;


class ImplementationClass : ILiveViewInterface
{
    // Explicit interface member implementation:
    void ILiveViewInterface.SetText(string sentence, int confidence)
    {
        //get current content page from appshell
        var appShell = (AppShell)Application.Current.MainPage;
        var liveView = (LiveView)appShell.CurrentPage;
        liveView.UpdateTextBox(sentence, confidence);
    }
}
public partial class LiveView : ContentPage
{
    public LiveView()
    {
        InitializeComponent();
        AccountManager.LoadSave().Wait();

        var settings = SettingsRepository.LoadAsync().Result;
        if (settings != null)
        {
            switch (settings.Language)
            {
                case "en-US":
                    languagePicker.SelectedItem = "English (US)";
                    break;
                case "en-UK":
                    languagePicker.SelectedItem = "English (UK)";
                    break;
                case "en-CA":
                    languagePicker.SelectedItem = "English (Canada)";
                    break;
                case "en-IN":
                    languagePicker.SelectedItem = "English (India)";
                    break;
                case "en-AU":
                    languagePicker.SelectedItem = "English (Australia)";
                    break;
                case "de":
                    languagePicker.SelectedItem = "German";
                    break;
                case "fr-FR":
                    languagePicker.SelectedItem = "French (French)";
                    break;
                case "fr-CA":
                    languagePicker.SelectedItem = "French (Canada)";
                    break;
                case "es-ES":
                    languagePicker.SelectedItem = "Spanish (Spain)";
                    break;
                case "es-MX":
                    languagePicker.SelectedItem = "Spanish (Mexico)";
                    break;
                case "zh-CN":
                    languagePicker.SelectedItem = "Chinese (Simplified, china)";
                    break;
                case "it":
                    languagePicker.SelectedItem = "Italian";
                    break;
                case "pt-BR":
                    languagePicker.SelectedItem = "Portuguese (Brazil)";
                    break;
                case "da":
                    languagePicker.SelectedItem = "Danish";
                    break;
                case "ja":
                    languagePicker.SelectedItem = "Japanese";
                    break;
            }
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        var selectedLanguage = picker.SelectedItem.ToString();
        var settings = SettingsRepository.LoadAsync().Result;

        if (settings != null)
        {
            switch (selectedLanguage)
            {
                case "English (US)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "en-US", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "English (UK)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "en-UK", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "English (Canada)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "en-CA", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "English (India)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "en-IN", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "English (Australia)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "en-AU", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "German":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "de", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "French (French)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "fr-FR", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "French (Canada)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "fr-CA", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Spanish (Spain)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "es-ES", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Spanish (Mexico)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "es-MX", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Chinese (Simplified, china)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "zh-CN", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Italian":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "it", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Portuguese (Brazil)":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "pt-BR", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Danish":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "da", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                case "Japanese":
                    SettingsRepository.SaveItemAsync(new SettingsConfig { Language = "ja", MinConfidence = settings.MinConfidence }).Wait();
                    break;
                default:
                    Debug.Print("Language not supported");
                    break;
            }

            Debug.Print($"Language changed to: {selectedLanguage}");
        }
    }

    public void UpdateTextBox(string text, int confidence)
    {
        textBox.Text = text;
        confidenceBox.Text = confidence.ToString() + "%";
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
