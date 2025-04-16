local type = type;
local pairs = pairs;
local setmetatable = setmetatable;
local getmetatable = getmetatable;
---直接获取值，不经过元表
local rawget = rawget;

local runtimeLib = {};

---取类名
---@return string
function runtimeLib.classname(object)
    return object._classname;
end;
---取对象的类表
function runtimeLib.getClass(object)
    return object._class;
end;

---复制对象
---@return any
function runtimeLib.clone(object)
    local lookUp = {};
    local function _copy(object)
        if type(object) ~= 'table' then
            return object;

        elseif lookUp[object] then
            return lookUp[object];
        else
            local tbl = {};
            lookUp[object] = tbl;

            for k, v in pairs(object) do
                tbl[_copy(k)] = _copy(v);

            end ;
            return setmetatable(tbl, getmetatable(object));

        end ;
    end;
    return _copy(object);
end;

---将一个对象输出为字符串: 一般是用于调试
--key仅支持string,number,boolean，value可以是任意类型. 对于不支持的类型key只打印出其类型
--但对于userdata,thread,function,class,object这些，只会打印出格式化的名字
---@param object  any
---@param isFormat boolean 是否格式化输出,默认为false
---@param depth  number   ,打印的深度默认为20
---@return string 返回字符串
function runtimeLib.toString(object, isFormat, depth)
    local inspect = require("inspect");
    local option = {};
    if isFormat then
        option.newline = "\n";
        option.indent = ' ';
    else
        option.newline = ' ';
        option.indent = '';

    end ;
    if not depth then
        depth = 10;
    end ;
    option.depth = depth;
    return inspect.inspect(object, option);
end;


--[[
    --实现OO机制，类定义如下, 切记类名在正确：
    MyClass = class("MyClass", BaseClass)
    
    --构造函数：在里面定义实例成员，构造函数的参数是可变的
    function MyClass._init(self, m1, m2)
        --这里调用父类的构造函数
        rtl.super(MyClass)._init(self, m1, m2)
        self.mem1 = m1
        self.mem2 = m2
    end;

    -- 定义静态成员
    MyClass.classmem1 = 1
    MyClass.classmem2 = "Hello"

    -- 定义方法
    MyClass.func(self, a, b)
        return self.mem1 * a + self.mem2 * b
    end;

    -- 使用如下：
    local mycls = MyClass(10, 20)
    mycls:func(1, 2)

]]
runtimeLib._class_metatable = nil;
---类定义
---@param className string 类名
---@param superClass table 父类
---@return table 返回类表
function runtimeLib.class(className, superClass)
    local klass = {};
    klass._className = className;
    klass._class = klass;
    klass._super = super;
    klass._type = 'class';
    klass.__index = klass;
    klass.__name = "Instance:" .. className;
    local _class_metatable = runtimeLib._class_metatable;
    if not _class_metatable then
        _class_metatable = {
            __index = function(t, k)
                local super = rawget(t, 'super');
                if super then
                    return super[k];
                else
                    return nil;
                end ;
            end,
            __call = function(cls, ...)
                local obj = {};
                obj.type = 'object';
                setmetatable(obj, cls);
                local _init = cls._init;
                if _init then
                    _init(obj, ...);
                end ;

                return obj
            end;
            __tostring = function(t)
                return "Class:" .. t._classname
            end;
        }
        runtimeLib._class_metatable = _class_metatable;
    end ;
    setmetatable(klass, _class_metatable);
    return klass;
end;
 