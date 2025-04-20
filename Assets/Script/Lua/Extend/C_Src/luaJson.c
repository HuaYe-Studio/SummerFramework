#include <cstddef>
#include <cstdint>
#define LUA_LIB
#include "conf.h"
#include "costream.h"
#include "lauxlib.h"
#include "lua.h"
#include <assert.h>
#include <ctype.h>
#include <errno.h>
#include <float.h>
#include <limits.h>
#include <math.h>
#include <setjmp.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// 封装Lua的json解析器，提供给lua调用
// Lua JSON解析器和序列化器实现
// 本文件实现了JSON字符串到Lua对象的解析，以及Lua对象到JSON字符串的序列化
// 支持最大嵌套层数、注释、空表处理、数字key转字符串等特性

// 向Lua栈添加一个对象（table）
static inline void l_add_object(lua_State *L) {
  luaL_checkstack(L, 6, NULL);
  lua_createtable(L, 0, 4);
}
// 开始一个键值对，压入key
static inline void l_begin_pair(lua_State *L, const char *k, size_t size) {
  lua_pushlstring(L, k, size);
}
// 结束一个键值对，设置到table
static inline void l_end_pair(lua_State *L) { lua_rawset(L, -3); }
// 向Lua栈添加一个数组（table）
static inline void l_add_array(lua_State *L) {
  luaL_checkstack(L, 6, NULL);
  lua_createtable(L, 4, 0);
}
// 向数组table添加一个元素
static inline void l_add_index(lua_State *L, int i) {
  lua_rawseti(L, -2, i + 1);
}
// 向Lua栈添加一个字符串
static inline void l_add_string(lua_State *L, const char *s, size_t size) {
  lua_pushlstring(L, s, size);
}
// 向Lua栈添加一个浮点数
static inline void l_add_float(lua_State *L, double f) {
  lua_pushnumber(L, (lua_Number)f);
}
// 向Lua栈添加一个整数
static inline void l_add_integer(lua_State *L, int64_t i) {
  lua_pushinteger(L, (lua_Integer)i);
}
// 向Lua栈添加一个布尔值
static inline void l_add_boolean(lua_State *L, int b) { lua_pushboolean(L, b); }
// 向Lua栈添加一个null（用lightuserdata NULL表示）
static inline void l_add_null(lua_State *L) { lua_pushlightuserdata(L, NULL); }
// 抛出Lua错误
static inline void l_error(lua_State *L, const char *msg) {
  luaL_error(L, msg);
}

// 事件宏定义，ud为lua_State*
#define ON_ADD_OBJECT(ud) l_add_object((lua_State *)(ud))
#define ON_BEGIN_PAIR(ud, k, sz) l_begin_pair((lua_State *)(ud), k, size)
#define ON_END_PAIR(ud) l_end_pair((lua_State *)(ud))
#define ON_ADD_ARRAY(ud) l_add_array((lua_State *)(ud))
#define ON_ADD_INDEX(ud, i) l_add_index((lua_State *)(ud), i)
#define ON_ADD_STRING(ud, s, sz) l_add_string((lua_State *)(ud), s, size)
#define ON_ADD_FLOAT(ud, f) l_add_float((lua_State *)(ud), f)
#define ON_ADD_INTEGER(ud, i) l_add_integer((lua_State *)(ud), i)
#define ON_ADD_BOOLEAN(ud, b) l_add_boolean((lua_State *)(ud), b)
#define ON_ADD_NULL(ud) l_add_null((lua_State *)(ud))
#define ON_ERROR(ud, msg) l_error((lua_State *)(ud), msg)

#define ERRMSG_SIZE 256
// JSON解析器结构体
typedef struct {
  const char *str; // json字符串
  const char *ptr; // 当前解析指针
  void *ud;        // 用户数据（lua_State*）
  membuffer_t buff; // 临时缓冲区
  int curdepth;             // 当前嵌套深度
  int maxdepth;             // 最大嵌套深度
  int allowcomment;         // 是否允许注释
  char errmsg[ERRMSG_SIZE]; // 错误信息
} json_parser_t;

