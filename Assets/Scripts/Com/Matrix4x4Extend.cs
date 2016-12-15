using UnityEngine;
using System.Collections;

/// <summary>
/// 矩阵扩展
/// </summary>
public static class Matrix4x4Extend 
{
    /// <summary>
    /// 重置矩阵
    /// </summary>
    /// <param name="m">输出矩阵</param>
    /// <returns></returns>
    public static Matrix4x4 MatrixIdentity(this Matrix4x4 m)
    {
        m = Matrix4x4.zero;
        m.m00 = 1; m.m11 = 0; m.m22 = 0; m.m33 = 0;
        return m;
    }

    /// <summary>
    /// 缩放
    /// </summary>
    /// <param name="m">输出矩阵</param>
    /// <param name="scale">缩放向量</param>
    /// <returns></returns>
    public static Matrix4x4 MatrixScale(this Matrix4x4 m, Vector3 scale)
    {
        m.m00 = scale.x;
        m.m11 = scale.y;
        m.m22 = scale.z;
        return m;
    }

    /// <summary>
    /// 平移
    /// </summary>
    /// <param name="m">输出矩阵</param>
    /// <param name="trans">位移向量</param>
    /// <returns></returns>
    public static Matrix4x4 MatrixTranslation(this Matrix4x4 m, Vector3 trans)
    {
        m.m30 = trans.x;
        m.m31 = trans.y;
        m.m32 = trans.z;
        return m;
    }

