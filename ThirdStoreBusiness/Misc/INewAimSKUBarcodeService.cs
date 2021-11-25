using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Misc;
using ThirdStoreData;

namespace ThirdStoreBusiness.Misc
{
    public interface INewAimSKUBarcodeService
    {
        string GetSKUByBarcode(string barcode);
    }

    public class NewAimSKUBarcodeService : INewAimSKUBarcodeService
    {
        private readonly IRepository<D_NewAimSKUBarcode> _newaimSKUBarcodeRepository;
        public NewAimSKUBarcodeService(IRepository<D_NewAimSKUBarcode> newaimSKUBarcodeRepository
            )
        {
            _newaimSKUBarcodeRepository = newaimSKUBarcodeRepository;
        }

        public string GetSKUByBarcode(string barcode)
        {
            var query = _newaimSKUBarcodeRepository.Table.Where(s => s.AlternateSKU2.Equals(barcode)).FirstOrDefault();
            if (query != null)
                return query.SKU;
            else
                return string.Empty;
        }
    }
}
