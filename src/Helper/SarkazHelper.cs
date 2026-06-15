using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model;

namespace Skz.Ocean.Helper;

public static class SarkazHelper
{
    public static T ModelDbGet<T>() where T : ThoughtModel
    {
        return ModelDb.GetById<T>(ModelDb.GetId<T>());
    }
}
