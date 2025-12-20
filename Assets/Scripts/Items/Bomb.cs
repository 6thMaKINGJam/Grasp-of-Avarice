using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : Item
{
    // 중복 폭발 방지 플래그
    private bool _hasExploded = false;

    public override void OnDroppedByPlayer()
    {
        ExplodeAfterDelay();
    }

    // 지정한 시간(초) 후에 Explode를 호출하는 코루틴
    private IEnumerator ExplodeDelayedCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    // 외부에서 1초 지연으로 폭발을 시작하기 위한 인터페이스
    public void ExplodeAfterDelay(float delay = 1f)
    {
        if (_hasExploded) return; // 이미 폭발 예약되었거나 폭발한 경우 무시
        StartCoroutine(ExplodeDelayedCoroutine(delay));
    }

    public void Explode()
    {
        if (_hasExploded) return; // 중복 호출 방지
        _hasExploded = true;

        float breakRadius = 1.1f;
        Vector3 bombPos = transform.position;

        // 등록된 모든 BreakableTilemap을 검사
        foreach (var breakable in BreakableTilemap.Instances)
        {
            if (breakable == null) continue;

            var tm = breakable.GetComponent<Tilemap>();
            if (tm == null) continue;

            // 폭탄의 위치를 해당 타일맵의 셀 좌표로 변환
            Vector3Int centerCell = tm.WorldToCell(bombPos);

            // 좌우 한 칸을 검사
            var checkCells = new[] { centerCell + Vector3Int.left, centerCell + Vector3Int.right };

            foreach (var cell in checkCells)
            {
                if (!tm.HasTile(cell)) continue;

                // 해당 셀의 월드 중심 좌표를 BreakTiles에 전달
                Vector3 cellWorld = tm.GetCellCenterWorld(cell);
                breakable.BreakTiles(cellWorld, breakRadius);
            }
        }

        // 폭탄 제거
        Destroy(gameObject);
    }
}