using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AdyralTruck.Extensions
{
    public static class ControllerExtensions
    {

        #region Set Message

        public static void SetMessage<T>(this T controller, string type, string message) where T : Controller
        {

            if (!IsMessageSet(controller))
            {
                controller.TempData["MessageType"] = type;
                controller.TempData["Message"] = message;
            }
        }

        public static bool IsMessageSet<T>(this T controller) where T : Controller
        {
            return controller.TempData["Message"] != null;
        }

        #endregion

        #region Distinct By
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        #endregion
    }
}
