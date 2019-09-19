using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ThirdStoreCommon.Models
{
    public class BaseEntity
    {
        public int ID { get; set; }
        
    }

    public class XMLObject
    {
        [XmlIgnoreAttribute]
        public string xmlString { get; set; }
    }
}