// 初始化解析器
static inline void parser_init(json_parser_t *parser, const char *str,
                               size_t size, void *ud, int maxdepth,
                               int allowcomment) {
  membuffer_init(&parser->buff);
  membuffer_ensure_space(&parser->buff, size);
  parser->str = str;
  parser->ptr = str;
  parser->ud = ud;
  parser->maxdepth = maxdepth;
  parser->curdepth = 0;
  parser->allowcomment = allowcomment;
}
// 释放解析器
static inline void parser_free(json_parser_t *parser) {
  membuffer_free(&parser->buff);
}
// 抛出解析错误
static void parser_throw_error(json_parser_t *parser, const char *fmt, ...) {
  membuffer_free(&parser->buff);
  va_list arg;
  va_start(arg, fmt);
  vsnprintf(parser->errmsg, ERRMSG_SIZE, fmt, arg);
  va_end(arg);
  ON_ERROR(parser->ud, parser->errmsg);
}

#define peekchar(p) (*(p)->ptr) // 查看当前字符
#define skipchar(p) (++(p)->ptr) // 跳过当前字符
#define get_and_next(p) (*(p)->ptr++) // 获取当前字符并前进
#define next_and_get(p) (*(++(p)->ptr)) // 前进并获取字符
#define savechar(p, c) membuffer_putc_unsafe(&(p)->buff, (c)) // 保存字符到缓冲区
#define currpos(p) (size_t)((p)->ptr - (p)->str) // 当前解析位置

// 获取解析错误内容（前50字节）
static const char *parser_error_content(json_parser_t *p) {
  size_t n = currpos(p);
  if (n > 50)
    n = 50;
  membuffer_reset(&p->buff);
  membuffer_putb(&p->buff, p->str - n, n);
  membuffer_putc(&p->buff, '\0');
  return p->buff.b;
}

// 增加嵌套深度，超出则报错
static inline void parser_add_depth(json_parser_t *p) {
  p->curdepth++;
  if (p->curdepth > p->maxdepth) {
    parser_throw_error(p, "Too many nested data, max depth is %d, at: %s[:%lu]",
                       p->maxdepth, parser_error_content(p), currpos(p));
  }
}
// 跳过空白字符
static inline void parser_skip_whitespaces(json_parser_t *p) {
  char ch = peekchar(p);
  while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
    ch = next_and_get(p);
}

/**
 * @brief 确保下一个字符为指定字符，否则报错
 */
static inline void parser_expect_char(json_parser_t *p, char c) {
  if (likely(peekchar(p) == c))
    skipchar(p);
  else
    parser_throw_error(p, "Expect '%c' at: %s[:%lu]", c,
                       parser_error_content(p), currpos(p));
}
// 处理false
static inline void parser_process_false(json_parser_t *p) {
  if (likely(p->ptr[0] == 'a' && p->ptr[1] == 'l' && p->ptr[2] == 's' &&
             p->ptr[3] == 'e')) {
    p->ptr += 4;
    ON_ADD_BOOLEAN(p->ud, 0);
  } else {
    parser_throw_error(p, "Invalid false at: %s[:%lu]", parser_error_content(p),
                       currpos(p));
  }
}
// 处理true
static inline void parser_process_true(json_parser_t *p) {
  if (likely(p->ptr[0] == 'r' && p->ptr[1] == 'u' && p->ptr[2] == 'e')) {
    p->ptr += 3;
    ON_ADD_BOOLEAN(p->ud, 1);
  } else {
    parser_throw_error(p, "Invalid true at: %s[:%lu]", parser_error_content(p),
                       currpos(p));
  }
}
// 处理null
static inline void parser_process_null(json_parser_t *p) {
  if (likely(p->ptr[0] == 'u' && p->ptr[1] == 'l' && p->ptr[2] == 'l')) {
    p->ptr += 3;
    ON_ADD_NULL(p->ud);
  } else {
    parser_throw_error(p, "Invalid null at: %s[:%lu]", parser_error_content(p),
                       currpos(p));
  }
}
/**
 * @brief 读取4位16进制unicode码点
 */
static inline uint32_t parser_read_hex(json_parser_t *p) {
  uint32_t cp = 0;
  unsigned char ch;
  int i = 4;
  while (i--) {
    ch = (unsigned char)get_and_next(p);
    if ('0' <= ch && ch <= '9') {
      ch -= '0';
    } else if (ch >= 'a' && ch <= 'f') {
      ch = ch - 'a' + 10;
    } else if (ch >= 'A' && ch <= 'F') {
      ch = ch - 'A' + 10;
    } else {
      parser_throw_error(p, "Invalid unicode at: %s[:%lu]",
                         parser_error_content(p), currpos(p));
    }
    cp = (cp << 4) + ch;
  }
  return cp;
}

