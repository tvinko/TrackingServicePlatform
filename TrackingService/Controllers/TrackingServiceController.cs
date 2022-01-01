using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TrackingService.Helpers;
using TrackingService.Models;
using TrackingService.Services.Db;
using TrackingService.Services.Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace TrackingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackingServiceController : ControllerBase
    {
        static IMqttService _mqttService;
        private readonly ITrackingServiceAccountUnit _trackingServiceAccountUnit;

        public TrackingServiceController(IMqttService mqttService,
            ITrackingServiceAccountUnit trackingServiceAccountUnit)
        {
            _trackingServiceAccountUnit = trackingServiceAccountUnit;
            _mqttService = mqttService;
        }

        /// <summary>
        /// Validates account and publish event if account is valid
        /// </summary>
        /// <param name="accountId">Account to validate</param>
        /// <param name="data">Published event message data</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{accountId}")]
        public IActionResult Get(int accountId, string data)
        {
            var accountStatusData = new AccountStatusData(accountId);
            // Validates account
            _trackingServiceAccountUnit.CheckAccountStatus(accountStatusData);
            // Account has passed validation
            if (accountStatusData.HttpCode == HTTP_CODES.SUCCESS_ACCOUNT_ACTIVE)
            {
                if (!_mqttService.Connect())
                {
                    accountStatusData.HttpCode = HTTP_CODES.ERROR_MQTT_CONN;
                    accountStatusData.HttpMessage = "MQTT Connection error";
                }
                else
                {
                    _mqttService.Publish(string.Format("{0}/{1}", _mqttService.RootTopic, accountId),
                         JsonConvert.SerializeObject(new PropagatedData(accountId, data)),
                         MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                         false);
                }
            }
            return new ObjectResult(accountStatusData.HttpMessage) { StatusCode = (int)accountStatusData.HttpCode };
        }
    }
}