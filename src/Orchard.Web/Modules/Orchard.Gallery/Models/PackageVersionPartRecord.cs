using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackageVersionPartRecord : ContentPartRecord {
        /// <summary>
        /// A sortable version number: ### ### ### ###
        /// e.g., 000 001 001 012 for 1.1.12
        /// </summary>
        public virtual int VersionMajor { get; set; }
        public virtual int VersionMinor { get; set; }
        public virtual int VersionBuild { get; set; }
        public virtual int VersionRevision { get; set; }
        public virtual string PackageVersionId { get; set; }
    }
}
