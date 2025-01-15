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

    public ShockerPage()
    {
        InitializeComponent();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Code to run every time the page is switched to
        SyncShockers().Wait();
        shockersList.ItemsSource = shockers;

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
        
    }

    private async void OnDelaySliderValueChanged(object sender, EventArgs e)
    {
        var slider = (Slider)sender;
        Delay = (int)slider.Value;

        foreach (var shocker in shockers)
        {
            shocker.Delay = Delay;
            await SettingsRepository.SaveItemAsync(shocker);
        }
        
        delayLabel.Text = $"Delay: {Delay}ms";
    }

    private async Task SyncShockers()
    {
        shockers = await SettingsRepository.ListAsync();
        // Get all owned shockers from API
        var response = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/shockers/own").Result;

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

            if (jsonResponse != null && jsonResponse.TryGetValue("data", out var dataObject) &&
                dataObject is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Array)
            {
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

                                if (shockers.Any(s => s.ID != id)) // only add to list if new
                                {
                                    shockers.Add(shocker);
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
        SettingsRepository.SyncItemsAsync(shockers).Wait();
        shockers = await SettingsRepository.ListAsync();	
    }

    private async void OnShockButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;

        shocker.Controltype = ControlType.Shock;
        SettingsRepository.SaveItemAsync(shocker).Wait();
        
        var shockersJson = new { shocks = new [] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
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

        shocker.Controltype = ControlType.Vibrate;
        SettingsRepository.SaveItemAsync(shocker).Wait();

        var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
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

        shocker.Controltype = ControlType.Sound;
        SettingsRepository.SaveItemAsync(shocker).Wait();

        // Send a post request to api
        var shockersJson = new { shocks = new[] { new { id = shocker.ID, type = shocker.Controltype.ToString(), intensity = shocker.Intensity, duration = shocker.Duration, exclusive = true } }, customName = "ProfanityShock API call" };
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
        
        await SettingsRepository.SaveItemAsync(shocker);
        
        var shocklist = await SettingsRepository.ListAsync();     
    }

    
    private async void OnWarningNoneButtonClicked(object sender, EventArgs e)
    {
        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Stop;
            await SettingsRepository.SaveItemAsync(shocker);
        }
        
        warningModeLabel.Text = "Warning mode: None";
    }

    private async void OnWarningVibrateButtonClicked(object sender, EventArgs e)
    {
        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Vibrate;
            await SettingsRepository.SaveItemAsync(shocker);
        }

        warningModeLabel.Text = "Warning mode: Vibrate";
    }

    private async void OnWarningSoundButtonClicked(object sender, EventArgs e)
    {
        foreach (var shocker in shockers)
        {
            shocker.Warning = ControlType.Sound;
            await SettingsRepository.SaveItemAsync(shocker);
        }

        warningModeLabel.Text = "Warning mode: Sound";
    }
}