using System;

namespace DataApi.Core.Domain
{
    public class Block
    {
        public string Id { get; set; }
        
        public int Number { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public int Size { get; set; }

        public int TransactionsCount { get; set; }

        public string PrevBlockId { get; set; }
    }
}
