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

        private GpioPin lightPin, tempPin, flamePin, metalPin;

        //private DispatcherTimer lightPinTimer, tempPinTimer, flamePinTimer, metalPinTimer;
        private DispatcherTimer sensorTimer;
        private const int SENSOR_CHECK_TIME = 2000;    // number of milliseconds to get input from the tilt sensor

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
            string lightPinRead, tempPinRead, flamePinRead, metalPinRead;

            lightPinRead = (lightPin.Read()).ToString();
            tempPinRead = (tempPin.Read()).ToString();
            flamePinRead = (flamePin.Read()).ToString();
            metalPinRead = (metalPin.Read()).ToString();

            Debug.WriteLine("Light:" + lightPinRead);
            Debug.WriteLine("Temp:" + tempPinRead);
            Debug.WriteLine("Flame:" + flamePinRead);
            Debug.WriteLine("Metal:" + metalPinRead);

            AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("DeviceID", "lightSensor", lightPinRead, DateTime.Now));

        }
        private string JSONify(string id, string sensorName, string sensorValue, DateTime outputTime)
        {
            //do some fancy JSON stuff to convert the message to a JSON
            Dictionary<string, string> json_message = new Dictionary<string, string>();
            json_message.Add("Id", id) ;
            json_message.Add("SensorName", sensorName);
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

            lightPin.SetDriveMode(GpioPinDriveMode.Input);
            tempPin.SetDriveMode(GpioPinDriveMode.Input);
            flamePin.SetDriveMode(GpioPinDriveMode.Input);
            metalPin.SetDriveMode(GpioPinDriveMode.Input);
            
        }
    }
}