using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]

// 파괴 가능한 타일맵 관리 스크립트
public class BreakableTilemap : MonoBehaviour
{
    private static readonly List<BreakableTilemap> _instances = new List<BreakableTilemap>();
    public static IReadOnlyList<BreakableTilemap> Instances => _instances.AsReadOnly();

    [SerializeField] private Tilemap tilemap;

    private void Reset()
    {
        // 컴포넌트 자동 할당
        if (tilemap == null) tilemap = GetComponent<Tilemap>();
    }

    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (tilemap == null) tilemap = GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        // 인스턴스 목록에 추가
        if (!_instances.Contains(this)) _instances.Add(this);
    }

    private void OnDisable()
    {
        // 인스턴스 목록에서 제거
        _instances.Remove(this);
    }

    // 지정된 월드 좌표와 반경 내의 타일을 제거
    public void BreakTiles(Vector3 worldPos, float radius)
    {
        if (tilemap == null) return;

        // 반경 내의 타일 좌표 계산
        Vector3Int center = tilemap.WorldToCell(worldPos);
        float cellSize = Mathf.Max(Mathf.Abs(tilemap.cellSize.x), Mathf.Abs(tilemap.cellSize.y));
        int cellRadius = Mathf.CeilToInt(radius / (cellSize > 0 ? cellSize : 1f));

        // 제거할 타일 좌표 수집
        var toClear = new List<Vector3Int>();
        for (int dx = -cellRadius; dx <= cellRadius; dx++)
        {
            for (int dy = -cellRadius; dy <= cellRadius; dy++)
            {
                var cell = new Vector3Int(center.x + dx, center.y + dy, center.z);
                var cellWorld = tilemap.GetCellCenterWorld(cell);
                if (Vector3.Distance(cellWorld, worldPos) <= radius && tilemap.HasTile(cell))
                    toClear.Add(cell);
            }
        }

        // 타일 제거
        foreach (var c in toClear)
            tilemap.SetTile(c, null);

        tilemap.RefreshAllTiles(); // 타일맵 갱신
    }
}