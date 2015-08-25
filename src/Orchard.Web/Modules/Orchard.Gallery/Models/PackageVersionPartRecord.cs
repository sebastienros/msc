using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackageVersionPartRecord : ContentPartRecord {
        /// <summary>
        /// A sortable version number: ### ### ### 
        /// e.g., 1 001 012 for 1.1.12
        /// </summary>
        public virtual int NormalizedVersion { get; set; }
    }
}
