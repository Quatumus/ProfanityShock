using ProfanityShock.Config;
using ProfanityShock.Services;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Animations;

namespace ProfanityShock;

public partial class ShockerPage : ContentPage
{
    // Create a list of Shocker objects
    List<Shocker> shockers = new List<Shocker>();

    internal int Delay = 0;
    internal int MinConfidence = 0;

    public ShockerPage()
    {
        InitializeComponent();

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        // Code to run every time the page is switched to
        await SyncShockers();
        shockersList.ItemsSource = shockers;

        if (shockers.Count > 0)
        {

            switch (shockers[0].Warning)
            {
                case ControlType.Stop:
                    warningModeLabel.Text = "Warning mode: None";
                    break;
                case ControlType.Vibrate:
                    warningModeLabel.Text = "Warning mode: Vibrate";
                    break;
                case ControlType.Sound:
                    warningModeLabel.Text = "Warning mode: Sound";
                    break;
            }

            Delay = shockers[0].Delay;
            delaySlider.Value = Delay;
            delayLabel.Text = $"Delay: {Delay}ms";

            var settings = await SettingsRepository.LoadAsync();
            if (settings != null)
            {
                confidenceLabel.Text = settings.MinConfidence.ToString() + "%";
                confidenceSlider.Value = settings.MinConfidence;
            }
        }
    }

    private async void OnDelaySliderValueChanged(object sender, EventArgs e)
    {
        var slider = (Slider)sender;
        Delay = (int)slider.Value;

        foreach (var shocker in shockers)
        {
            shocker.Delay = Delay;
            await ShockerRepository.SaveItemAsync(shocker);
        }
        
        delayLabel.Text = $"Delay: {Delay}ms";
    }
    
    private async void OnConfidenceSliderValueChanged(object sender, EventArgs e)
    {
        var slider = (Slider)sender;
        MinConfidence = (int)slider.Value;
        
        SettingsConfig settingsConfig = new SettingsConfig() { Language = SettingsRepository.LoadAsync().Result?.Language, MinConfidence = MinConfidence };

        await SettingsRepository.SaveItemAsync(settingsConfig);

        confidenceLabel.Text = $"Minimum Confidence: {MinConfidence}%";
    }

    private async Task SyncShockers() // Get all owned shockers from API then update the local database with new shockers
    {
        shockers = await ShockerRepository.ListAsync();
        
        var response = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/shockers/own").Result;

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

            if (jsonResponse != null && jsonResponse.TryGetValue("data", out var dataObject) &&
                dataObject is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Array)
            {
                var remoteShockers = dataElement.EnumerateArray()
                    .Select(hub => hub.GetProperty("shockers"))
                    .Where(shockersProperty => shockersProperty.ValueKind == JsonValueKind.Array)
                    .SelectMany(shockersProperty => shockersProperty.EnumerateArray())
                    .Where(shocker => shocker.TryGetProperty("id", out var idProperty) &&
                        idProperty.ValueKind == JsonValueKind.String)
                    .Select(shocker => shocker.GetProperty("id").GetString() ?? string.Empty)
                    .ToArray();

                shockers.RemoveAll(s => !remoteShockers.Contains(s.ID));

                foreach (var hubElement in dataElement.EnumerateArray())
                {
                    if (hubElement.TryGetProperty("shockers", out var shockersElement) &&
                        shockersElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var shockerElement in shockersElement.EnumerateArray())
                        {
                            if (shockerElement.TryGetProperty("name", out var nameProperty) &&
                                nameProperty.ValueKind == JsonValueKind.String &&
                                shockerElement.TryGetProperty("id", out var idProperty) &&
                                idProperty.ValueKind == JsonValueKind.String &&
                                shockerElement.TryGetProperty("isPaused", out var isPausedProperty) &&
                                (isPausedProperty.ValueKind == JsonValueKind.True || isPausedProperty.ValueKind == JsonValueKind.False))
                            {
                                string name = nameProperty.GetString() ?? "Error";
                                string id = idProperty.GetString() ?? "Error";
                                bool isPaused = isPausedProperty.GetBoolean();

                                Shocker shocker = new Shocker
                                {
                                    Name = name,
                                    ID = id,
                                    Paused = isPaused,
                                    Intensity = 20,
                                    Duration = 3000,
                                    Delay = 0,
                                    Warning = 0,
                                    Controltype = ControlType.Shock
                                };

                                Debug.Print(name);

                                if (!shockers.Any(s => s.ID == shocker.ID || shockers.Count() == 0)) // only add to list if new
                                {
                                    shockers.Add(shocker);
                                }
                                else
                                {
                                    Debug.Print("Shocker already exists");
                                }
                            }
                        }
                    }
                }

                Debug.Print("Shockers Fetched!");
            }
            else
            {
                Debug.Print(response.ToString());
            }
        }
        else
        {
            Debug.Print(response.StatusCode.ToString());
        }

        await ShockerRepository.SyncItemsAsync(shockers);
        shockers = await ShockerRepository.ListAsync();	
    }

    private async void OnShockButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;
        
        var shockersJson = new { shocks = new [] { new { id = shocker.ID, type = "Shock", intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock Manual API call" };
        var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
        var response = await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);

        if (!response.IsSuccessStatusCode)
        {
            Debug.Print($"Error: {response.StatusCode}");
        }
        else
        {
            Debug.Print("Request successful!");
        }

        Debug.Print($"Shock button pressed on Shocker with ID: {shocker.ID}");
    }

    private async void OnVibrateButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;

        var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = "Vibrate", intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock Manual API call" };
        var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
        var response = await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);
 
        if (!response.IsSuccessStatusCode)
        {
            Debug.Print($"Error: {response.StatusCode}");
        }
        else
        {
            Debug.Print("Request successful!");
        }

        Debug.Print($"Vibrate button pressed on Shocker with ID: {shocker.ID}");
    }

    private async void OnSoundButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;

        // Send a post request to api
        var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = "Sound", intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock Manual API call" };
        var content = new StringContent(JsonSerializer.Serialize(shockersJson), Encoding.UTF8, "application/json");
        var response = await NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/shockers/control", content);

        if (!response.IsSuccessStatusCode)
        {
            Debug.Print($"Error: {response.StatusCode}");
        }
        else
        {
            Debug.Print("Request successful!");
        }

        Debug.Print($"Sound button pressed on Shocker with ID: {shocker.ID}");
    }

    private async void SaveSettings(object sender, ValueChangedEventArgs e)
    {
        var shocker = (Shocker)((Slider)sender).BindingContext;
        
        await ShockerRepository.SaveItemAsync(shocker);

        var shocklist = await ShockerRepository.ListAsync();     
    }

    
    private async void OnWarningNoneButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        warningModeLabel.Text = "Warning mode: None";

        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Stop;
            await ShockerRepository.SaveItemAsync(shocker);
        }
    }

    private async void OnWarningVibrateButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        warningModeLabel.Text = "Warning mode: Vibrate";

        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Vibrate;
            await ShockerRepository.SaveItemAsync(shocker);
        }
    }

    private async void OnWarningSoundButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        warningModeLabel.Text = "Warning mode: Sound";

        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Sound;
            await ShockerRepository.SaveItemAsync(shocker);
        }
    }
}