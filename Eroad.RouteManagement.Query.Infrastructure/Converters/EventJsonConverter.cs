using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eroad.CQRS.Core.Events;
using Eroad.RouteManagement.Common;

namespace Eroad.RouteManagement.Query.Infrastructure.Converters
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
                nameof(RouteCreatedEvent) => JsonSerializer.Deserialize<RouteCreatedEvent>(json, newOptions),
                nameof(RouteUpdatedEvent) => JsonSerializer.Deserialize<RouteUpdatedEvent>(json, newOptions),
                nameof(RouteStatusChangedEvent) => JsonSerializer.Deserialize<RouteStatusChangedEvent>(json, newOptions),
                nameof(CheckpointAddedEvent) => JsonSerializer.Deserialize<CheckpointAddedEvent>(json, newOptions),
                nameof(CheckpointUpdatedEvent) => JsonSerializer.Deserialize<CheckpointUpdatedEvent>(json, newOptions),
                nameof(RouteScheduledEndTimeUpdatedEvent) => JsonSerializer.Deserialize<RouteScheduledEndTimeUpdatedEvent>(json, newOptions),
                _ => throw new JsonException($"{typeDiscriminator} is not supported yet!")
            };
        }

        public override void Write(Utf8JsonWriter writer, DomainEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

