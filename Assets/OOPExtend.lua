--#region 封装
Object = {};
function Object:new()
    local obj = {};
    setmetatable(obj, self);
    self.__index = self;
    return obj;
end

--#endregion

--#region 继承
-- classname:子类名
-- return:子类对象
function Object:SubClass(classname)
    _G[classname] = {};
    local obj = _G[classname];
    self.__index = self;
    --子类，定义一个Base属性，指向父类
    obj.base = self;
    setmetatable(obj, self);
end

--#endregion

--#region 多态
Object:SubClass("GameObject");
function GameObject:Move()
    print(1);
end

GameObject:SubClass("Player");
 function  Player:Move()
    print(2);
    --使用base必须用.调用,不能用:,参数要传self
    self.base.Move(self);
end

local obj = Player:new();
obj:Move();
