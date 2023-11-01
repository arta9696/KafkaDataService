using KafkaDataService.Models;
using Microsoft.EntityFrameworkCore;

namespace KafkaDataService
{
    public class WebContext: DbContext
    {
        public WebContext(DbContextOptions<WebContext> options)
            : base(options)
        {
        }

        public DbSet<Posts> Posts { get; set; } = default!;
        public DbSet<Comments> Comments { get; set; } = default!;
    }
}
