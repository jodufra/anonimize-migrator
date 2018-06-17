using MySql.Data.MySqlClient;
using System.Data;
using System;
using NLog;

namespace Anonimize.Migrator.Database
{
    public class DbManager : IDisposable
    {
        IDbConnection Connection { get; set; }
        Transaction transaction;

        public DbManager(string connectionString)
        {
            Connection = new MySqlConnection(connectionString);
        }

        public Transaction BeginTransaction()
        {
            if (transaction != null)
                return transaction;

            transaction = new Transaction(this, Connection);

            return transaction;
        }

        public void EndTransaction()
        {
            transaction?.Dispose();
            transaction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                EndTransaction();

                if (Connection != null)
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                    Connection.Dispose();
                }
            }
        }

        public class Transaction : IDisposable
        {
            static readonly Logger logger = LogManager.GetCurrentClassLogger();

            bool disposed;
            readonly DbManager dbManager;

            public IDbConnection Connection { get; set; }

            public Transaction(DbManager dbManager, IDbConnection connection)
            {
                this.dbManager = dbManager;
                Connection = connection;
                SetOpened();
            }

            bool IsConnectionOpen()
            {
                return Connection.State == ConnectionState.Open;
            }

            void SetOpened()
            {
                if (!IsConnectionOpen())
                {
                    Connection.Open();
                    logger.Debug("Transaction Opened");
                }
            }

            void SetClosed()
            {
                if (IsConnectionOpen())
                {
                    Connection.Close();
                    logger.Debug("Transaction Closed");
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    disposed = true;
                    SetClosed();
                    dbManager.EndTransaction();
                }
            }
        }
    }
}
