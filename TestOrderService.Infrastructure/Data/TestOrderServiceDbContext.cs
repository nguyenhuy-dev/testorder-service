using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TestOrderService.Domain.Entities;
namespace TestOrderService.Infrastructure.Data
{
    /// <summary>
    ///     Db Context of project.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class TestOrderServiceDbContext(DbContextOptions<TestOrderServiceDbContext> options) : DbContext(options)
    {

        /// <summary>
        ///     Gets or sets the test order patients.
        /// </summary>
        /// <value>
        ///     The test order patients.
        /// </value>
        public DbSet<TestOrder> TestOrders { get; set; }

        /// <summary>
        ///     Gets or sets the comments.
        /// </summary>
        /// <value>
        ///     The comments.
        /// </value>
        public DbSet<Comment> Comments { get; set; }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting
        ///     model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         If a model is explicitly set on the options for this context (via
        ///         <see
        ///             cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />
        ///         )
        ///         then this method will not be run. However, it will still run when creating a compiled model.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more
        ///         information and
        ///         examples.
        ///     </para>
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.HasDefaultSchema("public");
        }
    }
}
