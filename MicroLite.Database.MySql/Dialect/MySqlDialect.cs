﻿// -----------------------------------------------------------------------
// <copyright file="MySqlDialect.cs" company="MicroLite">
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
namespace MicroLite.Dialect
{
    using System;
    using System.Data;
    using System.Text;
    using MicroLite.Characters;
    using MicroLite.Mapping;

    /// <summary>
    /// The implementation of <see cref="ISqlDialect"/> for MySql server.
    /// </summary>
    internal sealed class MySqlDialect : SqlDialect
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MySqlDialect"/> class.
        /// </summary>
        internal MySqlDialect()
            : base(MySqlCharacters.Instance)
        {
        }

        public override bool SupportsSelectInsertedIdentifier
        {
            get
            {
                return true;
            }
        }

        public override SqlQuery BuildSelectInsertIdSqlQuery(IObjectInfo objectInfo)
        {
            return new SqlQuery("SELECT LAST_INSERT_ID()");
        }

        public override SqlQuery PageQuery(SqlQuery sqlQuery, PagingOptions pagingOptions)
        {
            if (sqlQuery == null)
            {
                throw new ArgumentNullException(nameof(sqlQuery));
            }

            var arguments = new SqlArgument[sqlQuery.Arguments.Count + 2];
            Array.Copy(sqlQuery.ArgumentsArray, 0, arguments, 0, sqlQuery.Arguments.Count);
            arguments[arguments.Length - 2] = new SqlArgument(pagingOptions.Offset, DbType.Int32);
            arguments[arguments.Length - 1] = new SqlArgument(pagingOptions.Count, DbType.Int32);

            var stringBuilder = new StringBuilder(sqlQuery.CommandText)
                .Replace(Environment.NewLine, string.Empty)
                .Append(" LIMIT ")
                .Append(this.SqlCharacters.GetParameterName(arguments.Length - 2))
                .Append(',')
                .Append(this.SqlCharacters.GetParameterName(arguments.Length - 1));

            return new SqlQuery(stringBuilder.ToString(), arguments);
        }
    }
}