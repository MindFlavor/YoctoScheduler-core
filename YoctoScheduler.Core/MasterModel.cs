namespace YoctoScheduler.Core
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Linq;

    public class MasterModel : DbContext
    {
        // Your context has been configured to use a 'MasterModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'YoctoScheduler.Core.MasterModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'MasterModel' 
        // connection string in the application configuration file.
        public MasterModel()
            : base("name=MasterModel")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<ExecutionStatus> ExecutionStatus { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Schedule> Scheduls { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExecutionStatus>()
                        .HasRequired(m => m.Server)
                        .WithMany(t => t.ExecutionStatuses)
                        .HasForeignKey(m => m.ServerID)
                        .WillCascadeOnDelete(true);

            modelBuilder.Entity<ExecutionStatus>()
                        .HasRequired(m => m.Task)
                        .WithMany(t => t.ExecutionStatuses)
                        .HasForeignKey(m => m.TaskID)
                        .WillCascadeOnDelete(false);

            #region IX_LastUpdate
            modelBuilder.Entity<ExecutionStatus>()
                        .Property(e => e.LastUpdate)
                        .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LastUpdate", 0)));

            modelBuilder.Entity<ExecutionStatus>()
                        .Property(e => e.TaskID)
                        .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LastUpdate", 1)));

            modelBuilder.Entity<ExecutionStatus>()
                        .Property(e => e.ServerID)
                        .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LastUpdate", 2)));
                
            modelBuilder.Entity<ExecutionStatus>()
                        .Property(e => e.Status)
                        .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_LastUpdate", 3)));
            #endregion
        }
    }

}