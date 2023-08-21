using System;

namespace Lua
{
    public struct LuaTopScope : IDisposable
    {
        private IntPtr m_state;
        private int m_top;

        public LuaTopScope(IntPtr state)
        {
            m_top = LuaAPI.lua_gettop(state);
            m_state = state;
        }

        public void Dispose()
        {
            if (m_state != IntPtr.Zero && m_top >= 0)
            {
                LuaAPI.lua_settop(m_state, m_top);
                m_state = IntPtr.Zero; m_top = -1;
            }
        }
    }
}