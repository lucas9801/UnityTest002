using UnityEngine;
using XLua;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class Injection
{
    public string name;             //注入后在Lua中的变量名
    public GameObject value;        //注入后在Lua中的值
}

public class LuaBehaviour : MonoBehaviour
{
    //lua文件路径
    public string m_luaScript;
    
    public delegate void OnClickDelegate(Button btn, UnityAction<LuaTable, GameObject> action);
    public delegate void OnValueChangedDelegate(Toggle btn, UnityAction<LuaTable, bool, Toggle> action);
    public delegate void OnIntValueChangedDelegate(Dropdown dropdown, UnityAction<LuaTable, int> action);
    public delegate void OnScrllRectValueChangedDelegate(ScrollRect scrollRect, UnityAction<LuaTable, Vector2> action);
    public delegate void OnSliderChangedDelegate(Slider slider, UnityAction<LuaTable, float, Slider> action);
    // public delegate void OnSliderDownDelegate(UISlider slider, UnityAction<LuaTable, float, UISlider> action);
    // public delegate void OnSliderUpDelegate(UISlider slider, UnityAction<LuaTable, float, UISlider> action);
    public delegate void OnInputFieldValueChangeDelegate(InputField inputField, UnityAction<LuaTable, string> action);
    public delegate void OnInputFieldEndEditDelegate(InputField inputField, UnityAction<LuaTable, string> action);
    // public delegate void OnTweenerMoveOnComplete(TweenerCore<Vector3, Vector3, VectorOptions> tweener, TweenCallback<LuaTable> call);
    
    private Dictionary<Button, UnityAction> m_buttonClickDelegate = new Dictionary<Button, UnityAction>();
    private Dictionary<Toggle, UnityAction<bool>> m_toggleDelegate = new Dictionary<Toggle, UnityAction<bool>>();
    private Dictionary<InputField, UnityAction<string>> m_inputFieldDelegate = new Dictionary<InputField, UnityAction<string>>();
    private Dictionary<InputField, UnityAction<string>> m_inputFieldOnEndEditDelegate = new Dictionary<InputField, UnityAction<string>>();

    private Action<LuaTable> m_luaAwake;
    private Action<LuaTable> m_luaStart;
    private Action<LuaTable> m_luaUpdate;
    private Action<LuaTable> m_luaLateUpdate;
    private Action<LuaTable> m_luaOnDestroy;
    private Action<LuaTable> m_luaOnEnable;
    private Action<LuaTable> m_luaOnDisable;
    private Action<LuaTable, bool> m_luaOnApplicationPause;
    private Action<LuaTable, bool> m_luaOnApplicationFocus;
    
    protected LuaTable m_scriptEnv;

    protected void Awake()
    {
        if (string.IsNullOrEmpty(m_luaScript))
            return;
        LuaEnv luaEnv = XLuaManager.Instance.GetXLuaEnv();

        byte[] bytes = XLuaManager.Instance.GetLuaFileBytes(m_luaScript);
        if (bytes == null)
            return;
        object[] ret = luaEnv.DoString(bytes, m_luaScript, null);

        if (ret == null || ret.Length <= 0)
        {
            Debug.LogError("lua文件执行错误");
            return;
        }

        m_scriptEnv = ret[0] as LuaTable;
        AddInjectionsBeforeLuaAwake();
        m_scriptEnv.Get("Awake", out m_luaAwake);
        m_scriptEnv.Get("Start", out m_luaStart);
        m_scriptEnv.Get("Update", out m_luaUpdate);
        m_scriptEnv.Get("LateUpdate", out m_luaLateUpdate);
        m_scriptEnv.Get("OnDestroy", out m_luaOnDestroy);
        m_scriptEnv.Get("OnEnable", out m_luaOnEnable);
        m_scriptEnv.Get("OnDisable", out m_luaOnDisable);
        if (m_luaAwake != null)
        {
            m_luaAwake(m_scriptEnv);
        }
        
    }

