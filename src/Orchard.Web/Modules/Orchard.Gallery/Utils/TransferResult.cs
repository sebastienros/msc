using System;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Gallery.Utils {
    
    /// <summary>
    /// Transfers execution to the supplied url.
    /// </summary>
    public class TransferResult : ActionResult {
        public string Url { get; private set; }

        public TransferResult(string url) {
            this.Url = url;
        }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null)
                throw new ArgumentNullException("context");

            var httpContext = HttpContext.Current;
            httpContext.Server.TransferRequest(this.Url, true);
        }
    }
}