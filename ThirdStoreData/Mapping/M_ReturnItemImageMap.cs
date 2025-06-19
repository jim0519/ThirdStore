using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreData.Mapping
{
    public class M_ReturnItemImageMap : EntityTypeConfiguration<M_ReturnItemImage>
    {
        public M_ReturnItemImageMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("M_ReturnItemImage");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ReturnItemID).HasColumnName("ReturnItemID");
            this.Property(t => t.ImageID).HasColumnName("ImageID");
            this.Property(t => t.DisplayOrder).HasColumnName("DisplayOrder");
            this.Property(t => t.StatusID).HasColumnName("StatusID");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

            // Relationships
            //this.HasRequired(t => t.Image)
            //    .WithMany(t => t.ItemImages)
            //    .HasForeignKey(d => d.ImageID);
            this.HasRequired(t => t.ReturnItem)
                .WithMany(t => t.ReturnItemImages)
                .HasForeignKey(d => d.ReturnItemID);

        }
    }
}
