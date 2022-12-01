using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace InvoiceServer.Business.Utils
{
    public class JsonDateTimeConverterString : DateTimeConverterBase
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || string.IsNullOrWhiteSpace(reader.Value.ToString())) return null;
            return DateTime.ParseExact(reader.Value.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dateConvert = Convert.ToDateTime(value);
            if (dateConvert != default(DateTime))
            {
                writer.WriteValue(dateConvert.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                writer.WriteValue("");
            }

            writer.Flush();
        }
    }
}