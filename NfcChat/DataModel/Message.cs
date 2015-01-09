using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcChat.DataModel
{
    /// <summary>
    /// A basic message class.
    /// </summary>
    public class Message
    {
        public string From { get; set; }
        public string Text { get; set; }
    }
}
