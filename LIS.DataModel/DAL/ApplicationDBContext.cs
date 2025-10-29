using LIS.DtoModel.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace LIS.DataAccess
{
    /// <summary>
    /// Application DB Context Class,Inherits DbContext
    /// </summary>
    [DbConfigurationType(typeof(EntityFrameworkConfiguration))]
    public class ApplicationDBContext : DbContext
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ApplicationDBContext() : base("name=DefaultConnection")
        {
            Database.SetInitializer<ApplicationDBContext>(new CreateDatabaseIfNotExists<ApplicationDBContext>());
        }

        public static ApplicationDBContext Create()
        {
            return new ApplicationDBContext();
        }

        
        //Equipment Master and mapping with HIS
        public virtual DbSet<EquipmentMaster> EquipmentMaster { get; set; }
        
        public virtual DbSet<LisTestValue> LisTestValues { get; set; }
        // AccuHealth Data
        public virtual DbSet<AccuHealthTestValue> AccuHealthTestValues { get; set; }
        public virtual DbSet<AccuHealthTestOrder> AccuHealthTestOrders { get; set; }
        public virtual DbSet<AccuHealthParamMapping> AccuHealthParamMappings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                //modelBuilder.Entity<AccuHealthTestOrder>()
                //.HasIndex(entity => new { entity.REF_VISITNO, entity.PARAMCODE, entity.Status }).IsUnique();

                modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
