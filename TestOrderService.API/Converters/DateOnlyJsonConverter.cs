using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace TestOrderService.API.Converters
{
    /// <summary>
    ///     JSON converter for DateOnly that uses MM/dd/yyyy format.
    /// </summary>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter&lt;System.DateOnly&gt;" />
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        /// <summary>
        ///     The date format
        /// </summary>
        private const string DateFormat = "MM/dd/yyyy";

        /// <summary>
        ///     Reads and converts the JSON to type <typeparamref name="T" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <exception cref="System.Text.Json.JsonException">
        ///     Date value cannot be null or empty.
        ///     or
        ///     Unable to parse '{value}' as a date. Excepted format: {DateFormat}.
        /// </exception>
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                throw new JsonException("Date value cannot be null or empty.");

            if (DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            throw new JsonException($"Unable to parse '{value}' as a date. Excepted format: {DateFormat}.");
        }

        /// <summary>
        ///     Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    ///     JSON converter for nullable DateOnly that uses MM/dd/yyyy format.
    /// </summary>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter&lt;System.DateOnly?&gt;" />
    public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        /// <summary>
        ///     The date format
        /// </summary>
        private const string DateFormat = "MM/dd/yyyy";

        /// <summary>
        ///     Reads and converts the JSON to type <typeparamref name="T" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>
        ///     The converted value.
        /// </returns>
        /// <exception cref="System.Text.Json.JsonException">Unable to parse '{value}' as a date. Excepted format: {DateFormat}.</exception>
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrEmpty(value))
                return null;

            if (DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            throw new JsonException($"Unable to parse '{value}' as a date. Excepted format: {DateFormat}.");
        }

        /// <summary>
        ///     Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
            else
                writer.WriteNullValue();
        }
    }
}
