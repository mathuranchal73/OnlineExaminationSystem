using System.Web;
using System.Web.Mvc;

namespace A6_Performing_CRUD_Using_Angular
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
