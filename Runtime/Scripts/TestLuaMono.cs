using System;
using System.IO;
using UnityEngine;

namespace Lua
{
    internal class TestLuaMono : MonoBehaviour
    {
        private IntPtr m_state;

        private static LuaLReg[] registers = new LuaLReg[]
        {
            new LuaLReg("print", Print),
            new LuaLReg(null, null),
        };

        private static string dostring = "test.print(\"aaaaaaaaaaaaaaa\")";

        private void Awake()
        {
            m_state = LuaAPI.luaL_newstate();
            try
            {
                LuaAPI.luaL_requiref(m_state, "package", LuaAPI.luaopen_package, 1);
                LuaAPI.lua_pop(m_state, 1);

                LuaAPI.lua_getglobal(m_state, "package");
                LuaAPI.lua_getfield(m_state, -1, "searchers");
                LuaAPI.lua_remove(m_state, -2);

                LuaAPI.lua_pushcfunction(m_state, Print);
                LuaAPI.lua_setglobal(m_state, "print");

                var len = LuaAPI.luaL_len(m_state, -1);
                LuaAPI.lua_pushcfunction(m_state, Loader);
                LuaAPI.lua_rawseti(m_state, -2, len + 1);
                LuaAPI.lua_pop(m_state, 1);

                if (LuaAPI.luaL_dofile(m_state, UnityEngine.Application.streamingAssetsPath + "/" + "test.lua") != LuaStatus.LUA_OK)
                {
                    Debug.Log(LuaAPI.lua_tostring(m_state, -1));
                }
            }
            finally
            {
                LuaAPI.lua_close(m_state);
            }
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
