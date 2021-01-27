using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ThirdStoreCommon.Models.Order;

namespace ThirdStoreData.Mapping
{
    public class D_Order_HeaderMap : EntityTypeConfiguration<D_Order_Header>
    {
        public D_Order_HeaderMap()
        {
            // Primary Key
            this.HasKey(t => t.ID);

            // Properties

            this.Property(t => t.ChannelOrderID)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.CustomerID)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ConsigneeName)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingAddress1)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingAddress2)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingSuburb)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingState)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingPostcode)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingCountry)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ConsigneeEmail)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ConsigneePhoneNo)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingName)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingAddress1)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingAddress2)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingSuburb)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingState)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingPostcode)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingCountry)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingEmail)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BillingPhoneNo)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ShippingMethod)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.PaymentMethod)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.PaymentTransactionID)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.Carrier)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.BuyerNote)
                .IsRequired()
                .HasMaxLength(4000);

            this.Property(t => t.OrderNote)
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

            this.Property(t => t.CreateBy)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.EditBy)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("D_Order_Header");
            this.Property(t => t.ID).HasColumnName("ID");
            this.Property(t => t.TypeID).HasColumnName("TypeID");
            this.Property(t => t.StatusID).HasColumnName("StatusID");
            this.Property(t => t.OrderTime).HasColumnName("OrderTime");
            this.Property(t => t.ChannelOrderID).HasColumnName("ChannelOrderID");
            this.Property(t => t.CustomerID).HasColumnName("CustomerID");
            this.Property(t => t.ConsigneeName).HasColumnName("ConsigneeName");
            this.Property(t => t.ShippingAddress1).HasColumnName("ShippingAddress1");
            this.Property(t => t.ShippingAddress2).HasColumnName("ShippingAddress2");
            this.Property(t => t.ShippingSuburb).HasColumnName("ShippingSuburb");
            this.Property(t => t.ShippingState).HasColumnName("ShippingState");
            this.Property(t => t.ShippingPostcode).HasColumnName("ShippingPostcode");
            this.Property(t => t.ShippingCountry).HasColumnName("ShippingCountry");
            this.Property(t => t.ConsigneeEmail).HasColumnName("ConsigneeEmail");
            this.Property(t => t.ConsigneePhoneNo).HasColumnName("ConsigneePhoneNo");
            this.Property(t => t.BillingName).HasColumnName("BillingName");
            this.Property(t => t.BillingAddress1).HasColumnName("BillingAddress1");
            this.Property(t => t.BillingAddress2).HasColumnName("BillingAddress2");
            this.Property(t => t.BillingSuburb).HasColumnName("BillingSuburb");
            this.Property(t => t.BillingState).HasColumnName("BillingState");
            this.Property(t => t.BillingPostcode).HasColumnName("BillingPostcode");
            this.Property(t => t.BillingCountry).HasColumnName("BillingCountry");
            this.Property(t => t.BillingEmail).HasColumnName("BillingEmail");
            this.Property(t => t.BillingPhoneNo).HasColumnName("BillingPhoneNo");
            this.Property(t => t.SubTotal).HasColumnName("SubTotal");
            this.Property(t => t.Postage).HasColumnName("Postage");
            this.Property(t => t.TotalAmount).HasColumnName("TotalAmount");
            this.Property(t => t.ShippingMethod).HasColumnName("ShippingMethod");
            this.Property(t => t.PaymentMethod).HasColumnName("PaymentMethod");
            this.Property(t => t.PaymentTransactionID).HasColumnName("PaymentTransactionID");
            this.Property(t => t.PaidTime).HasColumnName("PaidTime");
            this.Property(t => t.Carrier).HasColumnName("Carrier");
            this.Property(t => t.BuyerNote).HasColumnName("BuyerNote");
            this.Property(t => t.OrderNote).HasColumnName("OrderNote");
            this.Property(t => t.Ref1).HasColumnName("Ref1");
            this.Property(t => t.Ref2).HasColumnName("Ref2");
            this.Property(t => t.Ref3).HasColumnName("Ref3");
            this.Property(t => t.Ref4).HasColumnName("Ref4");
            this.Property(t => t.Ref5).HasColumnName("Ref5");
            this.Property(t => t.CreateTime).HasColumnName("CreateTime");
            this.Property(t => t.CreateBy).HasColumnName("CreateBy");
            this.Property(t => t.EditTime).HasColumnName("EditTime");
            this.Property(t => t.EditBy).HasColumnName("EditBy");
            this.Property(t => t.ShipmentTime).HasColumnName("ShipmentTime");
        }
    }
}
