// -----------------------------------------------------------------------
// <copyright file="ObjectInfoExtensions.cs" company="MicroLite">
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
    using System.IO;
    using MicroLite.TypeConverters;

    /// <summary>
    /// Extension methods for <see cref="IObjectInfo"/>.
    /// </summary>
    public static class ObjectInfoExtensions
    {
        /// <summary>
        /// Emits the mappings for the specified <see cref="IObjectInfo"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="objectInfo">The object information to emit.</param>
        /// <param name="textWriter">The text writer to write to.</param>
        public static void EmitMappings(this IObjectInfo objectInfo, TextWriter textWriter)
        {
            if (objectInfo == null)
            {
                throw new ArgumentNullException(nameof(objectInfo));
            }

            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            textWriter.WriteLine("MicroLite Mapping:");
            textWriter.WriteLine("------------------");
            textWriter.Write("Class '{0}' mapped to Table '{1}'", objectInfo.ForType.FullName, objectInfo.TableInfo.Name);

            if (objectInfo.TableInfo.Schema != null)
            {
                textWriter.WriteLine(" in Schema '{0}'", objectInfo.TableInfo.Schema);
            }
            else
            {
                textWriter.WriteLine();
            }

            textWriter.WriteLine();

            foreach (var columnInfo in objectInfo.TableInfo.Columns)
            {
                var actualPropertyType = TypeConverter.ResolveActualType(columnInfo.PropertyInfo.PropertyType);

                textWriter.WriteLine(
                    "Property '{0} ({1})' mapped to Column '{2} (DbType.{3})'",
                    columnInfo.PropertyInfo.Name,
                    actualPropertyType == columnInfo.PropertyInfo.PropertyType ? actualPropertyType.FullName : actualPropertyType.FullName + "?",
                    columnInfo.ColumnName,
                    columnInfo.DbType.ToString());

                textWriter.WriteLine("\tAllow Insert: {0}", columnInfo.AllowInsert.ToString());
                textWriter.WriteLine("\tAllow Update: {0}", columnInfo.AllowUpdate.ToString());
                textWriter.WriteLine("\tIs Identifier: {0}", columnInfo.IsIdentifier.ToString());

                if (columnInfo.IsIdentifier)
                {
                    textWriter.WriteLine("\tIdentifier Strategy: {0}", objectInfo.TableInfo.IdentifierStrategy.ToString());
                }

                if (columnInfo.SequenceName != null)
                {
                    textWriter.WriteLine("\tSequence Name: {0}", columnInfo.SequenceName);
                }

                textWriter.WriteLine();
            }
        }

        /// <summary>
        /// Emits the mappings for the specified <see cref="IObjectInfo"/> to the <see cref="Console"/>.
        /// </summary>
        /// <param name="objectInfo">The object information to emit.</param>
        public static void EmitMappingsToConsole(this IObjectInfo objectInfo)
        {
            if (Console.Out != null)
            {
                EmitMappings(objectInfo, Console.Out);
            }
        }
    }
}