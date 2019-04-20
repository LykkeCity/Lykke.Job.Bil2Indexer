using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    public static class PgExceptionsHelpers
    {
        public static bool IsConstraintViolationException(this DbUpdateException e)
        {
            return e.InnerException is PostgresException pgEx && pgEx.IsConstraintViolationException();
        }


        public static bool IsConstraintViolationException(this PostgresException e)
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
