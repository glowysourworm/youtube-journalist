﻿using System;
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
        protected override void OnClosed(EventArgs e)
        {
            _controller.Dispose();

            base.OnClosed(e);
        }

        private void SearchChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.OutputLB.ItemsSource = _controller.GetChannels(this.SearchTB.Text ?? "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // Shuts down -> OnClosed()
                App.Current.Shutdown();
            }
        }

        private void SearchVideosButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.OutputLB.ItemsSource = _controller.GetVideos(this.SearchTB.Text ?? "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // Shuts down -> OnClosed()
                App.Current.Shutdown();
            }
        }

        private void SearchPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.OutputLB.ItemsSource = _controller.GetPlaylists(this.SearchTB.Text ?? "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // Shuts down -> OnClosed()
                App.Current.Shutdown();
            }
        }

        private void FilterChannelsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.SearchTB.Text))
                    this.OutputLB.ItemsSource = _controller.GetChannelsByCategory(this.SearchTB.Text ?? "");

                else
                    MessageBox.Show("Must enter CategoryId for the search / filter");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // Shuts down -> OnClosed()
                App.Current.Shutdown();
            }
        }

        private void FilterVideosButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.SearchTB.Text))
                    this.OutputLB.ItemsSource = _controller.GetVideosByCategory(this.SearchTB.Text ?? "");

                else
                    MessageBox.Show("Must enter CategoryId for the search / filter");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // Shuts down -> OnClosed()
                App.Current.Shutdown();
            }
        }
    }
}
