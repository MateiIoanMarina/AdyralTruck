using AdyralTruck.Data.Context;
using AdyralTruck.Data.Entity;
using AdyralTruck.Model.Furnizor;
using AdyralTruck.Models;
using AutoMapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AdyralTruck.Extensions;

namespace AdyralTruck.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext dataContext;
        private readonly IMapper mapper;

        public HomeController(ILogger<HomeController> logger, DataContext dataContext, IMapper mapper)
        {
            _logger = logger;
            this.dataContext = dataContext;
            this.mapper = mapper;
        }

        public IActionResult Index()
        {
            var furnizori = dataContext.Furnizori.Where(w => !w.Inactiv).ToList();
            var furnizoriViewModels = furnizori.Select(s => mapper.Map<FurnizorViewModel>(s)).ToList();

            ViewData["Furnizori"] = furnizoriViewModels;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AdaugaFurnizor()
        {
            return View();
        }

        public IActionResult DetaliiFurnizor(Guid furnizorId, bool isUpdate = false, bool isSuccess = false)
        {
            ViewData["FurnizorId"] = furnizorId.ToString();

            var furnizor = dataContext.Furnizori.Include(x => x.ContracteTransport).FirstOrDefault(f => f.FurnizorId == furnizorId && !f.Inactiv);

            if(furnizor != null)
            {
                ViewData["Furnizor"] = mapper.Map<FurnizorViewModel>(furnizor);
                ViewData["IsUpdate"] = isUpdate;
                ViewData["IsSuccess"] = isSuccess;

                return View(mapper.Map<FurnizorViewModel>(furnizor));
            }

            return View();
        }

        public IActionResult StergeFurnizor(Guid furnizorId)
        {
            var furnizor = dataContext.Furnizori.FirstOrDefault(f => f.FurnizorId == furnizorId && !f.Inactiv);

            if (furnizor != null)
            {
                furnizor.Inactiv = true;
                dataContext.Furnizori.Update(furnizor);
                dataContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult AdaugaFurnizor(FurnizorUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = mapper.Map<Furnizor>(model);
            dataContext.Add<Furnizor>(item);
            dataContext.SaveChanges(true);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditeazaFurnizor(FurnizorViewModel model)
        {
            if (ModelState["ContracteTransport"] != null) ModelState["ContracteTransport"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;

            if (!ModelState.IsValid)
            {
                return RedirectToAction("DetaliiFurnizor", new { @furnizorId = model.FurnizorId, @isUpdate = true });
            }

            var item = dataContext.Furnizori.Include(x => x.ContracteTransport).FirstOrDefault(q => q.FurnizorId == model.FurnizorId);

            if(item == null)
            {
                return RedirectToAction("DetaliiFurnizor", new { @furnizorId = model.FurnizorId });
            }

            item = mapper.Map(model, item);

            dataContext.Update<Furnizor>(item);
            dataContext.SaveChanges(true);

            return RedirectToAction("DetaliiFurnizor", new { @furnizorId = model.FurnizorId, @isUpdate = true, @isSuccess = true });
        }
    }
}