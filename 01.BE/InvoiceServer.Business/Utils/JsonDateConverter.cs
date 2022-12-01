using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace InvoiceServer.Business.Utils
{
    public class JsonDateConverter : DateTimeConverterBase
    {
        private static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(reader.Value.ToString())) return null;
            var timeComvert = Convert.ToDouble(reader.Value);
            return BaseTime.AddMilliseconds(timeComvert);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TimeSpan diff = DateTime.SpecifyKind(Convert.ToDateTime(value), DateTimeKind.Utc) - BaseTime;
            writer.WriteValue(Convert.ToString(Math.Floor(diff.TotalMilliseconds)));
            writer.Flush();
        }
    }
}