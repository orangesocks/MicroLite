﻿// -----------------------------------------------------------------------
// <copyright file="AsyncSession.cs" company="MicroLite">
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
namespace MicroLite.Core
{
    using System;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using MicroLite.Dialect;
    using MicroLite.Driver;
    using MicroLite.Listeners;
    using MicroLite.Mapping;
    using MicroLite.TypeConverters;

    /// <summary>
    /// The default implementation of <see cref="IAsyncSession"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ConnectionScope: {ConnectionScope}")]
    internal sealed class AsyncSession : AsyncReadOnlySession, IAsyncSession, IAdvancedAsyncSession
    {
        private readonly SessionListeners sessionListeners;

        internal AsyncSession(
            ConnectionScope connectionScope,
            ISqlDialect sqlDialect,
            IDbDriver sqlDriver,
            SessionListeners sessionListeners)
            : base(connectionScope, sqlDialect, sqlDriver)
        {
            this.sessionListeners = sessionListeners;
        }

        public new IAdvancedAsyncSession Advanced
        {
            get
            {
                return this;
            }
        }

        public Task<bool> DeleteAsync(object instance)
        {
            return this.DeleteAsync(instance, CancellationToken.None);
        }

        public async Task<bool> DeleteAsync(object instance, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            for (int i = 0; i < this.sessionListeners.DeleteListeners.Count; i++)
            {
                this.sessionListeners.DeleteListeners[i].BeforeDelete(instance);
            }

            var objectInfo = ObjectInfo.For(instance.GetType());

            var identifier = objectInfo.GetIdentifierValue(instance);

            if (objectInfo.IsDefaultIdentifier(identifier))
            {
                throw new MicroLiteException(ExceptionMessages.Session_IdentifierNotSetForDelete);
            }

            var sqlQuery = this.SqlDialect.BuildDeleteSqlQuery(objectInfo, identifier);

            var rowsAffected = await this.ExecuteQueryAsync(sqlQuery, cancellationToken).ConfigureAwait(false);

            for (int i = this.sessionListeners.DeleteListeners.Count - 1; i >= 0; i--)
            {
                this.sessionListeners.DeleteListeners[i].AfterDelete(instance, rowsAffected);
            }

            return rowsAffected == 1;
        }

        public Task<bool> DeleteAsync(Type type, object identifier)
        {
            return this.DeleteAsync(type, identifier, CancellationToken.None);
        }

        public async Task<bool> DeleteAsync(Type type, object identifier, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var objectInfo = ObjectInfo.For(type);

            var sqlQuery = this.SqlDialect.BuildDeleteSqlQuery(objectInfo, identifier);

            var rowsAffected = await this.ExecuteQueryAsync(sqlQuery, cancellationToken).ConfigureAwait(false);

            return rowsAffected == 1;
        }

        public Task<int> ExecuteAsync(SqlQuery sqlQuery)
        {
            return this.ExecuteAsync(sqlQuery, CancellationToken.None);
        }

        public async Task<int> ExecuteAsync(SqlQuery sqlQuery, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (sqlQuery == null)
            {
                throw new ArgumentNullException(nameof(sqlQuery));
            }

            return await this.ExecuteQueryAsync(sqlQuery, cancellationToken).ConfigureAwait(false);
        }

        public Task<T> ExecuteScalarAsync<T>(SqlQuery sqlQuery)
        {
            return this.ExecuteScalarAsync<T>(sqlQuery, CancellationToken.None);
        }

        public async Task<T> ExecuteScalarAsync<T>(SqlQuery sqlQuery, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (sqlQuery == null)
            {
                throw new ArgumentNullException(nameof(sqlQuery));
            }

            return await this.ExecuteScalarQueryAsync<T>(sqlQuery, cancellationToken).ConfigureAwait(false);
        }

        public Task InsertAsync(object instance)
        {
            return this.InsertAsync(instance, CancellationToken.None);
        }

        public async Task InsertAsync(object instance, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            for (int i = 0; i < this.sessionListeners.InsertListeners.Count; i++)
            {
                this.sessionListeners.InsertListeners[i].BeforeInsert(instance);
            }

            var objectInfo = ObjectInfo.For(instance.GetType());
            objectInfo.VerifyInstanceForInsert(instance);

            object identifier = await this.InsertReturningIdentifierAsync(objectInfo, instance, cancellationToken).ConfigureAwait(false);

            for (int i = this.sessionListeners.InsertListeners.Count - 1; i >= 0; i--)
            {
                this.sessionListeners.InsertListeners[i].AfterInsert(instance, identifier);
            }
        }

        public Task<bool> UpdateAsync(object instance)
        {
            return this.UpdateAsync(instance, CancellationToken.None);
        }

