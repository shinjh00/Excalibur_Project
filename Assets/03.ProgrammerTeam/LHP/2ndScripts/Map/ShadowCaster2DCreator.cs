using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;



[RequireComponent(typeof(CompositeCollider2D))]
/// <summary>
/// 개발자 : 이형필 / 타일맵에 그림자를 만들어주는 클래스
/// </summary>
public class ShadowCaster2DCreator : MonoBehaviour
{
    [Space]
    [SerializeField]
    private bool selfShadows = true;

    [SerializeField]
    private Grid grid; // 전체 그리드를 정의할 Grid 컴포넌트

    private CompositeCollider2D tilemapCollider;
    private Tilemap tilemap; 
    Vector2[] gridBoundary;

    static readonly FieldInfo meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly FieldInfo shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);
    static readonly MethodInfo generateShadowMeshMethod = typeof(ShadowCaster2D)
                                    .Assembly
                                    .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
                                    .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
    /// <summary>
    /// 그림자 그리기위해 경로를 가져오고 해당 정보를 토대로 그림자를 그리는 메서드를 호출하는 메서드
    /// </summary>
    public void Create()
    {
        DestroyOldShadowCasters();
        tilemapCollider = GetComponent<CompositeCollider2D>();
        tilemap = GetComponent<Tilemap>(); 

        if (grid == null)
        {
            Debug.LogError("Grid is missing!");
            return;
        }

        if (tilemap == null)
        {
            Debug.LogError("Tilemap is missing!");
            return;
        }

        // 타일맵의 경계를 정의
        BoundsInt gridBounds = tilemap.cellBounds;
        Vector2 min = grid.CellToLocal(gridBounds.min);
        Vector2 max = grid.CellToLocal(gridBounds.max);

        // 그리드 꼭지점을 한바퀴 돌아그려야해서 원점 하나 더 추가
        gridBoundary = new Vector2[]
        {
            new Vector2(min.x, min.y),
            new Vector2(max.x, min.y),
            new Vector2(max.x, max.y),
            new Vector2(min.x, max.y),
            new Vector2(min.x, min.y)
        };

        // 타일맵 경로를 정의하고 그리드 꼭지점과 결합

        for (int i = 0; i < tilemapCollider.pathCount; i += 2)
        {
            if (i  >= tilemapCollider.pathCount-1) // 인덱스가 범위를 벗어나지 않도록 함
                break;


            Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
            
            tilemapCollider.GetPath(i, pathVertices);


            Vector2[] nextPathVertices = new Vector2[tilemapCollider.GetPathPointCount(i + 1)];
            tilemapCollider.GetPath(i + 1, nextPathVertices);



           
         /*   Vector2 curMin = pathVertices[0];
            Vector2 nextMin = nextPathVertices[0];
            // 두 경로의 좌하단 좌표를 비교
          / foreach (Vector2 v in pathVertices)
            {
                if (v.x < curMin.x || (v.x == curMin.x && v.y < curMin.y))
                {
                    curMin = v;
                }
            }
            foreach (Vector2 v in nextPathVertices)
            {
                if (v.x < nextMin.x || (v.x == nextMin.x && v.y < nextMin.y))
                {
                    nextMin = v;
                }
            }*/
            if (pathVertices.Length < 50)
            {
                
                    CreateShadow(pathVertices,nextPathVertices);
            }
            else
            {
                CreateShadow(nextPathVertices, null);
            }


        }
    }
    /// <summary>
    /// 받아온 좌표 리스트를 이어 그림자를 그릴 패스를 지정하고 만드는 메서드
    /// </summary>
    /// <param name="pathVertices"></param>
    /// <param name="nextVer"></param>
    void CreateShadow(Vector2[] pathVertices, Vector2[] nextVer)
    {
        GameObject shadowCaster = new GameObject($"shadow_caster");
        shadowCaster.transform.parent = gameObject.transform;
        ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
        shadowCaster.transform.localPosition = Vector3.zero;
        shadowCasterComponent.selfShadows = this.selfShadows;

        // 그리드 경계와 타일맵 경로를 결합하여 섀도캐스터 경로 정의
        List<Vector3> shadowPath = new List<Vector3>(gridBoundary.Length + pathVertices.Length);

        // 그리드 경계 추가
        if (pathVertices.Length > 100)
            shadowPath.AddRange(gridBoundary.Select(v => (Vector3)v));

        // 타일맵 경로 추가 (순서를 반대로 하여 추가)
        shadowPath.Add(pathVertices[0]);
        shadowPath.AddRange(pathVertices.Reverse().Select(v => (Vector3)v));

        // 경로 설정
        shapePathField.SetValue(shadowCasterComponent, shadowPath.ToArray());
        shapePathHashField.SetValue(shadowCasterComponent, Random.Range(int.MinValue, int.MaxValue));
        meshField.SetValue(shadowCasterComponent, new Mesh());
        generateShadowMeshMethod.Invoke(shadowCasterComponent, new object[] { meshField.GetValue(shadowCasterComponent), shapePathField.GetValue(shadowCasterComponent) });





        if (nextVer != null)
        {
            GameObject shadowCasterNext = new GameObject($"shadow_casterNext");
            shadowCasterNext.transform.parent = gameObject.transform;
            ShadowCaster2D shadowCasterComponentNext = shadowCasterNext.AddComponent<ShadowCaster2D>();
            shadowCasterNext.transform.localPosition = Vector3.zero;
            shadowCasterComponentNext.selfShadows = this.selfShadows;

            // 그리드 경계와 타일맵 경로를 결합하여 섀도캐스터 경로 정의
            List<Vector3> shadowPathNext = new List<Vector3>(gridBoundary.Length + nextVer.Length);

            // 그리드 경계 추가
            if (nextVer.Length > 100)
                shadowPathNext.AddRange(gridBoundary.Select(v => (Vector3)v));

            // 타일맵 경로 추가 (순서를 반대로 하여 추가)
            shadowPathNext.Add(nextVer[0]);
            shadowPathNext.AddRange(nextVer.Reverse().Select(v => (Vector3)v));

            // 경로 설정
            shapePathField.SetValue(shadowCasterComponentNext, shadowPathNext.ToArray());
            shapePathHashField.SetValue(shadowCasterComponentNext, Random.Range(int.MinValue, int.MaxValue));
            meshField.SetValue(shadowCasterComponentNext, new Mesh());
            generateShadowMeshMethod.Invoke(shadowCasterComponentNext, new object[] { meshField.GetValue(shadowCasterComponentNext), shapePathField.GetValue(shadowCasterComponentNext) });



            float allDistance = 0;
            float nextAllDistance = 0;
            for (int i = 0; i < shadowCasterComponent.shapePath.Length - 1; i++)
            {
                Vector3 vector1 = shadowCasterComponent.shapePath[i];
                Vector3 vector2 = shadowCasterComponent.shapePath[i + 1];
                float distance = Vector3.Distance(vector1, vector2);

                allDistance += distance;
            }
            for (int i = 0; i < shadowCasterComponent.shapePath.Length - 1; i++)
            {
                Vector3 vector1 = shadowCasterComponent.shapePath[i];
                Vector3 vector2 = shadowCasterComponent.shapePath[i + 1];
                float distance = Vector3.Distance(vector1, vector2);

                nextAllDistance += distance;
            }

            if (allDistance < nextAllDistance)
            {
                Destroy(shadowCaster);
            }
            else
            {
                Destroy(shadowCasterNext);
            }
        }
       

    }
    /// <summary>
    /// 원래 있던 섀도우캐스트 삭제
    /// </summary>
    public void DestroyOldShadowCasters()
    {
        var tempList = transform.Cast<Transform>().ToList();
        foreach (var child in tempList)
        {
            DestroyImmediate(child.gameObject);
        }
    }

   /*[CustomEditor(typeof(ShadowCaster2DCreator))]
    public class ShadowCaster2DTileMapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                var creator = (ShadowCaster2DCreator)target;
                creator.Create();
            }

            if (GUILayout.Button("Remove Shadows"))
            {
                var creator = (ShadowCaster2DCreator)target;
                creator.DestroyOldShadowCasters();
            }
            EditorGUILayout.EndHorizontal();
        }

    }*/
}
