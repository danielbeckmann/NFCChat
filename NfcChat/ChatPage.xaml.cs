using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NfcChat.DataModel;
using Windows.Networking;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace NfcChat
{
    public partial class ChatPage : PhoneApplicationPage
    {
        private ChatViewModel viewModel;

        private DataWriter dataWriter;
        private DataReader dataReader;

        public ChatPage()
        {
            InitializeComponent();

            // Sets the view model for the page
            this.viewModel = new ChatViewModel((a) => Dispatcher.BeginInvoke(a));
            this.DataContext = this.viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.NavigationContext.QueryString.ContainsKey("name"))
            {
                this.viewModel.Name = this.NavigationContext.QueryString["name"];
            }

            this.StartSearching();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.StopSearching();
            this.CloseConnection();
        }

        /// <summary>
        /// Starts the search for other peers.
        /// </summary>
        private void StartSearching()
        {
            var device = ProximityDevice.GetDefault();
            if (device != null)
            {
                PeerFinder.DisplayName = this.viewModel.Name;
                PeerFinder.AllowBluetooth = true;
                PeerFinder.AllowInfrastructure = true;

                PeerFinder.TriggeredConnectionStateChanged += PeerFinder_TriggeredConnectionStateChanged;
                PeerFinder.Start();

                this.viewModel.Status = "Suche nach Geräten...";
            }
        }

        /// <summary>
        /// Stops the peer search.
        /// </summary>
        private void StopSearching()
        {
            PeerFinder.TriggeredConnectionStateChanged -= PeerFinder_TriggeredConnectionStateChanged;
            PeerFinder.Stop();
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        private void CloseConnection()
        {
            if (this.dataReader != null)
            {
                this.dataReader.Dispose();
                this.dataReader = null;
            }

            if (this.dataWriter != null)
            {
                this.dataWriter.Dispose();
                this.dataWriter = null;
            }
        }

        private void PeerFinder_TriggeredConnectionStateChanged(object sender, TriggeredConnectionStateChangedEventArgs args)
        {
            switch (args.State)
            {
                case TriggeredConnectState.Listening:
                    // Connecting as host
                    this.viewModel.Status = "Eröffne Session...";
                    break;
                case TriggeredConnectState.Connecting:
                    // Connecting as a client
                    this.viewModel.Status = "Verbindungsaufbau...";
                    break;
                case TriggeredConnectState.PeerFound:
                    // Proximity gesture is complete and user can pull their devices away. Remaining work is to 
                    // establish the connection using a different transport, like TCP/IP or Bluetooth
                    this.viewModel.Status = "Session gefunden, versuche zu verbinden...";
                    break;
                case TriggeredConnectState.Completed:
                    // Connection completed, retrieve the socket over which to communicate
                    this.viewModel.Status = "Verbunden";
                    this.HandleConnection(args.Socket);
                    break;
                case TriggeredConnectState.Canceled:
                    this.viewModel.Status = "Verbindung abgebrochen";
                    this.viewModel.Connected = false;
                    break;
                case TriggeredConnectState.Failed:
                    // Connection was unsuccessful
                    this.viewModel.Status = "Fehler bei der Verbindung";
                    this.viewModel.Connected = false;
                    break;
            }
        }

        /// <summary>
        /// Handles the new connection.
        /// </summary>
        /// <param name="socket">The socket</param>
        private void HandleConnection(StreamSocket socket)
        {
            // Get DataReader and Writer
            this.dataReader = new DataReader(socket.InputStream);
            this.dataWriter = new DataWriter(socket.OutputStream);

            this.viewModel.Connected = true;
            this.StopSearching();

            this.ListenForIncomingMessage();
        }
        
        /// <summary>
        /// Waits for incoming messages.
        /// </summary>
        private async void ListenForIncomingMessage()
        {
            try
            {
                var message = await GetMessageAsync();

                // Add to chat
                this.UpdateChatBox(message, true);

                // Listen for next message
                this.ListenForIncomingMessage();
            }
            catch (Exception e)
            {
                this.viewModel.Status = string.Format("Fehler: {0}", e.Message);
                this.viewModel.Connected = false;
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Gets the next incoming message from the data reader.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetMessageAsync()
        {
            // Read message length
            await this.dataReader.LoadAsync(4);
            var messageLength = (uint)this.dataReader.ReadInt32();

            // Read message
            await this.dataReader.LoadAsync(messageLength);
            return this.dataReader.ReadString(messageLength);
        }

        /// <summary>
        /// Sends a message with the data writer.
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>An async operation</returns>
        private async Task SendMessageAsync(string message)
        {
            // Get the length of the message
            var messageLength = dataWriter.MeasureString(message);
            dataWriter.WriteInt32((int)messageLength);
            
            // Send the message
            var sentCommandSize = dataWriter.WriteString(message);
            await dataWriter.StoreAsync();
        }

        /// <summary>
        /// Adds a text to the textbox.
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="isIncoming">If the message is incoming from another peer</param>
        private void UpdateChatBox(string text, bool isIncoming)
        {
            var message = new Message { Text = text };

            if (isIncoming)
            {
                message.From = "Stranger";
            }
            else
            {
                message.From = this.viewModel.Name;
            }

            Dispatcher.BeginInvoke(() => this.viewModel.Conversation.Add(message));
        }

        private async void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageBox.Text) && this.viewModel.Connected && this.dataWriter != null)
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    await this.SendMessageAsync(MessageBox.Text);

                    this.UpdateChatBox(MessageBox.Text, false);
                    MessageBox.Text = "";

                    e.Handled = true;
                }
            }
        }
    }
}