﻿using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Orchard.Gallery.Models {
    public class PackageVersionPart : ContentPart {
        public CommonPart CommonPart {
            get { return this.As<CommonPart>(); }
        }

        public BodyPart BodyPart {
            get { return this.As<BodyPart>(); }
        }
        
        public string Version {
            get { return this.Retrieve(x => x.Version); }
            set { this.Store(x => x.Version, value); }
        }

        public string PackageUrl {
            get { return this.Retrieve(x => x.PackageUrl); }
            set { this.Store(x => x.PackageUrl, value); }
        }

        public int DownloadCount {
            get { return this.Retrieve(x => x.DownloadCount); }
            set { this.Store(x => x.DownloadCount, value); }
        }
    }
}
