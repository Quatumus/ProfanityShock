using ProfanityShock.Data;
using System.Diagnostics;
using ProfanityShock.Services;

namespace ProfanityShock;

public partial class CustomListPage : ContentPage
{
    public CustomListPage()
	{
		InitializeComponent();

		itemsList.ItemsSource = WordListManager.GetList();
	}
	
	
	void OnAddButtonClicked(object sender, EventArgs e)
	{
		if (textInput.Text != null)
		{
			var newItems = textInput.Text.Split(' ')
				.Select(word => word.Trim().ToLower())
				.Where(word => !string.IsNullOrWhiteSpace(word) && !WordListManager.words.Select(w => w.ToLower()).Contains(word))
				.ToList();
            WordListManager.words.AddRange(newItems);
			textInput.Text = string.Empty;
			itemsList.ItemsSource = null; // needs to be set to null first to update properly on the page 

            // clean up duplicates
            WordListManager.words = WordListManager.words.Distinct().ToList();
			itemsList.ItemsSource = WordListManager.words;

			foreach (var word in newItems)
            {
                WordListRepository.SaveItemAsync(word).Wait();
            }
		}
	}

	void OnRemoveButtonClicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
        WordListManager.words.RemoveAll(item => item == button.BindingContext.ToString());

        WordListRepository.DeleteItemAsync(button.BindingContext.ToString()).Wait();
		itemsList.ItemsSource = null;
		itemsList.ItemsSource = WordListManager.words;
	}
}