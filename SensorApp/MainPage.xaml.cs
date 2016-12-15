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
        private const int LIGHT_SENSOR_PIN = 26;
        private const int FLAME_SENSOR_PIN = 13;
        private const int METAL_SENSOR_PIN = 06;
        private const int GAS_SENSOR_PIN = 05;
        private const int KNOCK_SENSOR_PIN = 21;
        private GpioPin lightPin, flamePin, metalPin, gasPin, knockPin;
        private DispatcherTimer sensorTimer;
        private const int SENSOR_CHECK_TIME = 2000;    // number of milliseconds to get input from the tilt sensor
        private string deviceId = "Device01";
        int listPosition = 0;
        BMP280 BMP280;

        public MainPage()
        {
            this.InitializeComponent();

            InitGPIO();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = TimeSpan.FromMilliseconds(SENSOR_CHECK_TIME);
            sensorTimer.Tick += Sensor_Timer_Tick;

            if (lightPin != null && flamePin != null && metalPin != null)
            {
                sensorTimer.Start();
            }
            else
            {
                Debug.WriteLine("A sensor is null");
            }


        } //This method will be called by the application framework when the page is first loaded
       
        private async void Sensor_Timer_Tick(object sender, object e)   
        {
            string flamePinRead, metalPinRead, gasPinRead, knockPinRead;

            getLight();
            getTemp();
            flamePinRead = (flamePin.Read()).ToString();
            metalPinRead = (metalPin.Read()).ToString();
            gasPinRead = (gasPin.Read()).ToString();
            knockPinRead = (knockPin.Read()).ToString();

            if (gasPinRead.Equals("High"))
            {
                //Debug.WriteLine("Gas:" + gasPinRead);
                updateUI("Gas:" + gasPinRead);
                await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("gasSensor", gasPinRead, DateTime.Now));
            }

            if (flamePinRead.Equals("High"))
            {
                //Debug.WriteLine("Knock:" + knockPinRead);
                updateUI("Knock:" + knockPinRead);
                await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("knockSensor", knockPinRead, DateTime.Now));
            }

            if (flamePinRead.Equals("High"))
            {
                //Debug.WriteLine("Flame:" + flamePinRead);
                updateUI("Flame:" + flamePinRead);
                await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("flameSensor", flamePinRead, DateTime.Now));
            }

            if (metalPinRead.Equals("High"))
            {
                //Debug.WriteLine("Metal:" + metalPinRead);
                updateUI("Metal:" + metalPinRead);
                await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("metalSensor", metalPinRead, DateTime.Now));
            }
        }

        private string JSONify(string sensorType, string sensorValue, DateTime outputTime)
        {
            //do some fancy JSON stuff to convert the message to a JSON
            Dictionary<string, string> json_message = new Dictionary<string, string>();
            json_message.Add("DeviceId", deviceId);
            json_message.Add("SensorType", sensorType);
            json_message.Add("SensorValue", sensorValue);
            json_message.Add("OutputTime", outputTime.ToString());
            return JsonConvert.SerializeObject(json_message);
        }


        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            InitTemp();

            lightPin = gpio.OpenPin(LIGHT_SENSOR_PIN);
            //tempPin = gpio.OpenPin(TEMP_SENSOR_PIN);
            flamePin = gpio.OpenPin(FLAME_SENSOR_PIN);
            metalPin = gpio.OpenPin(METAL_SENSOR_PIN);
            gasPin = gpio.OpenPin(GAS_SENSOR_PIN);
            knockPin = gpio.OpenPin(KNOCK_SENSOR_PIN);

            //tempPin.SetDriveMode(GpioPinDriveMode.Input);
            flamePin.SetDriveMode(GpioPinDriveMode.Input);
            metalPin.SetDriveMode(GpioPinDriveMode.Input);
            gasPin.SetDriveMode(GpioPinDriveMode.Input);
            knockPin.SetDriveMode(GpioPinDriveMode.Input);
        }

        private void updateUI(string msg)
        {
            switch (listPosition)
            {
                case 0:
                    box1.Text = msg;
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

        private async void InitTemp()
        {
            try
            {
                //Create a new object for our barometric sensor class
                BMP280 = new BMP280();
                //Initialize the sensor
                await BMP280.Initialize();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception with init temp: " + ex.Message);
            }
        }

        private async void getTemp()
        {
            try
            {
                

                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
                float temp = 0;
                float pressure = 0;
                float altitude = 0;

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1013.25f;

                    temp = await BMP280.ReadTemperature();
                    pressure = await BMP280.ReadPreasure();
                    altitude = await BMP280.ReadAltitude(seaLevelPressure);

                    //Write the values to your debug console
                    Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");
                    Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");
                    Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

                    updateUI("Temperature: " + temp.ToString() + " deg C");
                    updateUI("Pressure: " + pressure.ToString() + " Pa");
                    updateUI("Altitude: " + altitude.ToString() + " m");

                    await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("temperature", temp.ToString() + " deg C", DateTime.Now));
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("pressure", pressure.ToString() + " Pa", DateTime.Now));
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("altitude", altitude.ToString() + " m", DateTime.Now));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private async void getLight()
        {
            lightPin.SetDriveMode(GpioPinDriveMode.Output);
            lightPin.Write(GpioPinValue.Low);
            await Task.Delay(500);
            lightPin.SetDriveMode(GpioPinDriveMode.Input);

            int measurement = 0;

            while (lightPin.Read() == GpioPinValue.Low)
                measurement += 1;

            //Debug.WriteLine("Light:" + measurement);
            updateUI("Light: " + measurement);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("lightSensor", measurement.ToString(), DateTime.Now));
        } 
    }
}