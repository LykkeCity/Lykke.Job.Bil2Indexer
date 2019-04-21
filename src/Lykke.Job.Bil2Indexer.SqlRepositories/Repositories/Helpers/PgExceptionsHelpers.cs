using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    internal static class PgExceptionsHelpers
    {
        public static bool IsUniqueConstraintViolationException(this DbUpdateException e)
        {
            return e.InnerException is PostgresException pgEx && pgEx.IsUniqueConstraintViolationException();
        }


        public static bool IsUniqueConstraintViolationException(this PostgresException e)
        {
            const string constraintViolationErrorCode = "23505";
            if (string.Equals(e.SqlState, constraintViolationErrorCode, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
