#if LUA_SIMPLE_TRANSLATOR

using System;

namespace Lua
{
    public interface ILuaDecoder<TValue>
    {
        TValue GetValue(IntPtr state, int index);
        bool TryGetValue(IntPtr state, int index, out TValue value);
    }

    public interface ILuaEncoder<TValue>
    {
        void PushValue(IntPtr state, in TValue value);
    }

    public interface ILuaTranslator<TValue> : ILuaDecoder<TValue>, ILuaEncoder<TValue>
    {

    }

    public abstract class LuaDecoder<TValue> : ILuaDecoder<TValue>
    {
        internal protected abstract LuaTypes LuaType { get; }

        TValue ILuaDecoder<TValue>.GetValue(IntPtr state, int index)
        {
            if (LuaType != LuaTypes.LUA_TNONE && LuaAPI.lua_type(state, index) != LuaType)
            {
                throw new Exception($"can not decoder to {typeof(TValue)}, need {LuaType}");
            }
            return GetValue(state, index);
        }

        bool ILuaDecoder<TValue>.TryGetValue(IntPtr state, int index, out TValue value)
        {
            if (LuaType != LuaTypes.LUA_TNONE && LuaAPI.lua_type(state, index) != LuaType)
            {
                value = default;
                return false;
            }
            return TryGetValue(state, index, out value);
        }

        protected abstract TValue GetValue(IntPtr state, int index);

        protected abstract bool TryGetValue(IntPtr state, int index, out TValue value);
    }

    public abstract class LuaTranslator<TValue> : LuaDecoder<TValue>, ILuaTranslator<TValue>
    {
        public abstract void PushValue(IntPtr state, in TValue value);
    }

    public class ByteLuaTranslator : LuaTranslator<byte>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TNUMBER;

        public override void PushValue(IntPtr state, in byte value)
        {
            LuaAPI.lua_pushinteger(state, value);
        }

        protected override byte GetValue(IntPtr state, int index)
        {
#if LUA_NUMBER_CHECK
            var value = LuaAPI.lua_tointeger(state, index);
            if (value > byte.MaxValue || value < byte.MinValue)
            {
                throw new Exception("value overflow.");
            }
            return (byte)value;
#else
            return (byte)LuaAPI.lua_tointeger(state, index);
#endif
        }

        protected override bool TryGetValue(IntPtr state, int index, out byte value)
        {
#if LUA_NUMBER_CHECK
            unchecked
            {
                var integer = LuaAPI.lua_tointeger(state, index);
                value = (byte)integer;
                return integer <= byte.MaxValue && integer >= byte.MinValue;
            }
#else
            value = (byte)LuaAPI.lua_tointeger(state, index);
            return true;
#endif
        }
    }

    public class IntLuaTranslator : LuaTranslator<int>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TNUMBER;

        public override void PushValue(IntPtr state, in int value)
        {
            LuaAPI.lua_pushinteger(state, value);
        }

        protected override int GetValue(IntPtr state, int index)
        {
#if LUA_NUMBER_CHECK
            var value = LuaAPI.lua_tointeger(state, index);
            if (value > int.MaxValue || value < int.MinValue)
            {
                throw new Exception("value overflow.");
            }
            return (int)value;
#else
            return (int)LuaAPI.lua_tointeger(state, index)
#endif
        }

        protected override bool TryGetValue(IntPtr state, int index, out int value)
        {
#if LUA_NUMBER_CHECK
            unchecked
            {
                var integer = LuaAPI.lua_tointeger(state, index);
                value = (int)integer;
                return integer <= int.MaxValue && integer >= int.MinValue;
            }
#else
            value = (int)LuaAPI.lua_tointeger(state, index);
            return true;
#endif
        }
    }

    public class LongLuaTranslator : LuaTranslator<long>
    {
        protected internal override LuaTypes LuaType => throw new NotImplementedException();

        public override void PushValue(IntPtr state, in long value)
        {
            LuaAPI.lua_pushinteger(state, value);
        }

        protected override long GetValue(IntPtr state, int index)
        {
            return LuaAPI.lua_tointeger(state, index);
        }

        protected override bool TryGetValue(IntPtr state, int index, out long value)
        {
            value = LuaAPI.lua_tointeger(state, index);
            return true;
        }
    }

    public class FloatLuaTranslator : LuaTranslator<float>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TNUMBER;

        public override void PushValue(IntPtr state, in float value)
        {
            LuaAPI.lua_pushnumber(state, value);
        }

        protected override float GetValue(IntPtr state, int index)
        {
#if LUA_NUMBER_CHECK
            var value = LuaAPI.lua_tonumber(state, index);
            if (value < float.MinValue || value > float.MaxValue)
            {
                throw new Exception("value overflow.");
            }
            return (float)value;
#else
            return (float)LuaAPI.lua_tonumber(state, index);
#endif
        }

        protected override bool TryGetValue(IntPtr state, int index, out float value)
        {
#if LUA_NUMBER_CHECK
            unchecked
            {
                var number = LuaAPI.lua_tonumber(state, index);
                value = (float)number;
                return value >= float.MinValue && value <= float.MaxValue;
            }
#else
            value = (float)LuaAPI.lua_tonumber(state, index);
            return true;
#endif
        }
    }

    public class DoubleLuaTranslator : LuaTranslator<double>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TNUMBER;

        public override void PushValue(IntPtr state, in double value)
        {
            LuaAPI.lua_pushnumber(state, value);
        }

        protected override double GetValue(IntPtr state, int index)
        {
            return LuaAPI.lua_tonumber(state, index);
        }

        protected override bool TryGetValue(IntPtr state, int index, out double value)
        {
            value = LuaAPI.lua_tonumber(state, index);
            return true;
        }
    }

    public class BooleanLuaTranslator : LuaTranslator<bool>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TBOOLEAN;

        public override void PushValue(IntPtr state, in bool value)
        {
            LuaAPI.lua_pushboolean(state, value);
        }

        protected override bool GetValue(IntPtr state, int index)
        {
            return LuaAPI.lua_toboolean(state, index);
        }

        protected override bool TryGetValue(IntPtr state, int index, out bool value)
        {
            value = LuaAPI.lua_toboolean(state, index);
            return true;
        }
    }

    public class StringLuaTranslator : LuaTranslator<string>
    {
        protected internal override LuaTypes LuaType => LuaTypes.LUA_TSTRING;

        public override void PushValue(IntPtr state, in string value)
        {
            LuaAPI.lua_pushstring(state, value);
        }

        protected override string GetValue(IntPtr state, int index)
        {
            return LuaAPI.lua_tostring(state, index);
        }

        protected override bool TryGetValue(IntPtr state, int index, out string value)
        {
            value = LuaAPI.lua_tostring(state, index);
            return true;
        }
    }
}

#endif