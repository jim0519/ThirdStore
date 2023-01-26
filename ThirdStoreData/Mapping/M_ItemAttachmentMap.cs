using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Attachment;

namespace ThirdStoreData.Mapping
{
    public class M_ItemAttachmentMap : EntityTypeConfiguration<M_ItemAttachment>
    {
        public M_ItemAttachmentMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties

            this.Property(t => t.Notes)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("M_ItemAttachment");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.AttachmentID).HasColumnName("AttachmentID");
            this.Property(t => t.DisplayOrder).HasColumnName("DisplayOrder");
            this.Property(t => t.Notes).HasColumnName("Notes");
            this.Property(t => t.StatusID).HasColumnName("StatusID");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

            // Relationships
            //this.HasRequired(t => t.Image)
            //    .WithMany(t => t.ItemImages)
            //    .HasForeignKey(d => d.ImageID);
            this.HasRequired(t => t.Item)
                .WithMany(t => t.ItemAttachments)
                .HasForeignKey(d => d.ItemID);

        }
    }
}
