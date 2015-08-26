using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackageVersionPartRecord : ContentPartRecord {
        /// <summary>
        /// A sortable version number: ### ### ### ###
        /// e.g., 000 001 001 012 for 1.1.12
        /// </summary>
        public virtual long NormalizedVersion { get; set; }
        public virtual string PackageVersionId { get; set; }
    }
}
