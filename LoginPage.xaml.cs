﻿using ProfanityShock.Config;
using ProfanityShock.Backend;
using ProfanityShock.Services;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using ProfanityShock;
using ProfanityShock.Data;

namespace ProfanityShock
{
    public partial class LoginPage : ContentPage
    {

        public LoginPage()
        {
            InitializeComponent();

            // AccountRepository.DropTableAsync().Wait();

            if (AccountManager.GetConfig().Token == "")
            {
                Debug.Print("No token in memory, loading from file");
                loggedInLayout.IsVisible = false;
                loginLayout.IsVisible = true;
                AccountManager.LoadSave().Wait();
            }
            
            if (AccountManager.GetConfig().Token != "")
            {
                Debug.Print("Token found: " + AccountManager.GetConfig().Token);
                loginLayout.IsVisible = false;
                loggedInLayout.IsVisible = true;
                var accountinfo = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/users/self").GetAwaiter().GetResult();
                var json = accountinfo.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var dataObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (dataObject != null && dataObject.TryGetValue("data", out var dataObjectValue) &&
                    dataObjectValue is JsonElement dataElement && dataElement.ValueKind == JsonValueKind.Object &&
                    dataElement.TryGetProperty("name", out var nameProperty) && nameProperty.ValueKind == JsonValueKind.String)
                {
                    loggedInAsLabel.Text = nameProperty.GetString();
                }
            }
            else
            {
                Debug.Print("No token found");
            }
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var usernameOrEmail = emailEntry.Text;
            var password = passwordEntry.Text;
            var turnstileResponse = "1";

            if (backendEntry.Text != null)
            {
                AccountManager.GetConfig().Backend = new Uri(backendEntry.Text);
            }

            loginButton.Text = "Logging in...";
            SemanticScreenReader.Announce(loginButton.Text);

            var requestBody = new {password, usernameOrEmail, turnstileResponse };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "2/account/login", content).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                Debug.Print("We are logged in!");

                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

                AccountManager.GetConfig().Email = usernameOrEmail;
                AccountManager.GetConfig().Password = password;

                var tokenRequest = new {name="ProfanityShock autogenerated", permissions=new string[] {"shockers.use", "shockers.edit", "shockers.pause", "devices.edit", "devices.auth"}};
                var tokenContent = new StringContent(JsonSerializer.Serialize(tokenRequest), Encoding.UTF8, "application/json");
                var tokenResponse = NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "1/tokens", tokenContent).GetAwaiter().GetResult();
                if (tokenResponse.IsSuccessStatusCode)
                {
                    Debug.Print("Token created!");
                    var tokenJson = JsonSerializer.Deserialize<Dictionary<string, object>>(await tokenResponse.Content.ReadAsStringAsync());
                    if (tokenJson != null && tokenJson.TryGetValue("token", out var token))
                    {
                        Debug.Print("Token: " + token.ToString());
                        AccountManager.GetConfig().Token = (token.ToString());
                        await AccountManager.SaveConfig();
                    }
                }
            }
            else
            {
                Debug.Print("Login failed!");
                loginButton.Text = "Invalid credentials";
                SemanticScreenReader.Announce(loginButton.Text);
            }

            await Task.Delay(1000);

            loginButton.Text = "Login";
            SemanticScreenReader.Announce(loginButton.Text);

            // await Navigation.PushAsync(new LiveView());
        }

        private void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            var response = NetManager.GetClient().PostAsync(AccountManager.GetConfig().Backend + "1/account/logout", null).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                Debug.Print("Logout successful!");

                AccountRepository.DeleteItemAsync(AccountManager.GetConfig().Token).Wait();

                AccountManager.GetConfig().Token = "";
                AccountManager.GetConfig().Email = "";
                AccountManager.GetConfig().Password = "";
                AccountManager.GetConfig().Backend = new Uri("https://api.openshock.app");

                NetManager.ChangeToken("");

                loginLayout.IsVisible = true;
                loggedInLayout.IsVisible = false;
            }
            else
            {
                Debug.Print("Logout failed!");
            }
        }

        private void OnSessionButtonClicked(object sender, EventArgs e)
        {
            var response = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/tokens/self").GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Debug.Print(json);
            }
        }

        private async void OnTokenLoginButtonClicked(object sender, EventArgs e)
        {
            var OpenShockToken = tokenEntry.Text;
            if (backendEntry.Text != null)
            {
                AccountManager.GetConfig().Backend = new Uri(backendEntry.Text);
            }
            NetManager.ChangeToken(OpenShockToken);

            // Make the login request
            var response = NetManager.GetClient().GetAsync(AccountManager.GetConfig().Backend + "1/tokens/self").GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());

                if (jsonResponse != null && jsonResponse.TryGetValue("permissions", out var permissions) && 
                    permissions is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array &&
                    jsonElement.EnumerateArray().Any(item => item.GetString() == "shockers.use"))
                {
                    Debug.Print("Token valid!");

                    tokenLoginButton.Text = "Token Valid!";
                    SemanticScreenReader.Announce(loginButton.Text);

                    AccountManager.GetConfig().Token = OpenShockToken;
                    await AccountManager.SaveConfig();

                    loginLayout.IsVisible = false;
                    loggedInLayout.IsVisible = true;
                }
                else
                {
                    Debug.Print("Insufficient permissions!");

                    tokenLoginButton.Text = "Token missing shockers.use permission!";
                    SemanticScreenReader.Announce(loginButton.Text);

                    await Task.Delay(1000);

                    tokenLoginButton.Text = "Login with token";
                    SemanticScreenReader.Announce(loginButton.Text);
                }
            }
            else
            {
                Debug.Print(response.StatusCode.ToString());

                tokenLoginButton.Text = "Error: " + response.StatusCode.ToString();
                SemanticScreenReader.Announce(loginButton.Text);

                await Task.Delay(1000);

                tokenLoginButton.Text = "Login with token";
                SemanticScreenReader.Announce(loginButton.Text);
            }
        }
    }
}
