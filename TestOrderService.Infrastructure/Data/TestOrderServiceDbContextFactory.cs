using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace TestOrderService.Infrastructure.Data
{
    /// <summary>
    ///     Supporter adds new migrations.
    /// </summary>
    /// <seealso
    ///     cref="Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory&lt;TestOrderService.Infrastructure.Data.PatientTestOrderServiceDbContext&gt;" />
    public class TestOrderServiceDbContextFactory : IDesignTimeDbContextFactory<TestOrderServiceDbContext>
    {
        /// <summary>
        ///     Creates a new instance of a derived context.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time service.</param>
        /// <returns>
        ///     An instance of <typeparamref name="TContext" />.
        /// </returns>
        public TestOrderServiceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TestOrderService.API"))
                .AddUserSecrets("939ca807-aa3d-4acc-ae94-b354db4aa375")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<TestOrderServiceDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new TestOrderServiceDbContext(optionsBuilder.Options);
        }
    }
}
