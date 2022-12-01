using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Utils
{
    public class JsonIntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToInt32(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToString(value));
            writer.Flush();
        }
    }
}
