using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model;

namespace Skz.Ocean.Helper;

public static class SarkazHelper
{
    public static ThoughtModel ModelDbGet<T>() where T : ThoughtModel
    {
        // 获取 private static Get(Type type) 方法
        MethodInfo method = typeof(ModelDb).GetMethod("Get", 
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(Type) },  // 参数：Type
            null);
        return (ThoughtModel)method.Invoke(null, new object[] { typeof(T) });
    }
}