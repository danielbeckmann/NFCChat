using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NfcChat.Resources;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace NfcChat
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void StartChat_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.TextBoxName.Text)) return;

            this.NavigationService.Navigate(new Uri(string.Format("/ChatPage.xaml?name={0}", this.TextBoxName.Text), UriKind.RelativeOrAbsolute));
        }
    }
}