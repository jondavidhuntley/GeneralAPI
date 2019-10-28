using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GeneralAPI.Model
{
    /// <summary>
    /// DbContext for The Document store
    /// </summary>
    /// <seealso cref="DbContext" />
    public class DocumentContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public DocumentContext(DbContextOptions<DocumentContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the reports.
        /// </summary>
        /// <value>
        /// The reports.
        /// </value>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in Microsoft.EntityFrameworkCore.DbSet`1 properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>().Property(b => b.Created).HasDefaultValueSql("getutcdate()");
            modelBuilder.Entity<Report>().Property(b => b.Period).HasConversion(new EnumToStringConverter<ReportPeriod>());
        }
    }
}