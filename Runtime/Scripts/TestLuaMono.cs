using System;
using UnityEngine;

namespace Lua
{
    internal class TestLuaMono : MonoBehaviour
    {
        private IntPtr m_state;

        private void Awake()
        {
            m_state = LuaAPI.luaL_newstate();
            try
            {

            }
            finally
            {
                LuaAPI.lua_close(m_state);
            }
        }
    }
}
