
namespace Organization.Shared.Helpers;

public static class JsonHelpers
{
/// <summary>
    /// De serialize json to objects
    /// </summary>
    /// <typeparam name="T">Type to return</typeparam>
    /// <param name="json">json to serialize</param>
    /// <returns>Objects of type T</returns>
    public static T? JsonDeSerialize<T>(string json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<T>(json, options);
    }
    /// <summary>
    /// De serialize json to objects
    /// </summary>
    /// <typeparam name="T">Type to return</typeparam>
    /// <param name="json">json to serialize</param>
    /// <returns>Objects of type T</returns>
    public static T? JsonDeSerialize<T>(byte[] json)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Deserialize<T>(json, options);
    }
    /// <summary>
    /// Serialize objects to Json.
    /// </summary>
    /// <typeparam name="T">Type to serialize</typeparam>
    /// <param name="value">The value to serialize</param>
    /// <returns>Json text</returns>
    public static string JsonSerializeToString<T>(T value)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return JsonSerializer.Serialize<T>(value, options);
    }
    /// <summary>
    /// Serialize objects to Json and return in byte array.
    /// </summary>
    /// <typeparam name="T">Type to serialize</typeparam>
    /// <param name="value">The value to serialize</param>
    /// <returns>Json as bytes</returns>
    public static byte[] JsonSerializeToBytes<T>(T value)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(value, options));
    }

    /// <summary>
    /// Convert JsonElement to Type
    /// </summary>
    /// <param name="value">Value to convert</param>
    /// <returns>Converted value.</returns>
    /// <exception cref="NotImplementedException">Type not found in switch statement.</exception>
    public static T JsonToType<T>(JsonElement value)
    {
        return typeof(T) switch
        {
            Type a when typeof(double) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetDouble(), typeof(T)),
            Type a when typeof(long) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetInt64(), typeof(T)),
            Type a when typeof(ulong) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetUInt64(), typeof(T)),
            Type a when typeof(decimal) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetDecimal(), typeof(T)),
            Type a when typeof(float) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetSingle(), typeof(T)),
            Type a when typeof(Int16) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetInt16(), typeof(T)),
            Type a when typeof(Int32) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetInt32(), typeof(T)),
            Type a when typeof(Int64) == typeof(T) && value.ValueKind == JsonValueKind.Number => (T)Convert.ChangeType(value.GetInt64(), typeof(T)),
            Type a when typeof(string) == typeof(T) && value.ValueKind == JsonValueKind.String => (T)Convert.ChangeType(value.ToString() ?? "", typeof(T)),
            _ => throw new NotImplementedException($"{nameof(JsonHelpers)}.{nameof(JsonToType)} Type {typeof(T)} not found in switch statement.")
        };
    }
}
