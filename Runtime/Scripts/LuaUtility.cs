using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lua
{
    public static class LuaUtility
    {
        static LuaUtility()
        {
#if LUA_SIMPLE_TRANSLATOR
            Register(new IntTranslator());
            Register(new LongTranslator());
            Register(new FloatTranslator());
            Register(new DoubleTranslator());
            Register(new BooleanTranslator());
            Register(new StringTranslator());
#endif
        }

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

#if LUA_SIMPLE_TRANSLATOR
        private static class RegisterTranslator<T>
        {
            public static ILuaTranslator<T> translator = null;
        }

        public static ILuaTranslator<T> GetLuaTranslator<T>()
        {
            return RegisterTranslator<T>.translator ?? throw new Exception($"{typeof(T).FullName} Not Register To Lua Translator");
        }

        public static bool TryGetLuaTranslator<T>(out ILuaTranslator<T> translator)
        {
            translator = RegisterTranslator<T>.translator;
            return translator != null;
        }

        public static void Register<T>(ILuaTranslator<T> translator)
        {
            RegisterTranslator<T>.translator = translator;
        }

        public static bool IsRegisterTranslator<T>()
        {
            return RegisterTranslator<T>.translator != null;
        }

        public static IEnumerable<T> Select<T>(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            var translator = GetLuaTranslator<T>();

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);           //table
                LuaAPI.lua_pushnil(state);                  //table nil

                int pairtop = LuaAPI.lua_gettop(state);
                while (LuaAPI.lua_next(state, -2))          //table key value
                {
                    if (translator.TryGetValueWithType(state, -1, out var value))
                    {
                        yield return value;
                    }
                    LuaAPI.lua_settop(state, pairtop);      //table key
                }
            }
        }

        public static IEnumerable<ValueTuple<TKey, TValue>> Select<TKey, TValue>(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            var keytrans = GetLuaTranslator<TKey>();
            var valuetrans = GetLuaTranslator<TValue>();

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);               //table
                LuaAPI.lua_pushnil(state);                      //table nil

                int pairtop = LuaAPI.lua_gettop(state);
                while (LuaAPI.lua_next(state, -2))              //table key value
                {
                    if (keytrans.TryGetValueWithType(state, -2, out var key) && valuetrans.TryGetValueWithType(state, -1, out var value))
                    {
                        yield return (key, value);
                    }
                    LuaAPI.lua_settop(state, pairtop);          //table key
                }
            }
        }

        public static bool TryGetValue<T>(IntPtr state, int idx, out T value)
        {
            if (!TryGetLuaTranslator<T>(out var translator))
            {
                value = default;
                return false;
            }
            return translator.TryGetValueWithType(state, idx, out value);
        }

        public static T GetValue<T>(IntPtr state, int idx)
        {
            var translator = GetLuaTranslator<T>();
            return translator.GetValueWithType(state, idx);
        }

        public static T[] GetArray<T>(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            var translator = GetLuaTranslator<T>();

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);                   //table

                var length = LuaAPI.luaL_len(state, -1);
                var temparray = new T[length];

                int arraycount = 0;
                for(arraycount = 0; arraycount < length; arraycount++)
                {
                    LuaAPI.lua_rawgeti(state, -1, arraycount + 1);  //table value
                    if (!translator.TryGetValueWithType(state, -1, out temparray[arraycount]))
                    {
                        LuaAPI.lua_pop(state, 1);
                        break;
                    }
                    LuaAPI.lua_pop(state, 1);                       //table
                }

                if (arraycount == length)
                {
                    return temparray;
                }

                var values = new T[arraycount];
                Array.Copy(temparray, values, arraycount);
                return values;
            }
        }

        public static Dictionary<TKey, TValue> GetTable<TKey, TValue>(IntPtr state, int idx)
        {
            if (LuaAPI.lua_type(state, idx) != LuaTypes.LUA_TTABLE)
            {
                throw new Exception($"Not Table");
            }

            var keytrans = GetLuaTranslator<TKey>();
            var valuetrans = GetLuaTranslator<TValue>();
            var dictionary = new Dictionary<TKey, TValue>();

            using (new LuaTopScope(state))
            {
                LuaAPI.lua_pushvalue(state, idx);               //table
                LuaAPI.lua_pushnil(state);                      //table nil

                int pairtop = LuaAPI.lua_gettop(state);
                while (LuaAPI.lua_next(state, -2))              //table key value
                {
                    if (keytrans.TryGetValueWithType(state, -2, out var key) && valuetrans.TryGetValueWithType(state, -1, out var value))
                    {
                        dictionary.Add(key, value);
                    }
                    LuaAPI.lua_settop(state, pairtop);          //table key
                }
            }

            return dictionary;
        }

        public static void PushValue<T>(IntPtr state, in T value)
        {
            var translator = GetLuaTranslator<T>();
            translator.PushValue(state, value);
        }

        public static void PushArray<T>(IntPtr state, T[] values)
        {
            var translator = GetLuaTranslator<T>();

            LuaAPI.lua_newtable(state, values.Length, 0);    //table

            for(var index = 0; index < values.Length; index++)
            {
                translator.PushValue(state, values[index]);         //table value
                LuaAPI.lua_rawseti(state, -2, index + 1);           //table
            }
        }

        public static void PushArray<T>(IntPtr state, ICollection<T> values)
        {
            var translator = GetLuaTranslator<T>();

            LuaAPI.lua_newtable(state, values.Count, 0);    //table

            var count = 0;
            foreach(var value in values)
            {
                translator.PushValue(state, value);         //table value
                LuaAPI.lua_rawseti(state, -2, ++count);     //table
            }
        }

        public static void PushArray<T>(IntPtr state, IEnumerable<T> values)
        {
            var translator = GetLuaTranslator<T>();

            LuaAPI.lua_newtable(state, 0, 0);               //table

            var count = 0;
            foreach (var value in values)
            {
                translator.PushValue(state, value);         //table value
                LuaAPI.lua_rawseti(state, -2, ++count);     //table
            }
        }

        public static void PushTable<TKey, TValue>(IntPtr state, ICollection<KeyValuePair<TKey, TValue>> values)
        {
            var keytrans = GetLuaTranslator<TKey>();
            var valuetrans = GetLuaTranslator<TValue>();

            LuaAPI.lua_newtable(state, 0, values.Count);    //table

            foreach(var (key, value) in values)
            {
                keytrans.PushValue(state, key);             //table key
                valuetrans.PushValue(state, value);         //table key value
                LuaAPI.lua_settable(state, -3);             //table
            }
        }

        public static void PushTable<TKey, TValue>(IntPtr state, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            var keytrans = GetLuaTranslator<TKey>();
            var valuetrans = GetLuaTranslator<TValue>();

            LuaAPI.lua_newtable(state, 0, 0);               //table

            foreach (var (key, value) in values)
            {
                keytrans.PushValue(state, key);             //table key
                valuetrans.PushValue(state, value);         //table key value
                LuaAPI.lua_settable(state, -3);             //table
            }
        }
#endif
    }
}
