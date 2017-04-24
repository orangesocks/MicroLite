﻿// -----------------------------------------------------------------------
// <copyright file="ConfigurationExtensions.cs" company="MicroLite">
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
namespace MicroLite.Configuration
{
    using System;
    using Dialect;
    using Driver;
    using Mapping;
    using Mapping.Attributes;

    /// <summary>
    /// Extension methods for IConfigureExtensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Configures an MS SQL 2005 (or later) connection using the connection string with the specified name
        /// in the connection strings section of the app/web config.
        /// </summary>
        /// <param name="configureConnection">The interface to configure a connection.</param>
        /// <param name="connectionName">The name of the connection string in the app/web config.</param>
        /// <returns>The next step in the fluent configuration.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="ConfigurationException">Thrown if the connection is not found in the app config.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForMs", Justification = "For MS, not Forms.")]
        public static ICreateSessionFactory ForMsSql2005Connection(this IConfigureConnection configureConnection, string connectionName)
        {
            if (configureConnection == null)
            {
                throw new ArgumentNullException(nameof(configureConnection));
            }

            return configureConnection.ForConnection(connectionName, new MsSql2005Dialect(), new MsSqlDbDriver());
        }

        /// <summary>
        /// Configures an MS SQL 2005 (or later) connection using the specified connection name,
        /// connection string string and provider name.
        /// </summary>
        /// <param name="configureConnection">The interface to configure a connection.</param>
        /// <param name="connectionName">The name for the connection.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">The name of the provider.</param>
        /// <returns>The next step in the fluent configuration.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForMs", Justification = "For MS, not Forms.")]
        public static ICreateSessionFactory ForMsSql2005Connection(this IConfigureConnection configureConnection, string connectionName, string connectionString, string providerName)
        {
            if (configureConnection == null)
            {
                throw new ArgumentNullException(nameof(configureConnection));
            }

            return configureConnection.ForConnection(connectionName, connectionString, providerName, new MsSql2005Dialect(), new MsSqlDbDriver());
        }

        /// <summary>
        /// Configures an MS SQL 2012 (or later) connection using the connection string with the specified name
        /// in the connection strings section of the app/web config.
        /// </summary>
        /// <param name="configureConnection">The interface to configure a connection.</param>
        /// <param name="connectionName">The name of the connection string in the app/web config.</param>
        /// <returns>The next step in the fluent configuration.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        /// <exception cref="ConfigurationException">Thrown if the connection is not found in the app config.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForMs", Justification = "For MS, not Forms.")]
        public static ICreateSessionFactory ForMsSql2012Connection(this IConfigureConnection configureConnection, string connectionName)
        {
            if (configureConnection == null)
            {
                throw new ArgumentNullException(nameof(configureConnection));
            }

            return configureConnection.ForConnection(connectionName, new MsSql2012Dialect(), new MsSqlDbDriver());
        }

        /// <summary>
        /// Configures an MS SQL 2012 (or later) connection using the specified connection name,
        /// connection string string and provider name.
        /// </summary>
        /// <param name="configureConnection">The interface to configure a connection.</param>
        /// <param name="connectionName">The name for the connection.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">The name of the provider.</param>
        /// <returns>The next step in the fluent configuration.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ForMs", Justification = "For MS, not Forms.")]
        public static ICreateSessionFactory ForMsSql2012Connection(this IConfigureConnection configureConnection, string connectionName, string connectionString, string providerName)
        {
            if (configureConnection == null)
            {
                throw new ArgumentNullException(nameof(configureConnection));
            }

            return configureConnection.ForConnection(connectionName, connectionString, providerName, new MsSql2012Dialect(), new MsSqlDbDriver());
        }

        /// <summary>
        /// Configures the MicroLite ORM Framework to use attribute based mapping instead of the default convention based mapping.
        /// </summary>
        /// <param name="configureExtensions">The interface to configure extensions.</param>
        /// <returns>The interface which provides the extension points.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        public static IConfigureExtensions WithAttributeBasedMapping(
            this IConfigureExtensions configureExtensions)
        {
            if (configureExtensions == null)
            {
                throw new ArgumentNullException(nameof(configureExtensions));
            }

            configureExtensions.SetMappingConvention(new AttributeMappingConvention());

            return configureExtensions;
        }

        /// <summary>
        /// Configures the MicroLite ORM Framework to use custom convention settings for the default convention based mapping.
        /// </summary>
        /// <param name="configureExtensions">The interface to configure extensions.</param>
        /// <param name="settings">The settings for the convention mapping.</param>
        /// <returns>The interface which provides the extension points.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if any argument is null.</exception>
        public static IConfigureExtensions WithConventionBasedMapping(
            this IConfigureExtensions configureExtensions,
            ConventionMappingSettings settings)
        {
            if (configureExtensions == null)
            {
                throw new ArgumentNullException(nameof(configureExtensions));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            configureExtensions.SetMappingConvention(new ConventionMappingConvention(settings));

            return configureExtensions;
        }
    }
}