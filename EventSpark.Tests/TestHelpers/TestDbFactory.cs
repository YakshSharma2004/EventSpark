using EventSpark.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory; 

namespace EventSpark.Tests.TestHelpers
{
    internal static class TestDbFactory
    {
        public static EventSparkDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<EventSparkDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new EventSparkDbContext(options);
        }
    }
}
