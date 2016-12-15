using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorApp
{
    class TempSensor
    {
        BMP280 BMP280;
        public string temperature;
        public string pressure;
        public string altitude;


        public TempSensor()
        {
            InitTemp();
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

        public async Task getTemp()
        {
            try
            {
                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
                float temp = 0;
                float prssre = 0;
                float alt = 0;

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1013.25f;

                temp = await BMP280.ReadTemperature();
                prssre = await BMP280.ReadPreasure();
                alt = await BMP280.ReadAltitude(seaLevelPressure);

                //Write the values to your debug console
                temperature = temp.ToString() + " deg C";
                pressure = prssre.ToString() + " Pa";
                altitude = alt.ToString() + " m";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
