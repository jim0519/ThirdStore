using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Misc;

namespace ThirdStoreData.Mapping
{
    public class T_LogMap : EntityTypeConfiguration<T_Log>
    {
        public T_LogMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Thread)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Level)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Logger)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Message)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.Exception)
                .HasMaxLength(2000);

            // Table & Column Mappings
            this.ToTable("T_Log");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Date).HasColumnName("Date");
            this.Property(t => t.Thread).HasColumnName("Thread");
            this.Property(t => t.Level).HasColumnName("Level");
            this.Property(t => t.Logger).HasColumnName("Logger");
            this.Property(t => t.Message).HasColumnName("Message");
            this.Property(t => t.Exception).HasColumnName("Exception");

            //this.HasMany(r => r.RolePermissions)
            //    .WithMany()
            //    .Map(m =>
            //    {
            //        m.MapLeftKey("RoleID");
            //        m.MapRightKey("PermissionID");
            //        m.ToTable("M_RolePermission");
            //    });
        }
    }
}
