using System;
using System.Reflection;
using System.Web;
using System.Web.SessionState;

namespace SingleSessionUser2.Helpers
{
    /// <summary>
    /// This class manages the creation of the session identification value (SessionID). 
    /// By overriding the "CreateSessionID" and "Validate" methods, you can create a 
    /// custom ID for the session.
    /// ATTENTION: to activate the new SessionID manager it is necessary to specify the 
    /// SessionState element in the web.config, inside the "system.web" node as follows:
    /// <sessionState cookieName="MySessionID" sessionIDManagerType="SingleSessionUser2.Helpers.CustomSessionIDManager, SingleSessionUser2" />
    /// </summary>
    public class CustomSessionIDManager : SessionIDManager
    {
        public override string CreateSessionID(HttpContext context)
        {
            /// The value of the SessionID in this case depends on the "User" session variable, 
            /// which identifies the logged in user. For a more correct implementation, 
            /// the check must be carried out on the user manager adopted in the portal.
            return (context.Session == null || string.IsNullOrEmpty((string)context.Session["User"]))
                ? Guid.NewGuid().ToString()
                : context.Session["User"].ToString(); 
        }
        public override bool Validate(string id)
        {
            /// At the moment, for a quick and easy demonstration, only the presence of a 
            /// non-null string value will be checked, but they can be considered checks 
            /// based on the algorithm used for the generation and the encryption used in
            /// the generation of the name.
            return !string.IsNullOrEmpty(id);
        }

        public static string RegenerateSessionId(HttpContext Context)
        {
            /// Here are the steps necessary to regenerate a new SessionID. 
            /// This method is normally invoked following the user login. 
            /// To centralize the operation and allow you to regenerate 
            /// the value anywhere in the code, the method is exposed statically.
            var manager = new CustomSessionIDManager();
            string oldId = manager.GetSessionID(Context);
            string newId = manager.CreateSessionID(Context);
            bool isAdd = false, isRedir = false;
            manager.SaveSessionID(Context, newId, out isRedir, out isAdd);
            HttpApplication ctx = (HttpApplication)Context.ApplicationInstance;
            HttpModuleCollection mods = ctx.Modules;
            SessionStateModule ssm = (SessionStateModule)mods.Get("Session");
            FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            SessionStateStoreProviderBase store = null;
            FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
                if (field.Name.Equals("_rqId")) rqIdField = field;
                if (field.Name.Equals("_rqLockId")) rqLockIdField = field;
                if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;
            }
            object lockId = rqLockIdField.GetValue(ssm);
            if ((lockId != null) && (oldId != null)) store.ReleaseItemExclusive(Context, oldId, lockId);
            rqStateNotFoundField.SetValue(ssm, true);
            rqIdField.SetValue(ssm, newId);
            return newId;
        }
    }
}