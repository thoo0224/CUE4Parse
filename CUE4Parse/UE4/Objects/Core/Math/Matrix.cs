﻿using System;
using System.Runtime.CompilerServices;
using CUE4Parse.UE4.Readers;
using CUE4Parse.Utils;

namespace CUE4Parse.UE4.Objects.Core.Math
{
    // Generic axis enum (mirrored for property use in Object.h)
    public enum EAxis
    {
        None,
        X,
        Y,
        Z,
    }

    public class FMatrix : IUStruct
    {
        public static FMatrix Identity => new(
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        );

        public float M00, M01, M02, M03;
        public float M10, M11, M12, M13;
        public float M20, M21, M22, M23;
        public float M30, M31, M32, M33;

        public FMatrix() {}

        public FMatrix(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            M00 = m00; M01 = m01; M02 = m02; M03 = m03;
            M10 = m10; M11 = m11; M12 = m12; M13 = m13;
            M20 = m20; M21 = m21; M22 = m22; M23 = m23;
            M30 = m30; M31 = m31; M32 = m32; M33 = m33;
        }

        public FMatrix(FArchive Ar)
        {
            M00 = Ar.Read<float>();
            M01 = Ar.Read<float>();
            M02 = Ar.Read<float>();
            M03 = Ar.Read<float>();
            M10 = Ar.Read<float>();
            M11 = Ar.Read<float>();
            M12 = Ar.Read<float>();
            M13 = Ar.Read<float>();
            M20 = Ar.Read<float>();
            M21 = Ar.Read<float>();
            M22 = Ar.Read<float>();
            M23 = Ar.Read<float>();
            M30 = Ar.Read<float>();
            M31 = Ar.Read<float>();
            M32 = Ar.Read<float>();
            M33 = Ar.Read<float>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FMatrix operator *(FMatrix a, FMatrix b) => new(
            a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20 + a.M03 * b.M30,
            a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21 + a.M03 * b.M31,
            a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22 + a.M03 * b.M32,
            a.M00 * b.M03 + a.M01 * b.M13 + a.M02 * b.M23 + a.M03 * b.M33,
            a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20 + a.M13 * b.M30,
            a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
            a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
            a.M10 * b.M03 + a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20 + a.M23 * b.M30,
            a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
            a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
            a.M20 * b.M03 + a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M30 * b.M00 + a.M31 * b.M10 + a.M32 * b.M20 + a.M33 * b.M30,
            a.M30 * b.M01 + a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
            a.M30 * b.M02 + a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
            a.M30 * b.M03 + a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector4 TransformFVector4(FVector4 p) => new(
            p.X * M00 + p.Y * M10 + p.Z * M20 + p.W * M30,
            p.X * M01 + p.Y * M11 + p.Z * M21 + p.W * M31,
            p.X * M02 + p.Y * M12 + p.Z * M22 + p.W * M32,
            p.X * M03 + p.Y * M13 + p.Z * M23 + p.W * M33
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector4 TransformPosition(FVector v) => TransformFVector4(new FVector4(v.X, v.Y, v.Z, 1.0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector InverseTransformPosition(FVector v)
        {
            var invSelf = InverseFast();
            return (FVector) invSelf.TransformPosition(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector4 TransformVector(FVector v) => TransformFVector4(new FVector4(v.X, v.Y, v.Z, 0.0f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FMatrix GetTransposed() => new(
            M00, M10, M20, M30,
            M01, M11, M21, M31,
            M02, M12, M22, M32,
            M03, M13, M23, M33
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Determinant() =>
            M00 * (
                M11 * (M22 * M33 - M23 * M32) -
                M21 * (M12 * M33 - M13 * M32) +
                M31 * (M12 * M23 - M13 * M22)
            ) -
            M10 * (
                M01 * (M22 * M33 - M23 * M32) -
                M21 * (M02 * M33 - M03 * M32) +
                M31 * (M02 * M23 - M03 * M22)
            ) +
            M20 * (
                M01 * (M12 * M33 - M13 * M32) -
                M11 * (M02 * M33 - M03 * M32) +
                M31 * (M02 * M13 - M03 * M12)
            ) -
            M30 * (
                M01 * (M12 * M23 - M13 * M22) -
                M11 * (M02 * M23 - M03 * M22) +
                M21 * (M02 * M13 - M03 * M12)
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RotDeterminant() =>
            M00 * (M11 * M22 - M12 * M21) -
            M10 * (M01 * M22 - M02 * M21) +
            M20 * (M01 * M12 - M02 * M11);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FMatrix InverseFast()
        {
            var result = new FMatrix();
            var det = new float[4];
            var tmp = new FMatrix();

            tmp.M00	= M22 * M33 - M23 * M32;
            tmp.M01	= M12 * M33 - M13 * M32;
            tmp.M02	= M12 * M23 - M13 * M22;

            tmp.M10	= M22 * M33 - M23 * M32;
            tmp.M11	= M02 * M33 - M03 * M32;
            tmp.M12	= M02 * M23 - M03 * M22;

            tmp.M20	= M12 * M33 - M13 * M32;
            tmp.M21	= M02 * M33 - M03 * M32;
            tmp.M22	= M02 * M13 - M03 * M12;

            tmp.M30	= M12 * M23 - M13 * M22;
            tmp.M31	= M02 * M23 - M03 * M22;
            tmp.M32	= M02 * M13 - M03 * M12;

            det[0] = M11*tmp.M00 - M21*tmp.M01 + M31*tmp.M02;
            det[1] = M01*tmp.M10 - M21*tmp.M11 + M31*tmp.M12;
            det[2] = M01*tmp.M20 - M11*tmp.M21 + M31*tmp.M22;
            det[3] = M01*tmp.M30 - M11*tmp.M31 + M21*tmp.M32;

            var determinant = M00*det[0] - M10*det[1] + M20*det[2] - M30*det[3];
            var rDet = 1.0f / determinant;

            result.M00 =  rDet * det[0];
            result.M01 = -rDet * det[1];
            result.M02 =  rDet * det[2];
            result.M03 = -rDet * det[3];
            result.M10 = -rDet * (M10*tmp.M00 - M20*tmp.M01 + M30*tmp.M02);
            result.M11 =  rDet * (M00*tmp.M10 - M20*tmp.M11 + M30*tmp.M12);
            result.M12 = -rDet * (M00*tmp.M20 - M10*tmp.M21 + M30*tmp.M22);
            result.M13 =  rDet * (M00*tmp.M30 - M10*tmp.M31 + M20*tmp.M32);
            result.M20 =  rDet * (
            	M10 * (M21 * M33 - M23 * M31) -
            	M20 * (M11 * M33 - M13 * M31) +
            	M30 * (M11 * M23 - M13 * M21)
            	);
            result.M21 = -rDet * (
            	M00 * (M21 * M33 - M23 * M31) -
            	M20 * (M01 * M33 - M03 * M31) +
            	M30 * (M01 * M23 - M03 * M21)
            	);
            result.M22 =  rDet * (
            	M00 * (M11 * M33 - M13 * M31) -
            	M10 * (M01 * M33 - M03 * M31) +
            	M30 * (M01 * M13 - M03 * M11)
            	);
            result.M23 = -rDet * (
            	M00 * (M11 * M23 - M13 * M21) -
            	M10 * (M01 * M23 - M03 * M21) +
            	M20 * (M01 * M13 - M03 * M11)
            	);
            result.M30 = -rDet * (
            	M10 * (M21 * M32 - M22 * M31) -
            	M20 * (M11 * M32 - M12 * M31) +
            	M30 * (M11 * M22 - M12 * M21)
            	);
            result.M31 =  rDet * (
            	M00 * (M21 * M32 - M22 * M31) -
            	M20 * (M01 * M32 - M02 * M31) +
            	M30 * (M01 * M22 - M02 * M21)
            	);
            result.M32 = -rDet * (
            	M00 * (M11 * M32 - M12 * M31) -
            	M10 * (M01 * M32 - M02 * M31) +
            	M30 * (M01 * M12 - M02 * M11)
            	);
            result.M33 =  rDet * (
            	M00 * (M11 * M22 - M12 * M21) -
            	M10 * (M01 * M22 - M02 * M21) +
            	M20 * (M01 * M12 - M02 * M11)
            	);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FMatrix Inverse()
        {
            // Check for zero scale matrix to invert
            if (GetScaledAxis(EAxis.X).IsNearlyZero(FMath.SmallNumber) &&
                GetScaledAxis(EAxis.Y).IsNearlyZero(FMath.SmallNumber) &&
                GetScaledAxis(EAxis.Z).IsNearlyZero(FMath.SmallNumber))
            {
                // just set to zero - avoids unsafe inverse of zero and duplicates what QNANs were resulting in before (scaling away all children)
                return Identity;
            }

            var det = Determinant();
            return det == 0.0f ? Identity : InverseFast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveScaling(float tolerance = FMath.SmallNumber)
        {
            // For each row, find magnitude, and if its non-zero re-scale so its unit length.
            var squareSum0 = M00*M00 + M01*M01 + M02*M02;
            var squareSum1 = M10*M10 + M11*M11 + M12*M12;
            var squareSum2 = M20*M20 + M21*M21 + M22*M22;

            //FloatSelect: return Comparand >= 0.f ? ValueGEZero : ValueLTZero;
            var scale0 = squareSum0 - tolerance >= 0 ? squareSum0.InvSqrt() : 1;
            var scale1 = squareSum1 - tolerance >= 0 ? squareSum1.InvSqrt() : 1;
            var scale2 = squareSum2 - tolerance >= 0 ? squareSum2.InvSqrt() : 1;

            M00 *= scale0;
            M01 *= scale0;
            M02 *= scale0;
            M10 *= scale1;
            M11 *= scale1;
            M12 *= scale1;
            M20 *= scale2;
            M21 *= scale2;
            M22 *= scale2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMaximumAxisScale()
        {
            var maxRowScaleSquared = MathF.Max(
                GetScaledAxis(EAxis.X).SizeSquared(),
                MathF.Max(
                    GetScaledAxis(EAxis.Y).SizeSquared(),
                    GetScaledAxis(EAxis.Z).SizeSquared()
                )
            );
            return MathF.Sqrt(maxRowScaleSquared);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector GetOrigin() => new(M30, M31, M32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector GetScaledAxis(EAxis axis) => axis switch
        {
            EAxis.X => new FVector(M00, M01, M02),
            EAxis.Y => new FVector(M10, M11, M12),
            EAxis.Z => new FVector(M20, M21, M22),
            _ => FVector.ZeroVector
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAxis(int i, FVector axis)
        {
            switch (i)
            {
                case 0:
                    M00 = axis.X;
                    M01 = axis.Y;
                    M02 = axis.Z;
                    break;
                case 1:
                    M10 = axis.X;
                    M11 = axis.Y;
                    M12 = axis.Z;
                    break;
                case 2:
                    M20 = axis.X;
                    M21 = axis.Y;
                    M22 = axis.Z;
                    break;
                case 3:
                    M30 = axis.X;
                    M31 = axis.Y;
                    M32 = axis.Z;
                    break;
            }
        }

        public FRotator Rotator()
        {
            var xAxis = GetScaledAxis(EAxis.X);
            var yAxis = GetScaledAxis(EAxis.Y);
            var zAxis = GetScaledAxis(EAxis.Z);

            var rotator = new FRotator(
                MathF.Atan2(xAxis.Z, MathF.Sqrt(xAxis.X*xAxis.X + xAxis.Y*xAxis.Y)) * 180.0f / MathF.PI,
                MathF.Atan2(xAxis.Y, xAxis.X) * 180.0f / MathF.PI,
                0
            );

            var syAxis = new FRotationMatrix(rotator).GetScaledAxis(EAxis.Y);
            rotator.Roll = MathF.Atan2(zAxis | syAxis, yAxis | syAxis) * 180.0f / MathF.PI;

            return rotator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FVector GetScaleVector(float tolerance = FMath.SmallNumber)
        {
            var Scale3D = new FVector(1, 1, 1);

            // For each row, find magnitude, and if its non-zero re-scale so its unit length.
            var SquareSum0 = (M00 * M00) + (M01 * M01) + (M02 * M02);
            Scale3D[0] = SquareSum0 > tolerance ? MathF.Sqrt(SquareSum0) : 0.0f;

            var SquareSum1 = (M10 * M10) + (M11 * M11) + (M12 * M12);
            Scale3D[1] = SquareSum1 > tolerance ? MathF.Sqrt(SquareSum1) : 0.0f;

            var SquareSum2 = (M20 * M20) + (M21 * M21) + (M22 * M22);
            Scale3D[2] = SquareSum2 > tolerance ? MathF.Sqrt(SquareSum2) : 0.0f;

            return Scale3D;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FQuat ToQuat() => new(this);

        public override string ToString() => $"[{M00:F1} {M01:F1} {M02:F1} {M03:F1}] [{M10:F1} {M11:F1} {M12:F1} {M13:F1}] [{M20:F1} {M21:F1} {M22:F1} {M23:F1}] [{M30:F1} {M31:F1} {M32:F1} {M33:F1}]";
    }
}
