using UnityEngine;
using System.Collections;

/// <summary>
/// 辅助计算
/// </summary>
public class AuxiliaryCalculation
{
    /// <summary>
    /// 解析车辆轮廓点
    /// </summary>
    /// <param name="car"></param>
    /// <param name="backAerial">后天线点位置</param>
    /// <param name="altitudeAngle">仰角（度）</param>
    /// <param name="pathAngle">航向角（度）</param>
    public static void AnalysisCarPoints(Car car, Vector2 backAerial, float altitudeAngle, float pathAngle)
    {
        car.backAerial = backAerial;
        car.altitudeAngle = altitudeAngle;//仰角
        car.pathAngle = pathAngle;//航向角

        for (int i = 0, j = car.origianlPoints.Length; i < j; i++)
        {
            GetProjectedPoint(car, car.origianlPoints[i], ref car.projectedPoints[i]);
        }

        GetProjectedPoint(car, car.frontAerial, ref car.projectedFrontAerial);
    }

    /// <summary>
    /// 计算偏移点的投影点
    /// </summary>
    /// <param name="car"></param>
    /// <param name="vec">输入点</param>
    /// <param name="rvec">返回结果点</param>
    public static void GetProjectedPoint(Car car, Vector2 vec, ref Vector2 rvec)
    {
        float t_altitudeAngle = car.altitudeAngle - car.altitudeOffset;//实际仰角
        float t_R = Mathf.Sqrt(vec.y * vec.y + car.backAerialHeight * car.backAerialHeight);//点到后天线点的x平面距离
        float t_angle = Mathf.Atan2(vec.y, car.backAerialHeight) * Mathf.Rad2Deg;//有正负角
        float t_angle2 = t_angle + t_altitudeAngle;//有正负值
        float x = vec.x;
        float y = t_R * Mathf.Sin(Mathf.Deg2Rad * t_angle2);//投影后的y

        LocalToGlobal(car.backAerial, car.pathAngle, new Vector2(x, y), ref rvec);
    }

    /// <summary>
    /// 根据局部坐标原点的在世界中的坐标，航向角，和偏移点在局部坐标系中的偏移值计算偏移点在世界中的位置
    /// </summary>
    /// <param name="basePos">局部坐标原点的世界中的坐标</param>
    /// <param name="pathAngle">局部坐标原点在世界中的航向角</param>
    /// <param name="inputPos">输入局部坐标</param>
    public static void LocalToGlobal(Vector2 basePos, float pathAngle, Vector2 inputPos, ref Vector2 outputPos)
    {
        outputPos.x = inputPos.x * Mathf.Cos(pathAngle * Mathf.Deg2Rad) + inputPos.y * Mathf.Sin(pathAngle * Mathf.Deg2Rad) + basePos.x;
        outputPos.y = inputPos.y * Mathf.Cos(pathAngle * Mathf.Deg2Rad) - inputPos.x * Mathf.Sin(pathAngle * Mathf.Deg2Rad) + basePos.y;
    }

    /// <summary>
    /// offsetPos从全局坐标转换到basePos为圆心的局部坐标
    /// </summary>
    /// <param name="basePos">基准点（全局坐标）</param>
    /// <param name="baseOffset">基准点的局部坐标</param>
    /// <param name="pathAngle">basePos的航向角</param>
    /// <param name="offsetPos">要转换的全局坐标</param>
    /// <param name="outputPos">返回结果点</param>
    public static void GlobalToLocal(Vector2 basePos, Vector2 baseOffset, float pathAngle, Vector2 inputPos, ref Vector2 outputPos)
    {
        float x = inputPos.x - basePos.x;
        float y = inputPos.y - basePos.y;//把basePos看成在世界原点，这样inputPos相当于围绕世界原点旋转

        outputPos.x = x * Mathf.Cos(-pathAngle * Mathf.Deg2Rad) + y * Mathf.Sin(-pathAngle * Mathf.Deg2Rad) + baseOffset.x;
        outputPos.y = y * Mathf.Cos(-pathAngle * Mathf.Deg2Rad) - x * Mathf.Sin(-pathAngle * Mathf.Deg2Rad) + baseOffset.y;
    }

