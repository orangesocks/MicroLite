﻿// -----------------------------------------------------------------------
// <copyright file="UpdateSqlBuilder.cs" company="MicroLite">
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
namespace MicroLite.Builder
{
    using System;
    using MicroLite.Builder.Syntax.Write;
    using MicroLite.Characters;
    using MicroLite.FrameworkExtensions;
    using MicroLite.Mapping;

    [System.Diagnostics.DebuggerDisplay("{InnerSql}")]
    internal sealed class UpdateSqlBuilder : WriteSqlBuilderBase, IUpdate, ISetOrWhere
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="UpdateSqlBuilder"/> class with the starting command text 'UPDATE '.
        /// </summary>
        /// <param name="sqlCharacters">The SQL characters.</param>
        internal UpdateSqlBuilder(SqlCharacters sqlCharacters)
            : base(sqlCharacters)
        {
            this.InnerSql.Append("UPDATE ");
        }

        public ISetOrWhere SetColumnValue(string columnName, object columnValue)
        {
            if (this.Arguments.Count > 0)
            {
                this.InnerSql.Append(',');
            }

            this.InnerSql.Append(this.SqlCharacters.EscapeSql(columnName))
                .Append(" = ")
                .Append(this.SqlCharacters.GetParameterName(this.Arguments.Count));

            this.Arguments.Add(new SqlArgument(columnValue));

            return this;
        }

        public ISetOrWhere Table(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException(ExceptionMessages.ArgumentNullOrEmpty.FormatWith("tableName"));
            }

            this.AppendTableName(tableName);
            this.InnerSql.Append(" SET ");

            return this;
        }

        public ISetOrWhere Table(Type forType)
        {
            var objectInfo = ObjectInfo.For(forType);

            return this.Table(objectInfo);
        }

        internal ISetOrWhere Table(IObjectInfo objectInfo)
        {
            this.AppendTableName(objectInfo);
            this.InnerSql.Append(" SET ");

            return this;
        }
    }
}