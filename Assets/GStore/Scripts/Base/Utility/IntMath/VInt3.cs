using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable, StructLayout(LayoutKind.Sequential)]
public struct VInt3
{
    public const int Precision = 0x3e8;//1000
    public const float FloatPrecision = 1000f;
    public const float PrecisionFactor = 0.001f;
    public int x;
    public int y;
    public int z;
    public static readonly VInt3 zero;
    public static readonly VInt3 one;
    public static readonly VInt3 half;
    public static readonly VInt3 forward;
    public static readonly VInt3 up;
    public static readonly VInt3 right;

    public VInt3(Vector3 position)
    {
        this.x = MMGame_Math.RoundToInt((double)(position.x * 1000f));
        this.y = MMGame_Math.RoundToInt((double)(position.y * 1000f));
        this.z = MMGame_Math.RoundToInt((double)(position.z * 1000f));
    }

    public VInt3(int _x, int _y, int _z)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }

    static VInt3()
    {
        zero = new VInt3(0, 0, 0);
        one = new VInt3(0x3e8, 0x3e8, 0x3e8);
        half = new VInt3(500, 500, 500);
        forward = new VInt3(0, 0, 0x3e8);
        up = new VInt3(0, 0x3e8, 0);
        right = new VInt3(0x3e8, 0, 0);
    }

    public VInt3 DivBy2()
    {
        this.x = this.x >> 1;
        this.y = this.y >> 1;
        this.z = this.z >> 1;
        return this;
    }

    public int this[int i]
    {
        get
        {
            return ((i != 0) ? ((i != 1) ? this.z : this.y) : this.x);
        }
        set
        {
            if (i == 0)
            {
                this.x = value;
            }
            else if (i == 1)
            {
                this.y = value;
            }
            else
            {
                this.z = value;
            }
        }
    }
    public static double Angle(VInt3 lhs, VInt3 rhs)
    {
        double d = ((double)Dot(lhs, rhs)) / (lhs.magnitude * rhs.magnitude);
        d = (d >= -1.0) ? ((d <= 1.0) ? d : 1.0) : -1.0;
        return Math.Acos(d) * Mathf.Rad2Deg;
    }

    public static VFactor AngleInt(VInt3 lhs, VInt3 rhs)
    {
        long den = lhs.magnitude * rhs.magnitude;
        return IntMath.acos((long)Dot(ref lhs, ref rhs), den);
    }

    public static int Dot(ref VInt3 lhs, ref VInt3 rhs)
    {
        return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
    }

    public static int Dot(VInt3 lhs, VInt3 rhs)
    {
        return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
    }

    public static long DotLong(VInt3 lhs, VInt3 rhs)
    {
        return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
    }

    public static long DotLong(ref VInt3 lhs, ref VInt3 rhs)
    {
        return (((lhs.x * rhs.x) + (lhs.y * rhs.y)) + (lhs.z * rhs.z));
    }

    public static long DotXZLong(ref VInt3 lhs, ref VInt3 rhs)
    {
        return ((lhs.x * rhs.x) + (lhs.z * rhs.z));
    }

    public static long DotXZLong(VInt3 lhs, VInt3 rhs)
    {
        return ((lhs.x * rhs.x) + (lhs.z * rhs.z));
    }

    public static VInt3 Cross(ref VInt3 lhs, ref VInt3 rhs)
    {
        return new VInt3(IntMath.Divide((int)((lhs.y * rhs.z) - (lhs.z * rhs.y)), 0x3e8), IntMath.Divide((int)((lhs.z * rhs.x) - (lhs.x * rhs.z)), 0x3e8), IntMath.Divide((int)((lhs.x * rhs.y) - (lhs.y * rhs.x)), 0x3e8));
    }

    public static VInt3 Cross(VInt3 lhs, VInt3 rhs)
    {
        return new VInt3(IntMath.Divide((int)((lhs.y * rhs.z) - (lhs.z * rhs.y)), 0x3e8), IntMath.Divide((int)((lhs.z * rhs.x) - (lhs.x * rhs.z)), 0x3e8), IntMath.Divide((int)((lhs.x * rhs.y) - (lhs.y * rhs.x)), 0x3e8));
    }

    public static VInt3 MoveTowards(VInt3 from, VInt3 to, int dt)
    {
        VInt3 num2 = to - from;
        if (num2.sqrMagnitudeLong <= (dt * dt))
        {
            return to;
        }
        VInt3 num = to - from;
        return (from + num.NormalizeTo(dt));
    }

    public VInt3 Normal2D()
    {
        return new VInt3(this.z, this.y, -this.x);
    }

    public VInt3 NormalizeTo(int newMagn)
    {
        long num = this.x * 100;
        long num2 = this.y * 100;
        long num3 = this.z * 100;
        long a = ((num * num) + (num2 * num2)) + (num3 * num3);
        if (a != 0)
        {
            long b = IntMath.Sqrt(a);
            long num6 = newMagn;
            this.x = (int)IntMath.Divide((long)(num * num6), b);
            this.y = (int)IntMath.Divide((long)(num2 * num6), b);
            this.z = (int)IntMath.Divide((long)(num3 * num6), b);
        }
        return this;
    }

    public VInt3 NormalizeV3()
    {
        VInt3 retVal = this;
        long num = retVal.x * 100;
        long num2 = retVal.y * 100;
        long num3 = retVal.z * 100;
        long a = ((num * num) + (num2 * num2)) + (num3 * num3);
        if (a != 0)
        {
            long b = IntMath.Sqrt(a);
            //if (b)
            //{
            //}
            long num6 = Precision;
            if (Math.Abs(num) == b)
            {
                retVal.x = (int)(num * num6 / b);
            }
            else
            {
                retVal.x = (int)IntMath.Divide((long)(num * num6), b);
            }
            if (Math.Abs(num2) == b)
            {
                retVal.y = (int)(num2 * num6 / b);
            }
            else
            {
                retVal.y = (int)IntMath.Divide((long)(num2 * num6), b);
            }
            if (Math.Abs(num3) == b)
            {
                retVal.z = (int)(num3 * num6 / b);
            }
            else
            {
                retVal.z = (int)IntMath.Divide((long)(num3 * num6), b);
            }
            //retVal.y = (int)IntMath.Divide((long)(num2 * num6), b);
            //retVal.z = (int)IntMath.Divide((long)(num3 * num6), b);
            //retVal *= Precision;
        }
        return retVal;
    }

    public VInt3 normalized
    {
        get
        {
            return NormalizeV3();
            //return ((Vector3)this).normalized;
        }
    }

    public long Normalize()
    {
        long num = this.x << 7;
        long num2 = this.y << 7;
        long num3 = this.z << 7;
        long a = ((num * num) + (num2 * num2)) + (num3 * num3);
        if (a == 0)
        {
            return 0L;
        }
        long b = IntMath.Sqrt(a);
        long num6 = 0x3e8L;
        this.x = (int)IntMath.Divide((long)(num * num6), b);
        this.y = (int)IntMath.Divide((long)(num2 * num6), b);
        this.z = (int)IntMath.Divide((long)(num3 * num6), b);
        return (b >> 7);
    }

    public Vector3 vec3
    {
        get
        {
            return new Vector3(this.x * 0.001f, this.y * 0.001f, this.z * 0.001f);
        }
    }
    public VInt2 xz
    {
        get
        {
            return new VInt2(this.x, this.z);
        }
    }
    public int magnitude
    {
        get
        {
            long x = this.x;
            long y = this.y;
            long z = this.z;
            return IntMath.Sqrt(((x * x) + (y * y)) + (z * z));
        }
    }
    public int magnitude2D
    {
        get
        {
            long x = this.x;
            long z = this.z;
            return IntMath.Sqrt((x * x) + (z * z));
        }
    }

    public static int Distance(VInt3 l, VInt3 r)
    {
        return IntMath.Divide((l - r).magnitude, Precision);
    }

    public VInt3 RotateY(ref VFactor radians)
    {
        VInt3 num;
        VFactor factor;
        VFactor factor2;
        IntMath.sincos(out factor, out factor2, radians.nom, radians.den);
        long num2 = factor2.nom * factor.den;
        long num3 = factor2.den * factor.nom;
        long b = factor2.den * factor.den;
        num.x = (int)IntMath.Divide((long)((this.x * num2) + (this.z * num3)), b);
        num.z = (int)IntMath.Divide((long)((-this.x * num3) + (this.z * num2)), b);
        num.y = 0;
        return num.NormalizeTo(Precision);
    }

    public VInt3 RotateY(int degree)
    {
        VInt3 num;
        VFactor factor;
        VFactor factor2;
        IntMath.sincos(out factor, out factor2, (long)(0x7ab8 * degree), 0x1b7740L);
        long num2 = factor2.nom * factor.den;
        long num3 = factor2.den * factor.nom;
        long b = factor2.den * factor.den;
        num.x = (int)IntMath.Divide((long)((this.x * num2) + (this.z * num3)), b);
        num.z = (int)IntMath.Divide((long)((-this.x * num3) + (this.z * num2)), b);
        num.y = 0;
        return num.NormalizeTo(Precision);
    }

    /// <summary>
    /// 球形插值
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public static VInt3 Slerp(VInt3 start, VInt3 end, VFactor percent)
    {
        if (percent == VFactor.zero)
        {
            return start;
        }
        else if (percent == VFactor.one)
        {
            return end;
        }

        long den = start.magnitude * end.magnitude;

        VFactor dot = new VFactor((long)Dot(ref start, ref end), den);

        VFactor angle = IntMath.acos(dot.nom, dot.den);

        VFactor theta = angle * percent;

        VInt3 RelativeVec = end - start * dot;
        RelativeVec.NormalizeTo(Precision);

        VFactor sin_theta, cos_thera;
        IntMath.sincos(out sin_theta, out cos_thera, theta);

        return start * cos_thera + RelativeVec * sin_theta;
    }

    /// <summary>
    /// 向量旋转
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="max_radians"></param>
    /// <returns></returns>
    public static VInt3 RotateTowards(VInt3 start, VInt3 end, VFactor max_radians)
    {
        VFactor angle = AngleInt(start, end);
        if (angle == VFactor.zero)
        {
            return end;
        }

        VFactor percent = IntMath.Min(max_radians / angle, VFactor.one);
        return Slerp(start, end, percent);
    }

    public int costMagnitude
    {
        get
        {
            return this.magnitude;
        }
    }
    public float worldMagnitude
    {
        get
        {
            double x = this.x;
            double y = this.y;
            double z = this.z;
            return (((float)Math.Sqrt(((x * x) + (y * y)) + (z * z))) * PrecisionFactor);
        }
    }
    public double sqrMagnitude
    {
        get
        {
            double x = this.x;
            double y = this.y;
            double z = this.z;
            return (((x * x) + (y * y)) + (z * z));
        }
    }
    public long sqrMagnitudeLong
    {
        get
        {
            long x = this.x;
            long y = this.y;
            long z = this.z;
            return (((x * x) + (y * y)) + (z * z));
        }
    }
    public long sqrMagnitudeLong2D
    {
        get
        {
            long x = this.x;
            long z = this.z;
            return ((x * x) + (z * z));
        }
    }
    public int unsafeSqrMagnitude
    {
        get
        {
            return (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
        }
    }
    public VInt3 abs
    {
        get
        {
            return new VInt3(Math.Abs(this.x), Math.Abs(this.y), Math.Abs(this.z));
        }
    }
    [Obsolete("Same implementation as .magnitude")]
    public float safeMagnitude
    {
        get
        {
            double x = this.x;
            double y = this.y;
            double z = this.z;
            return (float)Math.Sqrt(((x * x) + (y * y)) + (z * z));
        }
    }
    [Obsolete(".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)")]
    public float safeSqrMagnitude
    {
        get
        {
            float num = this.x * PrecisionFactor;
            float num2 = this.y * PrecisionFactor;
            float num3 = this.z * PrecisionFactor;
            return (((num * num) + (num2 * num2)) + (num3 * num3));
        }
    }
    public override string ToString()
    {
        object[] objArray1 = new object[] { "( ", this.x, ", ", this.y, ", ", this.z, ")" };
        return string.Concat(objArray1);
    }

    public override bool Equals(object o)
    {
        if (o == null)
        {
            return false;
        }
        VInt3 num = (VInt3)o;
        return (((this.x == num.x) && (this.y == num.y)) && (this.z == num.z));
    }

    public override int GetHashCode()
    {
        return (((this.x * 0x466f45d) ^ (this.y * 0x127409f)) ^ (this.z * 0x4f9ffb7));
    }

    public static VInt3 Lerp(VInt3 a, VInt3 b, float f)
    {
        return new VInt3(Mathf.RoundToInt(a.x * (1f - f)) + Mathf.RoundToInt(b.x * f), Mathf.RoundToInt(a.y * (1f - f)) + Mathf.RoundToInt(b.y * f), Mathf.RoundToInt(a.z * (1f - f)) + Mathf.RoundToInt(b.z * f));
    }

    public static VInt3 Lerp(VInt3 a, VInt3 b, VFactor f)
    {
        return new VInt3(((int)IntMath.Divide((long)((b.x - a.x) * f.nom), f.den)) + a.x, ((int)IntMath.Divide((long)((b.y - a.y) * f.nom), f.den)) + a.y, ((int)IntMath.Divide((long)((b.z - a.z) * f.nom), f.den)) + a.z);
    }

    public static VInt3 Lerp(VInt3 a, VInt3 b, int factorNom, int factorDen)
    {
        return new VInt3(IntMath.Divide((int)((b.x - a.x) * factorNom), factorDen) + a.x, IntMath.Divide((int)((b.y - a.y) * factorNom), factorDen) + a.y, IntMath.Divide((int)((b.z - a.z) * factorNom), factorDen) + a.z);
    }

    public long XZSqrMagnitude(VInt3 rhs)
    {
        long num = this.x - rhs.x;
        long num2 = this.z - rhs.z;
        return ((num * num) + (num2 * num2));
    }

    public long XZSqrMagnitude(ref VInt3 rhs)
    {
        long num = this.x - rhs.x;
        long num2 = this.z - rhs.z;
        return ((num * num) + (num2 * num2));
    }

    public bool IsEqualXZ(VInt3 rhs)
    {
        return ((this.x == rhs.x) && (this.z == rhs.z));
    }

    public bool IsEqualXZ(ref VInt3 rhs)
    {
        return ((this.x == rhs.x) && (this.z == rhs.z));
    }

    public static VInt3 ProjectOnXZ(VInt3 value)
    {
        value.y = 0;
        return value;
    }

    public static bool operator ==(VInt3 lhs, VInt3 rhs)
    {
        return (((lhs.x == rhs.x) && (lhs.y == rhs.y)) && (lhs.z == rhs.z));
    }

    public static bool operator !=(VInt3 lhs, VInt3 rhs)
    {
        return (((lhs.x != rhs.x) || (lhs.y != rhs.y)) || (lhs.z != rhs.z));
    }

    public static implicit operator VInt3(Vector3 ob)
    {
        return new VInt3(MMGame_Math.RoundToInt((double)(ob.x * FloatPrecision)), MMGame_Math.RoundToInt((double)(ob.y * FloatPrecision)), MMGame_Math.RoundToInt((double)(ob.z * FloatPrecision)));
    }

    public static implicit operator Vector3(VInt3 ob)
    {
        return new Vector3(ob.x * VInt3.PrecisionFactor, ob.y * VInt3.PrecisionFactor, ob.z * VInt3.PrecisionFactor);
    }

    public static VInt3 operator -(VInt3 lhs, VInt3 rhs)
    {
        lhs.x -= rhs.x;
        lhs.y -= rhs.y;
        lhs.z -= rhs.z;
        return lhs;
    }

    public static VInt3 operator -(VInt3 lhs)
    {
        lhs.x = -lhs.x;
        lhs.y = -lhs.y;
        lhs.z = -lhs.z;
        return lhs;
    }

    public static VInt3 operator +(VInt3 lhs, VInt3 rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }

    public static VInt3 operator *(VInt3 lhs, int rhs)
    {
        lhs.x *= rhs;
        lhs.y *= rhs;
        lhs.z *= rhs;
        return lhs;
    }

    public static VInt3 operator *(VInt3 lhs, float rhs)
    {
        lhs.x = MMGame_Math.RoundToInt((double)(lhs.x * rhs));
        lhs.y = MMGame_Math.RoundToInt((double)(lhs.y * rhs));
        lhs.z = MMGame_Math.RoundToInt((double)(lhs.z * rhs));
        return lhs;
    }

    public static VInt3 operator *(VInt3 lhs, double rhs)
    {
        lhs.x = MMGame_Math.RoundToInt(lhs.x * rhs);
        lhs.y = MMGame_Math.RoundToInt(lhs.y * rhs);
        lhs.z = MMGame_Math.RoundToInt(lhs.z * rhs);
        return lhs;
    }

    public static VInt3 operator *(VInt3 lhs, Vector3 rhs)
    {
        lhs.x = MMGame_Math.RoundToInt((double)(lhs.x * rhs.x));
        lhs.y = MMGame_Math.RoundToInt((double)(lhs.y * rhs.y));
        lhs.z = MMGame_Math.RoundToInt((double)(lhs.z * rhs.z));
        return lhs;
    }

    public static VInt3 operator *(VInt3 lhs, VInt3 rhs)
    {
        lhs.x *= rhs.x;
        lhs.y *= rhs.y;
        lhs.z *= rhs.z;
        return lhs;
    }

    public static VInt3 operator /(VInt3 lhs, float rhs)
    {
        lhs.x = MMGame_Math.RoundToInt((double)(((float)lhs.x) / rhs));
        lhs.y = MMGame_Math.RoundToInt((double)(((float)lhs.y) / rhs));
        lhs.z = MMGame_Math.RoundToInt((double)(((float)lhs.z) / rhs));
        return lhs;
    }

    public static implicit operator string(VInt3 ob)
    {
        return ob.ToString();
    }

    /// <summary>
    /// Transforms a vector by a quaternion rotation.
    /// </summary>
    /// <param name="vec">The vector to transform.</param>
    /// <param name="quat">The quaternion to rotate the vector by.</param>
    /// <returns></returns>
    public static VInt3 operator *(VQuaternion quat, VInt3 vec)
    {
        VInt3 result;
        VInt3.Transform(ref vec, ref quat, out result);
        return result;
    }

    /// <summary>
    /// Transforms a vector by a quaternion rotation.
    /// </summary>
    /// <param name="vec">The vector to transform.</param>
    /// <param name="quat">The quaternion to rotate the vector by.</param>
    /// <returns>The result of the operation.</returns>
    public static VInt3 Transform(VInt3 vec, VQuaternion quat)
    {
        VInt3 result;
        Transform(ref vec, ref quat, out result);
        return result;
    }

    /// <summary>
    /// Transforms a vector by a quaternion rotation.
    /// </summary>
    /// <param name="vec">The vector to transform.</param>
    /// <param name="quat">The quaternion to rotate the vector by.</param>
    /// <param name="result">The result of the operation.</param>
    public static void Transform(ref VInt3 vec, ref VQuaternion quat, out VInt3 result)
    {
        // Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:
        // vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)
        VInt3 xyz = quat.xyz, temp, temp2;
        temp = Cross(ref xyz, ref vec);
        temp2 = vec * quat.w / Precision;
        temp += temp2;
        temp = Cross(ref xyz, ref temp);
        temp *= 2;
        result = vec + temp;
    }
}

