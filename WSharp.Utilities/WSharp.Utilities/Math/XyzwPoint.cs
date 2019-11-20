using System;
using System.Drawing;

namespace WSharp.Utilities.Math
{
    /// <summary>A point in 4 dimensions (x, y, z, w).</summary>
    public struct XyzwPoint : IComparable, IComparable<XyzwPoint>, IEquatable<XyzwPoint>
    {
        /// <summary>Constructs a new value of a <see cref="XyzwPoint"/>.</summary>
        /// <param name="x">X vector of the point.</param>
        /// <param name="y">Y vector of the point.</param>
        /// <param name="z">Z vector of the point.</param>
        /// <param name="w">W vector of the point.</param>
        /// <param name="comparisonTolerance">Tolerance that is used to compare the values.</param>
        public XyzwPoint(double x = default, double y = default, double z = default, double w = default, double comparisonTolerance = 0.0001)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            ComparisonTolerance = comparisonTolerance;
        }

        #region PROPERTIES

        /// <summary>X vector of the point.</summary>
        public double X { get; set; }

        /// <summary>Y vector of the point.</summary>
        public double Y { get; set; }

        /// <summary>Z vector of the point.</summary>
        public double Z { get; set; }

        /// <summary>W vector of the point.</summary>
        public double W { get; set; }

        /// <summary>Tolerance that is used to compare the values.</summary>
        public double ComparisonTolerance { get; set; }

        /// <summary>Total magnitude of the vector.</summary>
        public double Magnitude
        {
            get
            {
                var sqVal = System.Math.Pow(X, 2) + System.Math.Pow(Y, 2) + System.Math.Pow(Z, 2) + System.Math.Pow(W, 2);
                return System.Math.Sqrt(sqVal);
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>Checks the equality between this point and the given point.</summary>
        /// <param name="other">Point to check.</param>
        /// <returns>Whether the points are equal.</returns>
        public bool Equals(XyzwPoint other)
            => System.Math.Abs(other.X - X) < ComparisonTolerance &&
            System.Math.Abs(other.Y - Y) < ComparisonTolerance &&
            System.Math.Abs(other.Z - Z) < ComparisonTolerance &&
            System.Math.Abs(other.W - W) < ComparisonTolerance;

        /// <summary>Checks the equality between this point and the given object.</summary>
        /// <param name="other">Object to check.</param>
        /// <returns>Whether the objects are equal.</returns>
        public override bool Equals(object obj) => obj != null && (obj is XyzwPoint xyzwPoint) && Equals(xyzwPoint);

        /// <summary>
        ///     Compares this point to another one. (-1 = less than, 1 = greater than, 0 = equals)
        /// </summary>
        /// <param name="other">The point to compare to.</param>
        /// <returns>The comparison (-1 = less than, 1 = greater than, 0 = equals).</returns>
        public int CompareTo(XyzwPoint other) => Magnitude.CompareTo(other.Magnitude);

        /// <summary>
        ///     Compares this point to another object. (-1 = less than, 1 = greater than, 0 = equals)
        /// </summary>
        /// <param name="other">The object to compare to.</param>
        /// <returns>The comparison (-1 = less than, 1 = greater than, 0 = equals).</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is XyzwPoint xyzwPoint))
                throw new ArgumentException($"Argument must be of type {nameof(XyzwPoint)}", nameof(obj));

            return CompareTo(xyzwPoint);
        }

        /// <summary>Generates a hash code from the point.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            var hashCode = 1464954422;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + W.GetHashCode();
            hashCode = hashCode * -1521134295 + ComparisonTolerance.GetHashCode();
            hashCode = hashCode * -1521134295 + Magnitude.GetHashCode();
            return hashCode;
        }

        /// <summary>Creates a string representation of this point.</summary>
        /// <returns>{X};{Y};{Z};{W}</returns>
        public override string ToString() => $"{X};{Y};{Z};{W}";

        /// <summary>Parses a point. (format should be {X};{Y};{Z};{W})</summary>
        /// <param name="str">String to parse.</param>
        /// <exception cref="ArgumentNullException">Input is null.</exception>
        /// <exception cref="ArgumentException">The string is empty.</exception>
        /// <exception cref="FormatException">The string does not fit the given format.</exception>
        /// <returns>The parsed point.</returns>
        public static XyzwPoint Parse(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (str == string.Empty)
                throw new ArgumentException("Cannot parse empty string", nameof(str));

            var split = str.Split(';');
            if (split.Length < 2)
                throw new FormatException($"Cannot parse a string that contains no ';' ({str})");

            var point = new XyzwPoint
            {
                X = double.Parse(split[0]),
                Y = double.Parse(split[1])
            };

            if (split.Length > 2)
                point.Z = double.Parse(split[2]);

            if (split.Length > 3)
                point.W = double.Parse(split[3]);

            return point;
        }

