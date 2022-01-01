using System;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TrackingServiceCLI.Services
{
    internal class Mqtt
    {
        internal string _host { get; set; }
        internal event EventHandler<MqttMsgPublishEventArgs> EventReceived;

        MqttClient client;
        internal Mqtt(string host)
        {
            _host = host;
        }

        /// <summary>
        /// Connect to MQTT broker and subscribe to topic
        /// </summary>
        /// <param name="clientId">id of the connecting client</param>
        /// <param name="topic">subscription topic</param>
        /// <returns>true if client is succesfully  subscribed to topic, otherwise false</returns>
        internal bool ConnectAndSubscribe(string clientId, string topic)
        {
            try
            {
                if (client == null || !client.IsConnected)
                {
                    client = new MqttClient(_host);
                    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

                    // Connect to existing session
                    client.Connect(clientId, null, null, false, 10);

                    if (client.IsConnected)
                        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                client = null;
                return false;
            }
            return client.IsConnected;
        }

        /// <summary>
        /// Disconnect from MQTT broker and unsubscribe from topic
        /// </summary>
        /// <param name="topic"></param>
        internal void DisconnectAndUnsubscribe(string topic)
        {
            try
            {
                if (client == null)
                    return;

                client.Unsubscribe(new string[] { topic });

                if (client.IsConnected)
                    client.Disconnect();

                client.MqttMsgPublishReceived -= Client_MqttMsgPublishReceived;
                client = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Notify subscribers that new event arrives
            if (EventReceived != null)
                EventReceived(sender, e);
        }
    }
}
