using System;
using System.Collections.Generic;

namespace Lua
{
    public static class LuaUtility
    {
        /// <summary>
        /// Pair Table
        /// </summary>
        /// <returns>KeyIdx and ValueIdx</returns>
        public static IEnumerable<ValueTuple<int, int>> Pair(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);   //table
                LuaAPI.lua_pushnil(state);          //table nil

                var keyidx = LuaAPI.lua_gettop(state);
                var valueidx = keyidx + 1;

                while (LuaAPI.lua_next(state, -2))   //table key value
                {
                    yield return (keyidx, valueidx);
                    LuaAPI.lua_settop(state, keyidx);
                }
            }
        }

        /// <summary>
        /// IPair Table
        /// </summary>
        /// <returns>Index and ValueIdx</returns>
        public static IEnumerable<ValueTuple<int, int>> IPair(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);               //table

                int pairtop = LuaAPI.lua_gettop(state);
                var valueidx = pairtop + 1;

                var length = LuaAPI.luaL_len(state, idx);
                for (var index = 0; index < length; index++)
                {
                    LuaAPI.lua_rawgeti(state, -1, index + 1);   //table value
                    yield return (index, valueidx);
                    LuaAPI.lua_settop(state, pairtop);
                }
            }
        }
    }
}