// 解析 JSON 字符串中的 \uXXXX 转义序列，支持UTF-16代理对，写入UTF-8字节
static inline void parser_process_utf8esc(json_parser_t *p) {
  uint32_t cp = parser_read_hex(p);
  // 处理UTF-16代理对
  if (cp >= 0xD800 && cp <= 0xDBFF) {
    char p0 = p->ptr[0];
    char p1 = p->ptr[1];
    if (p0 != '\\' || p1 != 'u')
      parser_throw_error(p, "Invalid utf8 escape sequence, at: %s[:%lu]",
                         parser_error_content(p), currpos(p));
    p->ptr += 2;
    uint32_t cp2 = parser_read_hex(p);
    if (cp2 < 0xDC00 || cp2 > 0xDFFF)
      parser_throw_error(p, "Invalid utf8 escape sequence, at: %s[:%lu]",
                         parser_error_content(p), currpos(p));
    cp = 0x10000 + (((cp & 0x03FF) << 10) | (cp2 & 0x03FF));
  }
  // 编码为UTF-8
  if (cp < 0x80) {
    membuffer_putc_unsafe(&p->buff, (char)cp);
  } else if (cp < 0x800) {
    membuffer_putc_unsafe(&p->buff, 0xC0 | (cp >> 6));
    membuffer_putc_unsafe(&p->buff, 0x80 | (cp & 0x3F));
  } else if (cp < 0x10000) {
    membuffer_putc_unsafe(&p->buff, 0xE0 | (cp >> 12));
    membuffer_putc_unsafe(&p->buff, 0x80 | ((cp >> 6) & 0x3F));
    membuffer_putc_unsafe(&p->buff, 0x80 | (cp & 0x3F));
  } else {
    membuffer_putc_unsafe(&p->buff, 0xF0 | (cp >> 18));
    membuffer_putc_unsafe(&p->buff, 0x80 | ((cp >> 12) & 0x3F));
    membuffer_putc_unsafe(&p->buff, 0x80 | ((cp >> 6) & 0x3F));
    membuffer_putc_unsafe(&p->buff, 0x80 | (cp & 0x3F));
  }
}
// 转义字符表
static const char escape2char[256] = {
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 0~19
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, '\"', 0,
    0,    0, 0,    0, // 20~39
    0,    0, 0,    0, 0, 0, 0, '/', 0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 40~59
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 60~79
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, '\\', 0, 0,    0,
    0,    0, '\b', 0, // 80~99
    0,    0, '\f', 0, 0, 0, 0, 0,   0, 0, '\n', 0, 0,    0, '\r', 0,
    '\t', 0, 0,    0, // 100~119
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 120~139
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 140~159
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 160~179
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 180~199
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0, // 200~219
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0,
    0,    0, 0,    0,                                                // 220~239
    0,    0, 0,    0, 0, 0, 0, 0,   0, 0, 0,    0, 0,    0, 0,    0, // 240~256
};
// 处理JSON字符串，支持转义
static inline void parser_process_string(json_parser_t *p) {
  membuffer_reset(&p->buff);
  char ch = get_and_next(p);
  for (;;) {
    if (ch == '\\') {
      unsigned char nch = (unsigned char)peekchar(p);
      if (likely(escape2char[nch])) {
        savechar(p, escape2char[nch]);
        skipchar(p);
      } else if (nch == 'u') {
        skipchar(p);
        parser_process_utf8esc(p);
      } else {
        parser_throw_error(p, "Invalid escape sequence, at: %s[:%lu]",
                           parser_error_content(p), currpos(p));
      }
    } else if (ch == '"') {
      break;
    } else if ((unsigned char)ch < 0x20) {
      parser_throw_error(p, "Invalid string, at: %s[:%lu]",
                         parser_error_content(p), currpos(p));
    } else {
      savechar(p, ch);
    }
    ch = get_and_next(p);
  }
}
#define invalid_number(p)                                                      \
  parser_throw_error(p, "Invalid value, at: %s[:%lu]",                         \
                     parser_error_content(p), currpos(p))
