using System.Collections.Generic;

namespace ThirdStoreBusiness.Setting
{
    public class DropshipzoneAPISettings : ISettings
    {
        public DropshipzoneAPISettings()
        {

        }

        public string URL { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
