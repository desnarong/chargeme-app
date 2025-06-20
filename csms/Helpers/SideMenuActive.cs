using Microsoft.AspNetCore.Mvc.Rendering;

namespace csms.Helpers
{
    public static class SideMenuActive
    {
        public static string IsActive(this IHtmlHelper htmlHelper, string controller, string action)
        {
            var routeData = new RouteData();
            routeData = htmlHelper.ViewContext.RouteData;

            //var routeAction = "";
            var routeController = "";

            //var tmpAction = routeData.Values["action"];
            //if (tmpAction != null)
            //{
            //    routeAction = tmpAction.ToString();
            //}

            var tmpController = routeData.Values["controller"];
            if (tmpController != null)
            {
                routeController = tmpController.ToString();
            }

            //var returnActive = (controller == routeController && (action == routeAction || routeAction == "Details"));
            var returnActive = (controller == routeController);

            return returnActive ? "active" : "";
        }
    }
}
