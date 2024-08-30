using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// 개발자 : 신지혜 / 마우스 포인터 설정 클래스
/// </summary>
public class MouseCursor : MonoBehaviour
{
    [SerializeField] Texture2D texture;  // 참조할 Texture2D (Cursors & Pointers)
    [SerializeField] Rect spriteRect;  // 사용할 스프라이트의 영역

    Vector2 hotSpot = Vector2.zero;  // hotSpot : 마우스의 좌표 (Vector2.zero일 경우 좌측 상단)
    CursorMode cursorMode = CursorMode.Auto;  // 커서 모드

    private void Start()
    {
        ChangeCursor(texture, spriteRect);
    }

    private void ChangeCursor(Texture2D texture, Rect rect)
    {
        Texture2D cursorTexture = new Texture2D((int)rect.width, (int)rect.height);
        Color[] pixels = texture.GetPixels(
            (int)rect.x,
            (int)rect.y,
            (int)rect.width,
            (int)rect.height
        );
        cursorTexture.SetPixels(pixels);
        cursorTexture.Apply();

        // 커서 설정
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }
}
