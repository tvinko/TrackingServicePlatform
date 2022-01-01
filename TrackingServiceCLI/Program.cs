using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using TrackingServiceCLI.Services;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TrackingServiceCLI
{
    class Program
    {
        static AutoResetEvent _wait = new AutoResetEvent(false);

        const int PERF_TEST_TIMEOUT = 10000;
        static bool _write_response = false;
        static Stopwatch _timer = new Stopwatch();
        static int _counter = 0;
        static int _loop_count = 500;
        const string TOPIC_NAME = "events/+";
        static string _current_topic = string.Empty;
        static Mqtt _mqtt;
        const string CLIENT_ID = "CLI_CLIENT";
        const string VALID_ACCOUNT_ID = "1";

        static void Main(string[] args)
        {
            _mqtt = new Mqtt(ConfigurationManager.AppSettings["mqtt_host"]);
            _mqtt.EventReceived += _mqtt_EventReceived;

            HandleUserInput();
        }

        /// <summary>
        /// Handles user action
        /// </summary>
        static void HandleUserInput()
        {
            while (true)
            {
                Console.WriteLine("Press ");
                Console.WriteLine("\t1) Subscribe to topic");
                Console.WriteLine("\t2) Unsubscribe from topic");
                Console.WriteLine("\t3) Performance tests");



                var decision = Console.ReadLine();
                Console.Clear();
                try
                {
                    switch (decision)
                    {
                        case "1":
                            TopicSubscription();
                            break;

                        case "2":
                            TopicUnsubscription();
                            break;

                        case "3":
                            PerfTest();
                            break;

                        default:
                            Console.WriteLine("Unknown action...");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Unsubscribe from the topic
        /// </summary>
        static void TopicUnsubscription()
        {
            Console.WriteLine("Specify topic");
            _current_topic = Console.ReadLine();
            _mqtt.DisconnectAndUnsubscribe(_current_topic);
        }

        /// <summary>
        /// Subscribe to the topic
        /// </summary>
        static void TopicSubscription()
        {
            // Event will be written to console log
            _write_response = true;
            Console.WriteLine($"Specify topic (default {TOPIC_NAME})");
            _current_topic = Console.ReadLine();
            _current_topic = string.IsNullOrEmpty(_current_topic) ? TOPIC_NAME : _current_topic;
            _mqtt.ConnectAndSubscribe(CLIENT_ID, _current_topic);
        }

        /// <summary>
        /// Performance test
        /// </summary>
        static void PerfTest()
        {
            // Events will not be written to console log
            _write_response = false;
            _counter = 0;
            Console.WriteLine("Loop count (default 500):");
            var loopCnt = Console.ReadLine();
            _loop_count = string.IsNullOrEmpty(loopCnt) ? 500 : Convert.ToInt32(loopCnt);
            var isSubscribed = _mqtt.ConnectAndSubscribe(CLIENT_ID, TOPIC_NAME);
            if (!isSubscribed)
                return;

            _timer.Reset();
            _timer.Start();

            // Fill urls that will be called by performance test
            List<string> urls = new List<string>();
            for (int i = 0; i <= _loop_count; i++)
            {
                urls.Add($"/{ConfigurationManager.AppSettings["api_root"]}/{VALID_ACCOUNT_ID}?data="
                    + i.ToString());
            }

            WebApiCall.CallWebApi(ConfigurationManager.AppSettings["base_address"], urls);

            // Wait until all events arrive to subscribed topic
            var isTimeoutExpired = !_wait.WaitOne(PERF_TEST_TIMEOUT);
            if (isTimeoutExpired)
                Console.WriteLine("Timeout expired");

            _mqtt.DisconnectAndUnsubscribe(TOPIC_NAME);
        }

        static void _mqtt_EventReceived(object? sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                if (_write_response)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(e.Message));
                }
                else
                {
                    // This is last event. Performance test can complete
                    if (_counter++ == _loop_count)
                    {
                        _timer.Stop();
                        Console.WriteLine($"Time taken: {_timer.Elapsed.ToString(@"m\:ss\.fff")}");
                        // Notify main thread that the last event arrives
                        _wait.Set();

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Event recevied error: {ex.Message}");
            }
        }
    }
}