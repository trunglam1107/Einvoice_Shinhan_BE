using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Utils
{
    public class JsonBoolConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Convert.ToBoolean(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToString(value));
            writer.Flush();
        }
    }
}