#define MAXBY10 (int64_t)(922337203685477580)
#define MAXLASTD (int)(7)
static double powersOf10[] = {10.,    100.,   1.0e4,   1.0e8,  1.0e16,
                              1.0e32, 1.0e64, 1.0e128, 1.0e256};
// 处理数字（整数或浮点数）
static inline void parser_process_number(json_parser_t *p, char ch) {
  double db;        // 浮点数
  int64_t in = 0;   // 整型值
  int isdouble = 0; // 是否是浮点数
  int neg = 0;      // 是否是负数
  int exponent = 0; // 指数位数

  if (ch == '-') { // 负值
    neg = 1;
    ch = get_and_next(p);
  }
  if (unlikely(ch == '0')) { // 0开头的后面只能是：.eE或\0
    ch = peekchar(p);
  } else if (likely(ch >= '1' && ch <= '9')) {
    in = ch - '0';
    ch = peekchar(p);
    while (ch >= '0' && ch <= '9') {
      if (unlikely(in >= MAXBY10 &&
                   (in > MAXBY10 ||
                    (ch - '0') > MAXLASTD + neg))) { // 更大的数字就用浮点数表示
        isdouble = 1;
        db = (double)in;
        do {
          db = db * 10.0 + (ch - '0');
          ch = next_and_get(p);
        } while (ch >= '0' && ch <= '9');
        break;
      }
      in = in * 10 + (ch - '0');
      ch = next_and_get(p);
    }
  } else {
    invalid_number(p);
  }

  if (ch == '.') { // 小数点部分
    if (likely(!isdouble)) {
      isdouble = 1;
      db = (double)in;
    }
    ch = next_and_get(p);
    if (unlikely(!(ch >= '0' && ch <= '9')))
      invalid_number(p); // .后面一定是数字
    do {
      db = db * 10. + (ch - '0');
      exponent--;
      ch = next_and_get(p);
    } while (ch >= '0' && ch <= '9');
  }

  if (ch == 'e' || ch == 'E') { // 指数部分
    if (!isdouble) {            // 有e强制认为是浮点数
      isdouble = 1;
      db = (double)in;
    }
    ch = next_and_get(p);
    int eneg = 0;
    if (ch == '-') {
      eneg = 1;
      ch = next_and_get(p);
    } else if (ch == '+') {
      ch = next_and_get(p);
    }
    if (unlikely(!(ch >= '0' && ch <= '9')))
      invalid_number(p); // 后面一定是数字
    int exp = 0;
    do {
      exp = exp * 10. + (ch - '0');
      ch = next_and_get(p);
    } while (ch >= '0' && ch <= '9');
    exponent += eneg ? (-exp) : (exp);
  }

  if (isdouble) {
    int n = exponent < 0 ? -exponent : exponent;
    if (unlikely(n > 511))
      n = 511; // inf
    double p10 = 1.0;
    double *d;
    for (d = powersOf10; n != 0; n >>= 1, d += 1) {
      if (n & 1)
        p10 *= *d;
    }
    if (exponent < 0)
      db /= p10;
    else
      db *= p10;
    if (neg)
      db = -db;
    ON_ADD_FLOAT(p->ud, db);
  } else {
    if (neg)
      in = -in;
    ON_ADD_INTEGER(p->ud, in);
  }
}
static void parser_process_value(json_parser_t *p);
// 处理对象
static inline void parser_process_object(json_parser_t *p) {
  parser_add_depth(p);
  ON_ADD_OBJECT(p->ud);
  parser_skip_whitespaces(p);
  char ch = peekchar(p);
  if (ch == '}') {
    skipchar(p);
    p->curdepth--;
    return;
  }
  for (;;) {
    parser_expect_char(p, '"');
    parser_process_string(p); // key
    ON_BEGIN_PAIR(p->ud, p->buff.b, p->buff.sz);

    parser_skip_whitespaces(p);
    parser_expect_char(p, ':');

    parser_process_value(p); // value
    ON_END_PAIR(p->ud);

    parser_skip_whitespaces(p);
    if (peekchar(p) == '}') {
      skipchar(p);
      p->curdepth--;
      return;
    } else {
      parser_expect_char(p, ',');
      parser_skip_whitespaces(p);
    }
  }
}
// 处理数组
static inline void parser_process_array(json_parser_t *p) {
  parser_add_depth(p);
  ON_ADD_ARRAY(p->ud);
  parser_skip_whitespaces(p);
  char ch = peekchar(p);
  if (ch == ']') {
    skipchar(p);
    p->curdepth--;
    return;
  }
  int i;
  for (i = 0;; i++) {
    parser_process_value(p);
    ON_ADD_INDEX(p->ud, i);
    parser_skip_whitespaces(p);
    if (peekchar(p) == ']') {
      skipchar(p);
      p->curdepth--;
      return;
    } else {
      parser_expect_char(p, ',');
    }
  }
}
// 处理任意值
static void parser_process_value(json_parser_t *p) {
  parser_skip_whitespaces(p);
  char ch = get_and_next(p);
  switch (ch) {
  case 'f':
    parser_process_false(p);
    break;
  case 't':
    parser_process_true(p);
    break;
  case 'n':
    parser_process_null(p);
    break;
  case '"':
    parser_process_string(p);
    ON_ADD_STRING(p->ud, p->buff.b, p->buff.sz);
    break;
  case '{':
    parser_process_object(p);
    break;
  case '[':
    parser_process_array(p);
    break;
  default:
    parser_process_number(p, ch);
    break;
  }
}
// 解析json文本
static void parser_do_parse(const char *str, size_t size, void *ud, int maxdepth, int allowcomment) {
  json_parser_t p;
  parser_init(&p, str, size, ud, maxdepth, allowcomment);
  //>>>if (setjmp(p.jb) == 0) {
    parser_process_value(&p);
    parser_skip_whitespaces(&p);
    if (peekchar(&p) != '\0') {
      parser_throw_error(&p, "Expect '<eof>' but got '%c', at: %s[:%lu]", peekchar(&p), 
        parser_error_content(&p), currpos(&p));
    }
    parser_free(&p);
  //>>>}
}