    /// <summary>
    /// 求点为center，半径为R，绕航向角pathAngle旋转后的点
    /// </summary>
    /// <param name="center">中心点的世界坐标</param>
    /// <param name="R">半径</param>
    /// <param name="pathAngle">绕行的航向角</param>
    /// <param name="outputPos">返回结果点的世界坐标</param>
    public static void GetRotatedPos(Vector2 center, float R, float pathAngle, ref Vector2 outputPos)
    {
        outputPos.x = R * Mathf.Sin(Mathf.Deg2Rad * pathAngle) + center.x;
        outputPos.y = R * Mathf.Cos(Mathf.Deg2Rad * pathAngle) + center.y;
    }

    /// <summary>
    /// 获取一个点按照某个向量位移后的点
    /// </summary>
    /// <param name="angle">航向角</param>
    /// <param name="dis">移动距离绝对值</param>
    /// <param name="direction">移动方向，1正，-1负，0，不动</param>
    /// <param name="inputPos">输入世界坐标</param>
    /// <param name="outputPos">返回结果点的世界坐标</param>
    public static void GetTransPos(float angle, float dis, int direction, Vector2 inputPos, ref Vector2 outputPos)
    {
        if (direction == 0)
            outputPos = inputPos;

        float t_angle = direction == 1 ? angle : direction == -1 ? angle + 180 : 0;

        outputPos.x = inputPos.x + dis * Mathf.Sin(Mathf.Deg2Rad * t_angle);
        outputPos.y = inputPos.y + dis * Mathf.Cos(Mathf.Deg2Rad * t_angle);
    }

    /// <summary>
    /// 根据内前轮转向角计算外前轮转向角
    /// </summary>
    /// <param name="beta">内前轮转向角,向右拐，传右侧轮子的角度，左拐传左侧轮子的角度</param>
    /// <param name="L">前后轴距</param>
    /// <param name="B">左右轴距</param>
    public static float CalculateWheelSteeringAngle(float beta, float L, float B)
    {
        float sign = Mathf.Sign(beta);
        float absBeta = Mathf.Abs(beta);
        return sign * Mathf.Atan(1 / (1 / Mathf.Tan(Mathf.Deg2Rad * absBeta) + B / L)) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 判断pos1,pos2组成的线段与pos3,pos4组成的线段是否平行,重合也算平行
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <param name="pos3"></param>
    /// <param name="pos4"></param>
    /// <returns></returns>
    public static bool IsParallel(Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4)
    {
        if (pos1.x - pos2.x == 0)
        {
            return pos3.x - pos4.x == 0;
        }
        else
        {
            if (pos3.x - pos4.x == 0)
                return false;

            float slope1 = (pos1.y - pos2.y) / (pos1.x - pos2.x);
            float slope2 = (pos3.y - pos4.y) / (pos3.x - pos4.x);
            return slope1 == slope2;
        }
    }

    /// <summary>
    /// 计算两个线段垂直平分线的交点,pos2为两线段交点
    /// 还未判断垂直坐标轴的情况
    /// </summary>
    /// <param name="startPointA"></param>
    /// <param name="endPointA"></param>
    /// <param name="startPointB"></param>
    /// <param name="endPointB"></param>
    /// <param name="intersectionPoint">如果有交点则返回交点坐标</param>
    /// <returns>是否有交点，平行和垂直坐标轴时都无交点</returns>
    public static bool ComputingIntersection(Vector2 startPointA, Vector2 endPointA, Vector2 startPointB, Vector2 endPointB, ref Vector2 intersectionPoint)
    {
        bool hasIntersection = IsParallel(startPointA, endPointA, startPointB, endPointB);

        if (hasIntersection)
            return false;

        float e = startPointA.y - endPointA.y;
        float f = startPointB.y - endPointB.y;

        if (e == 0)
            e = 0.000000001f;

        if (f == 0)
            f = 0.000000001f;

        float a = -(endPointA.x - startPointA.x) * (endPointA.x + startPointA.x) * 0.5f / e + (startPointA.y + endPointA.y) * 0.5f;
        float b = -(endPointB.x - startPointB.x) * (endPointB.x + startPointB.x) * 0.5f / f + (startPointB.y + endPointB.y) * 0.5f;
        float c = (endPointA.x - startPointA.x) / e;
        float d = (endPointB.x - startPointB.x) / f;

        float g = c - d;

        if (g == 0)
            g = 0.000000001f;

        intersectionPoint.x = (b - a) / g;
        intersectionPoint.y = c * intersectionPoint.x + a;

        return true;
    }
}
