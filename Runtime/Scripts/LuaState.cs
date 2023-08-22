using System;
using System.Collections.Generic;

namespace Lua
{
    public interface ILuaState
    {
        ref readonly IntPtr L { get; }
    }

    public sealed class LuaState : ILuaState, IDisposable
    {
        private IntPtr m_state = LuaAPI.luaL_newstate();
        private bool m_disposed = false;

        public ref readonly IntPtr L => ref m_state;

        ~LuaState()
        {
            OnDispose();
        }

        public void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        private void OnDispose()
        {
            if (!m_disposed)
            {
                LuaAPI.lua_close(m_state);
                m_state = IntPtr.Zero;
                m_disposed = true;
            }
        }
    }

    public static class LuaStateExtensions
    {
        //TODO：More Extension Function

        /// <see cref="LuaUtility.Pair(IntPtr, int)"/>
        public static IEnumerable<ValueTuple<int, int>> Pair(this ILuaState state, int idx)
        {
            return LuaUtility.Pair(state.L, idx);
        }

        /// <see cref="LuaUtility.IPair(IntPtr, int)"/>
        public static IEnumerable<ValueTuple<int, int>> IPair(this ILuaState state, int idx)
        {
            return LuaUtility.IPair(state.L, idx);
        }

#if LUA_SIMPLE_TRANSLATOR
        public static IEnumerable<T> Select<T>(this ILuaState state, int idx)
        {
            return LuaUtility.Select<T>(state.L, idx);
        }

        public static IEnumerable<ValueTuple<TKey, TValue>> Select<TKey, TValue>(this ILuaState state, int idx)
        {
            return LuaUtility.Select<TKey, TValue>(state.L, idx);
        }

        public static bool TryGetValue<T>(this ILuaState state, int idx, out T value)
        {
            return LuaUtility.TryGetValue(state.L, idx, out value);
        }

        public static T GetValue<T>(this ILuaState state, int idx)
        {
            return LuaUtility.GetValue<T>(state.L, idx);
        }

        public static T[] GetArray<T>(this ILuaState state, int idx)
        {
            return LuaUtility.GetArray<T>(state.L, idx);
        }

        public static Dictionary<TKey, TValue> GetTable<TKey, TValue>(this ILuaState state, int idx)
        {
            return LuaUtility.GetTable<TKey, TValue>(state.L, idx);
        }

        public static void PushValue<T>(this ILuaState state, in T value)
        {
            LuaUtility.PushValue(state.L, value);
        }

        public static void PushArray<T>(this ILuaState state, T[] values)
        {
            LuaUtility.PushArray(state.L, values);
        }

        public static void PushArray<T>(this ILuaState state, ICollection<T> values)
        {
            LuaUtility.PushArray(state.L, values);
        }

        public static void PushArray<T>(this ILuaState state, IEnumerable<T> values)
        {
            LuaUtility.PushArray(state.L, values);
        }

        public static void PushTable<TKey, TValue>(this ILuaState state, ICollection<KeyValuePair<TKey, TValue>> values)
        {
            LuaUtility.PushTable(state.L, values);
        }

        public static void PushTable<TKey, TValue>(this ILuaState state, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            LuaUtility.PushTable(state.L, values);
        }
#endif
    }
}
