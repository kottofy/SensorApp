using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.Devices.Spi;

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer sensorTimer;
        private const int SENSOR_CHECK_TIME = 2000,
            FLAME_SENSOR_PIN = 13,
            METAL_SENSOR_PIN = 06,
            LIGHT_SENSOR_PIN = 26,
            GAS_SENSOR_PIN = 05,
            KNOCK_SENSOR_PIN = 21;
        int listPosition = 0;
        Sensor lightSensor, flameSensor, metalSensor, gasSensor, knockSensor;
        TempSensor tempSensor;
        string deviceId = "Device01";
        Sensor[] sensors = new Sensor[5];

        public MainPage()
        {
            this.InitializeComponent();

            InitSensors();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = TimeSpan.FromMilliseconds(SENSOR_CHECK_TIME);
            sensorTimer.Tick += Sensor_Timer_Tick;
            sensorTimer.Start();
        }

        private void InitSensors()
        {
            lightSensor = new Sensor(LIGHT_SENSOR_PIN, "light");
            flameSensor = new Sensor(FLAME_SENSOR_PIN, "flame");
            metalSensor = new Sensor(METAL_SENSOR_PIN, "metal");
            gasSensor = new Sensor(GAS_SENSOR_PIN, "gas");
            knockSensor = new Sensor(KNOCK_SENSOR_PIN, "knock");
            tempSensor = new TempSensor();

            sensors[0] = lightSensor;
            sensors[1] = flameSensor;
            sensors[2] = metalSensor;
            sensors[3] = gasSensor;
            sensors[4] = knockSensor;
        }

        private async void Sensor_Timer_Tick(object sender, object e)   
        {
            await lightSensor.getLight();
            await tempSensor.getTemp();
            flameSensor.readSensor();
            metalSensor.readSensor();
            gasSensor.readSensor();
            knockSensor.readSensor();

            foreach (var snsr in sensors)
            {
                Debug.WriteLine("Sensor: " + snsr.sensorType + ", value: " + snsr.value);

                if (snsr.value.Equals("High"))
                {
                    updateUI(snsr.sensorType, snsr.value);
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(deviceId, snsr.sensorType, snsr.value);
                }
            }

            updateUI("Temperature", tempSensor.temperature);
            updateUI("Pressure", tempSensor.pressure);
            updateUI("Altitude", tempSensor.altitude);

            await AzureIoTHub.SendDeviceToCloudMessageAsync(deviceId, "Temperature", tempSensor.temperature);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(deviceId, "Pressure", tempSensor.pressure);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(deviceId, "Altitude", tempSensor.altitude);
        }

        private void updateUI(string type, string value)
        {
            string msg = "Type: " + type + " , Value: " + value;

            switch (listPosition)
            {
                case 0:
                    box1.Text = msg;
                    box2.Text = "";
                    box3.Text = "";
                    box4.Text = "";
                    box5.Text = "";
                    break;
                case 1:
                    box2.Text = msg;
                    break;
                case 2:
                    box3.Text = msg;
                    break;
                case 3:
                    box4.Text = msg;
                    break;
                case 4:
                    box5.Text = msg;
                    break;
            }

            listPosition++;

            if (listPosition > 4)
                listPosition = 0;
        }
    }
}