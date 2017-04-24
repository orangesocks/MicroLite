﻿// -----------------------------------------------------------------------
// <copyright file="InflectionService.cs" company="MicroLite">
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
namespace MicroLite.Mapping.Inflection
{
    /// <summary>
    /// A class which provides access to inflection services for different cultures.
    /// </summary>
    public static class InflectionService
    {
        /// <summary>
        /// Gets the IInflection service for English (en-GB).
        /// </summary>
        public static IInflectionService English { get; } = new EnglishInflectionService();
    }
}