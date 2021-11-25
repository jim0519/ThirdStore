using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.ReturnItem;

namespace ThirdStoreData.Mapping
{
    public class D_ReturnItemLineMap : EntityTypeConfiguration<D_ReturnItemLine>
    {
        public D_ReturnItemLineMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.SKU)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Location)
                .IsRequired()
                .HasMaxLength(500);

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

            this.Property(t => t.Weight).HasPrecision(18, 8);
            this.Property(t => t.CubicWeight).HasPrecision(18, 8);
            this.Property(t => t.Length).HasPrecision(18, 8);
            this.Property(t => t.Width).HasPrecision(18, 8);
            this.Property(t => t.Height).HasPrecision(18, 8);

            // Table & Column Mappings
            this.ToTable("D_ReturnItemLine");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.HeaderID).HasColumnName("HeaderID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.Qty).HasColumnName("Qty");
            this.Property(t => t.Location).HasColumnName("Location");
            this.Property(t => t.Weight).HasColumnName("Weight");
            this.Property(t => t.Length).HasColumnName("Length");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.CubicWeight).HasColumnName("CubicWeight");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.Ref1).HasColumnName("Ref1");
            this.Property(t => t.Ref2).HasColumnName("Ref2");
            this.Property(t => t.Ref3).HasColumnName("Ref3");
            this.Property(t => t.Ref4).HasColumnName("Ref4");
            this.Property(t => t.Ref5).HasColumnName("Ref5");

            // Relationships
            this.HasRequired(t => t.ReturnItem)
                .WithMany(t => t.ReturnItemLines)
                .HasForeignKey(d => d.HeaderID);

            this.HasRequired(t => t.Item)
                .WithMany()
                .HasForeignKey(d => d.ItemID);
        }
    }
}
