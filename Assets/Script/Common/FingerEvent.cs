using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 滑动事件类型
/// </summary>
public enum FingerType
{
    Left,
    Right,
    Up,
    Down
}

/// <summary>
/// 缩放事件类型
/// </summary>
public enum ZoomType
{
    In,
    Out
}

public class FingerEvent :  MonoBehaviour
{
    public static FingerEvent Instance;

    /// <summary>
    /// 起始滑动手指坐标
    /// </summary>
    Vector2 m_OldFingerPos;

    /// <summary>
    /// 滑动的朝向
    /// </summary>
    Vector2 m_Dir;

    /// <summary>
    /// 滑动事件
    /// </summary>
    public Action<FingerType> OnFingerDragEvent;

    /// <summary>
    /// 用户点击地面事件
    /// </summary>
    public Action OnPlayerClick;

    /// <summary>
    /// 缩放事件
    /// </summary>
    public Action<ZoomType> OnZoom;

    /// <summary>
    /// 用来用户是滑动还是点击(点击的话m_FingerState就是1,滑动的话滑动中是3,结束滑动是4,主要来避免滑动的时候玩家也会走路的问题)
    /// </summary>
    private int m_FingerState = 0;

    /// <summary>
    /// 移动平台的摄像机的缩放相关数据
    /// </summary>
    Vector2 m_OldFingerPos1;
    Vector2 m_OldFingerPos2;

    Vector2 m_CurFingerPos1;
    Vector2 m_CurFingerPos2;


    private void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
    	//启动时调用，这里开始注册手势操作的事件。
    	
    	//按下事件： OnFingerDown就是按下事件监听的方法，这个名子可以由你来自定义。方法只能在本类中监听。下面所有的事件都一样！！！
        FingerGestures.OnFingerDown += OnFingerDown;
        //抬起事件
		FingerGestures.OnFingerUp += OnFingerUp;
	    //开始拖动事件
	    FingerGestures.OnFingerDragBegin += OnFingerDragBegin;
        //拖动中事件...
        FingerGestures.OnFingerDragMove += OnFingerDragMove;
        //拖动结束事件
        FingerGestures.OnFingerDragEnd += OnFingerDragEnd; 
		//上、下、左、右、四个方向的手势滑动
		FingerGestures.OnFingerSwipe += OnFingerSwipe;
		//连击事件 连续点击事件
		FingerGestures.OnFingerTap += OnFingerTap;
		//按下事件后调用一下三个方法
		FingerGestures.OnFingerStationaryBegin += OnFingerStationaryBegin;
		FingerGestures.OnFingerStationary += OnFingerStationary;
		FingerGestures.OnFingerStationaryEnd += OnFingerStationaryEnd;
		//长按事件
		FingerGestures.OnFingerLongPress += OnFingerLongPress;
		
    }

    private void Update()
    {
#if UNITY_EDITOR_WIN || UNITY_EDITOR
        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (OnZoom != null)
                OnZoom(ZoomType.In);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (OnZoom != null)
                OnZoom(ZoomType.Out);
        }
#elif UNITY_ANDROID || UNITY_IOS
         if(Input.touchCount > 1)
        {
            if(Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                m_CurFingerPos1 = Input.GetTouch(0).position;
                m_CurFingerPos2 = Input.GetTouch(2).position;

                if(Vector2.Distance(m_OldFingerPos1, m_OldFingerPos2) < Vector2.Distance(m_CurFingerPos1, m_CurFingerPos2))
                {
                    //放大
                    if (OnZoom != null)
                        OnZoom(ZoomType.In);
                }
                else
                {
                    //缩小
                    if (OnZoom != null)
                        OnZoom(ZoomType.Out);
                }

                m_OldFingerPos1 = m_CurFingerPos1;
                m_OldFingerPos2 = m_CurFingerPos2;
            }
        }
