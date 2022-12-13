using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace ChatMain
{
    public partial class Form1 : Form
    {
        #region Global Veriables
        BaseClass baseClass = new BaseClass();
        ConnectionFactory factory;
        IModel channel;
        string frndRouteKey;
        #endregion

        public Form1()
        {
            InitializeComponent();
            panel1.Enabled = false;
            btnDisconnect.Visible = false;
            comboBox1.Text = comboBox1.Items[0].ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            factory = baseClass.returnFactory("amqps://zmspyaxr:5I14joWiJqU4hTBd4JvVwtwswuH9PZI5@woodpecker.rmq.cloudamqp.com/zmspyaxr");
            channel = baseClass.returnChannel(factory);
        }

        #region Click events
        private void btnConnect_Click(object sender, EventArgs e)
        {
            txtSelfID.Text = comboBox1.Text;
            if (txtSelfID.Text != "" && txtFrndID.Text != "" && txtSelfID.Text != "Self ID")
            {
                try
                {
                    SubcriberStarter();
                    PublisherConnect();

                    pnlConnValues.Enabled = false;
                    btnConnect.Text = "Connected";
                    btnDisconnect.Visible = true; btnDisconnect.Enabled = true;
                    panel1.Enabled = true;
                    txtMessage.Focus();
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            try
            {
                baseClass.sendMessage(message, channel, frndRouteKey);
                baseClass.AddMessage(lstMessages, "You", message);
                txtMessage.Clear(); txtMessage.Focus();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            baseClass.lastMessage(lstMessages);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            btnDisconnect.Text = "Disconnecting...";
            Thread.Sleep(250);
            pnlConnValues.Enabled = true;
            txtFrndID.ReadOnly = false;
            btnConnect.Text = "Connect";
            txtFrndID.Clear(); txtSelfID.Clear();
            panel1.Enabled=false;
            comboBox1.Text = comboBox1.Items[0].ToString(); lstMessages.Items.Clear();

            btnDisconnect.Text = "Disconnect"; btnDisconnect.Visible = false;

        }
        #endregion

        #region Connections
        internal void SubcriberStarter()
        {
            string self_queueName = $"direct-queue-M{txtSelfID.Text}";

            channel.QueueDeclare(self_queueName, false, false, false);
            channel.BasicQos(0, 1, false);
            var subcriber = new EventingBasicConsumer(channel); //Creating consumer
            channel.BasicConsume(self_queueName, false, subcriber);
            MessageBox.Show("Listening...", "Chat Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            subcriber.Received += (object senderr, BasicDeliverEventArgs ee) =>
            {
                var message = Encoding.UTF8.GetString(ee.Body.ToArray());
                baseClass.AddMessage(lstMessages, "Friend", message);

                channel.BasicAck(ee.DeliveryTag, false); //notifies rabbitmq that the message will be deleted
                baseClass.lastMessage(lstMessages);
            };
        }

        internal void PublisherConnect()
        {
            txtFrndID.ReadOnly = true;

            channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct); //add Exchange
            frndRouteKey = $"route-{txtFrndID.Text}";
            var frndqueueName = $"direct-queue-M{txtFrndID.Text}";
            channel.QueueBind(frndqueueName, "logs-direct", frndRouteKey, null);
        }
        #endregion
          
        #region Events
        private void btnSend_MouseEnter(object sender, EventArgs e)
        {
            btnSend.BackColor = Color.White; btnSend.ForeColor = Color.Black;
        }

        private void btnSend_MouseLeave(object sender, EventArgs e)
        {
            btnSend.BackColor = Color.Black; btnSend.ForeColor = Color.White;
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(btnSend, new EventArgs());
            }
        }
        #endregion
    }
}
