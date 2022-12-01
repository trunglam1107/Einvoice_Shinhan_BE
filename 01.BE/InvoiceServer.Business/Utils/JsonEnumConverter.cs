using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Utils
{
    public class JsonEnumConverter<T> : JsonConverter where T : struct
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (T)Enum.Parse(typeof(T), reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((T)Enum.Parse(typeof(T), value.ToString()));
            writer.Flush();
        }
    }
}
