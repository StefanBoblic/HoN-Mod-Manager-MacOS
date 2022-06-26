﻿#region Using Directives

using System;
using System.Text;

#endregion

namespace HonModManagerForMac
{
    internal class CustomVersion : IComparable<CustomVersion>
    {
        private int? m_build;
        private int? m_major;
        private int? m_minor;
        private int? m_revis;

        public CustomVersion(int? major, int? minor, int? build, int? revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revis = revision;
        }

        public CustomVersion(string version)
        {
            VersionString = version;
        }

        public CustomVersion()
        {
        }

        public int? Major
        {
            get => m_major;
            set => m_major = value;
        }

        public int? Minor
        {
            get => m_minor;
            set => m_minor = value;
        }

        public int? Build
        {
            get => m_build;
            set => m_build = value;
        }

        public int? Revis
        {
            get => m_revis;
            set => m_revis = value;
        }

        public int?[] IntArray
        {
            get { return new[] { m_major, m_minor, m_build, m_revis }; }
            set
            {
                for (var i = 0; i < 4; i++)
                {
                    if (value.Length <= i)
                        break;

                    if (i == 0)
                        m_major = value[i];
                    if (i == 1)
                        m_minor = value[i];
                    if (i == 2)
                        m_build = value[i];
                    if (i == 3)
                        m_revis = value[i];
                }
            }
        }

        public string VersionString
        {
            get
            {
                var sb = new StringBuilder();

                if (m_major.HasValue)
                    sb.Append(m_major.Value.ToString());
                else
                    sb.Append("*");

                sb.Append(".");

                if (m_minor.HasValue)
                    sb.Append(m_minor.Value.ToString());
                else
                    sb.Append("*");

                sb.Append(".");

                if (m_build.HasValue)
                    sb.Append(m_build.Value.ToString());
                else
                    sb.Append("*");

                sb.Append(".");

                if (m_revis.HasValue)
                    sb.Append(m_revis.Value.ToString());
                else
                    sb.Append("*");

                return sb.ToString();
            }
            set
            {
                var sa = value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (sa.Length == 0)
                    throw new ArgumentException("Incorrect VersionString");

                int tmp;

                m_major = sa.Length < 1 ||
                          string.Compare("*", sa[0], StringComparison.OrdinalIgnoreCase) == 0 ||
                          int.TryParse(sa[0], out tmp) == false
                    ? (int?)null
                    : tmp;


                m_minor = sa.Length < 2 ||
                          string.Compare("*", sa[1], StringComparison.OrdinalIgnoreCase) == 0 ||
                          int.TryParse(sa[1], out tmp) == false
                    ? (int?)null
                    : tmp;

                m_build = sa.Length < 3 ||
                          string.Compare("*", sa[2], StringComparison.OrdinalIgnoreCase) == 0 ||
                          int.TryParse(sa[2], out tmp) == false
                    ? (int?)null
                    : tmp;

                m_revis = sa.Length < 4 ||
                          string.Compare("*", sa[3], StringComparison.OrdinalIgnoreCase) == 0 ||
                          int.TryParse(sa[3], out tmp) == false
                    ? (int?)null
                    : tmp;
            }
        }

        #region IComparable<CustomVersion> Members

        public int CompareTo(CustomVersion other)
        {
            return CompareToArray(other);
        }

        #endregion

        public static implicit operator Version(CustomVersion cv)
        {
            return new Version(cv.Major.GetValueOrDefault(0), cv.Minor.GetValueOrDefault(0),
                cv.Build.GetValueOrDefault(0), cv.Revis.GetValueOrDefault(0));
        }

        public static implicit operator CustomVersion(Version v)
        {
            return new CustomVersion(v.Major, v.Minor, v.Build, v.Revision);
        }

        public static implicit operator string(CustomVersion cv)
        {
            return cv.ToString();
        }

        public static implicit operator CustomVersion(string s)
        {
            return new CustomVersion(s);
        }

        public override string ToString()
        {
            return VersionString;
        }

        public static bool operator <(CustomVersion cva, CustomVersion cvb)
        {
            return cva.CompareTo(cvb) < 0;
        }

        public static bool operator >(CustomVersion cva, CustomVersion cvb)
        {
            return cva.CompareTo(cvb) > 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(CustomVersion)) return false;
            return Equals((CustomVersion)obj);
        }

        public int CompareToArray(CustomVersion other)
        {
            var thisIntArray = IntArray;
            var otherIntArray = other.IntArray;

            for (var i = 0; i < thisIntArray.Length; i++)
                if (thisIntArray[i].HasValue && otherIntArray[i].HasValue)
                {
                    if (thisIntArray[i] == otherIntArray[i])
                        continue;
                    return thisIntArray[i].Value.CompareTo(otherIntArray[i].Value);
                }
                else
                {
                    if (thisIntArray[i].HasValue && !otherIntArray[i].HasValue)
                        return 1;

                    if (!thisIntArray[i].HasValue && otherIntArray[i].HasValue)
                        return -1;
                }

            // we found no differences
            return 0;
        }

