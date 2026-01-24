using System.Text.Json;
using System.Text.Json.Serialization;
using Eroad.CQRS.Core.Events;
using Eroad.DeliveryTracking.Common;

namespace Eroad.DeliveryTracking.Query.Infrastructure.Converters
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
                nameof(DeliveryCreatedEvent) => JsonSerializer.Deserialize<DeliveryCreatedEvent>(json, newOptions),
                nameof(DeliveryStatusChangedEvent) => JsonSerializer.Deserialize<DeliveryStatusChangedEvent>(json, newOptions),
                nameof(CheckpointReachedEvent) => JsonSerializer.Deserialize<CheckpointReachedEvent>(json, newOptions),
                nameof(IncidentReportedEvent) => JsonSerializer.Deserialize<IncidentReportedEvent>(json, newOptions),
                nameof(IncidentResolvedEvent) => JsonSerializer.Deserialize<IncidentResolvedEvent>(json, newOptions),
                nameof(ProofOfDeliveryCapturedEvent) => JsonSerializer.Deserialize<ProofOfDeliveryCapturedEvent>(json, newOptions),
                _ => throw new JsonException($"{typeDiscriminator} is not supported yet!")
            };
        }

        public override void Write(Utf8JsonWriter writer, DomainEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
