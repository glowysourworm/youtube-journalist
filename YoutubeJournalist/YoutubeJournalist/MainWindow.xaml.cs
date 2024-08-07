using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;

using YoutubeJournalist.Core;
using YoutubeJournalist.Core.WebAPI.Google.Apis.Youtube.V3;

namespace YoutubeJournalist
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller _controller;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = new Configuration();

            _controller = new Controller(configuration);

            this.DataContext = configuration;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(this.SearchTB.Text))
                    this.OutputLB.ItemsSource = _controller.GetChannels().Channels;

                else
                    this.OutputLB.ItemsSource = new List<Youtube_Channel>() { new Youtube_Channel()
                {
                    Id = "No Channel",
                    Kind = "No Kind",
                    ETag = "No ETag",
                    ContentOwnerDetails_ETag = "No Owner ETag",
                    ContentOwnerDetails_ContentOwner = "No Content Owner"
                }};
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
