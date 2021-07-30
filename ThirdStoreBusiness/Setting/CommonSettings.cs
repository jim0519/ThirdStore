using System.Collections.Generic;

namespace ThirdStoreBusiness.Setting
{
    public class CommonSettings : ISettings
    {
        public CommonSettings()
        {

        }

        public string ShippingDescriptionTemplate { get; set; }

        public decimal DropshipMarkupRate { get; set; }
    }
}
