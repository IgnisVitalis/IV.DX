using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application;

public sealed class DXMigrationParser : IDXMigrationParser
{
    private readonly JsonSerializer _serializer;

    public DXMigrationParser()
    {
        _serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            DateParseHandling = DateParseHandling.DateTime,
            NullValueHandling = NullValueHandling.Include
        });
    }

    public IReadOnlyList<DXParsedItem> ParseFile(string path)
        => Parse(File.ReadAllText(path));

    public IReadOnlyList<DXParsedItem> Parse(string json)
    {
        var root = JArray.Parse(json);
        var result = new List<DXParsedItem>();

        foreach (var token in root)
        {
            var meta = token["Meta"]!.ToObject<DXMeta>(_serializer)!;
            var data = token["Data"];

            switch (meta.Kind)
            {
                case "DXUnit":
                    result.Add(ParseDXUnit(meta, data));
                    break;

                case "DXElement":
                    result.Add(ParseDXElement(meta, data));
                    break;

                default:
                    throw new InvalidOperationException($"Unknown Meta.Kind: {meta.Kind}");
            }
        }

        return result;
    }

    private DXParsedItem ParseDXUnit(DXMeta meta, JToken? data)
    {
        return meta.Cardinality switch
        {
            "One" or "ZeroOrOne" => new DXParsedItem
            {
                Meta = meta,
                DXUnitObject = data?.Type == JTokenType.Null
                    ? null
                    : data!.ToObject<DXUnitObject>(_serializer)
            },

            "Many" or "OneOrMore" => new DXParsedItem
            {
                Meta = meta,
                DXUnitBatch = data!.ToObject<DXUnitBatch>(_serializer)
            },

            _ => throw new InvalidOperationException($"Invalid Cardinality for DXUnit: {meta.Cardinality}")
        };
    }

    private DXParsedItem ParseDXElement(DXMeta meta, JToken? data)
    {
        return meta.Cardinality switch
        {
            "One" or "ZeroOrOne" => new DXParsedItem
            {
                Meta = meta,
                DXElementObject = data?.Type == JTokenType.Null
                    ? null
                    : data!.ToObject<DXElementObject>(_serializer)
            },

            "Many" or "OneOrMore" => new DXParsedItem
            {
                Meta = meta,
                DXElementBatch = data!.ToObject<DXElementBatch>(_serializer)
            },

            _ => throw new InvalidOperationException($"Invalid Cardinality for DXElement: {meta.Cardinality}")
        };
    }
}