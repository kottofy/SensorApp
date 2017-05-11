using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorApp
{
    class SensorMessage
    {
        public string DeviceId { get; set; }
        public string SensorType { get; set; }
        public string SensorValue { get; set; }
        public DateTime Time { get; set; }

        public SensorMessage(string deviceId, string sensorType, string sensorValue)
        {
            DeviceId = deviceId;
            SensorType = sensorType;
            SensorValue = sensorValue;
        }

        public string JSONify()
        {
            this.Time = DateTime.Now;

            Dictionary<string, string> json_message = new Dictionary<string, string>();
            json_message.Add("DeviceId", this.DeviceId);
            json_message.Add("SensorType", this.SensorType);
            json_message.Add("SensorValue", this.SensorValue);
            json_message.Add("OutputTime", this.Time.ToString());
            return JsonConvert.SerializeObject(json_message);
        }
    }
}
