using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NfcChat.DataModel;

namespace NfcChat
{
    /// <summary>
    /// The view model for the chat items.
    /// </summary>
    public class ChatViewModel : BindableBase
    {
        private string status;
        private string name;
        private bool connected;

        public ChatViewModel(Action<Action> dispatcher)
            : base(dispatcher)
        {
            this.Connected = false;
            this.Conversation = new ObservableCollection<Message>();
        }

        public ObservableCollection<Message> Conversation { get; set; }

        public string Status
        {
            get { return this.status; }
            set { this.SetProperty(ref this.status, value); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        public bool Connected
        {
            get { return this.connected; }
            set { this.SetProperty(ref this.connected, value); }
        }
    }
}
