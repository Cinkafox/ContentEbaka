using NLua;
using Console = System.Console;

namespace Content.LuaTest;

public static class Programm
{
    public static void Main(string[] args)
    {
        Lua lua = new Lua();
        var obj = new SomeObject();
        lua["TEST"] = obj;
        lua.DoFile("test.lua");
        var func = lua["Initialize"] as LuaFunction;
        func.Call();
        
        Console.WriteLine(obj.Sum);
    }
}

public sealed class SomeObject
{
    public void SayHello()
    {
        Console.WriteLine("Hello");
    }

    public int Sum = 15 + 15;
}