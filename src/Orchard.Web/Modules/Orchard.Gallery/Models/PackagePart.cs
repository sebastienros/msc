using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Tags.Models;

namespace Orchard.Gallery.Models {
    public class PackagePart : ContentPart {

        public enum ExtensionTypes {
            Module,
            Theme
        }

        public TitlePart TitlePart {
            get { return this.As<TitlePart>(); }
        }

        public TagsPart TagsPart {
            get { return this.As<TagsPart>(); }
        }

        public CommonPart CommonPart {
            get { return this.As<CommonPart>(); }
        }

        public BodyPart BodyPart {
            get { return this.As<BodyPart>(); }
        }
        
        /// <summary>
        /// Gets or sets the type of extension, <code>"module"</code> or <code>"theme"</code>
        /// </summary>
        public ExtensionTypes ExtensionType {
            get { return this.Retrieve(x => x.ExtensionType); }
            set { this.Store(x => x.ExtensionType, value); }
        }

        public int DownloadCount {
            get { return this.Retrieve(x => x.DownloadCount); }
            set { this.Store(x => x.DownloadCount, value); }
        }

        public string License {
            get { return this.Retrieve(x => x.License); }
            set { this.Store(x => x.License, value); }
        }

        public string LicenseUrl {
            get { return this.Retrieve(x => x.LicenseUrl); }
            set { this.Store(x => x.LicenseUrl, value); }
        }

        public string ProjectUrl {
            get { return this.Retrieve(x => x.ProjectUrl); }
            set { this.Store(x => x.ProjectUrl, value); }
        }
    }
}
