using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace IV.DX.Kernel.Models
{
    public sealed class DXDataBlock<TRecord>
    {
        public DXMeta Meta { get; set; } = new DXMeta();
        public DXData<TRecord> Data { get; set; } = new DXData<TRecord>();
    }

    public sealed class DXMeta
    {
        public string Kind { get; set; } = null!;       // "DXUnit" | "DXElement" | "DXEnum" | others
        public string? Type { get; set; }               // concrete type name (optional for DXEnum rows)

        public string? Op { get; set; }                 // "Sync" | "Patch"
        public bool? IsMulti { get; set; }              // replaces Cardinality
        public bool? IsRequired { get; set; }           // replaces Cardinality

        public string? DXFilter { get; set; }           // selection filter (dxsql)
        public string? DXUnitContext { get; set; }      // for standalone DXElement
    }

    public sealed class DXData<TRecord>
    {
        [JsonConverter(typeof(DXSingleOrArrayConverter))]
        public List<TRecord>? Items { get; set; }

        [JsonConverter(typeof(DXSingleOrArrayConverter))]
        public List<DXDeleteRef>? Delete { get; set; }
    }

    public abstract class DXObjectRecord
    {
        public Guid ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? DXTitle { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken>? Fields { get; set; }
    }

    public sealed class DXUnitRecord : DXObjectRecord
    {
        public Dictionary<string, DXDataBlock<DXElementRecord>>? DXElements { get; set; }
    }

    public sealed class DXElementRecord : DXObjectRecord
    {
        public Guid DXUnitID { get; set; }
    }

    public sealed class DXEnumRecord : DXObjectRecord
    {
        public string? Type { get; set; }
        public JToken? Key { get; set; }
        public JToken? Value { get; set; }
    }

    public sealed class DXDeleteRef
    {
        public Guid ID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken>? Fields { get; set; }
    }

    // Accepts either a single object or an array for Items/Delete.
    public sealed class DXSingleOrArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(List<>);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject(objectType, serializer);
            }

            var elementType = objectType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(objectType)!;
            var item = token.ToObject(elementType, serializer);

            if (item != null)
            {
                list.Add(item);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            serializer.Serialize(writer, value);
        }
    }
}
