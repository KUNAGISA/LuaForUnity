using System;
using System.Collections;
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

        public static IEnumerable Pair(this ILuaState state, int idx)
        {
            return LuaUtility.Pair(state.L, idx);
        }

        public static IEnumerable<int> IPair(this ILuaState state, int idx)
        {
            return LuaUtility.IPair(state.L, idx);
        }

#if LUA_SIMPLE_TRANSLATOR
        public static bool TryGetValue<T>(this ILuaState state, int idx, out T value)
        {
            return LuaUtility.TryGetValue(state.L, idx, out value);
        }

        public static T GetValue<T>(this ILuaState state, int idx)
        {
            return LuaUtility.GetValue<T>(state.L, idx);
        }

        public static IEnumerable<T> GetValues<T>(this ILuaState state, int idx)
        {
            return LuaUtility.GetValues<T>(state.L, idx);
        }

        public static IEnumerable<ValueTuple<TKey, TValue>> GetValues<TKey, TValue>(this ILuaState state, int idx)
        {
            return LuaUtility.GetValues<TKey, TValue>(state.L, idx);
        }

        public static T[] GetArrayValue<T>(this ILuaState state, int idx)
        {
            return LuaUtility.GetArrayValue<T>(state.L, idx);
        }

        public static void PushValue<T>(this ILuaState state, in T value)
        {
            LuaUtility.PushValue(state.L, value);
        }

        public static void PushValues<T>(this ILuaState state, T[] values)
        {
            LuaUtility.PushValues(state.L, values);
        }

        public static void PushValues<T>(this ILuaState state, ICollection<T> values)
        {
            LuaUtility.PushValues(state.L, values);
        }

        public static void PushValues<T>(this ILuaState state, IEnumerable<T> values)
        {
            LuaUtility.PushValues(state.L, values);
        }

        public static void PushValues<TKey, TValue>(this ILuaState state, IDictionary<TKey, TValue> values)
        {
            LuaUtility.PushValues(state.L, values);
        }
#endif
    }
}
