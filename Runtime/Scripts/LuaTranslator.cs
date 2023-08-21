#if LUA_SIMPLE_TRANSLATOR

using System;

namespace Lua
{
    public interface ILuaTranslator<T>
    {
        LuaTypes Type { get; }
        void PushValue(in IntPtr state, in T value);
        protected internal bool TryGetValue(in IntPtr state, in int idx, out T value);
    }

    internal sealed class IntTranslator : ILuaTranslator<int>
    {
        public LuaTypes Type => LuaTypes.LUA_TNUMBER;

        public void PushValue(in IntPtr state, in int value)
        {
            LuaAPI.lua_pushvalue(state, value);
        }

        bool ILuaTranslator<int>.TryGetValue(in IntPtr state, in int idx, out int value)
        {
            unchecked
            {
                var number = LuaAPI.lua_tointeger(state, idx);
#if LUA_NUMBER_CHECK
                if (number < int.MinValue || number > int.MaxValue)
                {
                    value = 0;
                    return false;
                }
#endif
                value = (int)number;
                return true;
            }
        }
    }

    internal sealed class LongTranslator : ILuaTranslator<long>
    {
        public LuaTypes Type => LuaTypes.LUA_TNUMBER;

        public void PushValue(in IntPtr state, in long value)
        {
            LuaAPI.lua_pushinteger(state, value);
        }

        bool ILuaTranslator<long>.TryGetValue(in IntPtr state, in int idx, out long value)
        {
            value = LuaAPI.lua_tointeger(state, idx);
            return true;
        }
    }

    internal sealed class FloatTranslator : ILuaTranslator<float>
    {
        public LuaTypes Type => LuaTypes.LUA_TNUMBER;

        public void PushValue(in IntPtr state, in float value)
        {
            throw new NotImplementedException();
        }

        bool ILuaTranslator<float>.TryGetValue(in IntPtr state, in int idx, out float value)
        {
            unchecked
            {
                var number = LuaAPI.lua_tonumber(state, idx);
#if LUA_NUMBER_CHECK
                if (number < float.MinValue || number > float.MaxValue)
                {
                    value = 0;
                    return false;
                }
#endif
                value = (float)number;
                return true;
            }
        }
    }

    internal sealed class DoubleTranslator : ILuaTranslator<double>
    {
        public LuaTypes Type => LuaTypes.LUA_TNUMBER;

        public void PushValue(in IntPtr state, in double value)
        {
            LuaAPI.lua_pushnumber(state, value);
        }

        bool ILuaTranslator<double>.TryGetValue(in IntPtr state, in int idx, out double value)
        {
            value = LuaAPI.lua_tonumber(state, idx);
            return true;
        }
    }

    internal sealed class BooleanTranslator : ILuaTranslator<bool>
    {
        public LuaTypes Type => LuaTypes.LUA_TBOOLEAN;

        public void PushValue(in IntPtr state, in bool value)
        {
            LuaAPI.lua_pushboolean(state, value);
        }

        bool ILuaTranslator<bool>.TryGetValue(in IntPtr state, in int idx, out bool value)
        {
            value = LuaAPI.lua_toboolean(state, idx);
            return true;
        }
    }

    internal sealed class StringTranslator : ILuaTranslator<string>
    {
        public LuaTypes Type => LuaTypes.LUA_TSTRING;

        public void PushValue(in IntPtr state, in string value)
        {
            LuaAPI.lua_pushstring(state, value);
        }

        bool ILuaTranslator<string>.TryGetValue(in IntPtr state, in int idx, out string value)
        {
            value = LuaAPI.lua_tostring(state, idx);
            return value != null;
        }
    }

    public static class LuaTranslatorExtension
    {
        public static bool TryGetValueWithType<T>(this ILuaTranslator<T> translator, in IntPtr state, in int idx, out T value)
        {
            if (translator.Type != LuaTypes.LUA_TNONE && LuaAPI.lua_type(state, idx) != translator.Type)
            {
                value = default;
                return false;
            }
            return translator.TryGetValue(state, idx, out value);
        }

        public static T GetValueWithType<T>(this ILuaTranslator<T> translator, in IntPtr state, in int idx)
        {
            if (translator.Type != LuaTypes.LUA_TNONE && LuaAPI.lua_type(state, idx) != translator.Type)
            {
                throw new Exception($"Get Value Type {typeof(T).FullName} Faile");
            }
            if (!translator.TryGetValue(state, idx, out var value))
            {
                throw new Exception($"Get Value Type {typeof(T).FullName} Faile");
            }
            return value;
        }
    }
}

#endif