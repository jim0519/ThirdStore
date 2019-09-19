using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.AccessControl;

namespace ThirdStoreData.Mapping
{
    public class T_PermissionMap : EntityTypeConfiguration<T_Permission>
    {
        public T_PermissionMap()
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
            this.ToTable("T_Permission");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
           
        }
    }
}
