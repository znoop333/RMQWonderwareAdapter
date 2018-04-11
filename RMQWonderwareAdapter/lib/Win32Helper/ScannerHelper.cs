using CoreScanner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ASRSKeyboardInput
{
    class ScannerHelper
    {
        CCoreScannerClass m_pCoreScanner = null;
        bool m_bSuccessOpen = false;//Is open success
        Form1 parent = null;
        short[] m_arScannerTypes;
        bool[] m_arSelectedTypes;
        short m_nNumberOfTypes;

        Scanner[] m_arScanners;
        int m_nTotalScanners;
        XmlReader m_xml = new XmlReader();
        //List<string> claimlist = new List<string>();

        public class EventArgsBarcodeRead : EventArgs
        {
            public string Barcode { get; set; }
            public string Symbology { get; set; }
            public int EventType { get; set; }
            public Scanner Scanner { get; set; }
            public string RawScanData { get; set; }
        }

        public event EventHandler<EventArgsBarcodeRead> BarcodeRead;
        private void FireBarcodeRead(string Barcode, string Symbology, int EventType, Scanner Scanner, string RawScanData)
        {
            if (BarcodeRead != null)
                BarcodeRead(this, new EventArgsBarcodeRead() { Barcode = Barcode, Symbology = Symbology, EventType = EventType, Scanner = Scanner, RawScanData = RawScanData });
        }


        public ScannerHelper(Form1 parent_)
        {
            parent = parent_;

            m_arScanners = new Scanner[ScannerConstants.MAX_NUM_DEVICES];
            for (int i = 0; i < ScannerConstants.MAX_NUM_DEVICES; i++)
            {
                Scanner scanr = new Scanner();
                m_arScanners.SetValue(scanr, i);
            }

            try
            {
                m_pCoreScanner = new CoreScanner.CCoreScannerClass();
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
                m_pCoreScanner = new CoreScanner.CCoreScannerClass();
            }

            m_nNumberOfTypes = 0;
            m_arScannerTypes = new short[ScannerConstants.TOTAL_SCANNER_TYPES];
            m_arSelectedTypes = new bool[ScannerConstants.TOTAL_SCANNER_TYPES];

            if (m_pCoreScanner != null)
            {
                Connect();

                // Register events for COM services
                m_pCoreScanner.BarcodeEvent += new CoreScanner._ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);

                registerForEvents();
                GetScanners();
            }
        }

        /// <summary>
        /// BarcodeEvent received
        /// </summary>
        /// <param name="eventType">Type of event</param>
        /// <param name="scanData">Barcode string</param>
        void OnBarcodeEvent(short eventType, ref string scanData)
        {
            try
            {
                string tmpScanData = scanData;

                string Symbology;
                var bc = ParseBarcodeXML(tmpScanData, out Symbology);
                FireBarcodeRead(bc, Symbology, eventType, null, scanData);

            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Extract barcode(s) from XML. Returns symbology too.
        /// </summary>
        /// <param name="strXml">Barcode data XML</param>
        public static string ParseBarcodeXML(string strXml, out string symbology)
        {
            if (strXml == null || strXml.Length == 0)
            {
                symbology = "";
                return "";
            }

            System.Diagnostics.Debug.WriteLine("Initial XML" + strXml);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXml);

            string strData = String.Empty;
            string barcode = xmlDoc.DocumentElement.GetElementsByTagName("datalabel").Item(0).InnerText;
            string symbology_ = xmlDoc.DocumentElement.GetElementsByTagName("datatype").Item(0).InnerText;
            string[] numbers = barcode.Split(' ');

            foreach (string number in numbers)
            {
                if (String.IsNullOrEmpty(number))
                {
                    break;
                }

                strData += ((char)Convert.ToInt32(number, 16)).ToString();  // 
            }

            barcode = strData;

            symbology = ScannerConstants.GetSymbology((int)Convert.ToInt32(symbology_));

            return barcode;
        }

        /// <summary>
        /// Is Open successful
        /// </summary>
        /// <returns></returns>
        private bool IsMotoConnected()
        {
            return m_bSuccessOpen;
        }
        private string GetRegUnregIDs(out int nEvents)
        {
            string strIDs = "";
            nEvents = ScannerConstants.NUM_SCANNER_EVENTS;
            strIDs = ScannerConstants.SUBSCRIBE_BARCODE.ToString();
            strIDs += "," + ScannerConstants.SUBSCRIBE_IMAGE.ToString();
            strIDs += "," + ScannerConstants.SUBSCRIBE_VIDEO.ToString();
            strIDs += "," + ScannerConstants.SUBSCRIBE_RMD.ToString();
            strIDs += "," + ScannerConstants.SUBSCRIBE_PNP.ToString();
            strIDs += "," + ScannerConstants.SUBSCRIBE_OTHER.ToString();
            return strIDs;
        }

        private void registerForEvents()
        {
            if (IsMotoConnected())
            {
                int nEvents = 0;
                string strEvtIDs = GetRegUnregIDs(out nEvents);
                string inXml = "<inArgs>" +
                                    "<cmdArgs>" +
                                    "<arg-int>" + nEvents + "</arg-int>" +
                                    "<arg-int>" + strEvtIDs + "</arg-int>" +
                                    "</cmdArgs>" +
                                    "</inArgs>";

                int opCode = ScannerConstants.REGISTER_FOR_EVENTS;
                string outXml = "";
                int status = ScannerConstants.STATUS_FALSE;
                ExecCmd(opCode, ref inXml, out outXml, out status);
                DisplayResult(status, "REGISTER_FOR_EVENTS");
            }
        }

        private void DisplayResult(int status, string strCmd)
        {
            if (this.parent != null)
            {
                ;
            }

            //switch (status)
            //{
            //    case STATUS_SUCCESS:
            //        UpdateResults(strCmd + " - Command success.");
            //        break;
            //    case STATUS_LOCKED:
            //        UpdateResults(strCmd + " - Command failed. Device is locked by another application.");
            //        break;
            //    default:
            //        UpdateResults(strCmd + " - Command failed. Error:" + status.ToString());
            //        break;
            //}
        }

        private void ExecCmd(int opCode, ref string inXml, out string outXml, out int status)
        {
            outXml = "";
            status = ScannerConstants.STATUS_FALSE;
            if (m_bSuccessOpen)
            {
                try
                {
                    //if (!chkAsync.Checked)
                    //{
                    m_pCoreScanner.ExecCommand(opCode, ref inXml, out outXml, out status);
                    //}
                    //else
                    //{
                    //    m_pCoreScanner.ExecCommandAsync(opCode, ref inXml, out status);
                    //}
                }
                catch (Exception )
                {
                    //DisplayResult(status, "EXEC_COMMAND");
                    //UpdateResults("..." + ex.Message.ToString());
                }
            }
        }


        /// <summary>
        /// Calls Open command
        /// </summary>
        private void Connect()
        {
            if (m_bSuccessOpen)
            {
                return;
            }
            int appHandle = 0;

            m_arSelectedTypes[ScannerConstants.SCANNER_TYPES_SNAPI - 1] = true;

            GetSelectedScannerTypes();
            int status = ScannerConstants.STATUS_FALSE;

            try
            {
                m_pCoreScanner.Open(appHandle, m_arScannerTypes, m_nNumberOfTypes, out status);
                DisplayResult(status, "OPEN");
                if (ScannerConstants.STATUS_SUCCESS == status)
                {
                    m_bSuccessOpen = true;
                }
            }
            catch (Exception )
            {
                //MessageBox.Show("Error OPEN - " + exp.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (ScannerConstants.STATUS_SUCCESS == status)
                {
                    //SetControls();
                }
            }
        }

        /// <summary>
        /// Calls Close command
        /// </summary>
        private void Disconnect()
        {
            if (m_bSuccessOpen)
            {
                int appHandle = 0;
                int status = ScannerConstants.STATUS_FALSE;
                try
                {
                    m_pCoreScanner.Close(appHandle, out status);
                    DisplayResult(status, "CLOSE");
                    if (ScannerConstants.STATUS_SUCCESS == status)
                    {
                        m_bSuccessOpen = false;
                        //lstvScanners.Items.Clear();
                        //combSlcrScnr.Items.Clear();
                        //m_nTotalScanners = 0;
                        //InitScannersCount();
                        //UpdateScannerCountLabels();
                        //SetControls();
                    }
                }
                catch (Exception )
                {
                    //MessageBox.Show("CLOSE Error - " + exp.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GetSelectedScannerTypes()
        {
            m_nNumberOfTypes = 0;
            for (int index = 0, k = 0; index < ScannerConstants.TOTAL_SCANNER_TYPES; index++)
            {
                if (m_arSelectedTypes[index])
                {
                    m_nNumberOfTypes++;
                    switch (index + 1)
                    {
                        case ScannerConstants.SCANNER_TYPES_ALL:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_ALL;
                            return;

                        case ScannerConstants.SCANNER_TYPES_SNAPI:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_SNAPI;
                            break;

                        case ScannerConstants.SCANNER_TYPES_SSI:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_SSI;
                            break;

                        case ScannerConstants.SCANNER_TYPES_NIXMODB:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_NIXMODB;
                            break;

                        case ScannerConstants.SCANNER_TYPES_RSM:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_RSM;
                            break;

                        case ScannerConstants.SCANNER_TYPES_IMAGING:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_IMAGING;
                            break;

                        case ScannerConstants.SCANNER_TYPES_IBMHID:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_IBMHID;
                            break;

                        case ScannerConstants.SCANNER_TYPES_HIDKB:
                            m_arScannerTypes[k++] = ScannerConstants.SCANNER_TYPES_HIDKB;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Calls GetScanners command
        /// </summary>
        private void GetScanners()
        {
            string inXml = String.Empty;
            int status = ScannerConstants.STATUS_FALSE;

            m_arScanners.Initialize();
            if (m_bSuccessOpen)
            {
                m_nTotalScanners = 0;
                short numOfScanners = 0;
                int nScannerCount = 0;
                string outXML = "";
                int[] scannerIdList = new int[ScannerConstants.MAX_NUM_DEVICES];
                try
                {
                    m_pCoreScanner.GetScanners(out numOfScanners, scannerIdList, out outXML, out status);
                    DisplayResult(status, "GET_SCANNERS");
                    if (ScannerConstants.STATUS_SUCCESS == status)
                    {
                        m_nTotalScanners = numOfScanners;
                        m_xml.ReadXmlString_GetScanners(outXML, m_arScanners, numOfScanners, out nScannerCount);

                        for (int index = 0; index < m_nTotalScanners; index++)
                        {
                            Scanner objScanner = (Scanner)m_arScanners.GetValue(index);
                        }
                    }
                }
                catch (Exception )
                {
                    //MessageBox.Show("Error GETSCANNERS - " + ex.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}
