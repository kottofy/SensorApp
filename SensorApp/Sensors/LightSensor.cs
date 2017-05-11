using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Microsoft.Azure.Devices.Client;

namespace SensorApp
{
    class LightSensor
    {
        public string lightValue;

        private const Int32 SPI_CHIP_SELECT_LINE = 0;
        private const string SPI_CONTROLLER_NAME = "SPI0";
        private const byte MCP3008_CONFIG = 0x08;
        private SpiDevice spiAdc;
        private int adcValue;

        public LightSensor()
        {
            InitLight();
        }

        private async void InitLight()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                // 3.6MHz is the rated speed of the MCP3008 at 5v (1.35 MHz @ 2.7V)
                settings.ClockFrequency = 800000; // Set the clock frequency at or slightly below the specified rate speed
                                                  // The ADC expects idle-low clock polarity so we use Mode0
                settings.Mode = SpiMode.Mode0;
                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                spiAdc = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }
            catch (Exception ex)
            {
                throw new Exception("SPI initialization failed.", ex);
            }
        }

        public async Task getLight()
        {
            Debug.WriteLine("Getting Light.");

            await ReadAdcAsync();

        }

        private async Task ReadAdcAsync()
        {
            // Create a buffer to hold the read data
            byte[] readBuffer = new byte[3];
            byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };

            writeBuffer[0] = MCP3008_CONFIG;

            spiAdc.TransferFullDuplex(writeBuffer, readBuffer);
            adcValue = convertToInt(readBuffer);

            lightValue = adcValue.ToString();

            Debug.WriteLine("ADC VALUE: " + adcValue.ToString());

            await Task.Delay(1);
        }

        private int convertToInt(byte[] data)
        {
            int result = 0;

            result = data[1] & 0x03;
            result <<= 8;
            result += data[2];

            return result;
        }

        private double Map(int val, int inMin, int inMax, int outMin, int outMax)
        {
            return Math.Round((double)((val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin));
        }
    }
}
