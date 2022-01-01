namespace TrackingService.Helpers
{
    public enum HTTP_CODES
    {
        SUCCESS_ACCOUNT_ACTIVE = 200,
        SUCCESS_ACCOUNT_DISABLED = 201,
        SUCCESS_ACCOUNT_NOT_FOUND = 202,
        SUCCESS_MQTT_CONN = 203,
        SUCCESS_MQTT_PUBLISH = 204,
        SUCCESS_DB_CONN = 205,
        ERROR_DB_CONN = 405,
        ERROR_DB_QUERY = 406,
        ERROR_MQTT_CONN = 407,
        ERROR_MQTT_PUBLISH = 408
    }
}
