using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.ReturnItem;

namespace ThirdStoreData.Mapping
{
    public class D_ReturnItemMap : EntityTypeConfiguration<D_ReturnItem>
    {
        public D_ReturnItemMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties

            this.Property(t => t.DesignatedSKU)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.TrackingNumber)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Note)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Ref1)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.Ref2)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.Ref3)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.Ref4)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.Ref5)
               .IsRequired()
               .HasMaxLength(4000);

            this.Property(t => t.Note)
                .IsRequired()
                .HasMaxLength(4000);

            // Table & Column Mappings
            this.ToTable("D_ReturnItem");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.StatusID).HasColumnName("StatusID");
            this.Property(t => t.DesignatedSKU).HasColumnName("DesignatedSKU");
            this.Property(t => t.Note).HasColumnName("Note");
            this.Property(t => t.TrackingNumber).HasColumnName("TrackingNumber");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.Ref1).HasColumnName("Ref1");
            this.Property(t => t.Ref2).HasColumnName("Ref2");
            this.Property(t => t.Ref3).HasColumnName("Ref3");
            this.Property(t => t.Ref4).HasColumnName("Ref4");
            this.Property(t => t.Ref5).HasColumnName("Ref5");
            this.Property(t => t.Note).HasColumnName("Note");
        }
    }
}