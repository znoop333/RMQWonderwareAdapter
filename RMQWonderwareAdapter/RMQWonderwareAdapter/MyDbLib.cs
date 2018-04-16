using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Win32Helper;

namespace RMQWonderwareAdapter
{
    class MyDbLib
    {
        private static void LogError(SqlException ex)
        {
            if (ex != null)
                Win32Helper.LogHelper.AppendToLogfile("SQL-error.txt", ex.ToString());
        }

        public static int? UPDATE_TRV_PCUPTIME(string PC_ID)
        {
            ProgramInfo pi = new ProgramInfo();
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.QueriesTableAdapter())
                {
                    return ta.UPDATE_TRV_PCUPTIME(PC_ID, ProgramInfo.Boottime, ProgramInfo.WinInfo, ProgramInfo.Loadtime, pi.Filename, pi.FileDescription, pi.FileVersion, pi.FileComments, pi.IP);
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
                return null;
            }
        }
        
        public static int? UPDATE_TRV_PCINFO(string PC_ID, DateTime LastUpdate, int InternalState, string SEQUENCE_NO, string VIN, DateTime SysTime)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.QueriesTableAdapter())
                {
                    return ta.UPDATE_TRV_PCINFO(PC_ID, LastUpdate, InternalState, SEQUENCE_NO, VIN, SysTime);
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
                return null;
            }
        }

        public static OmmcMesDataSet.PLC_TAG_LOOKUP_PLC_IPRow PLC_TAG_LOOKUP_PLC_IP(string WW_ITEM_NAME)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_LOOKUP_PLC_IPTableAdapter())
                {
                    var dt = ta.GetData(WW_ITEM_NAME);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt[0];
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
            }
            return null;
        }

        public static OmmcMesDataSet.PLC_TAG_LOOKUP_WW_ITEM_NAMERow PLC_TAG_LOOKUP_WW_ITEM_NAME(string PLC_IP, string TAG_ID)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_LOOKUP_WW_ITEM_NAMETableAdapter())
                {
                    var dt = ta.GetData(PLC_IP, TAG_ID);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt[0];
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
            }
            return null;
        }

        public static int PLC_TAG_SUBSCRIBED(string PLC_IP, string TAG_ID, string RequesterIP, string RequesterName, string CorrelationId)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_SUBSCRIBEDTableAdapter())
                {
                    var dt = ta.GetData(PLC_IP, TAG_ID, RequesterIP, RequesterName, CorrelationId);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt[0].insertedRows;
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
            }
            return -1;
        }

        public static int PLC_TAG_SUBSCRIPTION_CHANGED(string PLC_IP, string TAG_ID, string USE_YN, string ADVISED_YN, string RequesterIP, string RequesterName, string CorrelationId)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_SUBSCRIPTION_CHANGEDTableAdapter())
                {
                    var dt = ta.GetData(PLC_IP, TAG_ID, USE_YN, ADVISED_YN, RequesterIP, RequesterName, CorrelationId);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt[0].updatedRows;
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
            }
            return -1;
        }

        public static int PLC_TAG_UPDATED(string PLC_IP, string TAG_ID, string Value, DateTime Updated, int Quality)
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_UPDATEDTableAdapter())
                {
                    var dt = ta.GetData(PLC_IP, TAG_ID, Value, Updated, Quality);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt[0].updatedRows;
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
            }
            return -1;
        }

        public static OmmcMesDataSet.PLC_TAG_LIST_USEDDataTable PLC_TAG_LIST_USED()
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_LIST_USEDTableAdapter())
                {
                    return ta.GetData();
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
                return null;
            }
        }

        public static OmmcMesDataSet.PLC_TAG_LIST_ADVISEDDataTable PLC_TAG_LIST_ADVISED()
        {
            try
            {
                using (var ta = new OmmcMesDataSetTableAdapters.PLC_TAG_LIST_ADVISEDTableAdapter())
                {
                    return ta.GetData();
                }
            }
            catch (SqlException ex)
            {
                LogError(ex);
                return null;
            }
        }


    }
}
