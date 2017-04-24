// -----------------------------------------------------------------------
// <copyright file="UppercaseWithUnderscoresConventionMappingSettings.cs" company="MicroLite">
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
    using System.Reflection;
    using MicroLite.FrameworkExtensions;

    /// <summary>
    /// A class containing the convention mapping settings for uppercase with underscore separators (e.g. 'CreditCard' -> 'CREDIT_CARD').
    /// </summary>
    public class UppercaseWithUnderscoresConventionMappingSettings : ConventionMappingSettings
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="UppercaseWithUnderscoresConventionMappingSettings"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "This is specifically intended to be lowercase")]
        public UppercaseWithUnderscoresConventionMappingSettings()
        {
            this.ResolveColumnName = (PropertyInfo propertyInfo) => ConventionMappingSettings.GetColumnName(propertyInfo).ToUnderscored().ToUpperInvariant();
            this.ResolveIdentifierColumnName = (PropertyInfo propertyInfo) => propertyInfo.Name.ToUnderscored().ToUpperInvariant();
            this.ResolveTableName = (Type type) =>
            {
                var tableName = UsePluralClassNameForTableName ? this.InflectionService.ToPlural(GetTableName(type)) : GetTableName(type);

                return tableName.ToUnderscored().ToUpperInvariant();
            };
        }
    }
}