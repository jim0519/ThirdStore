using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Misc;

namespace ThirdStoreData.Mapping
{
    public class D_NewAimSKUBarcodeMap : EntityTypeConfiguration<D_NewAimSKUBarcode>
    {
        public D_NewAimSKUBarcodeMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.SKU)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.AlternateSKU1)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.AlternateSKU2)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("D_NewAimSKUBarcode");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.AlternateSKU1).HasColumnName("AlternateSKU1");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

            //this.HasMany(r => r.RolePermissions)
            //    .WithMany()
            //    .Map(m =>
            //    {
            //        m.MapLeftKey("RoleID");
            //        m.MapRightKey("PermissionID");
            //        m.ToTable("M_RolePermission");
            //    });
        }
    }
}
