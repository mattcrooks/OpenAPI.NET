﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Exceptions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using SharpYaml.Serialization;

namespace Microsoft.OpenApi.Readers.ParseNodes
{
    internal abstract class ParseNode
    {
        protected ParseNode(ParsingContext parsingContext, OpenApiDiagnostic diagnostic)
        {
            Context = parsingContext;
            Diagnostic = diagnostic;
        }

        public ParsingContext Context { get; }

        public OpenApiDiagnostic Diagnostic { get; }

        public string DomainType { get; internal set; }

        public MapNode CheckMapNode(string nodeName)
        {
            var mapNode = this as MapNode;
            if (mapNode == null)
            {
                Diagnostic.Errors.Add(
                    new OpenApiError("", $"{nodeName} must be a map/object at " + Context.GetLocation()));
            }

            return mapNode;
        }

        internal string CheckRegex(string value, Regex versionRegex, string defaultValue)
        {
            if (!versionRegex.IsMatch(value))
            {
                Diagnostic.Errors.Add(new OpenApiError("", "Value does not match regex: " + versionRegex));
                return defaultValue;
            }

            return value;
        }

        public static ParseNode Create(ParsingContext context, OpenApiDiagnostic diagnostic, YamlNode node)
        {
            var listNode = node as YamlSequenceNode;

            if (listNode != null)
            {
                return new ListNode(context, diagnostic, listNode);
            }

            var mapNode = node as YamlMappingNode;
            if (mapNode != null)
            {
                return new MapNode(context, diagnostic, mapNode);
            }

            return new ValueNode(context, diagnostic, node as YamlScalarNode);
        }

        public virtual List<T> CreateList<T>(Func<MapNode, T> map)
        {
            throw new OpenApiException("Cannot create list");
        }

        public virtual Dictionary<string, T> CreateMap<T>(Func<MapNode, T> map)
        {
            throw new OpenApiException("Cannot create map");
        }

        public virtual Dictionary<string, T> CreateMapWithReference<T>(
            ReferenceType referenceType,
            Func<MapNode, T> map)
            where T : class, IOpenApiReferenceable
        {
            throw new OpenApiException("Cannot create map from reference");
        }

        public virtual List<T> CreateSimpleList<T>(Func<ValueNode, T> map)
        {
            throw new OpenApiException("Cannot create simple list");
        }

        public virtual Dictionary<string, T> CreateSimpleMap<T>(Func<ValueNode, T> map)
        {
            throw new OpenApiException("Cannot create simple map");
        }

        /// <summary>
        /// Create a <see cref="IOpenApiAny"/>
        /// </summary>
        /// <returns></returns>
        public virtual IOpenApiAny CreateAny()
        {
            throw new NotSupportedException();
        }

        public virtual string GetRaw()
        {
            throw new OpenApiException("Cannot get raw value");
        }

        public virtual string GetScalarValue()
        {
            throw new OpenApiException("Cannot get scalar value");
        }
    }
}