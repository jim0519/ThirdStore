using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Item;

namespace ThirdStoreData.Mapping
{
    public class D_Item_RelationshipMap : EntityTypeConfiguration<D_Item_Relationship>
    {
        public D_Item_RelationshipMap()
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
            this.ToTable("D_Item_Relationship");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.ParentItemID).HasColumnName("ParentItemID");
            this.Property(t => t.ChildItemID).HasColumnName("ChildItemID");
            this.Property(t => t.ChildItemQty).HasColumnName("ChildItemQty");
            this.Property(t => t.DisplayOrder).HasColumnName("DisplayOrder");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");

            // Relationships
            this.HasRequired(t => t.ChildItem)
                .WithMany(t => t.ParentItems)
                .HasForeignKey(d => d.ChildItemID);
            this.HasRequired(t => t.ParentItem)
                .WithMany(t => t.ChildItems)
                .HasForeignKey(d => d.ParentItemID);

        }
    }
}