    void Start()
    {
        try
        {
            if (m_luaStart != null)
            {
                m_luaStart(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }
    }

    private void Update()
    {
        try
        {
            if (m_luaUpdate != null)
            {
                m_luaUpdate(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }
    }

    private void OnEnable()
    {
        try
        {
            if (m_luaOnEnable != null)
            {
                m_luaOnEnable(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }
    }

    private void OnDisable()
    {
        try
        {
            if (m_luaOnDisable != null)
            {
                m_luaOnDisable(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }
    }

    private void LateUpdate()
    {
        try
        {
            if (m_luaLateUpdate != null)
            {
                m_luaLateUpdate(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }
    }

    private void OnDestroy()
    {
        try
        {
            if (m_luaOnDestroy != null)
            {
                m_luaOnDestroy(m_scriptEnv);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("lua{0} Start error:{1}", m_luaScript,e.Message));
        }

        m_luaAwake = null;
        m_luaStart = null;
        m_luaOnEnable = null;
        m_luaOnDisable = null;
        m_luaLateUpdate = null;
        m_luaOnDestroy = null;
        if (m_scriptEnv != null)
        {
            m_scriptEnv.Dispose();
            m_scriptEnv = null;
        }
    }

    /// <summary>
    /// 在Lua执行Awake之前注入一些自定义的东西
    /// </summary>
    protected virtual void AddInjectionsBeforeLuaAwake()
    {
        m_scriptEnv.Set("gameObject", this.gameObject);

        m_scriptEnv.Set<string, OnClickDelegate>("AddButtonOnClick", AddButtonOnClick);
        // m_scriptEnv.Set<string, OnValueChangedDelegate>("AddToggleOnValueChanged", AddToggleOnValueChanged);
        // m_scriptEnv.Set<string, OnIntValueChangedDelegate>("AddDropDownOnValueChanged", AddDropDownOnValueChanged);
        // m_scriptEnv.Set<string, OnScrllRectValueChangedDelegate>("AddScrollRectOnValueChanged", AddScrollRectOnValueChanged);
        // m_scriptEnv.Set<string, OnSliderChangedDelegate>("AddSliderOnValueChanged", AddSliderOnValueChanged);
        // m_scriptEnv.Set<string, OnSliderUpDelegate>("AddSliderOnPointerDown", AddSliderOnPointerDown);
        // m_scriptEnv.Set<string, OnSliderUpDelegate>("AddSliderOnPointerUp", AddSliderOnPointerUp);
        // m_scriptEnv.Set<string, OnInputFieldValueChangeDelegate>("AddInputFieldOnValueChange", AddInputFieldOnValueChange);
        // m_scriptEnv.Set<string, OnInputFieldEndEditDelegate>("AddInputFieldOnEndEdit", AddInputFieldOnEndEdit);
        //m_scriptEnv.Set<string, OnTweenerMoveOnComplete>("AddTweenerMoveOnComplete", AddTweenerMoveOnCompleteCallBack);

        //foreach (var injection in m_injections)
        //{
        //    m_scriptEnv.Set(injection.name, injection.value);
        //}
    }
    
    #region Lua注入接口
    /// <summary>
    /// 给一个Button添加Click事件
    /// </summary>
    /// <param name="button">Button组件</param>
    /// <param name="call">回调</param>
    public void AddButtonOnClick(Button button, UnityAction<LuaTable, GameObject> call)
    {
        if (button == null || call == null)
            return;
        UnityAction onClick = () =>
        {
            if (call != null)
            {
                call(m_scriptEnv, button.gameObject);
            }
        };
    
        //button.onClick.RemoveAllListeners();
    
        if (!m_buttonClickDelegate.ContainsKey(button))
        {
            m_buttonClickDelegate.Add(button, onClick);
        }
        else
        {
            button.onClick.RemoveListener(m_buttonClickDelegate[button]);
            m_buttonClickDelegate[button] = onClick;
        }
    
        button.onClick.AddListener(onClick);
        //if (!m_buttonClickDelegate.Contains(button))
        //    m_buttonClickDelegate.Add(button);
    }
    //
    // /// <summary>
    // /// 给一个toggle添加状态变化事件(之前只能注册一个事件，局限性太大，容易有bug，改成了lua里面只能注册一个事件)
    // /// </summary>
    // public void AddToggleOnValueChanged(Toggle toggle, UnityAction<LuaTable, bool, Toggle> call)
    // {
    //     if (toggle == null || call == null)
    //         return;
    //     UnityAction<bool> onValueChanged = (bool val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val, toggle);
    //         }
    //     };
    //     //toggle.onValueChanged.RemoveAllListeners();
    //
    //     if (!m_toggleDelegate.ContainsKey(toggle))
    //     {
    //         m_toggleDelegate.Add(toggle, onValueChanged);
    //     }
    //     else
    //     {
    //         toggle.onValueChanged.RemoveListener(m_toggleDelegate[toggle]);
    //         m_toggleDelegate[toggle] = onValueChanged;
    //     }
    //
    //     toggle.onValueChanged.AddListener(onValueChanged);
    // }
    //
    // public void AddDropDownOnValueChanged(Dropdown dropDown, UnityAction<LuaTable, int> call)
    // {
    //     if (dropDown == null || call == null)
    //         return;
    //     UnityAction<int> onValueChanged = (int val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val);
    //         }
    //     };
    //     dropDown.onValueChanged.RemoveAllListeners();
    //     dropDown.onValueChanged.AddListener(onValueChanged);
    //     if (!m_dropDownDelegate.Contains(dropDown))
    //         m_dropDownDelegate.Add(dropDown);
    // }
    //
    // public void AddScrollRectOnValueChanged(ScrollRect scroll, UnityAction<LuaTable, Vector2> call)
    // {
    //     if (scroll == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<Vector2> onValueChanged = (Vector2 val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val);
    //         }
    //     };
    //
    //     scroll.onValueChanged.RemoveAllListeners();
    //     scroll.onValueChanged.AddListener(onValueChanged);
    //     if (!m_scrollRectDelegate.Contains(scroll))
    //     {
    //         m_scrollRectDelegate.Add(scroll);
    //     }
    // }
    //
    // public void AddSliderOnValueChanged(Slider slider, UnityAction<LuaTable, float, Slider> call)
    // {
    //     if (slider == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<float> onValueChanged = (float val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val, slider);
    //         }
    //     };
    //
    //     slider.onValueChanged.RemoveAllListeners();
    //     slider.onValueChanged.AddListener(onValueChanged);
    //     if (!m_sliderDelegate.Contains(slider))
    //     {
    //         m_sliderDelegate.Add(slider);
    //     }
    // }
    //
    // public void AddSliderOnPointerDown(UISlider slider, UnityAction<LuaTable, float, UISlider> call)
    // {
    //     if (slider == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<float, UISlider> onValueChanged = (float val, UISlider uiSlider) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val, slider);
    //         }
    //     };
    //
    //     slider.OnSliderPointerDown = onValueChanged;
    //     if (!m_sliderDelegate.Contains(slider))
    //     {
    //         m_sliderDelegate.Add(slider);
    //     }
    // }
    //
    // public void AddSliderOnPointerUp(UISlider slider, UnityAction<LuaTable, float, UISlider> call)
    // {
    //     if (slider == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<float, UISlider> onValueChanged = (float val, UISlider uiSlider) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val, slider);
    //         }
    //     };
    //
    //     slider.OnSliderPointerUp = onValueChanged;
    //     if (!m_sliderDelegate.Contains(slider))
    //     {
    //         m_sliderDelegate.Add(slider);
    //     }
    // }
    //
    // public void AddInputFieldOnValueChange(InputField inputField, UnityAction<LuaTable, string> call)
    // {
    //     if (inputField == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<string> onValueChanged = (string val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val);
    //         }
    //     };
    //
    //     //inputField.onValueChanged.RemoveAllListeners();
    //     //inputField.onValueChanged.AddListener(onValueChanged);
    //     //if (!m_inputFieldDelegate.ContainsKey(inputField))
    //     //{
    //     //    m_inputFieldDelegate.Add(inputField, onValueChanged);
    //     //}
    //
    //     if (!m_inputFieldDelegate.ContainsKey(inputField))
    //     {
    //         m_inputFieldDelegate.Add(inputField, onValueChanged);
    //     }
    //     else
    //     {
    //         inputField.onValueChanged.RemoveListener(m_inputFieldDelegate[inputField]);
    //         m_inputFieldDelegate[inputField] = onValueChanged;
    //     }
    //
    //     inputField.onValueChanged.AddListener(onValueChanged);
    // }
    //
    // public void AddInputFieldOnEndEdit(InputField inputField, UnityAction<LuaTable, string> call)
    // {
    //     if (inputField == null || call == null)
    //     {
    //         return;
    //     }
    //
    //     UnityAction<string> onEndEdit = (string val) =>
    //     {
    //         if (call != null)
    //         {
    //             call(m_scriptEnv, val);
    //         }
    //     };
    //
    //     if (!m_inputFieldOnEndEditDelegate.ContainsKey(inputField))
    //     {
    //         m_inputFieldOnEndEditDelegate.Add(inputField, onEndEdit);
    //     }
    //     else
    //     {
    //         inputField.onEndEdit.RemoveListener(m_inputFieldOnEndEditDelegate[inputField]);
    //         m_inputFieldOnEndEditDelegate[inputField] = onEndEdit;
    //     }
    //
    //     inputField.onEndEdit.AddListener(onEndEdit);
    // }
    //
    // public void AddTweenerMoveOnCompleteCallBack(TweenerCore<Vector3, Vector3, VectorOptions> tweener, TweenCallback<LuaTable> call)
    // {
    //     TweenCallback oncom = () =>
    //     {
    //         if (call != null)
    //             call(m_scriptEnv);
    //     };
    //     tweener.onComplete = oncom;
    // }
    //
    #endregion
    
    
}