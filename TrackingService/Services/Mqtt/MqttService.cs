using Microsoft.Extensions.Options;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TrackingService.Services.Mqtt
{
    public class MqttService : IMqttService
    {
        private readonly MqttOptions _mqtt_options;
        static object _lock_connect = new object();
        MqttClient _client;

        public event EventHandler<MqttMsgPublishEventArgs> EventReceived;
        public bool IsConnected { get { return _client.IsConnected; } }
        public string RootTopic { get { return _mqtt_options.RootTopic; } }

        public MqttService(IOptions<MqttOptions> mqttOptions)
        {
            _mqtt_options = mqttOptions.Value;
        }

        /// <summary>
        /// Subscribe for events from MQTT broker
        /// </summary>
        /// <param name="topics">Array of topics</param>
        /// <returns>True if sucesfully subscribed, otherwise false</returns>
        public bool Subscribe(string[] topics)
        {
            if (!_client.IsConnected)
                return false;

            _client.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            _client.MqttMsgPublishReceived += _client_MqttMsgPublishReceived;
            return true;
        }

        /// <summary>
        /// Disconnects from MQTT broker
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_client.IsConnected)
                    _client.Disconnect();

                _client.MqttMsgPublishReceived -= _client_MqttMsgPublishReceived;
                _client = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void _client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Notify subscribers that new event arrives
            if (EventReceived != null)
                EventReceived(sender, e);
        }

        /// <summary>
        /// Publishes message to subscribers
        /// </summary>
        /// <param name="topic">topic to publish to</param>
        /// <param name="message">message to publish</param>
        /// <param name="qosLevel">Quality Of Service</param>
        /// <param name="retain">indicates that the message should be stored at the broker for its topic</param>
        /// <returns>The result is provided if a Publish message is successfully delivered (sent or acknowledged respectively to its QoS level).</returns>
        public ushort Publish(string topic, string message, byte qosLevel, bool retain)
        {
            return _client.Publish(topic, Encoding.UTF8.GetBytes(message), qosLevel, retain);
        }

        /// <summary>
        /// Connects to MQTT broker
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            lock (_lock_connect)
            {
                try
                {
                    if (_client == null || !_client.IsConnected)
                    {
                        _client = new MqttClient(_mqtt_options.Host);
                        // Connect with a clean session
                        _client.Connect(Guid.NewGuid().ToString(), null, null, true, 10);
                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
    }
}