//-----------------------------------------------------------------------------
// dumpper（序列化器）

typedef struct {
  membuffer_t buff;	// 临时缓存
  int maxdepth;	// 最大层次
  int format;			// 是否格式化
  int empty_as_array; // 空表是否当成数组
  int num_as_str;		// 数字Key转为字符串
  char errmsg[ERRMSG_SIZE];	// 保存错误消息 
} json_dumpper_t;

// 足够转换数字的缓存大小
#define NUMBER_BUFF_SZ 44
#define INTEGER_BUFF_SZ 24

// 抛出错误
static void dumpper_throw_error(json_dumpper_t *d, lua_State *L, const char *fmt, ...) {
  membuffer_free(&d->buff);
  va_list arg;
  va_start(arg, fmt);
  vsnprintf(d->errmsg, ERRMSG_SIZE, fmt, arg);
  va_end(arg);
  luaL_error(L, d->errmsg);
}

// 序列化整数
static void dumpper_process_integer(json_dumpper_t *d, lua_State *L, int idx) {
  char nbuff[INTEGER_BUFF_SZ];
  int i = INTEGER_BUFF_SZ;
  membuffer_ensure_space(&d->buff, INTEGER_BUFF_SZ);
  int64_t x = (int64_t)lua_tointeger(L, idx);
  uint64_t ux = (uint64_t)x;
  if (x < 0) {
    membuffer_putc_unsafe(&d->buff, '-');
    ux = ~ux + 1;
  }
  do {
    nbuff[--i] = (ux % 10) + '0';
  } while (ux /= 10);
  membuffer_putb_unsafe(&d->buff, nbuff+i, INTEGER_BUFF_SZ-i);
}

// 序列化浮点数
static void dumpper_process_number(json_dumpper_t *d, lua_State *L, int idx) {
  lua_Number num = lua_tonumber(L, idx);
   if (isinf(num) || isnan(num))
     dumpper_throw_error(d, L, "The number is NaN or Infinity");
  membuffer_ensure_space(&d->buff, NUMBER_BUFF_SZ);
  char *p = membuffer_getp(&d->buff);
  int len = sprintf(p, LUA_NUMBER_FMT, num);
  membuffer_add_size(&d->buff, len);
}

