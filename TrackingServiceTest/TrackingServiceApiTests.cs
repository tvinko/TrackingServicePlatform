using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using TrackingService;
using TrackingService.Helpers;
using TrackingService.Services.Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Xunit;

namespace TrackingServiceTest
{
    public class TrackingServiceApiTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        const int NO_OF_REQUESTS = 500;
        const string BASE_URL = "/TrackingService/{0}?data={1}";
        const string DISABLED_ACCOUNT_ID = "3";
        const string ACTIVE_ACCOUNT_ID = "1";
        const string NON_EXISTING_ACCOUNT_ID = "0";
        const int PERF_TEST_TIMEOUT = 1000;
 
        AutoResetEvent _wait = new AutoResetEvent(false);
        readonly HttpClient _client;
        readonly IMqttService _mqttService;

        readonly Stopwatch timer = new Stopwatch();
        int Counter = 0;

        public TrackingServiceApiTests(WebApplicationFactory<Startup> fixture)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            _client = fixture.CreateClient();
            _mqttService = fixture.Services.GetService(typeof(IMqttService)) as IMqttService;
        }

        [Fact]
        public void Get_Account_ReturnsNonExisting()
        {
            var response =
                _client.GetAsync(string.Format(BASE_URL, NON_EXISTING_ACCOUNT_ID, "11")).Result;
            ((int)response.StatusCode).Should().Be((int)HTTP_CODES.SUCCESS_ACCOUNT_NOT_FOUND);
        }

        [Fact]
        public void Get_Account_ReturnsDisabled()
        {
            var response = _client.GetAsync(string.Format(BASE_URL, DISABLED_ACCOUNT_ID, "11")).Result;
            ((int)response.StatusCode).Should().Be((int)HTTP_CODES.SUCCESS_ACCOUNT_DISABLED);
        }

        [Fact]
        public void Get_Account_ReturnsActive()
        {
            var response = _client.GetAsync(string.Format(BASE_URL, ACTIVE_ACCOUNT_ID, "11")).Result;
            ((int)response.StatusCode).Should().Be((int)HTTP_CODES.SUCCESS_ACCOUNT_ACTIVE);
        }

        [Fact]
        public void Performance_Test_500_Requests()
        {
            _mqttService.Connect();
            _mqttService.EventReceived += MqttService_EventReceived;
            _mqttService.Subscribe(new string[] { string.Format("{0}/{1}", _mqttService.RootTopic, ACTIVE_ACCOUNT_ID) });
            timer.Start();

            for (int i = 0; i <= NO_OF_REQUESTS; i++)
                _client.GetAsync(string.Format(BASE_URL, ACTIVE_ACCOUNT_ID, i.ToString()));

            _wait.WaitOne(PERF_TEST_TIMEOUT);
            timer.Elapsed.TotalMilliseconds.Should().BeLessThan(PERF_TEST_TIMEOUT);
            _mqttService.Disconnect();
            _mqttService.EventReceived -= MqttService_EventReceived;
        }


        private void MqttService_EventReceived(object sender, MqttMsgPublishEventArgs e)
        {

            Counter++;

            if (Counter == NO_OF_REQUESTS)
            {
                timer.Stop();
                _wait.Set();
            }
        }
    }
}