    /// <summary>
    /// 围绕指定轴旋转
    /// </summary>
    /// <param name="m">输出矩阵</param>
    /// <param name="axis">轴向量</param>
    /// <param name="angle">角度</param>
    /// <returns></returns>
    public static Matrix4x4 MatrixRotation(this Matrix4x4 m,Vector3 axis, float angle)
    {
        m.m00 = axis.x * axis.x * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) + Mathf.Cos(angle * Mathf.Deg2Rad);
        m.m01 = axis.x * axis.y * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) + axis.z * Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m02 = axis.x * axis.z * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) - axis.y * Mathf.Sin(angle * Mathf.Deg2Rad);

        m.m10 = axis.x * axis.y * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) - axis.z * Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m11 = axis.y * axis.y * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) + Mathf.Cos(angle * Mathf.Deg2Rad);
        m.m12 = axis.y * axis.z * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) + axis.x * Mathf.Sin(angle * Mathf.Deg2Rad);

        m.m20 = axis.x * axis.z * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) - axis.y * Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m21 = axis.y * axis.z * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) - Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m22 = axis.z * axis.z * (1 - Mathf.Cos(angle * Mathf.Deg2Rad)) + Mathf.Cos(angle * Mathf.Deg2Rad);

        return m;
    }

    /// <summary>
    /// 四元数转矩阵
    /// </summary>
    /// <param name="m">输出矩阵</param>
    /// <param name="q">输入四元数</param>
    /// <returns></returns>
    public static Matrix4x4 QuatMatrix(this Matrix4x4 m,Quaternion q)
    {
        float xx, xy, xz, xw;
        float yy, yz, yw;
        float zz, zw;

        xx = q[0]* q[0];
        xy = q[0]* q[1];
        xz = q[0]* q[2];
        xw = q[0]* q[3];
        yy = q[1]* q[1];
        yz = q[1]* q[2];
        yw = q[1]* q[3];
        zz = q[2]* q[2];
        zw = q[2]* q[3];

        m.m00 = 1-2*(yy+zz);
        m.m01 = 2 * (xy - zw);
        m.m02 = 2 * (xz + yw);
        m.m03 = 0;
        m.m10 = 2 * (xy + zw);
        m.m11 = 1 - 2 * (xx + zz);
        m.m12 = 2 * (yz - xw);
        m.m13 = 0;
        m.m20 = 2 * (xz - yw);
        m.m21 = 2 * (yz + xw);
        m.m22 = 1 - 2 * (xx + yy);
        m.m23 = 0;
        m.m30 = 0;
        m.m31 = 0;
        m.m32 = 0;
        m.m33 = 1;

        return m;
    }

    /// <summary>
    /// 矩阵转四元数
    /// </summary>
    /// <param name="q">输出</param>
    /// <param name="m">输入矩阵</param>
    /// <returns></returns>
    public static Quaternion MatrixQuat(this Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        float s;

        if (m.m00>m.m11&&m.m00>m.m22)
        {
            s = 2* Mathf.Sqrt(1f+m.m00-m.m11-m.m22);
            q[0] = 0.25f* s;
            q[1] = (m.m10+m.m01)/s;
            q[2] = (m.m02+m.m20)/s;
            q[3] = (m.m21-m.m12)/s;
        }
        else if (m.m11>m.m22)
        {
            s = 2 * Mathf.Sqrt(1f+ m.m11 - m.m00 - m.m22);
            q[0] = (m.m10 + m.m01) / s;
            q[1] = 0.25f* s;
            q[2] = (m.m21 +m.m12) / s;
            q[3] = (m.m02 - m.m20) / s;
        }
        else
        {
            s = 2* Mathf.Sqrt(1f+ m.m22 - m.m00 - m.m11);
            q[0] = (m.m02 + m.m20) /s;
            q[1] = (m.m21 + m.m12) / s;
            q[2] = 0.25f* s;
            q[3] = (m.m10 - m.m01) / s;
        }

        return q;
    }

    /// <summary>
    /// 欧拉角转矩阵
    /// </summary>
    /// <param name="angles"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    public static Matrix4x4 AngleMatrix(this Matrix4x4 m,Vector3 angles)
    {
        float cx, sx, cy, sy, cz, sz;
        float yx, yy;

        cx = Mathf.Cos(angles.x*Mathf.Deg2Rad);
        sx = Mathf.Sin(angles.x * Mathf.Deg2Rad);
        cy = Mathf.Cos(angles.y * Mathf.Deg2Rad);
        sy = Mathf.Sin(angles.y * Mathf.Deg2Rad);
        cz = Mathf.Cos(angles.z * Mathf.Deg2Rad);
        sz = Mathf.Sin(angles.z * Mathf.Deg2Rad);

        yx = sy* cx;
        yy = sy* sx;

        m.m00= cy * cz;
        m.m01 = -cy * sz;
        m.m02= sy;
        m.m03 = 0;
        m.m10 = yy * cz + cx * sz;
        m.m11= -yy * sz + cx * cz;
        m.m12= -sx;
        m.m13 = 0;
        m.m20= -yx * cz + sx * sz;
        m.m21= yx * sz + sx * cz;
        m.m22= cx * cy;
        m.m23 = 0;
        m.m30 = 0;
        m.m31 = 0;
        m.m32 = 0;
        m.m33 = 1;

        return m;
    }

    /// <summary>
    /// 矩阵转欧拉角
    /// </summary>
    /// <param name="m"></param>
    /// <param name="angles">3个角度</param>
    /// <returns></returns>
    public static Vector3 MatrixAngle(this Matrix4x4 m, Vector3 angles)
    {
        float c;
        float tx, ty;
        angles = new Vector3();

        angles.y = Mathf.Asin(m.m02);
        c = Mathf.Cos(angles.y);

        if (Mathf.Abs(c)>0.005f)
        {
            tx =  m.m22/c;
            ty = -m.m12/c;
            angles.x = Mathf.Atan2(ty, tx);
            tx =  m.m00/c;
            ty = -m.m01/c;
            angles.z = Mathf.Atan2(ty, tx);
        }
        else
        {
            angles.x = 0;
            tx = m.m11;
            ty = m.m10;
            angles.z = Mathf.Atan2(ty, tx);
        }

        return angles;
    }
    
}