        public async Task<bool> UpdateAsync(object instance, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            for (int i = 0; i < this.sessionListeners.UpdateListeners.Count; i++)
            {
                this.sessionListeners.UpdateListeners[i].BeforeUpdate(instance);
            }

            var objectInfo = ObjectInfo.For(instance.GetType());

            if (objectInfo.HasDefaultIdentifierValue(instance))
            {
                throw new MicroLiteException(ExceptionMessages.Session_IdentifierNotSetForUpdate);
            }

            var sqlQuery = this.SqlDialect.BuildUpdateSqlQuery(objectInfo, instance);

            var rowsAffected = await this.ExecuteQueryAsync(sqlQuery, cancellationToken).ConfigureAwait(false);

            for (int i = this.sessionListeners.UpdateListeners.Count - 1; i >= 0; i--)
            {
                this.sessionListeners.UpdateListeners[i].AfterUpdate(instance, rowsAffected);
            }

            return rowsAffected == 1;
        }

        public Task<bool> UpdateAsync(ObjectDelta objectDelta)
        {
            return this.UpdateAsync(objectDelta, CancellationToken.None);
        }

        public async Task<bool> UpdateAsync(ObjectDelta objectDelta, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();

            if (objectDelta == null)
            {
                throw new ArgumentNullException(nameof(objectDelta));
            }

            if (objectDelta.ChangeCount == 0)
            {
                throw new MicroLiteException(ExceptionMessages.ObjectDelta_MustContainAtLeastOneChange);
            }

            var sqlQuery = this.SqlDialect.BuildUpdateSqlQuery(objectDelta);

            var rowsAffected = await this.ExecuteQueryAsync(sqlQuery, cancellationToken).ConfigureAwait(false);

            return rowsAffected == 1;
        }

        private async Task<int> ExecuteQueryAsync(SqlQuery sqlQuery, CancellationToken cancellationToken)
        {
            try
            {
                this.ConfigureCommand(sqlQuery);

                var command = (DbCommand)this.Command;

                var result = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                this.CommandCompleted();

                return result;
            }
            catch (OperationCanceledException)
            {
                // Don't re-wrap Operation Canceled exceptions
                throw;
            }
            catch (MicroLiteException)
            {
                // Don't re-wrap MicroLite exceptions
                throw;
            }
            catch (Exception e)
            {
                throw new MicroLiteException(e.Message, e);
            }
        }

        private async Task<T> ExecuteScalarQueryAsync<T>(SqlQuery sqlQuery, CancellationToken cancellationToken)
        {
            try
            {
                this.ConfigureCommand(sqlQuery);

                var command = (DbCommand)this.Command;

                var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

                this.CommandCompleted();

                var resultType = typeof(T);
                var typeConverter = TypeConverter.For(resultType) ?? TypeConverter.Default;
                var converted = (T)typeConverter.ConvertFromDbValue(result, resultType);

                return converted;
            }
            catch (OperationCanceledException)
            {
                // Don't re-wrap Operation Canceled exceptions
                throw;
            }
            catch (MicroLiteException)
            {
                // Don't re-wrap MicroLite exceptions
                throw;
            }
            catch (Exception e)
            {
                throw new MicroLiteException(e.Message, e);
            }
        }

        private async Task<object> InsertReturningIdentifierAsync(IObjectInfo objectInfo, object instance, CancellationToken cancellationToken)
        {
            object identifier = null;

            var insertSqlQuery = this.SqlDialect.BuildInsertSqlQuery(objectInfo, instance);

            if (this.SqlDialect.SupportsSelectInsertedIdentifier
                && objectInfo.TableInfo.IdentifierStrategy != IdentifierStrategy.Assigned)
            {
                var selectInsertIdSqlQuery = this.SqlDialect.BuildSelectInsertIdSqlQuery(objectInfo);

                if (this.DbDriver.SupportsBatchedQueries)
                {
                    var combined = this.DbDriver.Combine(insertSqlQuery, selectInsertIdSqlQuery);
                    identifier = await this.ExecuteScalarQueryAsync<object>(combined, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await this.ExecuteQueryAsync(insertSqlQuery, cancellationToken).ConfigureAwait(false);
                    identifier = await this.ExecuteScalarQueryAsync<object>(selectInsertIdSqlQuery, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (objectInfo.TableInfo.IdentifierStrategy != IdentifierStrategy.Assigned)
            {
                identifier = await this.ExecuteScalarQueryAsync<object>(insertSqlQuery, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await this.ExecuteQueryAsync(insertSqlQuery, cancellationToken).ConfigureAwait(false);
            }

            return identifier;
        }
    }
}