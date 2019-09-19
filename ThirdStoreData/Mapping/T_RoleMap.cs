using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.AccessControl;

namespace ThirdStoreData.Mapping
{
    public class T_RoleMap : EntityTypeConfiguration<T_Role>
    {
        public T_RoleMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("T_Role");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.IsActive).HasColumnName("IsActive");

            this.HasMany(r => r.RolePermissions)
                .WithMany()
                .Map(m =>
                {
                    m.MapLeftKey("RoleID");
                    m.MapRightKey("PermissionID");
                    m.ToTable("M_RolePermission");
                });
        }
    }
}
