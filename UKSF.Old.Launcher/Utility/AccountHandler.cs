using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UKSF.Launcher.Network;

namespace UKSF.Old.Launcher.Utility {
    public static class AccountHandler {
        public static async Task Initialise() {
            LogHandler.LogSpacerMessage(Global.Severity.INFO, "Retrieving account info");
            try {
                string accountString = await ApiWrapper.Get("accounts");
                JObject account = JObject.Parse(accountString);
                Global.Settings.AccountName = account["displayName"].ToString();
                Global.Settings.Admin = bool.Parse(account["admin"].ToString());
                LogHandler.LogInfo($"Retrieved account info - Name: {Global.Settings.AccountName} - Admin: {Global.Settings.Admin}");
            } catch (Exception) {
                LogHandler.LogSeverity(Global.Severity.ERROR, "Failed to retrieve account info");
            }
        }
    }
}
