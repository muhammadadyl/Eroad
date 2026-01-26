using System.Text.Json;
using System.Text.Json.Serialization;
using Eroad.CQRS.Core.Events;
using Eroad.FleetManagement.Common;

namespace Eroad.FleetManagement.Query.Infrastructure.Converters
{
    public class EventJsonConverter : JsonConverter<DomainEvent>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableTo(typeof(DomainEvent));
        }

        public override DomainEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!JsonDocument.TryParseValue(ref reader, out var doc))
            {
                throw new JsonException($"Failed to parse {nameof(JsonDocument)}");
            }

            if (!doc.RootElement.TryGetProperty("Type", out var type))
            {
                throw new JsonException("Could not detect the Type discriminator property!");
            }

            var typeDiscriminator = type.GetString();
            var json = doc.RootElement.GetRawText();

            // Create new options without the converter to avoid infinite recursion
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();

            return typeDiscriminator switch
            {
                nameof(DriverAddedEvent) => JsonSerializer.Deserialize<DriverAddedEvent>(json, newOptions),
                nameof(DriverUpdatedEvent) => JsonSerializer.Deserialize<DriverUpdatedEvent>(json, newOptions),
                nameof(DriverStatusChangedEvent) => JsonSerializer.Deserialize<DriverStatusChangedEvent>(json, newOptions),
                nameof(VehicleAddedEvent) => JsonSerializer.Deserialize<VehicleAddedEvent>(json, newOptions),
                nameof(VehicleUpdatedEvent) => JsonSerializer.Deserialize<VehicleUpdatedEvent>(json, newOptions),
                nameof(VehicleStatusChangedEvent) => JsonSerializer.Deserialize<VehicleStatusChangedEvent>(json, newOptions),
                _ => throw new JsonException($"{typeDiscriminator} is not supported yet!")
            };
        }

        public override void Write(Utf8JsonWriter writer, DomainEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