// 字符转义表
static const char char2escape[256] = {
  'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'b', 't',  'n', 'u', 'f', 'r', 'u', 'u', 'u', 'u', 'u', 'u', // 0~19
  'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u', 'u',  'u', 'u',  0,   0,  '"',   0,   0,   0,   0,   0, // 20~39
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 40~59
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 60~79
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,  '\\', 0,   0,   0,   0,   0,   0,   0,  // 80~99
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 100~119
   0,   0,   0,   0,   0,   0,   0,  'u',  0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 120~139
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 140~159
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 160~179
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 180~199
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 200~219
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,   0,   0,   0,   0,  // 220~239
   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,    0,   0,   0,   0,   0,   0,                      // 240~256
};
static const char hex_digits[16] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

// 序列化字符串，带转义
static void dumpper_process_string(json_dumpper_t *d, lua_State *L, int idx) {
  membuffer_t *buff = &d->buff;
  size_t len, i;
  const char *str = lua_tolstring(L, idx, &len);
  membuffer_ensure_space(buff, len * 6 + 2);
  membuffer_putc_unsafe(buff, '\"');
  char esc;
  unsigned char ch;
  for (i = 0; i < len; ++i) {
    ch = (unsigned char)str[i];
    esc = char2escape[ch];
    if (likely(!esc)) 
      membuffer_putc_unsafe(buff, (char)ch);
    else {
      membuffer_putc_unsafe(buff, '\\');
      membuffer_putc_unsafe(buff, esc);
      if (esc == 'u') {
        membuffer_putc_unsafe(buff, '0');
        membuffer_putc_unsafe(buff, '0');
        membuffer_putc_unsafe(buff, hex_digits[(unsigned char)esc >> 4]);
        membuffer_putc_unsafe(buff, hex_digits[(unsigned char)esc & 0xF]);
      }
    }
  }
  membuffer_putc_unsafe(buff, '\"');
}

static void dumpper_process_value(json_dumpper_t *d, lua_State *L, int depth);

// 检查table是否为数组
static int dumpper_check_array(json_dumpper_t *d, lua_State *L, int *len) {
  int asize = lua_rawlen(L, -1);
  if (asize > 0) {
    lua_pushinteger(L, asize);
    if (lua_next(L, -2) == 0) {
      *len = asize;
      return 1;
    } else {
      lua_pop(L, 2);
      return 0;
    }
  } else {
    lua_pushnil(L);
    if (lua_next(L, -2) == 0) {
      *len = asize;
      return d->empty_as_array;
    } else {
      lua_pop(L, 2);
      return 0;
    }
  }
}

// 添加缩进
static inline void dumpper_add_indent(json_dumpper_t *d, int count) {
  membuffer_ensure_space(&d->buff, count);
  int i;
  for (i = 0; i < count; ++i)
    membuffer_putc_unsafe(&d->buff, '\t');
}

// 序列化数组
static void dumpper_process_array(json_dumpper_t *d, lua_State *L, int len, int depth) {
  membuffer_t *buff = &d->buff;
  membuffer_putc(buff, '[');

  int i;
  for (i = 1; i <= len; ++i) {
    if (unlikely(d->format && i == 1)) membuffer_putc(buff, '\n');
    lua_rawgeti(L, -1, i);
    if (unlikely(d->format)) dumpper_add_indent(d, depth);
    dumpper_process_value(d, L, depth);
    lua_pop(L, 1);
    if (i < len)
      membuffer_putc(buff, ',');
    if (unlikely(d->format)) membuffer_putc(buff, '\n');
  }

  if (unlikely(d->format && i > 1))  dumpper_add_indent(d, depth-1);
  membuffer_putc(buff, ']');
}

