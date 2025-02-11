﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Connectors
{
    [DebuggerDisplay("{ConnectorType}")]
    public class ConnectorSchema : SupportsConnectorErrors
    {
        public ConnectorType ConnectorType { get; }

        public FormulaValue DefaultValue { get; }

        internal OpenApiSchema Schema { get; }

        internal ConnectorExtensions ConnectorExtensions { get; }

        private bool UseHiddenTypes { get; }

        public string Title => Schema.Title;

        public FormulaType FormulaType => UseHiddenTypes ? ConnectorType.HiddenRecordType : ConnectorType.FormulaType;

        internal RecordType HiddenRecordType => ConnectorType.HiddenRecordType;

        public string Summary => ConnectorExtensions.Summary;

        public bool SupportsDynamicIntellisense => ConnectorType.SupportsDynamicIntellisense;
        
        internal ConnectorSchema(OpenApiParameter openApiParameter, IOpenApiExtensible bodyExtensions, bool useHiddenTypes, ConnectorCompatibility compatibility)
        {
            Schema = openApiParameter.Schema;
            UseHiddenTypes = useHiddenTypes;
            ConnectorType = AggregateErrors(openApiParameter.GetConnectorType(compatibility));
            DefaultValue = openApiParameter.Schema.TryGetDefaultValue(FormulaType, out FormulaValue defaultValue, this) && defaultValue is not BlankValue ? defaultValue : null;
            ConnectorExtensions = new ConnectorExtensions(openApiParameter, bodyExtensions);
        }

        internal ConnectorSchema(ConnectorSchema connectorSchema, ConnectorType connectorType)
        {
            Schema = connectorSchema.Schema;
            DefaultValue = connectorSchema.DefaultValue;
            ConnectorType = AggregateErrors(connectorType ?? connectorSchema.ConnectorType);
            ConnectorExtensions = connectorSchema.ConnectorExtensions;
            AggregateErrors(connectorSchema);
            AggregateErrors(connectorType);
        }
    }
}