#endif
    }

    void OnDisable()
    {
    	//关闭时调用，这里销毁手势操作的事件
    	//和上面一样
        FingerGestures.OnFingerDown -= OnFingerDown;
		FingerGestures.OnFingerUp -= OnFingerUp;
		FingerGestures.OnFingerDragBegin -= OnFingerDragBegin;
        FingerGestures.OnFingerDragMove -= OnFingerDragMove;
        FingerGestures.OnFingerDragEnd -= OnFingerDragEnd; 
		FingerGestures.OnFingerSwipe -= OnFingerSwipe;
		FingerGestures.OnFingerTap -= OnFingerTap;
		FingerGestures.OnFingerStationaryBegin -= OnFingerStationaryBegin;
		FingerGestures.OnFingerStationary -= OnFingerStationary;
		FingerGestures.OnFingerStationaryEnd -= OnFingerStationaryEnd;
		FingerGestures.OnFingerLongPress -= OnFingerLongPress;
    }

    //按下时调用
    void OnFingerDown( int fingerIndex, Vector2 fingerPos )
    {
        m_FingerState = 1;
    }
	
	//抬起时调用
	void OnFingerUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		if(m_FingerState == 1)
        {
            m_FingerState = -1;
            if (OnPlayerClick != null)
                OnPlayerClick();
        }
	}
	
	//开始滑动
	void OnFingerDragBegin( int fingerIndex, Vector2 fingerPos, Vector2 startPos )
    {
        m_FingerState = 2;
        m_OldFingerPos = fingerPos;
    }
	//滑动结束
	void OnFingerDragEnd( int fingerIndex, Vector2 fingerPos )
	{
        m_FingerState = 4;
    }
	//滑动中
    void OnFingerDragMove( int fingerIndex, Vector2 fingerPos, Vector2 delta )
    {
        m_FingerState = 3;
        m_Dir = fingerPos - m_OldFingerPos;

        if (OnFingerDragEvent == null)
            return;

        //向右滑动
        if (m_Dir.y < m_Dir.x && m_Dir.y > -m_Dir.x)
        {
            OnFingerDragEvent(FingerType.Right);
        }//向左滑动
        else if (m_Dir.y > m_Dir.x && m_Dir.y < -m_Dir.x)
        {
            OnFingerDragEvent(FingerType.Left);
        }//向上滑动
        else if (m_Dir.y > m_Dir.x && m_Dir.y > -m_Dir.x)
        {
            OnFingerDragEvent(FingerType.Up);
        }
        else//向下滑动
        {
            OnFingerDragEvent(FingerType.Down);
        }
    }
	//上下左右四方方向滑动手势操作
	void OnFingerSwipe( int fingerIndex, Vector2 startPos, FingerGestures.SwipeDirection direction, float velocity )
    {

    }
	
	//连续按下事件， tapCount就是当前连续按下几次
	void OnFingerTap( int fingerIndex, Vector2 fingerPos, int tapCount )
    {

    }
	
	//按下事件开始后调用，包括 开始 结束 持续中状态只到下次事件开始！
	void OnFingerStationaryBegin( int fingerIndex, Vector2 fingerPos )
	{
		
	}
	
	
	void OnFingerStationary( int fingerIndex, Vector2 fingerPos, float elapsedTime )
	{

	}
	
	void OnFingerStationaryEnd( int fingerIndex, Vector2 fingerPos, float elapsedTime )
	{
		
	}
	
	
	//长按事件
	void OnFingerLongPress( int fingerIndex, Vector2 fingerPos )
	{
		
	}
	
	//把Unity屏幕坐标换算成3D坐标
    Vector3 GetWorldPos( Vector2 screenPos )
    {
        Camera mainCamera = Camera.main;
        return mainCamera.ScreenToWorldPoint( new Vector3( screenPos.x, screenPos.y, Mathf.Abs( transform.position.z - mainCamera.transform.position.z ) ) ); 
    }
}
