using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quarter.Core.Models;

namespace Quarter.Core.Utils;

public class IdOfJsonConverter<T> : JsonConverter<IdOf<T>>
    where T : IAggregate<T>
{
    public override IdOf<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value == null
            ? null
            : IdOf<T>.Of(value);
    }

    public override void Write(Utf8JsonWriter writer, IdOf<T> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.AsString());
    }
}