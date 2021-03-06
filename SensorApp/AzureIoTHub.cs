﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics;


namespace SensorApp
{
    static class AzureIoTHub
    {
        const string iotHubUri = "Your-IoT-Hub-Hostname";
        const string deviceKey = "Your-device-key";

        public static async Task SendDeviceToCloudMessageAsync(string deviceId, string sensorType, string sensorValue)
        {
            var deviceClient = DeviceClient.Create(iotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Amqp);
            var sensorMessage = (new SensorMessage(deviceId, sensorType, sensorValue)).JSONify();
            var msg = new Message(Encoding.ASCII.GetBytes(sensorMessage));

            //for IoT Hub Routing
            if (sensorValue.Equals("High"))
                msg.Properties.Add("Alert", "alert!");

            try
            {
                await deviceClient.SendEventAsync(msg);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e);
                return;
            }
        }

        public static async Task<string> ReceiveCloudToDeviceMessageAsync(String deviceId)
        {
            var deviceClient = DeviceClient.Create(iotHubUri, AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Amqp);

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
    }
}