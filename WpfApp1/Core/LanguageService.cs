using System;
using System.Linq;
using System.Windows;

namespace FashionStore.Core
{
    public static class LanguageService
    {
        public static void SetLanguage(string languageCode)
        {
            try
            {
                var dict = new ResourceDictionary();
                switch (languageCode.ToLower())
                {
                    case "en":
                        dict.Source = new Uri("Resources/Languages/en.xaml", UriKind.Relative);
                        break;
                    case "vi":
                    default:
                        dict.Source = new Uri("Resources/Languages/vi.xaml", UriKind.Relative);
                        break;
                }

                // Remove existing language dictionaries
                var existingDicts = Application.Current.Resources.MergedDictionaries
                    .Where(d => d.Source != null && d.Source.OriginalString.Contains("Resources/Languages/"))
                    .ToList();

                foreach (var existing in existingDicts)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(existing);
                }

                // Find the root ResourceDictionary inside Application.Resources if it exists
                // In our new App.xaml structure, we will have a nested ResourceDictionary.
                var appResources = Application.Current.Resources;
                if (appResources.MergedDictionaries.Count > 0)
                {
                    appResources.MergedDictionaries.Add(dict);
                }
                else
                {
                    // If not using the nested structure yet, add to root
                    appResources.MergedDictionaries.Add(dict);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error switching language: {ex.Message}", "Language Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
