namespace DataApi.Models.Common
{
    public class Paginated<T>
    {
        public PaginationModel Pagination { get; set; }
        public T[] Data { get; set; }
    }
}
