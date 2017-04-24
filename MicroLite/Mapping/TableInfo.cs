﻿// -----------------------------------------------------------------------
// <copyright file="TableInfo.cs" company="MicroLite">
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
    using System.Collections.ObjectModel;
    using System.Linq;
    using MicroLite.FrameworkExtensions;

    /// <summary>
    /// A class which contains information about a database table which a class is mapped to.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Schema}.{Name}")]
    public sealed class TableInfo
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="TableInfo"/> class.
        /// </summary>
        /// <param name="columns">The columns that are mapped for the table.</param>
        /// <param name="identifierStrategy">The identifier strategy used by the table.</param>
        /// <param name="name">The name of the table.</param>
        /// <param name="schema">The database schema the table exists within (e.g. 'dbo'); otherwise null.</param>
        /// <exception cref="ArgumentNullException">Thrown if columns or name are null.</exception>
        /// <exception cref="MappingException">Thrown if no there is a problem with the column mappings.</exception>
        public TableInfo(
            IList<ColumnInfo> columns,
            IdentifierStrategy identifierStrategy,
            string name,
            string schema)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Columns = new ReadOnlyCollection<ColumnInfo>(columns);
            this.IdentifierStrategy = identifierStrategy;
            this.Name = name;
            this.Schema = schema;

            this.IdentifierColumn = columns.FirstOrDefault(c => c.IsIdentifier);

            this.InsertColumnCount = columns.Count(c => c.AllowInsert);
            this.UpdateColumnCount = columns.Count(c => c.AllowUpdate);

            this.ValidateColumns();
        }

        /// <summary>
        /// Gets the columns that are mapped for the table.
        /// </summary>
        public IReadOnlyList<ColumnInfo> Columns
        {
            get;
        }

        /// <summary>
        /// Gets the ColumnInfo of the column that is the table identifier column (primary key).
        /// </summary>
        public ColumnInfo IdentifierColumn
        {
            get;
        }

        /// <summary>
        /// Gets the identifier strategy used by the table.
        /// </summary>
        public IdentifierStrategy IdentifierStrategy
        {
            get;
        }

        /// <summary>
        /// Gets the number of columns which can be inserted.
        /// </summary>
        public int InsertColumnCount
        {
            get;
        }

        /// <summary>
        /// Gets the database schema the table exists within (e.g. 'dbo'); otherwise null.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the name of the schema the table exists within.
        /// </summary>
        public string Schema
        {
            get;
        }

        /// <summary>
        /// Gets the number of columns which can be updated.
        /// </summary>
        public int UpdateColumnCount
        {
            get;
        }

        private void ValidateColumns()
        {
            var duplicatedColumn = this.Columns
                .GroupBy(c => c.ColumnName)
                .Select(x => new
                {
                    Key = x.Key,
                    Count = x.Count()
                })
                .FirstOrDefault(x => x.Count > 1);

            if (duplicatedColumn != null)
            {
                throw new MappingException(ExceptionMessages.TableInfo_ColumnMappedMultipleTimes.FormatWith(duplicatedColumn.Key));
            }

            if (this.Columns.Count(c => c.IsIdentifier) > 1)
            {
                throw new MappingException(ExceptionMessages.TableInfo_MultipleIdentifierColumns.FormatWith(this.Schema, this.Name));
            }

            if (this.IdentifierStrategy == Mapping.IdentifierStrategy.Sequence
                && this.IdentifierColumn != null
                && string.IsNullOrEmpty(this.IdentifierColumn.SequenceName))
            {
                throw new MappingException(ExceptionMessages.TableInfo_SequenceNameNotSet.FormatWith(this.IdentifierColumn.ColumnName));
            }
        }
    }
}