using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    public static class PgExceptionsHelpers
    {
        public static bool IsConstraintViolationException(this DbUpdateException e)
        {
            const string constraintViolationErrorCode = "23505";
            if (e.InnerException is PostgresException pgEx)
            {
                if (string.Equals(pgEx.SqlState, constraintViolationErrorCode, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
