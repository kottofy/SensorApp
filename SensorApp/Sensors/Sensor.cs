using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            sensorType = snsrType;
            gpioPin = gpio.OpenPin(pin);
            gpioPin.SetDriveMode(GpioPinDriveMode.Input);

            if (gpioPin == null)
            {
                Debug.WriteLine(sensorType + " failed to initialize.");
                return;
            }
        }

        public async Task getLight()
        {
            Debug.WriteLine("Getting Light.");

            gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            gpioPin.Write(GpioPinValue.Low);
            await Task.Delay(500);
            gpioPin.SetDriveMode(GpioPinDriveMode.Input);

            int measurement = 0;

            while (gpioPin.Read() == GpioPinValue.Low)
                measurement += 1;

            value = measurement.ToString();

            Debug.WriteLine("Finished Getting Light.");
        }

        public void readSensor()
        {
            Debug.WriteLine("Reading sensor.");

            value = (gpioPin.Read()).ToString();

            Debug.WriteLine("Finished reading sensor.");

        }

    }
}
