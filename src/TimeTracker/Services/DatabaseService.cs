using MaSch.Core.Attributes;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TimeTracker.Services
{
    public class DatabaseService : IDatabaseService
    {
        private static readonly string DatabaseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "database.db");
        private static readonly object _commandLock = new object();
        private object _databaseLock = new object();

        private SqliteConnection _connection;

        public DatabaseService() { }

        public async Task<IDbCommand> CreateCommand(string cmdText)
        {
            await EnsureOpenConnection();
            return SynchronizeDispose(() =>
            {
                var cmd = _connection.CreateCommand();
                cmd.CommandText = cmdText;
                return cmd;
            });
        }

        private async Task EnsureOpenConnection()
        {
            Monitor.Enter(_databaseLock);
            try
            {
                if (_connection != null && _connection.State != ConnectionState.Closed && _connection.State != ConnectionState.Broken)
                    return;
                if (_connection != null)
                    _connection.Dispose();

                Directory.CreateDirectory(Path.GetDirectoryName(DatabaseFilePath));
                var isNewDb = !File.Exists(DatabaseFilePath);
                _connection = new SqliteConnection($"Data Source={DatabaseFilePath};");
                await _connection.OpenAsync();

                if (isNewDb)
                {
                    using var cmd = _connection.CreateCommand();
                    cmd.CommandText = SqlQueries.CreateTables;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally { Monitor.Exit(_databaseLock); }
        }


        private static IDbCommand SynchronizeDispose(Func<IDbCommand> commandFunc)
        {
            IDbCommand command;
            lock (_commandLock)
                command = commandFunc();
            return new DbCommandWrapper(command, _commandLock);
        }

    }

    [Wrapping(typeof(IDbCommand))]
    public partial class DbCommandWrapper : IDbCommand
    {
        private object _lockObject;
        public DbCommandWrapper(IDbCommand dbCommand, object lockObject)
        {
            _lockObject = lockObject;
            WrappedIDbCommand = dbCommand;
        }

        public async Task<int> ExecuteNonQueryAsync() => await WrappedIDbCommand.ExecuteNonQueryAsync();
        public async Task<int> ExecuteNonQueryAsync(CancellationToken token) => await WrappedIDbCommand.ExecuteNonQueryAsync(token);
        public async Task<IDataReader> ExecuteReaderAsync() => await WrappedIDbCommand.ExecuteReaderAsync();
        public async Task<IDataReader> ExecuteReaderAsync(CancellationToken token) => await WrappedIDbCommand.ExecuteReaderAsync(token);
        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior) => await WrappedIDbCommand.ExecuteReaderAsync(behavior);
        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken token) => await WrappedIDbCommand.ExecuteReaderAsync(behavior, token);
        public async Task<object> ExecuteScalarAsync() => await WrappedIDbCommand.ExecuteScalarAsync();
        public async Task<object> ExecuteScalarAsync(CancellationToken token) => await WrappedIDbCommand.ExecuteScalarAsync(token);

        public void Dispose()
        {
            lock (_lockObject)
                WrappedIDbCommand.Dispose();
        }
    }
}
