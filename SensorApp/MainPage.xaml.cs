using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int LIGHT_SENSOR_PIN = 26;
        private const int TEMP_SENSOR_PIN = 19;
        private const int FLAME_SENSOR_PIN = 13;
        private const int METAL_SENSOR_PIN = 06;
        private const int GAS_SENSOR_PIN = 05;
        private const int KNOCK_SENSOR_PIN = 21;
        private GpioPin lightPin, tempPin, flamePin, metalPin, gasPin, knockPin;
        private DispatcherTimer sensorTimer;
        private const int SENSOR_CHECK_TIME = 1000;    // number of milliseconds to get input from the tilt sensor
        private string deviceId = "Device01";
        private int measurement = 0;

        public MainPage()
        {
            this.InitializeComponent();

            InitGPIO();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = TimeSpan.FromMilliseconds(SENSOR_CHECK_TIME);
            sensorTimer.Tick += Sensor_Timer_Tick;

            if (lightPin != null && tempPin != null && flamePin != null && metalPin != null)
            {
                sensorTimer.Start();
            }
            else
            {
                Debug.WriteLine("A sensor is null");
            }
        }

        private async void Sensor_Timer_Tick(object sender, object e)
        {
            string lightPinRead, tempPinRead, flamePinRead, metalPinRead, gasPinRead, knockPinRead;

            getLight();

            //lightPinRead = getLight().ToString();
            //lightPinRead = measurement.ToString();
            tempPinRead = (tempPin.Read()).ToString();
            flamePinRead = (flamePin.Read()).ToString();
            metalPinRead = (metalPin.Read()).ToString();
            gasPinRead = (gasPin.Read()).ToString();
            knockPinRead = (knockPin.Read()).ToString();

            //Debug.WriteLine("Light:" + lightPinRead);
            Debug.WriteLine("Temp:" + tempPinRead);
            Debug.WriteLine("Gas:" + gasPinRead);
            Debug.WriteLine("Knock:" + knockPinRead);
            
            if (flamePinRead.Equals("High"))
            {
                Debug.WriteLine("Flame:" + flamePinRead);
                AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("flameSensor", flamePinRead, DateTime.Now));
            }

            if (metalPinRead.Equals("High"))
            {
                Debug.WriteLine("Metal:" + metalPinRead);
                AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("metalSensor", metalPinRead, DateTime.Now));
            }

            AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("tempSensor", tempPinRead, DateTime.Now));
            AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("gasSensor", gasPinRead, DateTime.Now));
            AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("knockSensor", knockPinRead, DateTime.Now));
        }

        private string JSONify(string sensorType, string sensorValue, DateTime outputTime)
        {
            //do some fancy JSON stuff to convert the message to a JSON
            Dictionary<string, string> json_message = new Dictionary<string, string>();
            json_message.Add("DeviceId", deviceId) ;
            json_message.Add("SensorType", sensorType);
            json_message.Add("SensorValue", sensorValue);
            json_message.Add("OutputTime", outputTime.ToString());
            return JsonConvert.SerializeObject(json_message);
        }


        private async void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            
            lightPin = gpio.OpenPin(LIGHT_SENSOR_PIN);
            tempPin = gpio.OpenPin(TEMP_SENSOR_PIN);
            flamePin = gpio.OpenPin(FLAME_SENSOR_PIN);
            metalPin = gpio.OpenPin(METAL_SENSOR_PIN);
            gasPin = gpio.OpenPin(GAS_SENSOR_PIN);
            knockPin = gpio.OpenPin(KNOCK_SENSOR_PIN);

            getLight();
            
            tempPin.SetDriveMode(GpioPinDriveMode.Input);
            flamePin.SetDriveMode(GpioPinDriveMode.Input);
            metalPin.SetDriveMode(GpioPinDriveMode.Input);
            gasPin.SetDriveMode(GpioPinDriveMode.Input);
            knockPin.SetDriveMode(GpioPinDriveMode.Input);
            
        }

        private async void getLight()
        {
            lightPin.SetDriveMode(GpioPinDriveMode.Output);
            lightPin.Write(GpioPinValue.Low);
            await Task.Delay(500);
            lightPin.SetDriveMode(GpioPinDriveMode.Input);

            measurement = 0;

            while (lightPin.Read() == GpioPinValue.Low)
                measurement += 1;

            Debug.WriteLine("Light:" + measurement);
            
            await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("lightSensor", measurement.ToString(), DateTime.Now));
        }
    }
}