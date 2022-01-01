using System;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TrackingService.Services.Mqtt
{
    public interface IMqttService
    {
        ushort Publish(string topic, string message, byte qosLevel, bool retain);
        bool Connect();
        bool IsConnected { get; }
        bool Subscribe(string[] topics);
        void Disconnect();
        event EventHandler<MqttMsgPublishEventArgs> EventReceived;
        string RootTopic { get; }
    }
}
