using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class XLuaManager : MonoBehaviour
{
    private static XLuaManager m_instance;

    public static XLuaManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    /// <summary>
    /// lua运行时虚拟机
    /// </summary>
    internal LuaEnv m_luaEnv;

    /// <summary>
    /// lua入口文件
    /// </summary>
    private LuaTable m_luaTableMain = null;

    public LuaEnv GetXLuaEnv()
    {
        return m_luaEnv;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LuaEnv luaenv = new LuaEnv();

        if (m_instance != null)
        {
            Debug.LogError("XluaManager初始化多份");
            return;
        }

        m_instance = this;
        m_luaEnv = new LuaEnv();
        m_luaEnv.GcPause = 100;
        // m_luaEnv.AddBuildin("memstream",XLua.LuaDLL.Lua.LoadMemStream);
        m_luaEnv.AddLoader(CustomLuaLoaderMethod);
        
        m_luaEnv.DoString(GetLuaFileBytes("GameMain.lua"), "GameMain", null);
    }

    private void OnDestroy()
    {
        m_luaEnv = null;
    }

    /// <summary>
    /// 自定义的require的加载器
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public byte[] CustomLuaLoaderMethod(ref string fileName)
    {
        return GetLuaFileBytes(fileName);
    }

    public byte[] GetLuaFileBytes(string loaderFileName)
    {
        try
        {
            if (string.IsNullOrEmpty(loaderFileName))
            {
                Debug.Log("require参数为空");
                return null;
            }
            
            //屏蔽emmylua
            if (loaderFileName == "emmy_core")
            {
                return null;
            }

            //无.lua后缀
            if (loaderFileName.IndexOf(".lua") <= -1)
            {
                loaderFileName += ".lua";
            }

            byte[] ret = File.ReadAllBytes(Application.streamingAssetsPath + "/Lua/" + loaderFileName);
            return ret;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return null;
    }
}
