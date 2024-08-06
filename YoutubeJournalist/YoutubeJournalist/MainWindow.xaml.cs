using System;
using System.Collections.Generic;
using System.Windows;

using YoutubeJournalist.Core.Data;
using YoutubeJournalist.Core.WebAPI.Base;
using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;

namespace YoutubeJournalist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IWebAPIService _service;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = new Configuration();
            _service = new Service(configuration.ApiKey, configuration.MaxSearchResults);

            this.DataContext = configuration;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.SearchTB.Text))
                this.OutputLB.ItemsSource = await _service.Search(this.SearchTB.Text);

            else
                this.OutputLB.ItemsSource = new List<SearchResult>() { new SearchResult()
                {
                    Name = "Error",
                    Description = "Please Enter Search Text"
                }};
        }
    }
}
