using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Image;

namespace ThirdStoreData.Mapping
{
    public class M_JobItemImageMap : EntityTypeConfiguration<M_JobItemImage>
    {
        public M_JobItemImageMap()
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
            this.ToTable("M_JobItemImage");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.JobItemID).HasColumnName("JobItemID");
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
            this.HasRequired(t => t.JobItem)
                .WithMany(t => t.JobItemImages)
                .HasForeignKey(d => d.JobItemID);

        }
    }
}
