using ArchestrA.MxAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMQWonderwareAdapter
{

    /// <summary>
    /// stores handle and other information for a given PLC tag (scalar value)
    /// </summary>
    public class WWMxItem
    {
        public string ItemName { get; set; }
        public Type ItemType { get; set; }
        public object LastValue { get; set; }
        public int LastQuality { get; set; }
        public string LastTimestamp { get; set; }
        public int hItem { get; set; }
        public bool Added { get; set; }
        public bool OnAdvise { get; set; }
        public bool ReadOnlyOnce { get; set; } // unadvise as soon as first read is done (but keep it if Writing is true)
        public bool Writing { get; set; } // was this Advised for writing?
        public string CorrelationId { get; set; }
    }

    public class WWMxWriteItemInfo
    {
        public WWMxItem Item { get; set; }
        public string Message { get; set; }
        public bool WriteOK { get; set; }
    }

    /// <summary>
    /// manages a list of PLC tags in Wonderware
    /// </summary>
    class WWMxAccessManager : IDisposable
    {
        ArchestrA.MxAccess.LMXProxyServerClass LMX_Server = null;
        private Dictionary<int, WWMxItem> RegisteredItems;

        public List<WWMxItem> GetSubScriptions()
        {
            var L = new List<WWMxItem>();

            foreach (var item in RegisteredItems)
            {
                if (item.Value.OnAdvise)
                {
                    L.Add(item.Value);
                }
            }

            return L;
        }

        public List<WWMxItem> GetAllTags()
        {
            return RegisteredItems.Values.ToList();
        }

        // handle of registered LMX server interface
        int hLMX = 0;

        public WWMxAccessManager()
        {
            LMX_Server = null;
        }

        public event EventHandler<String> LogMessage;
        private void PostLogMessage(string s)
        {
            LogMessage?.Invoke(this, s);
        }

        public event EventHandler<WWMxItem> DataChange;
        private void PostDataChange(WWMxItem i)
        {
            DataChange?.Invoke(this, i);
        }

        public event EventHandler<WWMxWriteItemInfo> WriteCompleted;
        private void PostWriteCompleted(WWMxWriteItemInfo i)
        {
            WriteCompleted?.Invoke(this, i);
        }

        public bool Subscribe(string strItemName, string CorrelationId)
        {
            return Advise(strItemName, false, CorrelationId);
        }

        public bool Unsubscribe(string strItemName, string CorrelationId)
        {
            return UnAdvise(strItemName, CorrelationId);
        }

        public bool ReadOnce(string strItemName, string CorrelationId)
        {
            return Advise(strItemName, true, CorrelationId);
        }

        public WWMxItem GetLastValue(string strItemName, string CorrelationId)
        {
            return LookupItem(strItemName);
        }

        public bool Write(string strItemName, object Value, string CorrelationId)
        {
            if(!Advise(strItemName, false, CorrelationId))
                return false;

            try
            {
                WWMxItem i = LookupItem(strItemName);
                if (i == null)
                    return false;

                i.ItemType = Value.GetType();
                i.LastValue = Value;
                i.Writing = true;

                LMX_Server.Write(hLMX, i.hItem, Value, 0);
                return true;
            }
            catch (Exception ex)
            {
                PostLogMessage(ex.ToString());
            }

            return false;
        }

        public bool Register()
        {
            try
            {
                if (LMX_Server == null)
                {
                    // instantiate an ArchestrA.MxAccess.LMXProxyServer
                    try
                    {
                        LMX_Server = new ArchestrA.MxAccess.LMXProxyServerClass();
                    }
                    catch (Exception ex)
                    {
                        PostLogMessage(ex.ToString());
                        return false;
                    }

                    // check whether Communication Management and SupervisoryConnection are available
                    bCommunicationManagementAvailable = false;
                    bSupervisoryConnectionAvailable = false;
                    if (LMX_Server != null)
                    {
                        if (LMX_Server is ILMXProxyServer4)
                        {
                            bCommunicationManagementAvailable = true;
                            bSupervisoryConnectionAvailable = true;
                        }
                    }
                }
                if ((LMX_Server != null) && (hLMX == 0))
                {
                    // Register with LMX and get the Registration handle
                    hLMX = LMX_Server.Register("RMQWonderwareAdapter");

                    RegisteredItems = new Dictionary<int, WWMxItem>();

                    // connect the event handlers
                    LMX_Server.OnDataChange += new _ILMXProxyServerEvents_OnDataChangeEventHandler(LMX_OnDataChange);
                    LMX_Server.OnWriteComplete += new _ILMXProxyServerEvents_OnWriteCompleteEventHandler(LMX_OnWriteComplete);

                    PostLogMessage("Registered");
                    return true;
                }
            }
            catch (Exception ex)
            {
                PostLogMessage("Register EX " + ex.ToString());
                return false;
            }

            return false;
        }

        private void LMX_OnDataChange(int hLMXServerHandle, int phItemHandle, object pvItemValue, int pwItemQuality, object pftItemTimeStamp, ref ArchestrA.MxAccess.MXSTATUS_PROXY[] ItemStatus)
        {
            WWMxItem i;
            if(RegisteredItems.TryGetValue(phItemHandle, out i))
            {
                if (ItemStatus[0].success != 0)
                {
                    i.LastValue = pvItemValue;
                    i.LastTimestamp = pftItemTimeStamp as string;
                    i.LastQuality = pwItemQuality;
                    i.ItemType = pvItemValue.GetType();
                    PostDataChange(i);

                    if(i.ReadOnlyOnce && !i.Writing)
                    {
                        UnAdvise(i.ItemName, i.CorrelationId);
                        PostLogMessage("UnAdvising " + i.ItemName + " ReadOnlyOnce ");
                    }
                }
                else
                {
                    var s = "OnDataChange w/error - cat: " + ItemStatus[0].category + "  Src: " + ItemStatus[0].detectedBy + "   detail: " + ItemStatus[0].detail;
                    PostLogMessage(s);
                }
            }
        }

        private void LMX_OnWriteComplete(int hLMXServerHandle, int phItemHandle, ref ArchestrA.MxAccess.MXSTATUS_PROXY[] ItemStatus)
        {
            WWMxWriteItemInfo ii = new WWMxWriteItemInfo();
            WWMxItem i;
            if (!RegisteredItems.TryGetValue(phItemHandle, out i))
            {
                ii.Item = null;
                ii.WriteOK = false;
                ii.Message = "Item " + phItemHandle + " not found!";
                PostWriteCompleted(ii);
                return;
            }

            ii.Item = i;

            if (ItemStatus[0].success != 0)
            {
                if (ItemStatus[0].category == ArchestrA.MxAccess.MxStatusCategory.MxCategoryPending)
                    ii.Message = "Write Pending...";
                else
                    ii.Message = "Write Complete - status OK";

                ii.WriteOK = true;
                PostWriteCompleted(ii);
                return;
            }

            ii.WriteOK = false;
            ii.Message = "Secured and Verified Write are not supported by this adapter";
            PostWriteCompleted(ii);
        }

        private WWMxItem LookupItem(string strItemName)
        {
            if (RegisteredItems == null)
                return null;

            foreach (var item in RegisteredItems)
            {
                if (item.Value.ItemName == strItemName)
                {
                    return item.Value;
                }
            }
            return null;
        }

        private bool AddItem(string strItemName, string CorrelationId)
        {
            // ensure we are Registered first
            if (hLMX == 0)
                Register();

            // make sure this item is not already added
            WWMxItem i = LookupItem(strItemName);
            if (i != null && i.Added)
            {
                PostLogMessage("Already added " + strItemName);
                return true;
            }

            i = new WWMxItem();

            try
            {
                if ((LMX_Server != null) && (hLMX != 0))
                {
                    var hItem = LMX_Server.AddItem(hLMX, strItemName);
                    i.hItem = hItem;
                    i.ItemName = strItemName;
                    i.OnAdvise = false;
                    i.Added = true;
                    i.CorrelationId = CorrelationId;

                    RegisteredItems.Add(hItem, i);

                    PostLogMessage("Added " + strItemName);
                    return true;
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                PostLogMessage("Unable to Add Item:\n  The MXAccess_Runtime license may be unavailable or expired,\n  or the AddItem parameter strItemDef may be an invalid pointer.");
            }
            catch (Exception ex)
            {
                PostLogMessage(ex.ToString());
            }

            return false;
        }

        private bool Advise(string strItemName, bool OnlyOnce, string CorrelationId)
        {
            WWMxItem i = LookupItem(strItemName);
            if (i == null)
            {
                // Note:  We should AddItem before attempting to Advise it (that's where the hItem comes from)
                if(!AddItem(strItemName, CorrelationId))
                {
                    PostLogMessage("Advise " + strItemName + " failed to AddItem");
                    return false;
                }
                i = LookupItem(strItemName);
            }

            i.ReadOnlyOnce = OnlyOnce;

            // put item on Advise only if it is not already on Advise
            try
            {
                if (i.hItem == 0)
                {
                    PostLogMessage("Failed to advise: " + strItemName + " - no hItem");
                    return false;
                }

                if (!i.OnAdvise)
                {
                    LMX_Server.Advise(hLMX, i.hItem);
                    i.OnAdvise = true;
                    PostLogMessage("Item On Advise: " + strItemName);
                }
                else
                {
                    PostLogMessage("Already advised " + strItemName);
                }

                return true;
            }
            catch (Exception ex)
            {
                PostLogMessage(ex.ToString());
            }

            return false;
        }

        public void UnAdviseAll()
        {
            try
            {
                foreach (var item in RegisteredItems)
                {
                    UnAdvise(item.Value.ItemName, item.Value.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                PostLogMessage("UnAdviseAll EX " + ex.ToString());
            }
        }

        public void Unregister()
        {
            try
            {
                // check whether we are currently Registered
                if ((LMX_Server != null) && (hLMX != 0))
                {
                    // Note:  We should UnAdvise all items and Remove all items before unregistering
                    UnAdviseAll();
                    RemoveAll();

                    LMX_Server.Unregister(hLMX);
                    LMX_Server = null;
                    hLMX = 0;

                    PostLogMessage("Application Un-Registered");
                }
            }
            catch (Exception ex)
            {
                PostLogMessage("Unregister EX " + ex.ToString());
            }
        }

        private bool UnAdvise(string strItemName, string CorrelationId)
        {
            PostLogMessage("UnAdvising " + strItemName);

            WWMxItem i = LookupItem(strItemName);
            if (i == null)
            {
                return false; // not known, can't remove
            }

            try
            {
                if ((LMX_Server != null) && (hLMX != 0) && i.OnAdvise)
                {
                    LMX_Server.UnAdvise(hLMX, i.hItem);
                    i.OnAdvise = false;
                    PostLogMessage("UnAdvise " + strItemName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                PostLogMessage("UnAdvise " + strItemName + "(" + i.hItem + ") EX: " + ex.ToString());
            }

            return false; 
        }

        private bool RemoveItem(string strItemName)
        {
            PostLogMessage("Removing " + strItemName);

            WWMxItem i = LookupItem(strItemName);
            if (i == null || !i.Added)
            {
                return false; // not known, can't remove
            }

            // Note:  If item is on advise, we should Unadvise it before we Remove it
            if (i.OnAdvise)
                UnAdvise(strItemName, i.CorrelationId);

            try
            {
                LMX_Server.RemoveItem(hLMX, i.hItem);
                i.Added = false;
                PostLogMessage("Item Removed " + strItemName);
                return true;
            }
            catch (Exception ex)
            {
                PostLogMessage("RemoveItem " + strItemName + " EX " +ex.ToString());
            }

            return false;
        }

        public void RemoveAll()
        {
            // first, ensure all items are unadvised
            UnAdviseAll();

            foreach (var item in RegisteredItems)
            {
                RemoveItem(item.Value.ItemName);
            }

            RegisteredItems.Clear();
        }

        public void Dispose()
        {
            UnAdviseAll();
            RemoveAll();
            Unregister();
        }


        bool bCommunicationManagementAvailable { get; set; }
        bool bSupervisoryConnectionAvailable { get; set; }
    }
}
