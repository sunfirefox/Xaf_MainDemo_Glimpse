using System;
using System.Configuration;
using System.Globalization;
using DevExpress.DemoData.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.Persistent.AuditTrail;

namespace MainDemo.Win {
    public class Program {
        [STAThread]
        public static void Main(string[] arguments) {
            MainDemoWinApplication winApplication = new MainDemoWinApplication();
#if DEBUG
            DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
            AuditTrailService.Instance.QueryCurrentUserName += new QueryCurrentUserNameEventHandler(Instance_QueryCurrentUserName);
            winApplication.CustomizeFormattingCulture += new EventHandler<CustomizeFormattingCultureEventArgs>(winApplication_CustomizeFormattingCulture);
            winApplication.LastLogonParametersReading += new EventHandler<LastLogonParametersReadingEventArgs>(winApplication_LastLogonParametersReading);
			winApplication.CreateCustomObjectSpaceProvider += (sender, e) => {
				e.ObjectSpaceProvider = new SecuredObjectSpaceProvider((ISelectDataSecurityProvider)winApplication.Security, e.ConnectionString, e.Connection, false);
			};
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["ConnectionString"];
            if(connectionStringSettings != null) {
                winApplication.ConnectionString = connectionStringSettings.ConnectionString;
            }
            else if(string.IsNullOrEmpty(winApplication.ConnectionString) && winApplication.Connection == null) {
                connectionStringSettings = ConfigurationManager.ConnectionStrings["SqlExpressConnectionString"];
                if(connectionStringSettings != null) {
                    winApplication.ConnectionString = DbEngineDetector.PatchConnectionString(connectionStringSettings.ConnectionString);
                }
            }
            try {
                winApplication.Setup();
                winApplication.Start();
            }
            catch(Exception e) {
                winApplication.HandleException(e);
            }
        }
        static void Instance_QueryCurrentUserName(object sender, QueryCurrentUserNameEventArgs e) {
            e.CurrentUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }
        static void winApplication_CustomizeFormattingCulture(object sender, CustomizeFormattingCultureEventArgs e) {
            e.FormattingCulture = CultureInfo.GetCultureInfo("en-US");
        }
        static void winApplication_LastLogonParametersReading(object sender, LastLogonParametersReadingEventArgs e) {
            if(string.IsNullOrEmpty(e.SettingsStorage.LoadOption("", "UserName"))) {
                e.SettingsStorage.SaveOption("", "UserName", "Sam");
            }
        }
    }
}
