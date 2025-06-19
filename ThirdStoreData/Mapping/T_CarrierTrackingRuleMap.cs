using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Misc;
using ThirdStoreCommon.Models.ReturnItem;

namespace ThirdStoreData.Mapping
{
    public class T_CarrierTrackingRuleMap : EntityTypeConfiguration<T_CarrierTrackingRule>
    {
        public T_CarrierTrackingRuleMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            this.Property(t => t.CarrierMatchCode)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.CarrierName)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("T_CarrierTrackingRule");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.CarrierMatchCode).HasColumnName("CarrierMatchCode");
            this.Property(t => t.TrackingPrefixDigit).HasColumnName("TrackingPrefixDigit");
            this.Property(t => t.TrackingMainDigit).HasColumnName("TrackingMainDigit");
            this.Property(t => t.SupplierID).HasColumnName("SupplierID");
            this.Property(t => t.CarrierName).HasColumnName("CarrierName");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

        }
    }
}