// 序列化对象
static void dumpper_process_object(json_dumpper_t *d, lua_State *L, int depth) {
  membuffer_t *buff = &d->buff;
  membuffer_putc(buff, '{');

  int ktp;
  int comma = 0;
  lua_pushnil(L);		// t nil
  while (lua_next(L, -2) != 0) {	// t k v
    if (comma) {
      membuffer_putc(buff, ',');
      if (unlikely(d->format)) membuffer_putc(buff, '\n');
    } else {
      comma = 1;
      if (unlikely(d->format)) membuffer_putc(buff, '\n');
    } 
    // key
    ktp = lua_type(L, -2);
    if (ktp == LUA_TSTRING) {
      if (unlikely(d->format)) dumpper_add_indent(d, depth);
      dumpper_process_string(d, L, -2);
      if (likely(!d->format))
        membuffer_putc(buff, ':');
      else
        membuffer_putb(buff, " : ", 3);
    } else if (ktp == LUA_TNUMBER && d->num_as_str) {
      if (unlikely(d->format)) dumpper_add_indent(d, depth);
      membuffer_putc(buff, '\"');
      if (lua_isinteger(L, -2))
        dumpper_process_integer(d, L, -2);
      else
        dumpper_process_number(d, L, -2);
      if (likely(!d->format))
        membuffer_putb(buff, "\":", 2);
      else
        membuffer_putb(buff, "\" : ", 4);
    } else {
      dumpper_throw_error(d, L, "Table key must be a string");
    }
    // value
    dumpper_process_value(d, L, depth);
    lua_pop(L, 1);
  }
  if (unlikely(d->format && comma)) {
    membuffer_putc(buff, '\n');
    dumpper_add_indent(d, depth-1);
  } 
  membuffer_putc(buff, '}');
}

// 序列化table
static inline void dumpper_process_table(json_dumpper_t *d, lua_State *L, int depth) {
  depth++;
  if (depth > d->maxdepth)
    dumpper_throw_error(d, L, "Too many nested data, max depth is %d", d->maxdepth);
  luaL_checkstack(L, 6, NULL);

  int len;
  if (dumpper_check_array(d, L, &len))
    dumpper_process_array(d, L, len, depth);
  else
    dumpper_process_object(d, L, depth);
}

// 序列化任意值
static void dumpper_process_value(json_dumpper_t *d, lua_State *L, int depth) {
  int tp = lua_type(L, -1);
  switch (tp) {
    case LUA_TSTRING:
      dumpper_process_string(d, L, -1);
      break;
    case LUA_TNUMBER:
      if (lua_isinteger(L, -1))
        dumpper_process_integer(d, L, -1);
      else
        dumpper_process_number(d, L, -1);
      break;
    case LUA_TBOOLEAN:
      if (lua_toboolean(L, -1))
        membuffer_putb(&d->buff, "true", 4);
      else
        membuffer_putb(&d->buff, "false", 5);
      break;
    case LUA_TTABLE:
      dumpper_process_table(d, L, depth);
      break;
    case LUA_TNIL:
      membuffer_putb(&d->buff, "null", 4);
      break;
    case LUA_TLIGHTUSERDATA:
      if (lua_touserdata(L, -1) == NULL) {
        membuffer_putb(&d->buff, "null", 4);
        break;
      }
      goto error;
    default:
    error:
      dumpper_throw_error(d, L, "Unsupport type %s", lua_typename(L, tp));
  }
}

//-----------------------------------------------------------------------------
// Lua接口
#define DEF_MAX_DEPTH 128

// 从字符串加载：json.load(str, maxdepth) -> obj
// 要求字符串必须以0结尾
static int l_load(lua_State *L) {
  size_t size;
  const char *str = luaL_checklstring(L, 1, &size);
  int maxdepth = (int)luaL_optinteger(L, 2, DEF_MAX_DEPTH);
  int allowcomment = lua_toboolean(L, 3);
  parser_do_parse(str, size, L, maxdepth, allowcomment);
  return 1;
}

// 保存到字符串: json.dump(obj) -> str
static int l_dump(lua_State *L) {
  luaL_checkany(L, 1);
  json_dumpper_t dumpper;
  membuffer_init(&dumpper.buff);
  dumpper.format = lua_toboolean(L, 2);
  dumpper.empty_as_array = lua_toboolean(L, 3);
  dumpper.num_as_str = lua_toboolean(L, 4);
  dumpper.maxdepth = (int)luaL_optinteger(L, 5, DEF_MAX_DEPTH);

  lua_settop(L, 1);
  dumpper_process_value(&dumpper, L, 0);
  lua_pushlstring(L, dumpper.buff.b, dumpper.buff.sz);
  membuffer_free(&dumpper.buff);
  return 1;
}

// Lua库注册表
static const luaL_Reg lib[] = {
  {"load", l_load},
  {"dump", l_dump},
  {NULL, NULL},
};

// 模块入口
LUAMOD_API int luaopen_colibc_json(lua_State *L) {
  luaL_newlib(L, lib);
  // json.null
  lua_pushlightuserdata(L, NULL);
  lua_setfield(L, -2, "null");
  return 1;
}