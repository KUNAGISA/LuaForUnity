using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Lua
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaAllocFunction(IntPtr ud, IntPtr ptr, int osize, int nsize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaReaderFunction(IntPtr state, IntPtr ud, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaWriterFunction(IntPtr state, IntPtr p, int size, IntPtr ud);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaKFunction(IntPtr state, LuaStatus status, IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaWarnFunction(IntPtr ud, IntPtr msg, int tocont);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LuaHookFunction(IntPtr luaState, IntPtr luaDebug);

    public enum LuaStatus : int
    {
        LUA_OK              = 0,
        LUA_YIELD           = 1,
        LUA_ERRRUN          = 2,
        LUA_ERRSYNTAX       = 3,
        LUA_ERRMEM          = 4,
        LUA_ERRERR          = 5,
    }

    public enum LuaTypes : int
    {
        LUA_TNONE           = -1,
        LUA_TNIL            = 0,
        LUA_TBOOLEAN        = 1,
        LUA_TLIGHTUSERDATA  = 2,
        LUA_TNUMBER         = 3,
        LUA_TSTRING         = 4,
        LUA_TTABLE          = 5,
        LUA_TFUNCTION       = 6,
        LUA_TUSERDATA       = 7,
        LUA_TTHREAD         = 8,
    }

    public enum LuaOps : int
    {
        LUA_OPADD           = 0,
        LUA_OPSUB           = 1,
        LUA_OPMUL           = 2,
        LUA_OPMOD           = 3,
        LUA_OPPOW           = 4,
        LUA_OPDIV           = 5,
        LUA_OPIDIV          = 6,
        LUA_OPBAND          = 7,
        LUA_OPBOR           = 8,
        LUA_OPBXOR          = 9,
        LUA_OPSHL           = 10,
        LUA_OPSHR           = 11,
        LUA_OPUNM           = 12,
        LUA_OPBNOT          = 13
    }

    public enum LuaCompare
    {
        LUA_OPEQ            = 0,
        LUA_OPLT            = 1,
        LUA_OPLE            = 2
    }

    public enum LuaGcOpts : int
    {
        LUA_GCSTOP          = 0,
        LUA_GCRESTART       = 1,
        LUA_GCCOLLECT       = 2,
        LUA_GCCOUNT         = 3,
        LUA_GCCOUNTB        = 4,
        LUA_GCSTEP          = 5,
        LUA_GCSETPAUSE      = 6,
        LUA_GCSETSTEPMUL    = 7,
        LUA_GCISRUNNING     = 9
    }

    public enum LuaEventCodes : int
    {
        LUA_HOOKCALL        = 0,
        LUA_HOOKRET         = 1,
        LUA_HOOKLINE        = 2,
        LUA_HOOKCOUNT       = 3,
        LUA_HOOKTAILRET     = 4
    }

    [Flags]
    public enum LuaEventMasks
    {
        LUA_MASKCALL        = (1 << LuaEventCodes.LUA_HOOKCALL),
        LUA_MASKRET         = (1 << LuaEventCodes.LUA_HOOKRET),
        LUA_MASKLINE        = (1 << LuaEventCodes.LUA_HOOKLINE),
        LUA_MASKCOUNT       = (1 << LuaEventCodes.LUA_HOOKCOUNT),
    }

    public static class LuaConst
    {
        public const int LUA_MULTRET = -1;

        public const int LUAI_MAXSTACK = 1000000;
        public const int LUA_REGISTRYINDEX = -LUAI_MAXSTACK - 1000;

        public const int LUA_RIDX_MAINTHREAD = 1;
        public const int LUA_RIDX_GLOBALS = 2;

        public const int LUA_NOREF = -2;
        public const int LUA_REFNIL = -1;

        // LUA VEISON CONST
        public const int LUA_VERSION_MAJOR_N = 5;
        public const int LUA_VERSION_MINOR_N = 4;
        public const int LUA_VERSION_RELEASE_N = 6;

        public const int LUA_VERSION_NUM = LUA_VERSION_MAJOR_N * 100 + LUA_VERSION_MINOR_N;
        public const int LUA_VERSION_RELEASE_NUM = LUA_VERSION_NUM * 100 + LUA_VERSION_RELEASE_N;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct LuaLReg
    {
        public static LuaLReg Null = new LuaLReg(null, null);

        public readonly string name;
        public readonly LuaCSFunction func;

        public LuaLReg(string name, LuaCSFunction func)
        {
            this.name = name;
            this.func = func;
        }
    }

    public static class LuaAPI
    {
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        private const string LuaDLL = "__Internal";
#else
        private const string LuaDLL = "lua54";
#endif

        private static string GetString(in IntPtr strPtr, in IntPtr lenPtr)
        {
            if (strPtr == IntPtr.Zero)
            {
                return null;
            }

            var len = lenPtr.ToInt32();

#if (UNITY_WSA && !UNITY_EDITOR)
            var buffer = new byte[len];
            Marshal.Copy(strPtr, buffer, 0, len);
            return Encoding.UTF8.GetString(buffer);
#else
            var ret = Marshal.PtrToStringAnsi(strPtr, len);
            if (ret == null)
            {
                var buffer = new byte[len];
                Marshal.Copy(strPtr, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }
            return ret;
#endif
        }

#pragma warning disable IDE1006

        #region lua.h

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newstate(LuaAllocFunction f, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newthread(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_closethread(IntPtr state, IntPtr from);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_resetthread(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaCSFunction lua_atpanic(IntPtr state, LuaCSFunction paincf);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_version(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_absindex(IntPtr state, int idx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int lua_upvalueindex(int i)
        {
            return LuaConst.LUA_REGISTRYINDEX - i;
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr state, int idx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_pop(IntPtr state, int n)
        {
            lua_settop(state, -n - 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rotate(IntPtr state, int idx, int n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_insert(IntPtr state, int idx)
        {
            lua_rotate(state, idx, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_remove(IntPtr state, int idx)
        {
            lua_rotate(state, idx, -1);
            lua_pop(state, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_replace(IntPtr L, int idx)
        {
            lua_copy(L, -1, idx);
            lua_pop(L, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_copy(IntPtr state, int fromidx, int toidx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_checkstack(IntPtr state, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_xmove(IntPtr from, IntPtr to, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isnumber(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isstring(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_iscfunction(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isinteger(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isuserdata(IntPtr state, int idx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isfunction(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TFUNCTION;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_istable(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TTABLE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_islightuserdata(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TLIGHTUSERDATA;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isnil(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TNIL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isboolean(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TBOOLEAN;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isthread(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TTHREAD;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isnone(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TNONE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_isnoneornil(IntPtr state, int idx)
        {
            return (int)lua_type(state, idx) <= 0;
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_type(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_typename(IntPtr state, LuaTypes tp);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern double lua_tonumberx(IntPtr state, int idx, IntPtr isnum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double lua_tonumberx(IntPtr state, int idx, out bool isnum)
        {
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            var value = lua_tonumberx(state, idx, pointer);
            isnum = pointer.ToInt32() == 1;
            Marshal.FreeHGlobal(pointer);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double lua_tonumber(IntPtr state, int idx)
        {
            return lua_tonumberx(state, idx, IntPtr.Zero);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern long lua_tointegerx(IntPtr state, int idx, IntPtr isnum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long lua_tointegerx(IntPtr state, int idx, out bool isnum)
        {
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            var value = lua_tointegerx(state, idx, pointer);
            isnum = pointer.ToInt32() == 1;
            Marshal.FreeHGlobal(pointer);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long lua_tointeger(IntPtr state, int idx)
        {
            return lua_tointegerx(state, idx, IntPtr.Zero);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_toboolean(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr lua_tolstring(IntPtr state, int idx, out IntPtr len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr lua_tolstring(IntPtr state, int idx, out int len)
        {
            len = 0;
            var str = lua_tolstring(state, idx, out IntPtr lenPtr);
            if (str != IntPtr.Zero)
            {
                len = lenPtr.ToInt32();
            }
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string lua_tostring(IntPtr state, int idx)
        {
            var str = lua_tolstring(state, idx, out IntPtr len);
            return GetString(str, len);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaCSFunction lua_tocfunction(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_touserdata(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_tothread(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_topointer(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong lua_rawlen(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_arith(IntPtr state, LuaOps op);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_rawequal(IntPtr state, int idx1, int idx2);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_compare(IntPtr state, int idx1, int idx2, LuaCompare op);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnil(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushnumber(IntPtr state, double n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushinteger(IntPtr state, long n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void lua_pushlstring(IntPtr state, byte[] str, int size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_pushlstring(IntPtr state, byte[] str)
        {
            lua_pushlstring(state, str, str.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_pushlstring(IntPtr state, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            lua_pushlstring(state, bytes, bytes.Length);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr state, string str);

        public static void lua_pushglobaltable(IntPtr state)
        {
            lua_rawgeti(state, LuaConst.LUA_REGISTRYINDEX, LuaConst.LUA_RIDX_GLOBALS);
        }

        /* UNITY NOT SUPORT __arglist
        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvfstring(IntPtr state, string fmt, __arglist);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushfstring(IntPtr state, string fmt, __arglist);
        */

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr state, LuaCSFunction fn, int n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_pushcfunction(IntPtr L, LuaCSFunction func)
        {
            lua_pushcclosure(L, func, 0);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushboolean(IntPtr state, bool b);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushlightuserdata(IntPtr state, IntPtr p);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_pushthread(IntPtr state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_register(IntPtr state, string name, LuaCSFunction func)
        {
            lua_pushcfunction(state, func);
            lua_setglobal(state, name);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_getglobal(IntPtr state, string str);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_gettable(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_getfield(IntPtr state, int idx, string key);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_geti(IntPtr state, int idx, long n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_rawget(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_rawgeti(IntPtr state, int idx, long n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_rawgetp(IntPtr state, int idx, IntPtr p);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_createtable(IntPtr state, int narr, int nrec);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_newtable(IntPtr state, int narr = 0, int nrec = 0)
        {
            lua_createtable(state, narr, nrec);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newuserdatauv(IntPtr state, int size, int nuvalue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_newuserdata(IntPtr state, int size)
        {
            lua_newuserdatauv(state, size, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_getmetatable(IntPtr state, int objindex);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_getiuservalue(IntPtr state, int idx, int n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaTypes lua_getuservalue(IntPtr state, int idx)
        {
            return lua_getiuservalue(state, idx, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setglobal(IntPtr state, string name);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settable(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setfield(IntPtr state, int idx, string name);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_seti(IntPtr state, int idx, long n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawset(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawseti(IntPtr state, int idx, long n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rawsetp(IntPtr state, int idx, IntPtr p);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_setmetatable(IntPtr state, int objindex);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_setiuservalue(IntPtr state, int idx, int n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool lua_setuservalue(IntPtr state, int idx)
        {
            return lua_setiuservalue(state, idx, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_callk(IntPtr state, int nargs, int nresults, IntPtr ctx, LuaKFunction k);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void lua_call(IntPtr state, int nargs = 0, int nresults = LuaConst.LUA_MULTRET)
        {
            lua_callk(state, nargs, nresults, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_pcallk(IntPtr state, int nargs, int nresults, int errfunc, IntPtr ctx, LuaKFunction k);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus lua_pcall(IntPtr state, int nargs = 0, int nresults = LuaConst.LUA_MULTRET, int errfunc = 0)
        {
            return lua_pcallk(state, nargs, nresults, errfunc, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_load(IntPtr state, LuaReaderFunction reader, IntPtr data, string chunkname, string mode);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_dump(IntPtr state, LuaWriterFunction writer, IntPtr data, int strip);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_yieldk(IntPtr state, int nresults, IntPtr ctx, LuaKFunction k);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus lua_yield(IntPtr state, int nresults = 0)
        {
            return lua_yieldk(state, nresults, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern LuaStatus lua_resume(IntPtr state, IntPtr from, int narg, out IntPtr nresults);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus lua_resume(IntPtr state, IntPtr from, int narg, out int nresults)
        {
            nresults = 0;
            var status = lua_resume(state, from, narg, out IntPtr pointer);
            if (status == LuaStatus.LUA_OK)
            {
                nresults = pointer.ToInt32();
            }
            return status;
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus lua_status(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_isyieldable(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setwarnf(IntPtr state, LuaWarnFunction f, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_warning(IntPtr state, string msg, int tocont);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gc(IntPtr state, LuaGcOpts what, int data);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_error(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_next(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_concat(IntPtr state, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_len(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_stringtonumber(IntPtr state, string s);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaAllocFunction lua_getallocf(IntPtr state, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setallocf(IntPtr state, LuaAllocFunction f, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_toclose(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_closeslot(IntPtr state, int idx);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr lua_getextraspace(IntPtr state)
        {
            var ptrval = state.ToInt64();
            ptrval -= IntPtr.Size;
            return (IntPtr)ptrval;
        }

        /*TO DO: DEBUG STRUCT
        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_getstack(IntPtr state, int level, IntPtr debug);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_getinfo(IntPtr state, string what, IntPtr debug);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_getlocal(IntPtr state, IntPtr debug, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_setlocal(IntPtr state, IntPtr debug, int n);
        */

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_getupvalue(IntPtr state, int funcIndex, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_setupvalue(IntPtr state, int funcIndex, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_upvalueid(IntPtr state, int fidx, int n);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string lua_upvaluejoin(IntPtr state, int fidx1, int n1, int fidx2, int n2);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_sethook(IntPtr state, LuaHookFunction func, LuaEventMasks mask, int cout);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaHookFunction lua_gethook(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaEventMasks lua_gethookmask(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gethookcount(IntPtr state);

        #endregion lua.h

        #region lualib.h

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_base(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_coroutine(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_table(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_io(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_os(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_string(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_utf8(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_math(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_debug(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_package(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_openlibs(IntPtr state);

        #endregion lualib.h

        #region lauxlib.h

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_checkversion_(IntPtr state, double ver, int sz);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_checkversion(IntPtr state)
        {
            luaL_checkversion_(state, LuaConst.LUA_VERSION_NUM, sizeof(long) * 16 + sizeof(double));
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes luaL_getmetafield(IntPtr state, int obj, string e);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaL_callmeta(IntPtr state, int obj, string e);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr luaL_tolstring(IntPtr state, int idx, out IntPtr len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_tolstring(IntPtr state, int idx)
        {
            var str = luaL_tolstring(state, idx, out var len);
            return GetString(str, len);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_argerror(IntPtr state, int arg, string extramsg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_typeerror(IntPtr state, int arg, string tname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr luaL_checklstring(IntPtr state, int arg, out IntPtr len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_checklstring(IntPtr state, int arg)
        {
            var str = luaL_checklstring(state, arg, out var len);
            return GetString(str, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_checkstring(IntPtr state, int arg)
        {
            return luaL_checklstring(state, arg);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr luaL_optlstring(IntPtr state, int arg, string def, out IntPtr len);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_optlstring(IntPtr state, int arg, string def)
        {
            var str = luaL_optlstring(state, arg, def, out var len);
            return GetString(str, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_optstring(IntPtr state, int arg, string def)
        {
            return luaL_optlstring(state, arg, def);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double luaL_checknumber(IntPtr state, int arg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double luaL_optnumber(IntPtr state, int arg, double def);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern long luaL_checkinteger(IntPtr state, int arg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern long luaL_optinteger(IntPtr state, int arg, double def);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_checkstack(IntPtr state, int sz, string msg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_checktype(IntPtr state, int arg, LuaTypes t);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_checkany(IntPtr state, int arg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaL_newmetatable(IntPtr state, string tname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_setmetatable(IntPtr state, string tname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_testudata(IntPtr state, int ud, string tname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_checkudata(IntPtr state, int ud, string tname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_where(IntPtr state, int lvl);

        /* UNITY NOT SUPORT __arglist
        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_error(IntPtr state, string fmt, __arglist);
        */

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_error(IntPtr state, string msg);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_checkoption(IntPtr state, int arg, string def, string[] lst);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_fileresult(IntPtr state, int stat, string fname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_execresult(IntPtr state, int stat);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_ref(IntPtr state, int t);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int luaL_ref(IntPtr state)
        {
            return luaL_ref(state, LuaConst.LUA_REGISTRYINDEX);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaL_unref(IntPtr state, int t, int @ref);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int luaL_unref(IntPtr state, int @ref)
        {
            return luaL_unref(state, LuaConst.LUA_REGISTRYINDEX, @ref);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_getref(IntPtr state, int t, int @ref)
        {
            lua_rawgeti(state, t, @ref);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_getref(IntPtr state, int @ref)
        {
            lua_rawgeti(state, LuaConst.LUA_REGISTRYINDEX, @ref);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaStatus luaL_loadfilex(IntPtr state, string filename, string mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_loadfile(IntPtr state, string filename)
        {
            return luaL_loadfilex(state, filename, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern LuaStatus luaL_loadbufferx(IntPtr state, byte[] buff, int size, string name, string mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_loadbufferx(IntPtr state, byte[] buff, string name, string mode)
        {
            return luaL_loadbufferx(state, buff, buff.Length, name, mode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_loadbuffer(IntPtr state, byte[] buff, string name)
        {
            return luaL_loadbufferx(state, buff, name, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_loadbuffer(IntPtr state, string buff, string name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(buff);
            return luaL_loadbuffer(state, bytes, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_loadstring(IntPtr state, string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return luaL_loadbuffer(state, bytes, s);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern long luaL_len(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_setfuncs(IntPtr state, LuaLReg[] l, int nup);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool luaL_getsubtable(IntPtr state, int idx, string fname);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_traceback(IntPtr state, IntPtr l1, string msg, int level);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void luaL_requiref(IntPtr state, string modname, LuaCSFunction openf, int glb);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_newlibtable(IntPtr state, LuaLReg[] l)
        {
            lua_createtable(state, 0, l.Length - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_newlib(IntPtr state, LuaLReg[] l)
        {
            luaL_checkversion(state);
            luaL_newlibtable(state, l);
            luaL_setfuncs(state, l, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_argcheck(IntPtr state, bool cond, int arg, string extramsg)
        {
            if (!cond)
            {
                luaL_argerror(state, arg, extramsg);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_argexpected(IntPtr state, bool cond, int arg, string tname)
        {
            if (!cond)
            {
                luaL_typeerror(state, arg, tname);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string luaL_typename(IntPtr state, int idx)
        {
            return lua_typename(state, lua_type(state, idx));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_dofile(IntPtr state, string filename)
        {
            var status = luaL_loadfile(state, filename);
            if (status == LuaStatus.LUA_OK)
            {
                status = lua_pcall(state, 0, LuaConst.LUA_MULTRET, 0);
            }
            return status;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaStatus luaL_dostring(IntPtr state, string str)
        {
            var status = luaL_loadstring(state, str);
            if (status == LuaStatus.LUA_OK) 
            {
                status = lua_pcall(state, 0, LuaConst.LUA_MULTRET, 0);
            }
            return status;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaTypes luaL_getmetatable(IntPtr state, string name)
        {
            return lua_getfield(state, LuaConst.LUA_REGISTRYINDEX, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void luaL_pushfail(IntPtr state)
        {
            lua_pushnil(state);
        }

        //Not Support
        //Generic Buffer manipulation
        //File handles for IO library
        //compatibility with old module system
        //"Abstraction Layer" for basic report of messages and errors
        //Compatibility with deprecated conversions

        #endregion lauxlib.h

#pragma warning restore IDE1006
    }
}