        /// <summary>Tries to pars a point. (format should be {X};{Y};{Z};{W})</summary>
        /// <param name="str">String to parse.</param>
        /// <exception cref="ArgumentNullException">Input is null.</exception>
        /// <exception cref="ArgumentException">The string is empty.</exception>
        /// <exception cref="FormatException">The string does not fit the given format.</exception>
        /// <returns>Whether the string was parsed.</returns>
        public static bool TryParse(string str, out XyzwPoint point)
        {
            try
            {
                if (str == null)
                    throw new ArgumentNullException(nameof(str));
                if (str == string.Empty)
                    throw new ArgumentException("Cannot parse empty string", nameof(str));

                var split = str.Split(';');
                if (split.Length < 2)
                    throw new FormatException($"Cannot parse a string that contains no ';' ({str})");

                point = new XyzwPoint
                {
                    X = double.Parse(split[0]),
                    Y = double.Parse(split[1])
                };

                if (split.Length > 2)
                    point.Z = double.Parse(split[2]);

                if (split.Length > 3)
                    point.W = double.Parse(split[3]);

                return true;
            }
            catch
            {
                point = default;
                return false;
            }
        }

        #region operators

        /// <summary>Returns a copy of this point.</summary>
        /// <param name="point">The point to copy.</param>
        /// <returns>A copy of the first point.</returns>
        public static XyzwPoint operator +(XyzwPoint point) => new XyzwPoint(point.X, point.Y, point.Z, point.W);

        /// <summary>Adds two points together.</summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The sum of the two points.</returns>
        public static XyzwPoint operator +(XyzwPoint first, XyzwPoint second) => new XyzwPoint(first.X + second.X, first.Y + second.Y, first.Z + second.Z, first.W + second.W);

        /// <summary>Returns an inverse copy of this point.</summary>
        /// <param name="point">The point to copy.</param>
        /// <returns>The inverse copy of the first point.</returns>
        public static XyzwPoint operator -(XyzwPoint point) => new XyzwPoint(-point.X, -point.Y, -point.Z, -point.W);

        /// <summary>Subtracts two points from each other.</summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>The difference between the two points.</returns>
        public static XyzwPoint operator -(XyzwPoint first, XyzwPoint second) => new XyzwPoint(first.X - second.X, first.Y - second.Y, first.Z - second.Z, first.W - second.W);

        /// <summary>Multiplies a point by a given value.</summary>
        /// <param name="point">Point to multiply</param>
        /// <param name="value">Multiplier</param>
        /// <returns>The product.</returns>
        public static XyzwPoint operator *(XyzwPoint point, double value) => new XyzwPoint(point.X * value, point.Y * value, point.Z * value, point.W * value);

        /// <summary>Devides a point by a given value.</summary>
        /// <param name="point">The point to devide.</param>
        /// <param name="value">Devider</param>
        /// <returns>The devision.</returns>
        public static XyzwPoint operator /(XyzwPoint point, double value) => new XyzwPoint(point.X / value, point.Y / value, point.Z / value, point.W / value);

        /// <summary>Check whether two points are equal to each other.</summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>Whether the two points are equal.</returns>
        public static bool operator ==(XyzwPoint first, XyzwPoint second) => first.Equals(second);

        /// <summary>Check whether two points are different from each other.</summary>
        /// <param name="first">The first point.</param>
        /// <param name="second">The second point.</param>
        /// <returns>Whether the two points are different.</returns>
        public static bool operator !=(XyzwPoint first, XyzwPoint second) => !first.Equals(second);

        /// <summary>
        ///     Cast a <see cref="XyzwPoint"/> point to a <see cref="Point"/> (unneeded vectors are disposed).
        /// </summary>
        /// <param name="xyzw">The point to cast.</param>
        public static explicit operator Point(XyzwPoint xyzw) => new Point((int)xyzw.X, (int)xyzw.Y);

        /// <summary>
        ///     Casts a <see cref="Point"/> to a <see cref="XyzwPoint"/> (z and w are empty)
        /// </summary>
        /// <param name="point">The point to cast.</param>
        public static explicit operator XyzwPoint(Point point) => new XyzwPoint(x: point.X, y: point.Y);

        #endregion operators

        #endregion METHODS
    }
}