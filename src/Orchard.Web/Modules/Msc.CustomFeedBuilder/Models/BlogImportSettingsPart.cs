using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Msc.CustomFeedBuilder.Models {
    [OrchardFeature("Msc.BlogImport")]
    public class BlogImportSettingsPart : ContentPart {
        public string Uri {
            get { return this.Retrieve(x => x.Uri); }
            set { this.Store(x => x.Uri, value); }
        }

        public int Delay {
            get { return this.Retrieve(x => x.Delay); }
            set { this.Store(x => x.Delay, value); }
        }
    }
}