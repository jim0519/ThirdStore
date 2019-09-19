using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreData.Mapping
{
    public class V_ItemRelationshipMap : EntityTypeConfiguration<V_ItemRelationship>
    {
        public V_ItemRelationshipMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties
            

            // Table & Column Mappings
            this.ToTable("V_SKURelationship_Recursive");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ItemID).HasColumnName("ItemID");
            this.Property(t => t.SKU).HasColumnName("SKU");
            this.Property(t => t.ItemType).HasColumnName("ItemType");
            this.Property(t => t.Qty).HasColumnName("Qty");
            this.Property(t => t.BottomItemID).HasColumnName("BottomItemID");
            this.Property(t => t.BottomSKU).HasColumnName("BottomSKU");
        }
    }
}
