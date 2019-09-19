using System;
using System.Collections;
using System.Collections.Generic;
using ThirdStoreCommon;

namespace ThirdStoreBusiness.ReportPrint
{
    public interface IReportPrintService
    {
        PrintResult PrintLocalReport(LocalReportPrintingParameter printingParam);
    }
}
