#ifndef CONF_H
#define CONF_H

#include "lua.h"

// lua 5.4以上用lua_newuserdatauv更好
#if LUA_VERSION_NUM >= 504
#define co_newuserdata(L, sz) lua_newuserdatauv(L, sz, 0)
#else
#define co_newuserdata(L, sz) lua_newuserdata(L, sz)
#endif // LUA_VERSION_NUM >= 504

// 内存分配函数，方便替换
#define _malloc malloc
#define _free free
#define _realloc realloc
#define _calloc calloc


#if !defined(likely)
#if defined(__GNUC__)
#define likely(x)	(__builtin_expect(((x) != 0), 1))
#define unlikely(x)	(__builtin_expect(((x) != 0), 0))
#else
#define likely(x)	(x)
#define unlikely(x)	(x)
#endif

#endif


#endif // CONF_H