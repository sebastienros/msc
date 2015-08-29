using System;
using System.Linq;

namespace Orchard.Gallery.Models {
    public class PackageVersion : IComparable<PackageVersion> {
        private int[] _parts = new[] { 0, 0, 0, 0 };

        public static PackageVersion Parse(string version) {
            var packageVersion = new PackageVersion();

            if (!String.IsNullOrWhiteSpace(version)) {

                var versionParts = version.Split('.');

                for (int i = 0; i < 4; i++) {
                    if (versionParts.Length > i) {
                        int part;
                        if (Int32.TryParse(versionParts[i], out part)) {
                            packageVersion._parts[i] = part;
                        }
                    }
                }
            }

            return packageVersion;
        }

        public PackageVersion() {

        }

        public PackageVersion(int[] parts) {
            if(parts == null) {
                throw new ArgumentNullException();

            }

            _parts = parts.Union(new int[] { 0, 0, 0, 0 }).Take(4).ToArray();
        }

        public int Major { get { return _parts[0]; } }
        public int Minor { get { return _parts[1]; } }
        public int Patch { get { return _parts[2]; } }
        public int Build { get { return _parts[3]; } }

        public int CompareTo(PackageVersion other) {
            return Compare(this, other);
        }

        public static int Compare(PackageVersion obj1, PackageVersion obj2) {
            if (ReferenceEquals(obj1, obj2)) {
                return 0;
            }

            if ((object)obj1 == null) {
                return -1;
            }

            if ((object)obj2 == null) {
                return 1;
            }

            for (var i = 0; i < 4; i++) {
                var compare = obj1._parts[i].CompareTo(obj2._parts[i]);

                if (compare != 0) {
                    return compare;
                }
            }

            return 0;
        }
        

        public static bool operator <(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) < 0;
        }

        public static bool operator >(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) > 0;
        }

        public static bool operator ==(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) == 0;
        }

        public static bool operator !=(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) != 0;
        }

        public static bool operator <=(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) <= 0;
        }

        public static bool operator >=(PackageVersion obj1, PackageVersion obj2) {
            return Compare(obj1, obj2) >= 0;
        }

        public override string ToString() {
            return String.Join(".", _parts.TakeWhile((i, x) => i < 3 || x > 0));
        }

        public override bool Equals(object obj) {
            if (!(obj is PackageVersion)) {
                return false;
            }

            return this == (PackageVersion)obj;
        }

        public override int GetHashCode() {
            int hash = 37;
            hash = hash * 23 + Major.GetHashCode();
            hash = hash * 23 + Minor.GetHashCode();
            hash = hash * 23 + Patch.GetHashCode();
            hash = hash * 23 + Build.GetHashCode();
            return hash;
        }

    }
}