﻿// -----------------------------------------------------------------------
// <copyright file="ObjectInfo.cs" company="MicroLite">
// Copyright 2012 - 2016 Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
namespace MicroLite.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using MicroLite.FrameworkExtensions;
    using MicroLite.Logging;

    /// <summary>
    /// The class which describes a type and the table it is mapped to.
    /// </summary>
    public static class ObjectInfo
    {
        private static readonly ILog log = LogManager.GetCurrentClassLog();
        private static IMappingConvention mappingConvention;
        private static IDictionary<Type, IObjectInfo> objectInfos = GetObjectInfos();

        internal static IMappingConvention MappingConvention
        {
            get
            {
                if (mappingConvention == null)
                {
                    mappingConvention = mappingConvention = new ConventionMappingConvention(ConventionMappingSettings.Default);
                }

                return mappingConvention;
            }

            set
            {
                mappingConvention = value;
            }
        }

        /// <summary>
        /// Gets the object info for the specified type.
        /// </summary>
        /// <param name="forType">The type to get the object info for.</param>
        /// <returns>The <see cref="ObjectInfo" /> for the specified <see cref="Type" />.</returns>
        /// <exception cref="ArgumentNullException">Thrown if forType is null.</exception>
        /// <exception cref="MicroLiteException">Thrown if the specified type cannot be used with MicroLite.</exception>
        public static IObjectInfo For(Type forType)
        {
            if (forType == null)
            {
                throw new ArgumentNullException(nameof(forType));
            }

            if (log.IsDebug)
            {
                log.Debug(LogMessages.ObjectInfo_RetrievingObjectInfo, forType.FullName);
            }

            IObjectInfo objectInfo;

            if (!objectInfos.TryGetValue(forType, out objectInfo))
            {
                if (forType.IsGenericType)
                {
                    var forGenericType = forType.GetGenericTypeDefinition();

                    if (objectInfos.TryGetValue(forGenericType, out objectInfo))
                    {
                        return objectInfo;
                    }
                }

                VerifyType(forType);

                if (log.IsDebug)
                {
                    log.Debug(LogMessages.ObjectInfo_CreatingObjectInfo, forType.FullName);
                }

                objectInfo = MappingConvention.CreateObjectInfo(forType);

                if (log.IsDebug)
                {
                    using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        objectInfo.EmitMappings(stringWriter);
                        log.Debug(stringWriter.ToString());
                    }
                }

                var newObjectInfos = new Dictionary<Type, IObjectInfo>(objectInfos)
                {
                    [forType] = objectInfo
                };

                objectInfos = newObjectInfos;
            }

            return objectInfo;
        }

        private static Dictionary<Type, IObjectInfo> GetObjectInfos()
        {
            var expandoObjectInfo = new ExpandoObjectInfo();
            var tupleObjectInfo = new TupleObjectInfo();

            var dictionary = new Dictionary<Type, IObjectInfo>()
            {
                [typeof(System.Dynamic.ExpandoObject)] = expandoObjectInfo,
                [typeof(object)] = expandoObjectInfo, // If the generic argument <dynamic> is used (in ISession.Fetch for example), typeof(T) will return object.

                [typeof(Tuple<>)] = tupleObjectInfo,
                [typeof(Tuple<,>)] = tupleObjectInfo,
                [typeof(Tuple<,,>)] = tupleObjectInfo,
                [typeof(Tuple<,,,>)] = tupleObjectInfo,
                [typeof(Tuple<,,,,>)] = tupleObjectInfo,
                [typeof(Tuple<,,,,,>)] = tupleObjectInfo,
                [typeof(Tuple<,,,,,,>)] = tupleObjectInfo
            };

            return dictionary;
        }

        private static void VerifyType(Type forType)
        {
            if (!forType.IsClass)
            {
                throw new MappingException(ExceptionMessages.ObjectInfo_TypeMustBeClass.FormatWith(forType.Name));
            }

            if (forType.IsAbstract)
            {
                throw new MappingException(ExceptionMessages.ObjectInfo_TypeMustNotBeAbstract.FormatWith(forType.Name));
            }

            if (!forType.IsPublic && !forType.IsNestedPublic)
            {
                throw new MappingException(ExceptionMessages.ObjectInfo_TypeMustBePublic.FormatWith(forType.Name));
            }

            if (forType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new MappingException(ExceptionMessages.ObjectInfo_TypeMustHaveDefaultConstructor.FormatWith(forType.Name));
            }
        }
    }
}