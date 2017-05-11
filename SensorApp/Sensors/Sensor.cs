using System.Diagnostics;
using Windows.Devices.Gpio;

namespace SensorApp
{
    class Sensor
    {
        private GpioPin gpioPin;
        GpioController gpio = GpioController.GetDefault();

        public string sensorType;
        public string value;

        public Sensor(int pin, string snsrType)
        {
            this.sensorType = snsrType;
            gpioPin = gpio.OpenPin(pin);
            gpioPin.SetDriveMode(GpioPinDriveMode.Input);

            if (gpioPin == null)
            {
                Debug.WriteLine(this.sensorType + " failed to initialize.");
                return;
            }
        }

        public void ReadSensor()
        {
            Debug.WriteLine("Reading " + this.sensorType);

            this.value = (gpioPin.Read()).ToString();

            Debug.WriteLine("Finished reading sensor, value is " + this.value);

        }
    }
}
