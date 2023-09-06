using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdyralTruck.Controllers.Extensions
{
    public class NavigationLocationFilterAttribute : ActionFilterAttribute
    {
        public string CurrentLocation { get; set; }

        public NavigationLocationFilterAttribute(string currentLocation)
        {
            CurrentLocation = currentLocation;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = (Controller)filterContext.Controller;
            controller.ViewData.Add("NavigationLocation", CurrentLocation);
        }
    }
}
