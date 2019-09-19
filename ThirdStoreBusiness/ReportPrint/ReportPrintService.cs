
using System;
using System.IO;
using System.Drawing;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using System.Collections;
using System.Collections.Generic;

namespace ThirdStoreBusiness.ReportPrint
{
    public class ReportPrintService : IReportPrintService
    {
        private readonly ILocalReportPrinting _localReportPrinting;

        public ReportPrintService(ILocalReportPrinting localReportPrinting)
        {
            _localReportPrinting = localReportPrinting;
        }

        public PrintResult PrintLocalReport(LocalReportPrintingParameter printingParam)
        {
            PrintResult pr = new PrintResult();
            try
            {
                //foreach (var datasource in datasources)
                //{
                //    printingParam.Datasources.Add(datasource.Key, datasource.Value);
                //}
                printingParam.PrinterName = ThirdStoreConfig.Instance.ThirdStorePrinterName;

                _localReportPrinting.SetReportPrintingParameters(printingParam);

                pr = _localReportPrinting.PrintReport();

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.InnerException.Message+ex.InnerException.StackTrace);
                pr.Success = false;
                pr.Message = ex.ToString();
            }

            return pr;
        }
    }



    public class ThirdStoreBarcodeReportPrintingParameter : LocalReportPrintingParameter
    {
        public ThirdStoreBarcodeReportPrintingParameter()
        {
            
        }

        public override string ReportPath
        {
            get
            {
                return ThirdStoreConfig.Instance.ThirdStoreBarcodeReportPath;
            }
        }

    }

    public class PrintModelBase
    {

    }

    public class ThirdStorePrintModel : PrintModelBase
    {
        public ThirdStoreBarcodeData[] DataSet1 { get; set; }
    }

    public class ThirdStoreBarcodeData
    {
        public int ID { get; set; }
    }
}
