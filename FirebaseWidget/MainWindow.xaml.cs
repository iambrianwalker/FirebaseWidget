using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseWidget
{
    public partial class MainWindow : Window
    {

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string firebaseUrl = "https://brian-s-portfolio-1b813-default-rtdb.firebaseio.com/contactForm.json";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveToBottomRight();
            await LoadAndDisplayDatabaseAsync();

        }

        private async Task LoadAndDisplayDatabaseAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(firebaseUrl);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    string displayText = ParseEntries(json);
                    DisplayEntry(displayText);
                }
                else
                {
                    DisplayEntry("❌ Error fetching data.");
                }
            }
            catch (Exception ex)
            {
                DisplayEntry($"❌ Exception: {ex.Message}");
            }
        }

        private string ParseEntries(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return "No messages yet.";

            var root = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
            var result = new StringBuilder();
            bool isFirstMessage = true; // Track if it's the most recent message.

            foreach (var child in root.Reverse()) // newest first
            {
                var entry = child.Value;
                var name = entry.ContainsKey("name") ? entry["name"]?.ToString() : "Unknown";
                var message = entry.ContainsKey("message") ? entry["message"]?.ToString() : "";

                // Highlight the most recent message
                if (isFirstMessage)
                {
                    result.AppendLine($"🔴 {name} (New)"); // You can change 🔴 to any emoji or color if needed
                    result.AppendLine($"\"{message}\"");
                    result.AppendLine(new string('-', 30));
                    isFirstMessage = false; // Ensure only the first entry is highlighted
                }
                else
                {
                    result.AppendLine($"🟢 {name}");
                    result.AppendLine($"\"{message}\"");
                    result.AppendLine(new string('-', 30));
                }
            }

            return result.ToString().Trim();
        }



        private void DisplayEntry(string text)
        {
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));

            fadeOut.Completed += (s, e) =>
            {
                MainTextBlock.Text = text;
                MainTextBlock.BeginAnimation(OpacityProperty, fadeIn);
            };

            MainTextBlock.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void MoveToBottomRight()
        {
            var workArea = SystemParameters.WorkArea;
            this.Left = workArea.Right - this.Width - 10;
            this.Top = workArea.Bottom - this.Height - 10;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadAndDisplayDatabaseAsync();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Open the context menu when right-click is detected
            ContextMenu contextMenu = this.ContextMenu;
            contextMenu.IsOpen = true; // Show context menu
        }

    }
}



