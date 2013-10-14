using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models;
using Samson.Core;
using Samson.Models;

namespace Samson.Samples.Mvc.Controllers
{
    public class HomeController : BootstrapBaseController
    {
        private MinistryPlatformDataContext dataContext;

        public HomeController()
        {
            dataContext = new MinistryPlatformDataContext();
        }


        public ActionResult Index()
        {
           
            //var homeInputModels = _models;  
            var models = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetAllCongregations");                        
            return View(models);
        }

        [HttpPost]
        public ActionResult Create(Congregation model)
        {
            if (ModelState.IsValid)
            {
                dataContext.Create<Congregation>(model);
                Success("Your information was saved!");
                return RedirectToAction("Index");
            }
            Error("there were some errors in your form.");
            return View(model);
        }

        public ActionResult Create()
        {
            return View(new Congregation());
        }

        public ActionResult Delete(int Congregation_ID)
        {
            //_models.Remove(_models.Get(id));
            Information("You're not allowed to delete... or are you?? :)");
            //if(_models.Count==0)
            //{
            //    Attention("You have deleted all the models! Create a new one to continue the demo.");
            //}
            return RedirectToAction("index");
        }
        public ActionResult Edit(int id)
        {
            var model = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetSingleCongregation", new { CongregationID = id }).SingleOrDefault();
            return View("Create", model);
        }
        [HttpPost]
        public ActionResult Edit(Congregation model, int id)
        {
            if(ModelState.IsValid)
            {
                //_models.Remove(_models.Get(id));
                //model.Id = id;
                //_models.Add(model);
                var oldModel = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetSingleCongregation", new { CongregationID = id }).SingleOrDefault();
                dataContext.Update<Congregation>(oldModel, model);
                Success("The model was updated!");
                return RedirectToAction("index");
            }
            return View("Create", model);
        }

        public ActionResult Details(int id)
        {
            var model = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetSingleCongregation", new { CongregationID = id }).SingleOrDefault();
            return View(model);
        }


        internal ActionResult Admin()
        {
            // used for demonstrationg route filters
            throw new NotImplementedException();
        }
    }
}
