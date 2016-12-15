using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SensorApp
{
    static class AzureIoTHub
    {
        const string deviceConnectionString = "***DEVICE CONNECTION STRING HERE***";

        public static async Task SendDeviceToCloudMessageAsync(string deviceId, string sensorType, string sensorValue)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);    
            var message = new Message(Encoding.ASCII.GetBytes(JSONify(deviceId, sensorType, sensorValue, DateTime.Now)));
            await deviceClient.SendEventAsync(message);
        }

        public static async Task<string> ReceiveCloudToDeviceMessageAsync()
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);
                    return messageData;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }


        private static string JSONify(string deviceId, string sensorType, string sensorValue, DateTime outputTime)
        {
            //do some fancy JSON stuff to convert the message to a JSON
            Dictionary<string, string> json_message = new Dictionary<string, string>();
            json_message.Add("DeviceId", deviceId);
            json_message.Add("SensorType", sensorType);
            json_message.Add("SensorValue", sensorValue);
            json_message.Add("OutputTime", outputTime.ToString());
            return JsonConvert.SerializeObject(json_message);
        }
    }
}