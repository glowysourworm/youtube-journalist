using System;
using System.Windows;
using System.Windows.Controls;

using WpfCustomUtilities.UI.View;

namespace YoutubeJournalist.View
{
    public partial class BasicSearchView : UserControl
    {
        public event EventHandler<PlatformType> PlatformTypeChanged;
        public event EventHandler<string> SearchEvent;

        public BasicSearchView()
        {
            InitializeComponent();
        }

        private void PlatformCB_EnumValueChanged(object sender, RoutedEventArgs e)
        {
            if (this.PlatformTypeChanged != null)
                this.PlatformTypeChanged(this, (PlatformType)((sender as EnumComboBox).EnumValue));
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchEvent != null)
                this.SearchEvent(this, this.SearchTB.Text);
        }
    }
}
