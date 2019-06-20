using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Management;
using System.Windows;
using assignment.Models;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace assignment.Controllers
//Install-Package System.Management -Version 4.5.0 this package need to be instal through package manager console.

{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            CustomModel returnobjet = new CustomModel();
            List<adapterlist> adapaterinfoobject = new List<adapterlist>();
            List<ipadresslist> ipinfoobject = new List<ipadresslist>();
            List<macadresslist> macinfoobject = new List<macadresslist>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        adapaterinfoobject.Add(new adapterlist { adaptername = nic.Name });
                    }
                }
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        adapaterinfoobject.Add(new adapterlist { adaptername = nic.Name });
                    }
                      
                }
            }
            //foreach (IPAddress currrentIPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            //{
            //    if (currrentIPAddress.AddressFamily.ToString() == System.Net.Sockets.AddressFamily.InterNetwork.ToString())
            //    {
            //        ipinfoobject.Add(new ipadresslist { ipadress = currrentIPAddress.ToString() ?? "" });
            //        //  IPAddress_LstB.Items.Add(currrentIPAddress.ToString());
            //    }
            //}

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {
                    dynamic r = mo["Caption"].ToString().Substring(11);
                    adapaterinfoobject.Add(new adapterlist { adaptername = mo["Caption"].ToString().Substring(11) });
                   
                    string[] ipaddresses = (string[])mo["IPAddress"];
                    
                    ipinfoobject.Add(new ipadresslist { ipadress = ipaddresses[0]});
                    
                    macinfoobject.Add(new macadresslist { macadress = mo["MacAddress"].ToString() });
                }
            }

            returnobjet.adapterlists=adapaterinfoobject;
            returnobjet.ipadresslists = ipinfoobject;
            returnobjet.macadresslists  = macinfoobject;


            return View(returnobjet);
        }

        public ActionResult About()
        {

            List<adapterinformation> returnobject = new List<adapterinformation>();

            IntPtr PAdaptersAddresses = new IntPtr();
            bool AdapterFound = false;
            uint pOutLen = 100;

            PAdaptersAddresses = Marshal.AllocHGlobal(100);
            uint ret = GetAdaptersAddresses(0, 0, (IntPtr)0,
                           PAdaptersAddresses, ref pOutLen);

            // if 111 error, use 
            if (ret == 111)
            {
                Marshal.FreeHGlobal(PAdaptersAddresses);
                PAdaptersAddresses = Marshal.AllocHGlobal((int)pOutLen);
                ret = GetAdaptersAddresses(0, 0, (IntPtr)0,
                                           PAdaptersAddresses, ref pOutLen);
            }

            IP_Adapter_Addresses adds = new IP_Adapter_Addresses();
            IntPtr pTemp = PAdaptersAddresses;
            IntPtr pTempIP = new IntPtr();

            while (pTemp != (IntPtr)0)
            {
                Marshal.PtrToStructure(pTemp, adds);
                string adapterName = Marshal.PtrToStringAnsi(adds.AdapterName);
                string FriendlyName = Marshal.PtrToStringAuto(adds.FriendlyName);
                string tmpString = string.Empty;
                for (int i = 0; i < 6; i++)
                {
                    tmpString += string.Format("{0:X2}", adds.PhysicalAddress[i]);
                    if (i < 5)
                    {
                        tmpString += ":";
                    }
                }

                RegistryKey theLocalMachine = Registry.LocalMachine;
                RegistryKey theSystem = theLocalMachine.OpenSubKey(@"SYSTEM\Current" +
                                        @"ControlSet\Services\Tcpip\Parameters\Interfaces");
                RegistryKey theInterfaceKey = theSystem.OpenSubKey(adapterName);

                if (theInterfaceKey != null)
                {
                    string DhcpIPAddress = (string)theInterfaceKey.GetValue("DhcpIPAddress");
                    // system is using DHCP
                    if (DhcpIPAddress != null)
                    {
                        string tArray = (string)
                        theInterfaceKey.GetValue("DhcpIPAddress", theInterfaceKey);

                        returnobject.Add(new adapterinformation { networkadapter = FriendlyName.ToString(), ipaddress = tArray, macaddress = tmpString });


                        string x = "Network adapter: " +
                                                   FriendlyName.ToString() + "\r\n";
                        x += "IP Address: " + tArray + "\r\n";
                        x += "MAC Address: " + tmpString + "\r\n\r\n";


                        AdapterFound = true;
                    }
                    else
                    {

                        //string[] tArray = (string[])
                        //theInterfaceKey.GetValue("IPAddress", theInterfaceKey);

                        string xx = theInterfaceKey.GetValue("IPAddress", theInterfaceKey).ToString();
                        string[] IPAddress = { xx };
                        string[] tArray= { };
                        if (!string.IsNullOrEmpty(xx))
                        {
                            tArray = IPAddress;
                        }

                         
                        for (int Interface = 0; Interface < tArray.Length; Interface++)
                        {

                            if (tArray[Interface].Length > 30)
                            {
                                returnobject.Add(new adapterinformation { networkadapter = FriendlyName.ToString(), ipaddress = "0.0.0.0", macaddress = tmpString });
                            }
                            else

                            {
                                returnobject.Add(new adapterinformation { networkadapter = FriendlyName.ToString(), ipaddress = tArray[Interface], macaddress = tmpString });
                            }
                                
                        }
                    }
                }
                pTemp = adds.Next;
            }

            if (AdapterFound != true)
            {
               string x= "No network adapters configured/present\r\n";
            }

            return View(returnobject);
        }

        public ActionResult Contact()
        {

             
            return View();
        }




        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class IP_Adapter_Addresses
        {
            public uint Length;
            public uint IfIndex;
            public IntPtr Next;

            public IntPtr AdapterName;
            public IntPtr FirstUnicastAddress;
            public IntPtr FirstAnycastAddress;
            public IntPtr FirstMulticastAddress;
            public IntPtr FirstDnsServerAddress;

            public IntPtr DnsSuffix;
            public IntPtr Description;

            public IntPtr FriendlyName;

            [MarshalAs(UnmanagedType.ByValArray,
                 SizeConst = 8)]
            public Byte[] PhysicalAddress;

            public uint PhysicalAddressLength;
            public uint flags;
            public uint Mtu;
            public uint IfType;

            public uint OperStatus;

            public uint Ipv6IfIndex;
            public uint ZoneIndices;

            public IntPtr FirstPrefix;
        }

        [DllImport("Iphlpapi.dll")]
        public static extern uint GetAdaptersAddresses(uint Family, uint flags, IntPtr Reserved,
            IntPtr PAdaptersAddresses, ref uint pOutBufLen);
    }
}