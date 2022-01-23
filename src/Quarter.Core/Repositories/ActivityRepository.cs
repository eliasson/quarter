using System;
using System.Collections.Generic;
using Npgsql;
using Quarter.Core.Models;

namespace Quarter.Core.Repositories
{
    public interface IActivityRepository : IRepository<Activity>
    {
    }

    public class InMemoryActivityRepository : InMemoryRepositoryBase<Activity>, IActivityRepository
    {
    }

    public class PostgresActivityRepository : PostgresRepositoryBase<Activity>, IActivityRepository
    {
        private readonly IdOf<User> _userId;
        private const string TableName = "activity";
        private const string AggregateName = "Activity";
        private const string UserIdColumnName = "userid";

        public PostgresActivityRepository(IPostgresConnectionProvider connectionProvider, IdOf<User> userId)
            : base(connectionProvider, TableName, AggregateName)
        {
            _userId = userId;
        }

        protected override IEnumerable<string> AdditionalColumns()
            => new [] { UserIdColumnName };

        protected override object AdditionalColumnValue(string columnName, Activity aggregate)
            => columnName switch
            {
                UserIdColumnName => _userId.Id,
                _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
            };

        protected override NpgsqlParameter? WithAccessCondition()
            => new NpgsqlParameter(UserIdColumnName, _userId.Id);
    }
}