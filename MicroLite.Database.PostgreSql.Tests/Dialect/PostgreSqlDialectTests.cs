﻿namespace MicroLite.Tests.Dialect
{
    using System;
    using System.Data;
    using MicroLite.Dialect;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="PostgreSqlDialect"/> class.
    /// </summary>
    public class PostgreSqlDialectTests : UnitTest
    {
        [Fact]
        public void BuildSelectInsertIdSqlQuery()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = sqlDialect.BuildSelectInsertIdSqlQuery(ObjectInfo.For(typeof(Customer)));

            Assert.Equal("SELECT lastval()", sqlQuery.CommandText);
            Assert.Equal(0, sqlQuery.Arguments.Count);
            Assert.Equal(0, sqlQuery.Arguments.Count);
        }

        [Fact]
        public void InsertInstanceQueryForIdentifierStrategyAssigned()
        {
            UnitTest.SetConventionMapping(IdentifierStrategy.Assigned);

            var sqlDialect = new PostgreSqlDialect();

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlQuery = sqlDialect.BuildInsertSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("INSERT INTO \"Sales\".\"Customers\" (\"Created\",\"CreditLimit\",\"DateOfBirth\",\"Id\",\"Name\",\"CustomerStatusId\",\"Website\") VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6)", sqlQuery.CommandText);
            Assert.Equal(7, sqlQuery.Arguments.Count);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[0].DbType);
            Assert.Equal(customer.Created, sqlQuery.Arguments[0].Value);

            Assert.Equal(DbType.Decimal, sqlQuery.Arguments[1].DbType);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[1].Value);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[2].DbType);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[2].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[3].DbType);
            Assert.Equal(customer.Id, sqlQuery.Arguments[3].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[4].DbType);
            Assert.Equal(customer.Name, sqlQuery.Arguments[4].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[5].DbType);
            Assert.Equal((int)customer.Status, sqlQuery.Arguments[5].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[6].DbType);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[6].Value);
        }

        [Fact]
        public void InsertInstanceQueryForIdentifierStrategyDbGenerated()
        {
            UnitTest.SetConventionMapping(IdentifierStrategy.DbGenerated);

            var sqlDialect = new PostgreSqlDialect();

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlQuery = sqlDialect.BuildInsertSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("INSERT INTO \"Sales\".\"Customers\" (\"Created\",\"CreditLimit\",\"DateOfBirth\",\"Name\",\"CustomerStatusId\",\"Website\") VALUES (@p0,@p1,@p2,@p3,@p4,@p5)", sqlQuery.CommandText);
            Assert.Equal(6, sqlQuery.Arguments.Count);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[0].DbType);
            Assert.Equal(customer.Created, sqlQuery.Arguments[0].Value);

            Assert.Equal(DbType.Decimal, sqlQuery.Arguments[1].DbType);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[1].Value);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[2].DbType);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[2].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[3].DbType);
            Assert.Equal(customer.Name, sqlQuery.Arguments[3].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[4].DbType);
            Assert.Equal((int)customer.Status, sqlQuery.Arguments[4].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[5].DbType);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[5].Value);
        }

        [Fact]
        public void PageNonQualifiedQuery()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers");

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers LIMIT @p0 OFFSET @p1", paged.CommandText);

            Assert.Equal(DbType.Int32, paged.Arguments[0].DbType);
            Assert.Equal(25, paged.Arguments[0].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(0, paged.Arguments[1].Value);
        }

        [Fact]
        public void PageNonQualifiedWildcardQuery()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT * FROM Customers");

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT * FROM Customers LIMIT @p0 OFFSET @p1", paged.CommandText);

            Assert.Equal(DbType.Int32, paged.Arguments[0].DbType);
            Assert.Equal(25, paged.Arguments[0].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(0, paged.Arguments[1].Value);
        }

        [Fact]
        public void PageQueryThrowsArgumentNullExceptionForNullSqlCharacters()
        {
            var sqlDialect = new PostgreSqlDialect();

            var exception = Assert.Throws<ArgumentNullException>(
                () => sqlDialect.PageQuery(null, PagingOptions.None));
        }

        [Fact]
        public void PageWithMultiWhereAndMultiOrderByMultiLine()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery(@"SELECT
 CustomerId,
 Name,
 DateOfBirth,
 CustomerStatusId
 FROM
 Customers
 WHERE
 (CustomerStatusId = @p0 AND DoB > @p1)
 ORDER BY
 Name ASC,
 DoB ASC", CustomerStatus.Active, new DateTime(1980, 01, 01));

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE (CustomerStatusId = @p0 AND DoB > @p1) ORDER BY Name ASC, DoB ASC LIMIT @p2 OFFSET @p3", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);
            Assert.Equal(sqlQuery.Arguments[1], paged.Arguments[1]);

            Assert.Equal(DbType.Int32, paged.Arguments[2].DbType);
            Assert.Equal(25, paged.Arguments[2].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[3].DbType);
            Assert.Equal(0, paged.Arguments[3].Value);
        }

        [Fact]
        public void PageWithNoWhereButOrderBy()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId ASC");

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers ORDER BY CustomerId ASC LIMIT @p0 OFFSET @p1", paged.CommandText);

            Assert.Equal(DbType.Int32, paged.Arguments[0].DbType);
            Assert.Equal(25, paged.Arguments[0].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(0, paged.Arguments[1].Value);
        }

        [Fact]
        public void PageWithNoWhereOrOrderByFirstResultsPage()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers");

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers LIMIT @p0 OFFSET @p1", paged.CommandText);

            Assert.Equal(DbType.Int32, paged.Arguments[0].DbType);
            Assert.Equal(25, paged.Arguments[0].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(0, paged.Arguments[1].Value);
        }

        [Fact]
        public void PageWithNoWhereOrOrderBySecondResultsPage()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers");

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 2, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers LIMIT @p0 OFFSET @p1", paged.CommandText);

            Assert.Equal(DbType.Int32, paged.Arguments[0].DbType);
            Assert.Equal(25, paged.Arguments[0].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(25, paged.Arguments[1].Value);
        }

        [Fact]
        public void PageWithWhereAndOrderBy()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC", CustomerStatus.Active);

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC LIMIT @p1 OFFSET @p2", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(25, paged.Arguments[1].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[2].DbType);
            Assert.Equal(0, paged.Arguments[2].Value);
        }

        [Fact]
        public void PageWithWhereAndOrderByMultiLine()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery(@"SELECT
 CustomerId,
 Name,
 DateOfBirth,
 CustomerStatusId
 FROM
 Customers
 WHERE
 CustomerStatusId = @p0
 ORDER BY
 Name ASC", CustomerStatus.Active);

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 ORDER BY Name ASC LIMIT @p1 OFFSET @p2", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(25, paged.Arguments[1].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[2].DbType);
            Assert.Equal(0, paged.Arguments[2].Value);
        }

        [Fact]
        public void PageWithWhereButNoOrderBy()
        {
            var sqlDialect = new PostgreSqlDialect();

            var sqlQuery = new SqlQuery("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0", CustomerStatus.Active);

            var paged = sqlDialect.PageQuery(sqlQuery, PagingOptions.ForPage(page: 1, resultsPerPage: 25));

            Assert.Equal("SELECT CustomerId, Name, DateOfBirth, CustomerStatusId FROM Customers WHERE CustomerStatusId = @p0 LIMIT @p1 OFFSET @p2", paged.CommandText);
            Assert.Equal(sqlQuery.Arguments[0], paged.Arguments[0]);

            Assert.Equal(DbType.Int32, paged.Arguments[1].DbType);
            Assert.Equal(25, paged.Arguments[1].Value);

            Assert.Equal(DbType.Int32, paged.Arguments[2].DbType);
            Assert.Equal(0, paged.Arguments[2].Value);
        }

        [Fact]
        public void SupportsSelectInsertedIdentifierReturnsTrue()
        {
            var sqlDialect = new PostgreSqlDialect();

            Assert.True(sqlDialect.SupportsSelectInsertedIdentifier);
        }

        [Fact]
        public void UpdateInstanceQuery()
        {
            UnitTest.SetConventionMapping(IdentifierStrategy.Assigned);

            var sqlDialect = new PostgreSqlDialect();

            var customer = new Customer
            {
                Created = new DateTime(2011, 12, 24),
                CreditLimit = 10500.00M,
                DateOfBirth = new System.DateTime(1975, 9, 18),
                Id = 134875,
                Name = "Joe Bloggs",
                Status = CustomerStatus.Active,
                Updated = DateTime.Now,
                Website = new Uri("http://microliteorm.wordpress.com")
            };

            var sqlQuery = sqlDialect.BuildUpdateSqlQuery(ObjectInfo.For(typeof(Customer)), customer);

            Assert.Equal("UPDATE \"Sales\".\"Customers\" SET \"CreditLimit\" = @p0,\"DateOfBirth\" = @p1,\"Name\" = @p2,\"CustomerStatusId\" = @p3,\"Updated\" = @p4,\"Website\" = @p5 WHERE (\"Id\" = @p6)", sqlQuery.CommandText);
            Assert.Equal(7, sqlQuery.Arguments.Count);

            Assert.Equal(DbType.Decimal, sqlQuery.Arguments[0].DbType);
            Assert.Equal(customer.CreditLimit, sqlQuery.Arguments[0].Value);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[1].DbType);
            Assert.Equal(customer.DateOfBirth, sqlQuery.Arguments[1].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[2].DbType);
            Assert.Equal(customer.Name, sqlQuery.Arguments[2].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[3].DbType);
            Assert.Equal((int)customer.Status, sqlQuery.Arguments[3].Value);

            Assert.Equal(DbType.DateTime2, sqlQuery.Arguments[4].DbType);
            Assert.Equal(customer.Updated, sqlQuery.Arguments[4].Value);

            Assert.Equal(DbType.String, sqlQuery.Arguments[5].DbType);
            Assert.Equal("http://microliteorm.wordpress.com/", sqlQuery.Arguments[5].Value);

            Assert.Equal(DbType.Int32, sqlQuery.Arguments[6].DbType);
            Assert.Equal(customer.Id, sqlQuery.Arguments[6].Value);
        }
    }
}