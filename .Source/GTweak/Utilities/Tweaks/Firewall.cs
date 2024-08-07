using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal abstract class Firewall
    {
        private static readonly string nameRules = @"GTweak - Windows Update blocking";
        private static readonly SortedList<string, string> programPaths = new SortedList<string, string>
        {
            ["MoUso_New"] = @"Windows\UUS\amd64\MoUsoCoreWorker.exe",
            ["MoUso_Old"] = @"Windows\System32\MoUsoCoreWorker.exe",
            ["Uso"] = @"Windows\System32\usoclient.exe",
        };
        private static bool CheckRulesWindows(string nameRule)
        {
            bool isCheck = true;
            INetFwPolicy2 firewallRuleCheck = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            foreach (INetFwRule rule in firewallRuleCheck.Rules)
            {
                if (rule.Name == nameRule)
                    isCheck = false;
            }
            return isCheck;
        }

        protected static void BlockWindowsUpdate(bool isChoose)
        {
            try
            {
                if (CheckRulesWindows(nameRules) && isChoose)
                {
                    Parallel.Invoke(() => { 
                        RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["MoUso_New"], nameRules);
                        RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["MoUso_Old"], string.Concat(nameRules, " (Old Way)"));
                        RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["Uso"], string.Concat(nameRules," (Update Orchestrator)")); 
                    },
                    () => {
                        RulesUpdateOUT(isChoose, Settings.PathSystemDisk + @programPaths["MoUso_New"], nameRules); 
                        RulesUpdateOUT(isChoose, Settings.PathSystemDisk + programPaths["MoUso_Old"], string.Concat(nameRules, " (Old Way)")); 
                        RulesUpdateOUT(isChoose, Settings.PathSystemDisk + programPaths["Uso"], string.Concat(nameRules, " (Update Orchestrator)")); 
                    });
                }
                else
                {
                    Parallel.Invoke(() =>
                    {
                        try
                        {
                            RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["MoUso_New"], nameRules);
                            RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["MoUso_Old"], string.Concat(nameRules, " (Old Way)"));
                            RulesUpdateIN(isChoose, Settings.PathSystemDisk + programPaths["Uso"], string.Concat(nameRules, " (Update Orchestrator)"));
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                        try
                        {
                            RulesUpdateOUT(isChoose, Settings.PathSystemDisk + programPaths["MoUso_New"], nameRules);
                            RulesUpdateOUT(isChoose, Settings.PathSystemDisk + programPaths["MoUso_Old"], string.Concat(nameRules, " (Old Way)"));
                            RulesUpdateOUT(isChoose, Settings.PathSystemDisk + programPaths["Uso"], string.Concat(nameRules, " (Update Orchestrator)"));
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    });
                }
            } 
            catch 
            {
                App.ViewLang();
                new ViewNotification().Show("", (string)Application.Current.Resources["title0_notification"], (string)Application.Current.Resources["firewalloff_notification"]); 
            }
        }

        private static void RulesUpdateIN(in bool isChoose, in string pathProgram, in string nameRule)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule.ApplicationName = pathProgram;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            firewallRule.Description = "Windows update blocking";
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = nameRule;

            if (isChoose)
            {
                firewallPolicy.Rules.Remove(nameRule);
                firewallPolicy.Rules.Add(firewallRule);
            }
            else
                firewallPolicy.Rules.Remove(nameRule);
        }

        private static void RulesUpdateOUT(in bool isChoose, in string pathProgram, in string nameRule)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule.ApplicationName = pathProgram;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRule.Description = "Windows update blocking";
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = nameRule;

            if (isChoose)
            {
                firewallPolicy.Rules.Remove(nameRule);
                firewallPolicy.Rules.Add(firewallRule);
            }
            else
                firewallPolicy.Rules.Remove(nameRule);

        }

        protected static void BlockSpyDomain(in bool isCheck)
        {
            App.ViewLang();
            try
            {
                if (CheckRulesWindows(@"GTweak - Spy domain names") && isCheck)
                    RulesHosts(isCheck);
                else
                {
                    try { RulesHosts(isCheck); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                }
            }
            catch { new ViewNotification().Show("", (string)Application.Current.Resources["title0_notification"], (string)Application.Current.Resources["firewalloff_notification"]); }
        }

        private static void RulesHosts(in bool isChoose)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule.RemoteAddresses = "13.64.90.137,13.66.56.243,13.68.31.193,13.68.82.8,13.68.92.143,13.68.233.9,13.69.109.130,13.69.109.131," +
                "13.69.131.175,13.73.26.107,13.74.169.109,13.78.130.220,13.78.232.226,13.78.233.133,13.88.21.125,13.92.194.212,13.104.215.69,13.105.28.32," +
                "13.105.28.48,20.44.86.43,20.49.150.241,20.54.232.160,20.60.20.4,20.69.137.228,20.190.169.24,20.190.169.25,23.99.49.121,23.102.4.253,23.102." +
                "5.5,23.102.21.4,23.103.182.126,40.68.222.212,40.69.153.67,40.70.184.83,40.70.220.248,40.77.228.47,40.77.228.87,40.77.228.92,40.77.232.101," +
                "40.78.128.150,40.79.85.125,40.88.32.150,40.112.209.200,40.115.3.210,40.115.119.185,40.119.211.203,40.124.34.70,40.126.41.96,40.126.41.160," +
                "51.104.136.2,51.105.218.222,51.140.40.236,51.140.157.153,51.143.53.152,51.143.111.7,51.143.111.81,51.144.227.73,52.138.204.217,52.147.198.2" +
                "01,52.155.94.78,52.157.234.37,52.158.208.111,52.164.241.205,52.169.189.83,52.170.83.19,52.174.22.246,52.178.147.240,52.178.151.212,52.178.223." +
                "23,52.182.141.63,52.183.114.173,52.184.221.185,52.229.39.152,52.230.85.180,52.230.222.68,52.236.42.239,52.236.43.202,52.255.188.83,65.52.100." +
                "7,65.52.100.9,65.52.100.11,65.52.100.91,65.52.100.92,65.52.100.93,65.52.100.94,65.52.161.64,65.55.29.238,65.55.83.120,65.55.113.11,65.55.113.12," +
                "65.55.113.13,65.55.176.90,65.55.252.43,65.55.252.63,65.55.252.70,65.55.252.71,65.55.252.72,65.55.252.93,65.55.252.190,65.55.252.202,66.119." +
                "147.131,104.41.207.73,104.42.151.234,104.43.137.66,104.43.139.21,104.43.139.144,104.43.140.223,104.43.193.48,104.43.228.53,104.43.228.202," +
                "104.43.237.169,104.45.11.195,104.45.214.112,104.46.1.211,104.46.38.64,104.46.162.224,104.46.162.226,104.210.4.77,104.210.40.87,104.210.212." +
                "243,104.214.35.244,104.214.78.152,131.253.6.87,131.253.6.103,131.253.34.230,131.253.34.234,131.253.34.237,131.253.34.243,131.253.34.246,131." +
                "253.34.247,131.253.34.249,131.253.34.252,131.253.34.255,131.253.40.37,134.170.30.202,134.170.30.203,134.170.30.204,134.170.30.221,134.170.52.151," +
                "134.170.235.16,157.56.74.250,157.56.91.77,157.56.106.184,157.56.106.185,157.56.106.189,157.56.113.217,157.56.121.89,157.56.124.87,57.56.149.250," +
                "157.56.194.72,157.56.194.73,157.56.194.74,168.61.24.141,168.61.146.25,168.61.149.17,168.61.161.212,168.61.172.71,168.62.187.13,168.63.100.61," +
                "168.63.108.233,191.236.155.80,191.237.218.239,191.239.50.18,191.239.50.77,191.239.52.100,191.239.54.52,207.68.166.254";
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRule.Description = "Spy domain names";
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = @"GTweak - Spy domain names";

            if (isChoose)
            {
                firewallPolicy.Rules.Remove(@"GTweak - Spy domain names");
                firewallPolicy.Rules.Add(firewallRule);
            }
            else
                firewallPolicy.Rules.Remove(@"GTweak - Spy domain names");
        }
    }
}
