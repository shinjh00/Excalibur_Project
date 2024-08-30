using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 개발자 : 이형필 / 전역에서 베지어곡선을 사용할 수 있게 만드는 클래스
/// </summary>
public static class BezierCurve
{
    public static Vector3 p1;
    public static Vector3 p2;
    public static Vector3 p3;
    public static Vector3 p4;

    /// <summary>
    /// 각 정점들을 기준으로 곡선형태로 잇는 정점을 구하는 함수
    /// </summary>
    /// <param name="p_1"></param>
    /// <param name="p_2"></param>
    /// <param name="p_3"></param>
    /// <param name="p_4"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Vector3 Bezier(
        Vector3 p_1,
        Vector3 p_2,
        Vector3 p_3,
        Vector3 p_4,
        float value)
    {
        Vector3 A = Vector2.Lerp(p_1, p_2, value);
        Vector3 B = Vector2.Lerp(p_2, p_3, value);
        Vector3 C = Vector2.Lerp(p_3, p_4, value);
                        
        Vector3 D = Vector2.Lerp(A, B, value);
        Vector3 E = Vector2.Lerp(B, C, value);
                    
        Vector3 F = Vector2.Lerp(D, E, value);

        return F;
    }
}
