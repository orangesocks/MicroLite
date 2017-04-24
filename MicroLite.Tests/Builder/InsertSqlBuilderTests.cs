﻿namespace MicroLite.Tests.Builder
{
    using System.Data;
    using MicroLite.Builder;
    using MicroLite.Characters;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="InsertSqlBuilder"/> class.
    /// </summary>
    public class InsertSqlBuilderTests : UnitTest
    {
        public InsertSqlBuilderTests()
        {
            UnitTest.SetConventionMapping(IdentifierStrategy.DbGenerated);
        }

        [Fact]
        public void InsertIntoColumnsValues()
        {
            var sqlBuilder = new InsertSqlBuilder(SqlCharacters.Empty);

            var sqlQuery = sqlBuilder
                .Into("Table")
                .Columns("Column1", "Column2")
                .Values("Foo", 12)
                .ToSqlQuery();

            Assert.Equal("INSERT INTO Table (Column1,Column2) VALUES (?,?)", sqlQuery.CommandText);

            Assert.Equal(2, sqlQuery.Arguments.Count);

            Assert.Equal(DbType.String, sqlQuery.Arguments[0].DbType);
            Assert.Equal("Foo", sqlQuery.Arguments[0].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[1].DbType);
            Assert.Equal(12, sqlQuery.Arguments[1].Value);
        }

        [Fact]
        public void InsertIntoColumnsValuesWithSqlCharacters()
        {
            var sqlBuilder = new InsertSqlBuilder(MsSqlCharacters.Instance);

            var sqlQuery = sqlBuilder
                .Into("Table")
                .Columns("Column1", "Column2")
                .Values("Foo", 12)
                .ToSqlQuery();

            Assert.Equal("INSERT INTO [Table] ([Column1],[Column2]) VALUES (@p0,@p1)", sqlQuery.CommandText);

            Assert.Equal(2, sqlQuery.Arguments.Count);

            Assert.Equal(DbType.String, sqlQuery.Arguments[0].DbType);
            Assert.Equal("Foo", sqlQuery.Arguments[0].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[1].DbType);
            Assert.Equal(12, sqlQuery.Arguments[1].Value);
        }

        [Fact]
        public void InsertIntoSpecifyingTableName()
        {
            var sqlBuilder = new InsertSqlBuilder(SqlCharacters.Empty);

            var sqlQuery = sqlBuilder
                .Into("Table")
                .ToSqlQuery();

            Assert.Equal("INSERT INTO Table", sqlQuery.CommandText);
            Assert.Empty(sqlQuery.Arguments);
        }

        [Fact]
        public void InsertIntoSpecifyingTableNameWithSqlCharacters()
        {
            var sqlBuilder = new InsertSqlBuilder(MsSqlCharacters.Instance);

            var sqlQuery = sqlBuilder
                .Into("Table")
                .ToSqlQuery();

            Assert.Equal("INSERT INTO [Table]", sqlQuery.CommandText);
            Assert.Empty(sqlQuery.Arguments);
        }

        [Fact]
        public void InsertIntoSpecifyingType()
        {
            var sqlBuilder = new InsertSqlBuilder(SqlCharacters.Empty);

            var sqlQuery = sqlBuilder
                .Into(typeof(Customer))
                .ToSqlQuery();

            Assert.Equal("INSERT INTO Sales.Customers", sqlQuery.CommandText);
            Assert.Empty(sqlQuery.Arguments);
        }

        [Fact]
        public void InsertIntoSpecifyingTypeWithSqlCharacters()
        {
            var sqlBuilder = new InsertSqlBuilder(MsSqlCharacters.Instance);

            var sqlQuery = sqlBuilder
                .Into(typeof(Customer))
                .ToSqlQuery();

            Assert.Equal("INSERT INTO [Sales].[Customers]", sqlQuery.CommandText);
            Assert.Empty(sqlQuery.Arguments);
        }
    }
}