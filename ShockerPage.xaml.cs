using ProfanityShock.Config;
using ProfanityShock.Services;
using ProfanityShock.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Windows.Input;

namespace ProfanityShock;

public partial class ShockerPage : ContentPage
{
    // Create an instance of SettingsRepository
    SettingsRepository repository = new SettingsRepository();

    // Create a list of Shocker objects
    List<Shocker> shockers = new List<Shocker>();

    public ShockerPage()
    {
        InitializeComponent(); // pretty much all errors here are just gaslight by intellisense

        //IntensitySlider.ValueChanged += IntensitySliderChanged;
        //DurationSlider.ValueChanged += DurationSliderChanged;


        if (shockers.Count == 0)
        { Debug.Print("List Empty"); }

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Code to run every time the page is switched to
        SyncShockers().Wait();
        shockersList.ItemsSource = shockers;
        
    }

    private async Task SyncShockers()
    {
        
        // Get all owned shockers from API
        var response = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/shockers/own").Result;

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

            shockers.Clear();

            // Debug.Print(await response.Content.ReadAsStringAsync());

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
                                Debug.Print(shocker.Name);
                                shockers.Add(shocker);
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
        // Call SyncItemsAsync on the instance
        repository.SyncItemsAsync(shockers).Wait();
        shockers = await repository.ListAsync();	
    }

    private async void OnShockButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;

        shocker.Controltype = ControlType.Shock;
        repository.SaveItemAsync(shocker).Wait();
        
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

        Debug.Print($"Button pressed on Shocker with ID: {shocker.ID}");
    }

    private async void OnVibrateButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (button.BindingContext is not Shocker shocker)
            return;

        shocker.Controltype = ControlType.Vibrate;
        repository.SaveItemAsync(shocker).Wait();

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
        repository.SaveItemAsync(shocker).Wait();

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

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var slider = (Slider)sender;
        var shocker = (Shocker)slider.BindingContext;
    
        if (shocker != null)
        {
            shocker.Intensity = (int)e.NewValue;
            // Assuming there is a label associated with the slider to display its value
            var label = (Label)slider.FindByName("AmountLabel");
        }
    }
    private void IntensitySlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        var slider = (Slider)sender;
        var shocker = (Shocker)slider.BindingContext;
        shocker.Intensity = (int)e.NewValue;
    }

    /*
    private void IntensitySliderChanged(object sender, EventArgs e)
    {
        var slider = (Slider)sender;
        var intensityText = (Label)FindByName("IntensityText");
        IntensityText.Text = slider.Value.ToString();
    }

    private void DurationSliderChanged(object sender, EventArgs e)
    {
        var slider = (Slider)sender;
        var durationText = (Label)FindByName("DurationText");
        durationText.Text = slider.Value.ToString();
    }
    */

    private async void SaveSettings(object sender, ValueChangedEventArgs e)
    {
        var shocker = (Shocker)((Slider)sender).BindingContext;
        
        await repository.SaveItemAsync(shocker);

        var shocklist = await repository.ListAsync();

        foreach (var item in shocklist)
        {
            Debug.Print(item.Intensity.ToString());
        }

        //Debug.Print(shocker.Intensity.ToString());
        
    }
}