﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Remote.Linq.Newtonsoft.Json.Converters
{
    using Aqua.Newtonsoft.Json.Converters;
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using Remote.Linq.DynamicQuery;
    using System.Collections.Generic;

    public sealed class VariableQueryArgumentConverter : ObjectConverter<VariableQueryArgument>
    {
        protected override void ReadObjectProperties(JsonReader reader, VariableQueryArgument result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            reader.CheckNotNull(nameof(reader)).AssertProperty(nameof(VariableQueryArgument.Type));
            var typeInfo = reader.Read<TypeInfo>(serializer) ?? throw reader.CreateException($"{nameof(VariableQueryArgument.Type)} must not be null.");

            reader.AssertProperty(nameof(VariableQueryArgument.Value));
            var value = reader.Read(typeInfo, serializer);

            reader.AssertEndObject();

            result.CheckNotNull(nameof(result)).Type = typeInfo;
            result.Value = value;
        }

        protected override void WriteObjectProperties(JsonWriter writer, VariableQueryArgument instance, IReadOnlyCollection<Property> properties, JsonSerializer serializer)
        {
            writer.CheckNotNull(nameof(writer)).WritePropertyName(nameof(VariableQueryArgument.Type));
            serializer.CheckNotNull(nameof(serializer)).Serialize(writer, instance.CheckNotNull(nameof(instance)).Type);

            writer.WritePropertyName(nameof(VariableQueryArgument.Value));
            serializer.Serialize(writer, instance.Value, instance.Type?.Type);
        }
    }
}