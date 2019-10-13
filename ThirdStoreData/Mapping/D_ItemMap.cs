using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreData.Mapping
{
    public class D_ItemMap : EntityTypeConfiguration<D_Item>
    {
        public D_ItemMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.SKU)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Description)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

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

            this.Property(t => t.GrossWeight).HasPrecision(18, 8);
            this.Property(t => t.NetWeight).HasPrecision(18, 8);
            this.Property(t => t.CubicWeight).HasPrecision(18, 8);
            this.Property(t => t.Length).HasPrecision(18, 8);
            this.Property(t => t.Width).HasPrecision(18, 8);
            this.Property(t => t.Height).HasPrecision(18, 8);

            // Table & Column Mappings
            this.ToTable("D_Item");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.Type).HasColumnName("Type");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.Cost).HasColumnName("Cost");
            this.Property(t => t.Price).HasColumnName("Price");
            this.Property(t => t.GrossWeight).HasColumnName("GrossWeight");
            this.Property(t => t.NetWeight).HasColumnName("NetWeight");
            this.Property(t => t.CubicWeight).HasColumnName("CubicWeight");
            this.Property(t => t.Length).HasColumnName("Length");
            this.Property(t => t.Width).HasColumnName("Width");
            this.Property(t => t.Height).HasColumnName("Height");
            this.Property(t => t.SupplierID).HasColumnName("SupplierID");
            this.Property(t => t.IsReadyForList).HasColumnName("IsReadyForList");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.Ref1).HasColumnName("Ref1");
            this.Property(t => t.Ref2).HasColumnName("Ref2");
            this.Property(t => t.Ref3).HasColumnName("Ref3");
            this.Property(t => t.Ref4).HasColumnName("Ref4");
            this.Property(t => t.Ref5).HasColumnName("Ref5");
        }
    }
}
