using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model;

namespace Skz.Ocean.Helper;

public static class SarkazHelper
{
    public static IEnumerable<ThoughtModel> _allThoughts;
    public static T ModelDbGet<T>() where T : AbstractModel
    {
        return ModelDb.GetById<T>(ModelDb.GetId<T>());
    }

    public static AbstractModel ModelDbGet(Type type)
    {
        MethodInfo method = typeof(ModelDb).GetMethod("Get",
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            new Type[] {typeof(Type)},
            null);
        return method.Invoke(null, new object[] {type}) as AbstractModel;
    }
    
    public static IEnumerable<ThoughtModel> GetAllThoughts()
    {
        // 获取 private static Get(Type type) 方法
        MethodInfo method = typeof(ModelDb).GetMethod("Get", 
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(Type) },  // 参数：Type
            null);
        return _allThoughts ?? (_allThoughts = from t in ModelDb.AllAbstractModelSubtypes 
            where t.IsSubclassOf(typeof(ThoughtModel)) 
            select (ThoughtModel)method.Invoke(null, new object[] {t})
        );
    }
}
