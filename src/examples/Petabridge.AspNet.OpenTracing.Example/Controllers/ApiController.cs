// -----------------------------------------------------------------------
//   <copyright file="ApiController.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2023 .NET Petabridge, LLC
//   </copyright>
// -----------------------------------------------------------------------

using System.Web.Http.Results;
using System.Web.Mvc;

namespace Petabridge.AspNet.OpenTracing.Example.Controllers
{
    public class ApiController: Controller
    {
        public void Data()
        {
            Response.Redirect("http://localhost:5000/Api/Result");
        }

        public ActionResult Result()
        {
            return new JsonResult
            {
                Data = new { result = "OK" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}