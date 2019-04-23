using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    internal static class PgExceptionsHelpers
    { 
        // primary key or unique index whick contains this considered as natural key. Should applied to db scheme
        private const string NaturalKeyNamingPolicy = "natural_key";
        public static bool IsNaturalKeyViolationException(this DbUpdateException e)
        {
            return e.InnerException is PostgresException pgEx && pgEx.IsUniqueConstraintViolationException();
        }


        public static bool IsUniqueConstraintViolationException(this PostgresException e)
        {
            const string constraintViolationErrorCode = "23505";
            if (string.Equals(e.SqlState, constraintViolationErrorCode, StringComparison.InvariantCultureIgnoreCase)
                && e.ConstraintName.Contains(NaturalKeyNamingPolicy))
            {
                return true;
            }

            return false;
        }
    }
}
