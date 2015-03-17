﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Cygnus
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                //routeTemplate: "api/{controller}/{action}/{id}",
                routeTemplate: "api/{controller}/{id}",
                //defaults: new { action = "all", id = RouteParameter.Optional }
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}