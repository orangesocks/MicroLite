﻿namespace MicroLite.Tests.Listeners
{
    using System;
    using MicroLite.Listeners;
    using MicroLite.Mapping;
    using MicroLite.Tests.TestEntities;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="DbGeneratedListener"/> class.
    /// </summary>
    public class DbGeneratedListenerTests : UnitTest
    {
        [Fact]
        public void AfterInsertSetsIdentifierValue()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer();
            int scalarResult = 4354;

            var listener = new DbGeneratedListener();
            listener.AfterInsert(customer, scalarResult);

            Assert.Equal(scalarResult, customer.Id);
        }

        [Fact]
        public void AfterInsertSetsIdentifierValueConvertingItToThePropertyType()
        {
            ObjectInfo.MappingConvention = new ConventionMappingConvention(
                UnitTest.GetConventionMappingSettings(IdentifierStrategy.DbGenerated));

            var customer = new Customer();
            decimal scalarResult = 4354;

            var listener = new DbGeneratedListener();
            listener.AfterInsert(customer, scalarResult);

            Assert.Equal(Convert.ToInt32(scalarResult), customer.Id);
        }

        [Fact]
        public void AfterInsertThrowsArgumentNullExceptionForNullExecuteScalarResult()
        {
            var listener = new DbGeneratedListener();

            var exception = Assert.Throws<ArgumentNullException>(() => listener.AfterInsert(new Customer(), null));

            Assert.Equal("executeScalarResult", exception.ParamName);
        }

        [Fact]
        public void AfterInsertThrowsArgumentNullExceptionForNullInstance()
        {
            var listener = new DbGeneratedListener();

            var exception = Assert.Throws<ArgumentNullException>(() => listener.AfterInsert(null, 1));

            Assert.Equal("instance", exception.ParamName);
        }
    }
}