        public int CompareToExpanded(CustomVersion other)
        {
            if (Major.HasValue && other.Major.HasValue)
            {
                if (Major == other.Major)
                {
                    if (Minor.HasValue && other.Minor.HasValue)
                    {
                        if (Minor == other.Minor)
                        {
                            if (Build.HasValue && other.Build.HasValue)
                            {
                                if (Build == other.Build)
                                {
                                    if (Revis.HasValue && other.Revis.HasValue)
                                    {
                                        if (Revis == other.Revis)
                                            return 0;
                                        return Revis.Value.CompareTo(other.Revis.Value);
                                    }

                                    if (Revis.HasValue && !other.Revis.HasValue)
                                        return 1;

                                    if (!Revis.HasValue && other.Revis.HasValue)
                                        return -1;
                                }
                                else
                                {
                                    return Build.Value.CompareTo(other.Build.Value);
                                }
                            }
                            else
                            {
                                if (Build.HasValue && !other.Build.HasValue)
                                    return 1;

                                if (!Build.HasValue && other.Build.HasValue)
                                    return -1;
                            }
                        }
                        else
                        {
                            return Minor.Value.CompareTo(other.Minor.Value);
                        }
                    }
                    else
                    {
                        if (Minor.HasValue && !other.Minor.HasValue)
                            return 1;

                        if (!Minor.HasValue && other.Minor.HasValue)
                            return -1;
                    }
                }
                else
                {
                    return Major.Value.CompareTo(other.Major.Value);
                }
            }
            else
            {
                if (Major.HasValue && !other.Major.HasValue)
                    return 1;

                if (!Major.HasValue && other.Major.HasValue)
                    return -1;
            }

            return 0;
        }

        public bool IsCompatibleWithArray(CustomVersion other)
        {
            var thisIntArray = IntArray;
            var otherIntArray = other.IntArray;

            for (var i = 0; i < thisIntArray.Length; i++)
                if (thisIntArray[i].HasValue && otherIntArray[i].HasValue)
                {
                    if (thisIntArray[i] == otherIntArray[i])
                        continue;
                    return false;
                }
                else
                {
                    if (thisIntArray[i].HasValue && !otherIntArray[i].HasValue)
                        continue;

                    if (!thisIntArray[i].HasValue && otherIntArray[i].HasValue)
                        continue;
                }

            // we found no differences
            return true;
        }

        public bool IsCompatibleWithExpanded(CustomVersion other)
        {
            if (Major.HasValue && other.Major.HasValue)
            {
                if (Major == other.Major)
                {
                    if (Minor.HasValue && other.Minor.HasValue)
                    {
                        if (Minor == other.Minor)
                        {
                            if (Build.HasValue && other.Build.HasValue)
                            {
                                if (Build == other.Build)
                                {
                                    if (Revis.HasValue && other.Revis.HasValue)
                                    {
                                        if (Revis == other.Revis)
                                            return true;
                                        return false;
                                    }

                                    if (Revis.HasValue && !other.Revis.HasValue)
                                        return true;

                                    if (!Revis.HasValue && other.Revis.HasValue)
                                        return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (Build.HasValue && !other.Build.HasValue)
                                    return true;

                                if (!Build.HasValue && other.Build.HasValue)
                                    return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (Minor.HasValue && !other.Minor.HasValue)
                            return true;

                        if (!Minor.HasValue && other.Minor.HasValue)
                            return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (Major.HasValue && !other.Major.HasValue)
                    return true;

                if (!Major.HasValue && other.Major.HasValue)
                    return true;
            }

            return true;
        }

        public bool IsCompatibleWith(CustomVersion other)
        {
            return IsCompatibleWithArray(other);
        }

        public bool Equals(CustomVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.m_major.Equals(m_major) && other.m_minor.Equals(m_minor) && other.m_build.Equals(m_build) &&
                   other.m_revis.Equals(m_revis);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = m_major.HasValue ? m_major.Value : 0;
                result = (result * 397) ^ (m_minor.HasValue ? m_minor.Value : 0);
                result = (result * 397) ^ (m_build.HasValue ? m_build.Value : 0);
                result = (result * 397) ^ (m_revis.HasValue ? m_revis.Value : 0);
                return result;
            }
        }
    }

    internal class CustomRequirement
    {
        public CustomRequirement()
            : this(new CustomVersion(), new CustomVersion())
        {
        }

        public CustomRequirement(CustomVersion min, CustomVersion max)
        {
            Min = min;
            Max = max;
        }

        public CustomRequirement(string requirement)
        {
            var sa = requirement.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (sa.Length == 0)
            {
                Min = new CustomVersion();
                Max = new CustomVersion();
            }
            else
            {
                if (sa.Length == 1)
                {
                    Min = new CustomVersion();
                    Max = new CustomVersion(sa[0]);
                }
                else
                {
                    if (sa.Length == 2)
                    {
                        Min = new CustomVersion(sa[0]);
                        Max = new CustomVersion(sa[1]);
                    }
                    else
                    {
                        Range = new CustomVersion[sa.Length];
                        for (var i = 0; i < sa.Length; i++) Range[i] = new CustomVersion(sa[i]);
                    }
                }
            }
        }

        public CustomVersion Min { get; set; }

        public CustomVersion Max { get; set; }

        public CustomVersion[] Range { get; }

        public static implicit operator CustomRequirement(string s)
        {
            return new CustomRequirement(s);
        }

        public static implicit operator string(CustomRequirement cr)
        {
            return cr.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Min == null && Max == null)
            {
                for (var i = 0; i < Range.Length; i++)
                {
                    sb.Append(Range[i].ToString());

                    if (i < Range.Length - 1)
                        sb.Append("-");
                }
            }
            else
            {
                sb.Append(Min == null ? "*" : Min.ToString());
                sb.Append("-");
                sb.Append(Max == null ? "*" : Max.ToString());
            }

            return sb.ToString();
        }


        public bool IsCompatibleWith(CustomVersion cv)
        {
            if (cv == null)
                return false;

            if (Min == null || Max == null)
            {
                for (var i = 0; i < Range.Length; i++)
                    if (cv.IsCompatibleWith(Range[i]))
                        return true;

                // we didn't find any compatible version
                return false;
            }

            return cv.IsCompatibleWith(Min) && cv.IsCompatibleWith(Max);
        }
    }
}