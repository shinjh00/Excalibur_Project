using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자 : 신지혜
/// EventSystem 없으면 추가해주는 함수
/// UI 관련 게임오브젝트, 컴포넌트 바인딩, 텍스트 데이터 세팅
/// </summary>
public class BaseUI : MonoBehaviourPun
{
    //EventSystem _eventSystem;

    protected Dictionary<string, GameObject> gameObjectDic;
    protected Dictionary<string, Component> componentDic;

    Transform[] transforms;
    Component[] components;

    protected virtual void Awake()
    {
        EnsureEventSystem();
        Bind();
    }

    public void EnsureEventSystem()
    {
        // 동기화 할 때
        if (EventSystem.current != null)
        {
            return;
        }
        EventSystem eventSystem = Resources.Load<EventSystem>("8.UI/EventSystem");
        Instantiate(eventSystem);
    }

    /// <summary>
    /// 해당 UI의 GameObject, Component 전부 Dictionary에 담는 메소드
    /// </summary>
    private void Bind()
    {
        // GameObject 찾아서 담기
        transforms = GetComponentsInChildren<Transform>(true);
        gameObjectDic = new Dictionary<string, GameObject>(transforms.Length * 2);
        foreach (Transform child in transforms)
        {
            string name = $"{child.gameObject.name}";
            if (gameObjectDic.ContainsKey(name))
            {
                return;
            }
            gameObjectDic.TryAdd(name, child.gameObject);
        }

        // Component 찾아서 담기
        components = GetComponentsInChildren<Component>(true);
        componentDic = new Dictionary<string, Component>(components.Length * 2);
        foreach (Component child in components)
        {
            // name = 오브젝트이름>>컴포넌트이름
            string name = $"{child.gameObject.name}>>{components.GetType().Name}";
            if (componentDic.ContainsKey(name))
            {
                return;
            }
            componentDic.TryAdd(name, child);
        }
    }


    #region GetUI
    /// <summary>
    /// UI 게임오브젝트 가져오는 메소드
    /// </summary>
    /// <param name="name"> gameObjectDic의 키 값 </param>
    /// <returns> gameObject </returns>
    public GameObject GetUI(string name)
    {
        gameObjectDic.TryGetValue(name, out GameObject gameObject);
        return gameObject;
    }

    /// <summary>
    /// UI 컴포넌트 가져오는 메소드
    /// </summary>
    /// <typeparam name="T"> 일반화 타입 </typeparam>
    /// <param name="name"> componentDic의 키 값 </param>
    /// <returns> component </returns>
    public T GetUI<T>(string name) where T : Component
    {
        // 사용예시) GetUI<Button>("NextButton").interactable = false;

        componentDic.TryGetValue($"{name}>>{typeof(T).Name}", out Component component);
        if (component != null)
            return component as T;

        gameObjectDic.TryGetValue(name, out GameObject gameObject);
        if (gameObject == null)
            return null;

        component = gameObject.GetComponent<T>();
        if (component == null)
            return null;

        componentDic.TryAdd($"{name}_{typeof(T).Name}", component);
        return component as T;
    }
    #endregion
}
