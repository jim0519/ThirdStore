using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Order;

namespace ThirdStoreData.Mapping
{
    public class D_Order_LineMap : EntityTypeConfiguration<D_Order_Line>
    {
        public D_Order_LineMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.SKU)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref1)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref2)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref3)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref4)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref5)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref6)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref7)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref8)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref9)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Ref10)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("D_Order_Line");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.HeaderID).HasColumnName("HeaderID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.Qty).HasColumnName("Qty");
            this.Property(t => t.ItemPrice).HasColumnName("ItemPrice");
            this.Property(t => t.SubTotal).HasColumnName("SubTotal");
            this.Property(t => t.Ref1).HasColumnName("Ref1");
            this.Property(t => t.Ref2).HasColumnName("Ref2");
            this.Property(t => t.Ref3).HasColumnName("Ref3");
            this.Property(t => t.Ref4).HasColumnName("Ref4");
            this.Property(t => t.Ref5).HasColumnName("Ref5");
            this.Property(t => t.Ref6).HasColumnName("Ref6");
            this.Property(t => t.Ref7).HasColumnName("Ref7");
            this.Property(t => t.Ref8).HasColumnName("Ref8");
            this.Property(t => t.Ref9).HasColumnName("Ref9");
            this.Property(t => t.Ref10).HasColumnName("Ref10");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

            // Relationships
            this.HasRequired(t => t.Order)
                .WithMany(t => t.OrderLines)
                .HasForeignKey(d => d.HeaderID);

        }
    }
}
