﻿namespace MicroLite.Tests.Driver
{
    using System.Data;
    using System.Data.OleDb;
    using MicroLite.Driver;
    using Xunit;

    /// <summary>
    /// Unit Tests for the <see cref="MySqlDbDriver"/> class.
    /// </summary>
    public class MySqlDbDriverTests : UnitTest
    {
        /// <summary>
        /// Issue #6 - The argument count check needs to cater for the same argument being used twice.
        /// </summary>
        [Fact]
        public void BuildCommandForSqlQueryWithSqlTextWhichUsesSameParameterTwice()
        {
            var command = new OleDbCommand();

            var sqlQuery = new SqlQuery(
                "SELECT * FROM `Table` WHERE `Table`.`Id` = @p0 AND `Table`.`Value1` = @p1 OR @p1 IS NULL",
                100, "hello");

            var dbDriver = new MySqlDbDriver();
            dbDriver.BuildCommand(command, sqlQuery);

            Assert.Equal(sqlQuery.CommandText, command.CommandText);
            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal(2, command.Parameters.Count);

            var parameter1 = (IDataParameter)command.Parameters[0];
            Assert.Equal(DbType.Int32, parameter1.DbType);
            Assert.Equal(ParameterDirection.Input, parameter1.Direction);
            Assert.Equal("@p0", parameter1.ParameterName);
            Assert.Equal(sqlQuery.Arguments[0].Value, parameter1.Value);

            var parameter2 = (IDataParameter)command.Parameters[1];
            Assert.Equal(DbType.String, parameter2.DbType);
            Assert.Equal(ParameterDirection.Input, parameter2.Direction);
            Assert.Equal("@p1", parameter2.ParameterName);
            Assert.Equal(sqlQuery.Arguments[1].Value, parameter2.Value);
        }

        [Fact]
        public void BuildCommandForSqlQueryWithStoredProcedureWithoutParameters()
        {
            var command = new OleDbCommand();

            var sqlQuery = new SqlQuery("CALL GetTableContents");

            var dbDriver = new MySqlDbDriver();
            dbDriver.BuildCommand(command, sqlQuery);

            // The command text should only contain the stored procedure name.
            Assert.Equal("GetTableContents", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public void BuildCommandForSqlQueryWithStoredProcedureWithParameters()
        {
            var command = new OleDbCommand();

            var sqlQuery = new SqlQuery(
                "CALL GetTableContents @identifier, @Cust_Name",
                100, "hello");

            var dbDriver = new MySqlDbDriver();
            dbDriver.BuildCommand(command, sqlQuery);

            // The command text should only contain the stored procedure name.
            Assert.Equal("GetTableContents", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
            Assert.Equal(2, command.Parameters.Count);

            var parameter1 = (IDataParameter)command.Parameters[0];
            Assert.Equal(DbType.Int32, parameter1.DbType);
            Assert.Equal(ParameterDirection.Input, parameter1.Direction);
            Assert.Equal("@identifier", parameter1.ParameterName);
            Assert.Equal(sqlQuery.Arguments[0].Value, parameter1.Value);

            var parameter2 = (IDataParameter)command.Parameters[1];
            Assert.Equal(DbType.String, parameter2.DbType);
            Assert.Equal(ParameterDirection.Input, parameter2.Direction);
            Assert.Equal("@Cust_Name", parameter2.ParameterName);
            Assert.Equal(sqlQuery.Arguments[1].Value, parameter2.Value);
        }

        [Fact]
        public void SupportsBatchedQueriesReturnsTrue()
        {
            var dbDriver = new MySqlDbDriver();

            Assert.True(dbDriver.SupportsBatchedQueries);
        }

        public class WhenCallingCombine_WithAnIEnumerableSqlQuery
        {
            private readonly SqlQuery combinedQuery;
            private readonly SqlQuery sqlQuery1;
            private readonly SqlQuery sqlQuery2;

            public WhenCallingCombine_WithAnIEnumerableSqlQuery()
            {
                this.sqlQuery1 = new SqlQuery("SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1", "Foo", 100);
                this.sqlQuery1.Timeout = 38;

                this.sqlQuery2 = new SqlQuery("SELECT `Column_1`, `Column_2` FROM `dbo`.`Table_2` WHERE (`Column_1` = @p0 OR @p0 IS NULL) AND `Column_2` < @p1", "Bar", -1);
                this.sqlQuery2.Timeout = 42;

                var dbDriver = new MySqlDbDriver();

                this.combinedQuery = dbDriver.Combine(new[] { this.sqlQuery1, this.sqlQuery2 });
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheFirstArgumentOfTheFirstQuery()
            {
                Assert.Equal(this.sqlQuery1.Arguments[0], this.combinedQuery.Arguments[0]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheFirstArgumentOfTheSecondQuery()
            {
                Assert.Equal(this.sqlQuery2.Arguments[0], this.combinedQuery.Arguments[2]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheNumberOfArgumentsInTheSourceQueries()
            {
                Assert.Equal(this.sqlQuery1.Arguments.Count + this.sqlQuery2.Arguments.Count, this.combinedQuery.Arguments.Count);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheSecondArgumentOfTheFirstQuery()
            {
                Assert.Equal(this.sqlQuery1.Arguments[1], this.combinedQuery.Arguments[1]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheSecondArgumentOfTheSecondQuery()
            {
                Assert.Equal(this.sqlQuery2.Arguments[1], this.combinedQuery.Arguments[3]);
            }

            [Fact]
            public void TheCombinedCommandTextShouldBeSeparatedUsingTheSelectSeparator()
            {
                Assert.Equal(
                    "SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1;\r\nSELECT `Column_1`, `Column_2` FROM `dbo`.`Table_2` WHERE (`Column_1` = @p2 OR @p2 IS NULL) AND `Column_2` < @p3",
                    this.combinedQuery.CommandText);
            }

            [Fact]
            public void TheTimeoutShouldBeSetToTheLongestTimeoutOfTheSourceQueries()
            {
                Assert.Equal(this.sqlQuery2.Timeout, this.combinedQuery.Timeout);
            }
        }

        /// <summary>
        /// Issue #90 - Re-Writing parameters should not happen if the query is a stored procedure.
        /// </summary>
        public class WhenCallingCombine_WithAnIEnumerableSqlQuery_AndAnSqlQueryIsForAStoredProcedure
        {
            private readonly SqlQuery combinedQuery;
            private readonly SqlQuery sqlQuery1;
            private readonly SqlQuery sqlQuery2;

            public WhenCallingCombine_WithAnIEnumerableSqlQuery_AndAnSqlQueryIsForAStoredProcedure()
            {
                this.sqlQuery1 = new SqlQuery("SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1", "Foo", 100);
                this.sqlQuery2 = new SqlQuery("CALL CustomersByStatus @StatusId", 2);

                var dbDriver = new MySqlDbDriver();

                this.combinedQuery = dbDriver.Combine(new[] { this.sqlQuery1, this.sqlQuery2 });
            }

            [Fact]
            public void TheParameterNamesForTheStoredProcedureShouldNotBeRenamed()
            {
                Assert.Equal(
                    "SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1;\r\nCALL CustomersByStatus @StatusId",
                    this.combinedQuery.CommandText);
            }
        }

        /// <summary>
        /// Issue #90 - Re-Writing parameters should not happen if the query is a stored procedure.
        /// </summary>
        public class WhenCallingCombine_WithAnTwoSqlQueries_AndAnSqlQueryIsForAStoredProcedure
        {
            private readonly SqlQuery combinedQuery;
            private readonly SqlQuery sqlQuery1;
            private readonly SqlQuery sqlQuery2;

            public WhenCallingCombine_WithAnTwoSqlQueries_AndAnSqlQueryIsForAStoredProcedure()
            {
                this.sqlQuery1 = new SqlQuery("SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1", "Foo", 100);
                this.sqlQuery2 = new SqlQuery("CALL CustomersByStatus @StatusId", 2);

                var dbDriver = new MySqlDbDriver();

                this.combinedQuery = dbDriver.Combine(this.sqlQuery1, this.sqlQuery2);
            }

            [Fact]
            public void TheParameterNamesForTheStoredProcedureShouldNotBeRenamed()
            {
                Assert.Equal(
                    "SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1;\r\nCALL CustomersByStatus @StatusId",
                    this.combinedQuery.CommandText);
            }
        }

        public class WhenCallingCombine_WithTwoSqlQueries
        {
            private readonly SqlQuery combinedQuery;
            private readonly SqlQuery sqlQuery1;
            private readonly SqlQuery sqlQuery2;

            public WhenCallingCombine_WithTwoSqlQueries()
            {
                this.sqlQuery1 = new SqlQuery("SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1", "Foo", 100);
                this.sqlQuery1.Timeout = 38;

                this.sqlQuery2 = new SqlQuery("SELECT `Column_1`, `Column_2` FROM `dbo`.`Table_2` WHERE (`Column_1` = @p0 OR @p0 IS NULL) AND `Column_2` < @p1", "Bar", -1);
                this.sqlQuery2.Timeout = 42;

                var dbDriver = new MySqlDbDriver();

                this.combinedQuery = dbDriver.Combine(this.sqlQuery1, this.sqlQuery2);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheFirstArgumentOfTheFirstQuery()
            {
                Assert.Equal(this.sqlQuery1.Arguments[0], this.combinedQuery.Arguments[0]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheFirstArgumentOfTheSecondQuery()
            {
                Assert.Equal(this.sqlQuery2.Arguments[0], this.combinedQuery.Arguments[2]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheNumberOfArgumentsInTheSourceQueries()
            {
                Assert.Equal(this.sqlQuery1.Arguments.Count + this.sqlQuery2.Arguments.Count, this.combinedQuery.Arguments.Count);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheSecondArgumentOfTheFirstQuery()
            {
                Assert.Equal(this.sqlQuery1.Arguments[1], this.combinedQuery.Arguments[1]);
            }

            [Fact]
            public void TheCombinedArgumentsShouldContainTheSecondArgumentOfTheSecondQuery()
            {
                Assert.Equal(this.sqlQuery2.Arguments[1], this.combinedQuery.Arguments[3]);
            }

            [Fact]
            public void TheCombinedCommandTextShouldBeSeparatedUsingTheSelectSeparator()
            {
                Assert.Equal(
                    "SELECT `Column1`, `Column2`, `Column3` FROM `dbo`.`Table1` WHERE `Column1` = @p0 AND `Column2` > @p1;\r\nSELECT `Column_1`, `Column_2` FROM `dbo`.`Table_2` WHERE (`Column_1` = @p2 OR @p2 IS NULL) AND `Column_2` < @p3",
                    this.combinedQuery.CommandText);
            }

            [Fact]
            public void TheTimeoutShouldBeSetToTheLongestTimeoutOfTheSourceQueries()
            {
                Assert.Equal(this.sqlQuery2.Timeout, this.combinedQuery.Timeout);
            }
        }
    }
}