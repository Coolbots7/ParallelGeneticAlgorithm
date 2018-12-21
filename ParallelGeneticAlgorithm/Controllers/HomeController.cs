using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ParallelGeneticAlgorithm.Controllers
{
    [NoCache]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        //public JsonResult _getCurrentGeneration()
        //{
        //    int generation = this.standardGeneticAlgorithm.GetGeneration();
        //    return Json(generation, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult _startNetwork()
        //{
        //    bool success = this.standardGeneticAlgorithm.Start();
        //    return Json(success, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult _stopNetwork()
        //{
        //    bool success = this.standardGeneticAlgorithm.Stop();
        //    return Json(success, JsonRequestBehavior.AllowGet);
        //}

        //public JsonResult _getRuntime()
        //{
        //    long runtime = this.standardGeneticAlgorithm.GetRuntime();
        //    return Json(runtime, JsonRequestBehavior.AllowGet);
        //}
    }
}
