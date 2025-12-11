//using EventSpark.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.InMemory; // Added this namespace for UseInMemoryDatabase extension method  

//namespace EventSpark.Tests.TestHelpers
//{
//    internal static class TestDbFactory
//    {
//        public static EventSparkDbContext CreateInMemoryDbContext(string databaseName)
//        {
//            var options = new DbContextOptionsBuilder<EventSparkDbContext>()
//                .UseInMemoryDatabase(databaseName) // This requires the Microsoft.EntityFrameworkCore.InMemory package  
//                .Options;

//            return new EventSparkDbContext(options);
//        }
//    }
//}
