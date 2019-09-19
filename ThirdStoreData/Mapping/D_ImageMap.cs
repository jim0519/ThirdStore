using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreData.Mapping
{
    public class D_ImageMap : EntityTypeConfiguration<D_Image>
    {
        public D_ImageMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.ImageName)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ImageLocalPath)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("D_Image");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ImageName).HasColumnName("ImageName");
            this.Property(t => t.ImageLocalPath).HasColumnName("ImageLocalPath");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
        }
    }
}
