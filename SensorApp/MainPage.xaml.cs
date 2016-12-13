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
        private const int TEMP_SENSOR_PIN = 19;
        private const int FLAME_SENSOR_PIN = 13;
        private const int METAL_SENSOR_PIN = 06;
        private const int GAS_SENSOR_PIN = 05;
        private const int KNOCK_SENSOR_PIN = 21;
        private GpioPin lightPin, tempPin, flamePin, metalPin, gasPin, knockPin;
        private DispatcherTimer sensorTimer;
        private const int SENSOR_CHECK_TIME = 2000;    // number of milliseconds to get input from the tilt sensor
        private string deviceId = "Device01";
        int listPosition = 0;
        enum ADCChip { mcp3008 }
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        byte[] readBuffer = null;                           /* this is defined to hold the output data*/
        byte[] writeBuffer = null;                          /* we will hold the command to send to the chipbuild this in the constructor for the chip we are using */
        private SpiDevice SpiDisplay;

        public MainPage()
        {
            this.InitializeComponent();

            InitGPIO();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = TimeSpan.FromMilliseconds(SENSOR_CHECK_TIME);
            sensorTimer.Tick += Sensor_Timer_Tick;

            if (lightPin != null && tempPin != null && flamePin != null && metalPin != null)
                sensorTimer.Start();
            else
                Debug.WriteLine("A sensor is null");

            readBuffer = new byte[3] { 0x00, 0x00, 0x00 };
            writeBuffer = new byte[3] { 0x01, 0x80, 0x00 };

            InitSPI();
        }

        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;// 10000000;
                settings.Mode = SpiMode.Mode3; //Mode3;
                var controller = await SpiController.GetDefaultAsync();
                SpiDisplay = controller.GetDevice(settings);
            }
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }
        private string getTemp()
        {
            SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            int res = convertToInt(readBuffer);
            return res.ToString();
        }
        private int convertToInt(byte[] data)
        {
            int result = 0;
            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];
            return result;
        }

        private async void Sensor_Timer_Tick(object sender, object e)
        {
            string flamePinRead, metalPinRead, gasPinRead, knockPinRead;

            getLight();
            //getTemperature();
            var tempPinRead = getTemp();
            flamePinRead = (flamePin.Read()).ToString();
            metalPinRead = (metalPin.Read()).ToString();
            gasPinRead = (gasPin.Read()).ToString();
            knockPinRead = (knockPin.Read()).ToString();

            try
            {
                Debug.WriteLine("Temp: " + tempPinRead);
                updateUI("Temp: " + tempPinRead);
                await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("tempSensor", tempPinRead, DateTime.Now));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception:" + ex.ToString());
            }

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

            lightPin = gpio.OpenPin(LIGHT_SENSOR_PIN);
            tempPin = gpio.OpenPin(TEMP_SENSOR_PIN);
            flamePin = gpio.OpenPin(FLAME_SENSOR_PIN);
            metalPin = gpio.OpenPin(METAL_SENSOR_PIN);
            gasPin = gpio.OpenPin(GAS_SENSOR_PIN);
            knockPin = gpio.OpenPin(KNOCK_SENSOR_PIN);

            tempPin.SetDriveMode(GpioPinDriveMode.Input);
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


        private async void getLight()
        {
            lightPin.SetDriveMode(GpioPinDriveMode.Output);
            lightPin.Write(GpioPinValue.Low);
            await Task.Delay(500);
            lightPin.SetDriveMode(GpioPinDriveMode.Input);

            int measurement = 0;

            while (lightPin.Read() == GpioPinValue.Low)
                measurement += 1;

            Debug.WriteLine("Light:" + measurement);
            updateUI("Light: " + measurement);
            await AzureIoTHub.SendDeviceToCloudMessageAsync(JSONify("lightSensor", measurement.ToString(), DateTime.Now));
        } 
    }
}