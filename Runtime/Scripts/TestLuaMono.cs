using System;
using System.IO;
using UnityEngine;

namespace Lua
{
    internal class TestLuaMono : MonoBehaviour
    {
        private IntPtr m_state;

        private static readonly LuaLReg[] registers = new LuaLReg[]
        {
            new LuaLReg("啊啊啊", Print),
            LuaLReg.Null,
        };

        private void Awake()
        {
            //RunConsole();
            m_state = LuaAPI.luaL_newstate();
            try
            {
                LuaAPI.luaL_newlib(m_state, registers);
                LuaAPI.lua_setglobal(m_state, "test");

                LuaAPI.luaL_openlibs(m_state);

                LuaAPI.lua_getglobal(m_state, "package");
                LuaAPI.lua_getfield(m_state, -1, "searchers");
                LuaAPI.lua_remove(m_state, -2);

                LuaAPI.lua_pushcfunction(m_state, Print);
                LuaAPI.lua_setglobal(m_state, "print");

                LuaAPI.lua_pushstring(m_state, "啊啊啊");
                LuaAPI.lua_setglobal(m_state, "test_string");

                LuaAPI.lua_pushcfunction(m_state, TestFunction);
                LuaAPI.lua_setglobal(m_state, "test_func");

                GC.Collect();
                GC.Collect();

                var len = LuaAPI.luaL_len(m_state, -1);
                LuaAPI.lua_pushcfunction(m_state, Loader);
                LuaAPI.lua_rawseti(m_state, -2, len + 1);
                LuaAPI.lua_pop(m_state, 1);

                if (LuaAPI.luaL_dofile(m_state, UnityEngine.Application.streamingAssetsPath + "/" + "test.lua") != LuaStatus.LUA_OK)
                {
                    Debug.LogError(LuaAPI.lua_tostring(m_state, -1));
                }
            }
            finally
            {
                LuaAPI.lua_close(m_state);
            }
        }

        private int TestFunction(IntPtr state)
        {
            return Print(state);
        }

        private static int Print(IntPtr state)
        {
            var str = LuaAPI.luaL_tolstring(state, -1);
            Debug.Log(str);
            return 0;
        }

        private static int Loader(IntPtr state)
        {
            try
            {
                string filename = LuaAPI.lua_tostring(state, 1).Replace('.', '/') + ".lua";
                var filepath = UnityEngine.Application.streamingAssetsPath + "/" + filename;

                if (File.Exists(filepath))
                {
                    var bytes = File.ReadAllBytes(filepath);
                    if (LuaAPI.luaL_loadbuffer(state, bytes, "@" + filename) != 0)
                    {
                        return LuaAPI.luaL_error(state, string.Format("error loading module {0} from streamingAssetsPath, {1}",
                            LuaAPI.lua_tostring(state, 1), LuaAPI.lua_tostring(state, -1)));
                    }
                }
                else
                {
                    LuaAPI.lua_pushstring(state, string.Format("\n\tno such file '{0}' in streamingAssetsPath!", filename));
                }
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(state, "c# exception in LoadFromStreamingAssetsPath:" + e);
            }
        }
    }
}
