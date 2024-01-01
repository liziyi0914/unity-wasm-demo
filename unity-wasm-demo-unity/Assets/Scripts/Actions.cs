using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Wasmtime;
using XLua;
using Debug = UnityEngine.Debug;

public class Actions : MonoBehaviour
{
    
    const int N = 40;
    
    public TextAsset luaScript;
    XLua.LuaEnv luaenv = new XLua.LuaEnv();
    
    void Start()
    {
        CallUnityFib();
        CallLuaFib();
        CallLuacFib();
        CallWasmFib();
    }
    
    public static long Fib(long n)
    {
        if (n <= 1)
        {
            return n;
        }

        return Fib(n - 1) + Fib(n - 2);
    }
    
    [CSharpCallLua]
    public delegate long FibDelegate(long n);

    public void CallLuaFib()
    {
        Debug.Log("-------- Lua --------");
        
        var scriptEnv = luaenv.NewTable();
        
        luaenv.DoString(luaScript.text,"FibScript", scriptEnv);
        FibDelegate f = scriptEnv.Get<FibDelegate>("fib");
        
        var watch = new Stopwatch();
        
        long total = 0;
        long result = 0;
        for (int i = 1; i <= 3; i++)
        {
            watch.Reset();
            watch.Start();
        
            result = f.Invoke(N);
        
            watch.Stop();
            Debug.Log("第"+i+"轮用时: " + watch.ElapsedMilliseconds + "ms");
            total += watch.ElapsedMilliseconds;
        }
        Debug.Log("平均用时: " + total/3 + "ms"); 
        Debug.Log("fib("+N+"): " + result);
    }

    public void CallLuacFib()
    {
        Debug.Log("-------- Luac --------");
        
        var scriptEnv = luaenv.NewTable();
        luaenv.AddLoader((ref string path) => {
            if (path == "FibScript")
            {
                return System.IO.File.ReadAllBytes("D:\\Projects\\unity-wasm-demo\\unity-wasm-demo-unity\\Assets\\Scripts\\fib.bytes");
            }
            return null;
        });
        // luaenv.AddLoader((ref string filename) =>
        // {
        //     if (filename == "InMemory")
        //     {
        //         Debug.Log("加载Lua脚本");
        //         string script = "return {ccc = 9999}";
        //         return System.Text.Encoding.UTF8.GetBytes(script);
        //     }
        //     return null;
        // });
        luaenv.DoString("require('FibScript')");
        
        FibDelegate f = luaenv.Global.Get<FibDelegate>("fib");
        
        // luaenv.DoString("print('233')","chunk", scriptEnv);
        // luaenv.DoString("require('FibScript')","chunk", scriptEnv);
        // luaenv.DoString("require('FibScript')","FibScript", scriptEnv);
        // FibDelegate f = scriptEnv.Get<FibDelegate>("fib");
        //
        var watch = new Stopwatch();
        
        long total = 0;
        long result = 0;
        for (int i = 1; i <= 3; i++)
        {
            watch.Reset();
            watch.Start();
        
            result = f.Invoke(N);
        
            watch.Stop();
            Debug.Log("第"+i+"轮用时: " + watch.ElapsedMilliseconds + "ms");
            total += watch.ElapsedMilliseconds;
        }
        Debug.Log("平均用时: " + total/3 + "ms"); 
        Debug.Log("fib("+N+"): " + result);
    }

    public void CallUnityFib()
    {
        Debug.Log("-------- Unity --------");
        var watch = new Stopwatch();

        long total = 0;
        long result = 0;
        for (int i = 1; i <= 3; i++)
        {
            watch.Reset();
            watch.Start();

            result = Fib(N);
        
            watch.Stop();
            Debug.Log("第"+i+"轮用时: " + watch.ElapsedMilliseconds + "ms");
            total += watch.ElapsedMilliseconds;
        }
        Debug.Log("平均用时: " + total/3 + "ms"); 
        Debug.Log("fib("+N+"): " + result);
    }

    public void CallWasmFib()
    {
        Debug.Log("-------- Wasm --------");
        
        var engine = new Engine();
        
        WasiConfiguration wasiConfig = new WasiConfiguration();
        wasiConfig.WithInheritedStandardOutput().WithInheritedStandardError();

        var module = Module.FromBytes(engine, "wasm", System.IO.File.ReadAllBytes("D:\\Projects\\unity-wasm-demo\\rust\\target\\wasm32-wasi\\debug\\unity_wasm_demo_rust.wasm"));

        var linker = new Linker(engine);
        var store = new Store(engine);
        
        linker.DefineWasi();
        store.SetWasiConfiguration(wasiConfig);

        var instance = linker.Instantiate(store, module);
        var run = instance.GetFunction("fib", typeof(long), new Type[]{typeof(long)});


        var watch = new Stopwatch();

        long total = 0;
        long result = 0;
        for (int i = 1; i <= 3; i++)
        {
            watch.Reset();
            watch.Start();

            result = (long)run.Invoke(N);
        
            watch.Stop();
            Debug.Log("第"+i+"轮用时: " + watch.ElapsedMilliseconds + "ms");
            total += watch.ElapsedMilliseconds;
        }
        Debug.Log("平均用时: " + total/3 + "ms"); 
        Debug.Log("fib("+N+"): " + result);
        
        engine.Dispose();
    }
}
