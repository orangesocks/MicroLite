﻿namespace MicroLite.Tests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using MicroLite.Characters;
    using MicroLite.Core;
    using MicroLite.Dialect;
    using MicroLite.Driver;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="AsyncReadOnlySession"/> class.
    /// </summary>
    public class AsyncReadOnlySessionTests : UnitTest
    {
        public AsyncReadOnlySessionTests()
        {
            UnitTest.SetConventionMapping(IdentifierStrategy.DbGenerated);
        }

        [Fact]
        public void AdvancedReturnsSameSessionByDifferentInterface()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            var advancedSession = session.Advanced;

            Assert.Same(session, advancedSession);
        }

        [Fact]
        public async void AllCreatesASelectAllQueryExecutesAndReturnsResults()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.SqlCharacters).Returns(SqlCharacters.Empty);

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customers = session.Include.All<Customer>();

            await session.ExecutePendingQueriesAsync();

            Assert.Equal(1, customers.Values.Count);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void FetchExecutesAndReturnsResults()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customers = await session.FetchAsync<Customer>(new SqlQuery(""));

            Assert.Equal(1, customers.Count);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void FetchThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await session.FetchAsync<Customer>(null));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public async void FetchThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            using (session)
            {
            }

            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await session.FetchAsync<Customer>(null));
        }

        [Fact]
        public void IncludeManyThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            SqlQuery sqlQuery = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.Include.Many<Customer>(sqlQuery));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public void IncludeReturnsSameSessionByDifferentInterface()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            var includeSession = session.Include;

            Assert.Same(session, includeSession);
        }

        [Fact]
        public async void IncludeScalarSqlQueryExecutesAndReturnsResult()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.FieldCount).Returns(1);
            mockReader.Setup(x => x[0]).Returns(10);
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var includeScalar = session.Include.Scalar<int>(new SqlQuery(""));

            await session.ExecutePendingQueriesAsync();

            Assert.Equal(10, includeScalar.Value);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public void IncludeScalarThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            SqlQuery sqlQuery = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.Include.Scalar<int>(sqlQuery));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public void IncludeSingleThrowsArgumentNullExceptionForNullIdentifier()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            object identifier = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.Include.Single<Customer>(identifier));

            Assert.Equal("identifier", exception.ParamName);
        }

        [Fact]
        public void IncludeSingleThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            SqlQuery sqlQuery = null;

            var exception = Assert.Throws<ArgumentNullException>(
                () => session.Include.Single<Customer>(sqlQuery));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public async void MicroLiteExceptionsCaughtByExecutePendingQueriesShouldNotBeWrappedInAnotherMicroLiteException()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.BuildCommand(It.IsNotNull<IDbCommand>(), It.IsNotNull<SqlQuery>())).Throws<MicroLiteException>();

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            // We need at least 1 queued query otherwise we will get an exception when doing queries.Dequeue() instead.
            session.Include.Scalar<int>(new SqlQuery(""));

            var exception = await Assert.ThrowsAsync<MicroLiteException>(
                async () => await session.ExecutePendingQueriesAsync());

            Assert.IsNotType<MicroLiteException>(exception.InnerException);
        }

        [Fact]
        public async void PagedExecutesAndReturnsResultsForFirstPageWithOnePerPage()
        {
            var sqlQuery = new SqlQuery("SELECT * FROM TABLE");
            var countQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE");
            var pagedQuery = new SqlQuery("SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");
            var combinedQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE;SELECT Id FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.CountQuery(sqlQuery)).Returns(countQuery);
            mockSqlDialect.Setup(x => x.PageQuery(sqlQuery, PagingOptions.ForPage(1, 1))).Returns(pagedQuery);

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.FieldCount).Returns(new Queue<int>(new[] { 1, 0 }).Dequeue);
            mockReader.Setup(x => x[0]).Returns(1000); // Simulate 1000 records in the count query
            mockReader.Setup(x => x.NextResult()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
            mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);
            mockDbDriver.Setup(x => x.Combine(countQuery, pagedQuery)).Returns(combinedQuery);

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var page = await session.PagedAsync<Customer>(sqlQuery, PagingOptions.ForPage(1, 1));

            Assert.Equal(1, page.Page);
            Assert.Equal(1, page.Results.Count);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void PagedExecutesAndReturnsResultsForFirstPageWithTwentyFivePerPage()
        {
            var sqlQuery = new SqlQuery("SELECT * FROM TABLE");
            var countQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE");
            var pagedQuery = new SqlQuery("SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");
            var combinedQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE;SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.CountQuery(sqlQuery)).Returns(countQuery);
            mockSqlDialect.Setup(x => x.PageQuery(sqlQuery, PagingOptions.ForPage(1, 25))).Returns(pagedQuery);

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.FieldCount).Returns(new Queue<int>(new[] { 1, 0 }).Dequeue);
            mockReader.Setup(x => x[0]).Returns(1000); // Simulate 1000 records in the count query
            mockReader.Setup(x => x.NextResult()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
            mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);
            mockDbDriver.Setup(x => x.Combine(countQuery, pagedQuery)).Returns(combinedQuery);

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var page = await session.PagedAsync<Customer>(sqlQuery, PagingOptions.ForPage(1, 25));

            Assert.Equal(1, page.Page);
            Assert.Equal(1, page.Results.Count);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void PagedExecutesAndReturnsResultsForTenthPageWithTwentyFivePerPage()
        {
            var sqlQuery = new SqlQuery("SELECT * FROM TABLE");
            var countQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE");
            var pagedQuery = new SqlQuery("SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");
            var combinedQuery = new SqlQuery("SELECT COUNT(*) FROM TABLE;SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS MicroLiteRowNumber FROM Customers) AS Customers");

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.CountQuery(sqlQuery)).Returns(countQuery);
            mockSqlDialect.Setup(x => x.PageQuery(sqlQuery, PagingOptions.ForPage(10, 25))).Returns(pagedQuery);

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.FieldCount).Returns(new Queue<int>(new[] { 1, 0 }).Dequeue);
            mockReader.Setup(x => x[0]).Returns(1000); // Simulate 1000 records in the count query
            mockReader.Setup(x => x.NextResult()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
            mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);
            mockDbDriver.Setup(x => x.Combine(countQuery, pagedQuery)).Returns(combinedQuery);

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var page = await session.PagedAsync<Customer>(sqlQuery, PagingOptions.ForPage(10, 25));

            Assert.Equal(10, page.Page);
            Assert.Equal(1, page.Results.Count);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void PagedThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await session.PagedAsync<Customer>(null, PagingOptions.ForPage(1, 25)));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public async void PagedThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            using (session)
            {
            }

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await session.PagedAsync<Customer>(null, PagingOptions.ForPage(1, 25)));
        }

        [Fact]
        public async void SingleIdentifierExecutesAndReturnsNull()
        {
            object identifier = 100;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildSelectSqlQuery(It.IsNotNull<IObjectInfo>(), identifier)).Returns(new SqlQuery(""));

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(false);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customer = await session.SingleAsync<Customer>(identifier);

            Assert.Null(customer);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void SingleIdentifierExecutesAndReturnsResult()
        {
            object identifier = 100;

            var mockSqlDialect = new Mock<ISqlDialect>();
            mockSqlDialect.Setup(x => x.BuildSelectSqlQuery(It.IsNotNull<IObjectInfo>(), identifier)).Returns(new SqlQuery(""));

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customer = await session.SingleAsync<Customer>(identifier);

            Assert.NotNull(customer);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
            mockSqlDialect.VerifyAll();
        }

        [Fact]
        public async void SingleIdentifierThrowsArgumentNullExceptionForNullIdentifier()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            object identifier = null;

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await session.SingleAsync<Customer>(identifier));

            Assert.Equal("identifier", exception.ParamName);
        }

        [Fact]
        public async void SingleIdentifierThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            using (session)
            {
            }

            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await session.SingleAsync<Customer>(1));
        }

        [Fact]
        public async void SingleSqlQueryExecutesAndReturnsNull()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(false);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customer = await session.SingleAsync<Customer>(new SqlQuery(""));

            Assert.Null(customer);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
        }

        [Fact]
        public async void SingleSqlQueryExecutesAndReturnsResult()
        {
            var mockSqlDialect = new Mock<ISqlDialect>();

            var mockReader = new Mock<IDataReader>();
            mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);
            mockReader.As<IDisposable>().Setup(x => x.Dispose());

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

            var mockDbDriver = new Mock<IDbDriver>();
            mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));

            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                mockSqlDialect.Object,
                mockDbDriver.Object);

            var customer = await session.SingleAsync<Customer>(new SqlQuery(""));

            Assert.NotNull(customer);

            mockReader.VerifyAll();
            mockCommand.VerifyAll();
            mockConnection.VerifyAll();
        }

        [Fact]
        public async void SingleSqlQueryThrowsArgumentNullExceptionForNullSqlQuery()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            SqlQuery sqlQuery = null;

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await session.SingleAsync<Customer>(sqlQuery));

            Assert.Equal("sqlQuery", exception.ParamName);
        }

        [Fact]
        public async void SingleSqlQueryThrowsObjectDisposedExceptionIfDisposed()
        {
            var session = new AsyncReadOnlySession(
                ConnectionScope.PerTransaction,
                new Mock<ISqlDialect>().Object,
                new Mock<IDbDriver>().Object);

            using (session)
            {
            }

            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await session.SingleAsync<Customer>(new SqlQuery("")));
        }

        public class WhenCallingPagedUsingPagingOptionsNone
        {
            [Fact]
            public async void AMicroLiteExceptionIsThrown()
            {
                var session = new AsyncReadOnlySession(
                    ConnectionScope.PerTransaction,
                    new Mock<ISqlDialect>().Object,
                    new Mock<IDbDriver>().Object);

                var exception = await Assert.ThrowsAsync<MicroLiteException>(
                    async () => await session.PagedAsync<Customer>(new SqlQuery(""), PagingOptions.None));

                Assert.Equal(ExceptionMessages.Session_PagingOptionsMustNotBeNone, exception.Message);
            }
        }

        public class WhenExecutingMultipleQueriesAndTheSqlDialectUsedDoesNotSupportBatching
        {
            private Mock<IDbCommand> mockCommand = new Mock<IDbCommand>();
            private Mock<IDbConnection> mockConnection = new Mock<IDbConnection>();
            private Mock<IDbDriver> mockDbDriver = new Mock<IDbDriver>();

            public WhenExecutingMultipleQueriesAndTheSqlDialectUsedDoesNotSupportBatching()
            {
                var mockSqlDialect = new Mock<ISqlDialect>();
                mockSqlDialect.Setup(x => x.BuildSelectSqlQuery(It.IsNotNull<IObjectInfo>(), It.IsNotNull<object>())).Returns(new SqlQuery(""));

                mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(() =>
                {
                    var mockReader = new Mock<IDataReader>();
                    mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);

                    return mockReader.Object;
                });

                mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

                mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
                mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(false);

                var session = new AsyncReadOnlySession(
                    ConnectionScope.PerTransaction,
                    mockSqlDialect.Object,
                    mockDbDriver.Object);

                var includeCustomer = session.Include.Single<Customer>(2);
                var customer = session.SingleAsync<Customer>(1).Result;
            }

            [Fact]
            public void TheDbDriverShouldBuildTwoIDbCommands()
            {
                this.mockDbDriver.Verify(x => x.BuildCommand(It.IsNotNull<IDbCommand>(), It.IsNotNull<SqlQuery>()), Times.Exactly(2));
            }

            [Fact]
            public void TheDbDriverShouldNotCombineTheQueriesUsingTheIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<IEnumerable<SqlQuery>>()), Times.Never());
            }

            [Fact]
            public void TheDbDriverShouldNotCombineTheQueriesUsingTheNonIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<SqlQuery>(), It.IsNotNull<SqlQuery>()), Times.Never());
            }
        }

        public class WhenExecutingMultipleQueriesAndTheSqlDialectUsedSupportsBatching
        {
            private Mock<IDbDriver> mockDbDriver = new Mock<IDbDriver>();

            public WhenExecutingMultipleQueriesAndTheSqlDialectUsedSupportsBatching()
            {
                var mockSqlDialect = new Mock<ISqlDialect>();
                mockSqlDialect.Setup(x => x.BuildSelectSqlQuery(It.IsNotNull<IObjectInfo>(), It.IsNotNull<object>())).Returns(new SqlQuery(""));

                this.mockDbDriver.Setup(x => x.Combine(It.IsNotNull<IEnumerable<SqlQuery>>())).Returns(new SqlQuery(""));

                var mockReader = new Mock<IDataReader>();
                mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false, true, false }).Dequeue);

                var mockCommand = new Mock<IDbCommand>();
                mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

                var mockConnection = new Mock<IDbConnection>();
                mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

                mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
                mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);

                var session = new AsyncReadOnlySession(
                    ConnectionScope.PerTransaction,
                    mockSqlDialect.Object,
                    mockDbDriver.Object);

                var includeCustomerA = session.Include.Single<Customer>(3);
                var includeCustomerB = session.Include.Single<Customer>(2);
                var customer = session.SingleAsync<Customer>(1).Result;
            }

            [Fact]
            public void TheDbDriverShouldNotCombineTheQueriesUsingTheNonIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<SqlQuery>(), It.IsNotNull<SqlQuery>()), Times.Never());
            }

            [Fact]
            public void TheSqlDialectShouldBuildOneIDbCommand()
            {
                this.mockDbDriver.Verify(x => x.BuildCommand(It.IsNotNull<IDbCommand>(), It.IsNotNull<SqlQuery>()), Times.Once());
            }

            [Fact]
            public void TheSqlDialectShouldCombineTheQueriesUsingTheIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<IEnumerable<SqlQuery>>()), Times.Once());
            }
        }

        public class WhenExecutingTwoQueriesAndTheSqlDialectUsedSupportsBatching
        {
            private Mock<IDbDriver> mockDbDriver = new Mock<IDbDriver>();

            public WhenExecutingTwoQueriesAndTheSqlDialectUsedSupportsBatching()
            {
                var mockSqlDialect = new Mock<ISqlDialect>();
                mockSqlDialect.Setup(x => x.BuildSelectSqlQuery(It.IsNotNull<IObjectInfo>(), It.IsNotNull<object>())).Returns(new SqlQuery(""));

                this.mockDbDriver.Setup(x => x.Combine(It.IsNotNull<SqlQuery>(), It.IsNotNull<SqlQuery>())).Returns(new SqlQuery(""));

                var mockReader = new Mock<IDataReader>();
                mockReader.Setup(x => x.Read()).Returns(new Queue<bool>(new[] { true, false }).Dequeue);

                var mockCommand = new Mock<IDbCommand>();
                mockCommand.Setup(x => x.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(mockReader.Object);

                var mockConnection = new Mock<IDbConnection>();
                mockConnection.Setup(x => x.CreateCommand()).Returns(mockCommand.Object);

                mockDbDriver.Setup(x => x.CreateConnection()).Returns(new MockDbConnectionWrapper(mockConnection.Object));
                mockDbDriver.Setup(x => x.SupportsBatchedQueries).Returns(true);

                var session = new AsyncReadOnlySession(
                    ConnectionScope.PerTransaction,
                    mockSqlDialect.Object,
                    mockDbDriver.Object);

                var includeCustomer = session.Include.Single<Customer>(2);
                var customer = session.SingleAsync<Customer>(1).Result;
            }

            [Fact]
            public void TheDbDriverShouldCombineTheQueriesUsingTheNonIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<SqlQuery>(), It.IsNotNull<SqlQuery>()), Times.Once());
            }

            [Fact]
            public void TheSqlDialectShouldBuildOneIDbCommand()
            {
                this.mockDbDriver.Verify(x => x.BuildCommand(It.IsNotNull<IDbCommand>(), It.IsNotNull<SqlQuery>()), Times.Once());
            }

            [Fact]
            public void TheSqlDialectShouldNotCombineTheQueriesUsingTheIEnumerableOverload()
            {
                this.mockDbDriver.Verify(x => x.Combine(It.IsNotNull<IEnumerable<SqlQuery>>()), Times.Never());
            }
        }
    }
}