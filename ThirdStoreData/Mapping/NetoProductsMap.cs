using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreData.Mapping
{
    public class NetoProductsMap : EntityTypeConfiguration<NetoProducts>
    {
        public NetoProductsMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);


            // Properties

            this.Property(t => t.NetoProductID)
                .HasMaxLength(30);

            this.Property(t => t.SKU)
                .HasMaxLength(128);

            this.Property(t => t.Name)
                .HasMaxLength(500);

            this.Property(t => t.Description)
                .HasColumnType("nvarchar(max)");

            this.Property(t => t.PrimarySupplier)
                .HasMaxLength(30);

            this.Property(t => t.Image1)
                .HasMaxLength(500);

            this.Property(t => t.Image2)
                .HasMaxLength(500);

            this.Property(t => t.Image3)
                .HasMaxLength(500);

            this.Property(t => t.Image4)
                .HasMaxLength(500);

            this.Property(t => t.Image5)
                .HasMaxLength(500);

            this.Property(t => t.Image6)
                .HasMaxLength(500);

            this.Property(t => t.Image7)
                .HasMaxLength(500);

            this.Property(t => t.Image8)
                .HasMaxLength(500);

            this.Property(t => t.Image9)
                .HasMaxLength(500);

            this.Property(t => t.Image10)
                .HasMaxLength(500);

            this.Property(t => t.Image11)
                .HasMaxLength(500);

            this.Property(t => t.Image12)
                .HasMaxLength(500);

            this.Property(t => t.ShippingLength)
                .HasMaxLength(30);

            this.Property(t => t.ShippingHeight)
                .HasMaxLength(30);

            this.Property(t => t.ShippingWidth)
                .HasMaxLength(30);

            this.Property(t => t.ShippingWeight)
                .HasMaxLength(30);

            this.Property(t => t.Qty)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("NetoProducts");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.NetoProductID).HasColumnName("NetoProductID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Description).HasColumnName("Description");
            this.Property(t => t.DefaultPrice).HasColumnName("DefaultPrice");
            this.Property(t => t.PrimarySupplier).HasColumnName("PrimarySupplier");
            this.Property(t => t.Image1).HasColumnName("Image1");
            this.Property(t => t.Image2).HasColumnName("Image2");
            this.Property(t => t.Image3).HasColumnName("Image3");
            this.Property(t => t.Image4).HasColumnName("Image4");
            this.Property(t => t.Image5).HasColumnName("Image5");
            this.Property(t => t.Image6).HasColumnName("Image6");
            this.Property(t => t.Image7).HasColumnName("Image7");
            this.Property(t => t.Image8).HasColumnName("Image8");
            this.Property(t => t.Image9).HasColumnName("Image9");
            this.Property(t => t.Image10).HasColumnName("Image10");
            this.Property(t => t.Image11).HasColumnName("Image11");
            this.Property(t => t.Image12).HasColumnName("Image12");
            this.Property(t => t.ShippingLength).HasColumnName("ShippingLength");
            this.Property(t => t.ShippingHeight).HasColumnName("ShippingHeight");
            this.Property(t => t.ShippingWidth).HasColumnName("ShippingWidth");
            this.Property(t => t.ShippingWeight).HasColumnName("ShippingWeight");
            this.Property(t => t.Qty).HasColumnName("Qty");
        }
    }
}
