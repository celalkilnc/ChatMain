using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;

namespace ChatMain
{
    internal class BaseClass
    { //45
        #region Methods
        internal ConnectionFactory returnFactory(string uri)
        {
            ConnectionFactory factory = new ConnectionFactory(); //RAbbitMQ connection
            factory.Uri = new Uri(uri); //created the uri connection channel using the string on the site
            return factory;
        }

        internal IModel returnChannel(ConnectionFactory factory)
        {
            var connection = factory.CreateConnection();// connection open
            var channel = connection.CreateModel(); // created channel
            return channel;
        }

        internal void sendMessage(string message, IModel channel, string routeKey)
        {
            byte[] messageBody = Encoding.UTF8.GetBytes(message); //get the bytes of the defined message
            channel.BasicPublish("logs-direct", routeKey, null, messageBody);
        }

        internal void lastMessage(ListBox YourListBox)
        {
            YourListBox.SetSelected(YourListBox.Items.Count - 1, true);
            YourListBox.SetSelected(YourListBox.Items.Count - 1, false);
        }
        public void AddMessage(ListBox lst, string publishTag, string _message)
        {
            string sentence = $"{publishTag}: {_message}                 [{Time()}]";
            lst.Items.Add(sentence);
        }
        private string Time()
        {
            return $"{DateTime.Now.Hour}.{DateTime.Now.Minute}";
        }

        #endregion
    }
}
