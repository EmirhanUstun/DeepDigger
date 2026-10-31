using System.Collections;
using UnityEngine;

public class MinerController : MonoBehaviour
{
    [Header("Hareket")]
    [Min(0.1f)]
    public float moveSpeed = 3f;

    [Header("Madenci Gorseli")]
    public SpriteRenderer bodyRenderer;

    public int CurrentRow { get; private set; } = -1;
    public int CurrentColumn { get; private set; } = 2;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        if (bodyRenderer == null)
            bodyRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int row, int column, Vector3 localPosition)
    {
        StopAllCoroutines();
        IsMoving = false;
        CurrentRow = row;
        CurrentColumn = column;
        transform.localPosition = localPosition;
    }

    // Bir blogu vurmak icin madenci onun hemen yaninda olmali.
    public bool CanMine(int row, int column)
    {
        if (IsMoving)
            return false;

        int rowDistance = 
Mathf.Abs(row - CurrentRow);
        int columnDistance =
        Mathf.Abs(column - CurrentColumn);

        
        return rowDistance + columnDistance == 1;
    }

    public void MoveToCell(int row, int column, Vector3 targetLocalPosition)
    {
        if (IsMoving)
            return;

        if (bodyRenderer != null)
        {
            if (column > CurrentColumn)
                bodyRenderer.flipX = false;
            else if (column < CurrentColumn)
                bodyRenderer.flipX = true;
        }

        CurrentRow = row;
        CurrentColumn = column;
        StartCoroutine(MoveRoutine(targetLocalPosition));
    }

    private IEnumerator MoveRoutine(Vector3 targetLocalPosition)
    {
        IsMoving = true;

        while (Vector3.Distance(transform.localPosition, targetLocalPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                targetLocalPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.localPosition = targetLocalPosition;
        IsMoving = false;
    }
}
