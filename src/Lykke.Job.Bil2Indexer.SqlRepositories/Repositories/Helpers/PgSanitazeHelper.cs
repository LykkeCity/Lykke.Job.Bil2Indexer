using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    public static class PgSanitazeHelper
    {
        public static string SanitazeJson(this string source)
        {
            //https://stackoverflow.com/questions/31671634/handling-unicode-sequences-in-postgresql
            return source?.Replace('\u0000'.ToString(), "");
        }
    }
}
