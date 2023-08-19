using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Lua
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int LuaCSFunction(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaAllocFunction(IntPtr ud, IntPtr ptr, int osize, int nsize);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaReaderFunction(IntPtr state, IntPtr ud, uint size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaWriterFunction(IntPtr state, IntPtr p, uint size, IntPtr ud);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaKFunction(IntPtr state, LuaThreadStatus status, IntPtr ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LuaWarnFunction(IntPtr ud, IntPtr msg, int tocont);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LuaHookFunction(IntPtr luaState, IntPtr luaDebug);

    public enum LuaThreadStatus : int
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

    public static class LuaAPI
    {
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        private const string LuaDLL = "__Internal";
#else
        private const string LuaDLL = "lua54";
#endif

        public const int Multret = -1;
        public const int MinStack = 20;
        public const int MaxStack = 1000000;
        public const int RegistryIndex = -MaxStack - 1000;
        public const int RidxMainThread = 1;
        public const int RidxGlobals = 2;

#pragma warning disable IDE1006

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newstate(LuaAllocFunction f, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr luaL_newstate();

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_close(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newthread(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_closethread(IntPtr state, IntPtr from);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_resetthread(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaCSFunction lua_atpanic(IntPtr state, LuaCSFunction paincf);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double lua_version(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_absindex(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lua_gettop(IntPtr state);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_settop(IntPtr state, int idx);

        public static void lua_pop(IntPtr state, int n)
        {
            lua_settop(state, -n - 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvalue(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_rotate(IntPtr state, int idx, int n);

        public static void lua_insert(IntPtr state, int idx)
        {
            lua_rotate(state, idx, 1);
        }

        public static void lua_remove(IntPtr state, int idx)
        {
            lua_rotate(state, idx, -1);
            lua_pop(state, 1);
        }

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

        public static bool lua_isfunction(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TFUNCTION;
        }

        public static bool lua_istable(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TTABLE;
        }

        public static bool lua_islightuserdata(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TLIGHTUSERDATA;
        }

        public static bool lua_isnil(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TNIL;
        }

        public static bool lua_isboolean(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TBOOLEAN;
        }

        public static bool lua_isthread(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TTHREAD;
        }

        public static bool lua_isnone(IntPtr state, int idx)
        {
            return lua_type(state, idx) == LuaTypes.LUA_TNONE;
        }

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

        public static double lua_tonumberx(IntPtr state, int idx, out bool isnum)
        {
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            var value = lua_tonumberx(state, idx, pointer);
            isnum = pointer.ToInt32() == 1;
            Marshal.FreeHGlobal(pointer);
            return value;
        }

        public static double lua_tonumber(IntPtr state, int idx)
        {
            return lua_tonumberx(state, idx, IntPtr.Zero);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern long lua_tointegerx(IntPtr state, int idx, IntPtr isnum);

        public static long lua_tointegerx(IntPtr state, int idx, out bool isnum)
        {
            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            var value = lua_tointegerx(state, idx, pointer);
            isnum = pointer.ToInt32() == 1;
            Marshal.FreeHGlobal(pointer);
            return value;
        }

        public static long lua_tointeger(IntPtr state, int idx)
        {
            return lua_tointegerx(state, idx, IntPtr.Zero);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_toboolean(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr lua_tolstring(IntPtr state, int idx, out UIntPtr len);

        public static IntPtr lua_tolstring(IntPtr state, int idx, out uint len)
        {
            len = 0u;
            var str = lua_tolstring(state, idx, out UIntPtr strlen);
            if (str != IntPtr.Zero)
            {
                len = strlen.ToUInt32();
            }
            return str;
        }

        public static string lua_tostring(IntPtr state, int idx)
        {
            var str = lua_tolstring(state, idx, out uint len);
            if (str != IntPtr.Zero)
            {
#if XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
                int len = strlen.ToInt32();
                byte[] buffer = new byte[len];
                Marshal.Copy(str, buffer, 0, (int)len);
                return Encoding.UTF8.GetString(buffer);
#else
                string ret = Marshal.PtrToStringAnsi(str, (int)len);
                if (ret == null)
                {
                    byte[] buffer = new byte[len];
                    Marshal.Copy(str, buffer, 0, (int)len);
                    return Encoding.UTF8.GetString(buffer);
                }
                return ret;
#endif
            }
            else
            {
                return null;
            }
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

        public static void lua_pushlstring(IntPtr state, byte[] str)
        {
            lua_pushlstring(state, str, str.Length);
        }

        public static void lua_pushlstring(IntPtr state, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            lua_pushlstring(state, bytes, bytes.Length);
        }

#if NATIVE_LUA_PUSHSTRING
        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushstring(IntPtr state, [MarshalAs(UnmanagedType.LPStr)] string str);
#else
        public static void lua_pushstring(IntPtr state, string str) //业务使用
        {
            if (str == null)
            {
                lua_pushnil(state);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                lua_pushlstring(state, bytes, bytes.Length);
            }
        }
#endif

        public static void lua_pushglobaltable(IntPtr state)
        {
            lua_rawgeti(state, RegistryIndex, RidxGlobals);
        }

        /* UNITY NOT SUPORT __arglist
        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushvfstring(IntPtr state, string fmt, __arglist);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushfstring(IntPtr state, string fmt, __arglist);
        */

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_pushcclosure(IntPtr state, LuaCSFunction fn, int n);

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

        public static void lua_newtable(IntPtr state, int narr = 0, int nrec = 0)
        {
            lua_createtable(state, narr, nrec);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr lua_newuserdatauv(IntPtr state, uint size, int nuvalue);

        public static void lua_newuserdata(IntPtr state, uint size)
        {
            lua_newuserdatauv(state, size, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool lua_getmetatable(IntPtr state, int objindex);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaTypes lua_getiuservalue(IntPtr state, int idx, int n);

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

        public static bool lua_setuservalue(IntPtr state, int idx)
        {
            return lua_setiuservalue(state, idx, 1);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_callk(IntPtr state, int nargs, int nresults, IntPtr ctx, LuaKFunction k);

        public static void lua_call(IntPtr state, int nargs = 0, int nresults = Multret)
        {
            lua_callk(state, nargs, nresults, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_pcallk(IntPtr state, int nargs, int nresults, int errfunc, IntPtr ctx, LuaKFunction k);

        public static LuaThreadStatus lua_pcall(IntPtr state, int nargs = 0, int nresults = Multret, int errfunc = 0)
        {
            return lua_pcallk(state, nargs, nresults, errfunc, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_load(IntPtr state, LuaReaderFunction reader, IntPtr data, string chunkname, string mode);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_dump(IntPtr state, LuaWriterFunction writer, IntPtr data, int strip);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_yieldk(IntPtr state, int nresults, IntPtr ctx, LuaKFunction k);

        public static LuaThreadStatus lua_yield(IntPtr state, int nresults = 0)
        {
            return lua_yieldk(state, nresults, IntPtr.Zero, null);
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern LuaThreadStatus lua_resume(IntPtr state, IntPtr from, int narg, out IntPtr nresults);

        public static LuaThreadStatus lua_resume(IntPtr state, IntPtr from, int narg, out int nresults)
        {
            nresults = 0;
            var status = lua_resume(state, from, narg, out IntPtr pointer);
            if (status == LuaThreadStatus.LUA_OK)
            {
                nresults = pointer.ToInt32();
            }
            return status;
        }

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaThreadStatus lua_status(IntPtr state);

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
        public static extern uint lua_stringtonumber(IntPtr state, string s);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LuaAllocFunction lua_getallocf(IntPtr state, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_setallocf(IntPtr state, LuaAllocFunction f, IntPtr ud);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_toclose(IntPtr state, int idx);

        [DllImport(LuaDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_closeslot(IntPtr state, int idx);

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

#pragma warning restore IDE1006
    }
}