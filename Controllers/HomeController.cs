using AdyralTruck.Data.Context;
using AdyralTruck.Data.Entity;
using AdyralTruck.Model.Furnizor;
using AdyralTruck.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AdyralTruck.Model.ContractTransport;
using Rotativa.AspNetCore;
using AdyralTruck.Utility;
using Microsoft.AspNetCore.Authorization;
using AdyralTruck.Model.ComandaTransport;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AdyralTruck.Controllers.Extensions;

namespace AdyralTruck.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext dataContext;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, DataContext dataContext, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            this.dataContext = dataContext;
            this.mapper = mapper;
            this.hostingEnvironment = hostingEnvironment;
        }

        #region Furnizori

        [NavigationLocationFilter("Index")]
        public async Task<IActionResult> Index(string search = null)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var furnizori = dataContext.Furnizori.Where(w => !w.Inactiv && (search == null || w.Email.ToLower().Contains(search.ToLower()) || w.Nume.ToLower().Contains(search.ToLower()))).Include(x => x.ContracteTransport).ToList();
            var furnizoriViewModels = furnizori.Select(s => mapper.Map<FurnizorViewModel>(s)).ToList();

            ViewData["Furnizori"] = furnizoriViewModels;

            return View();
        }

        public IActionResult Privacy()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult AdaugaFurnizor()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult DetaliiFurnizor(Guid furnizorId, bool isUpdate = false, bool isSuccess = false)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["FurnizorId"] = furnizorId.ToString();

            var furnizor = dataContext.Furnizori.Include(x => x.ContracteTransport.Where(w => !w.Sters).OrderByDescending(o => o.NumarContract)).FirstOrDefault(f => f.FurnizorId == furnizorId && !f.Inactiv);

            if (furnizor != null)
            {
                ViewData["Furnizor"] = mapper.Map<FurnizorViewModel>(furnizor);
                ViewData["IsUpdate"] = isUpdate;
                ViewData["IsSuccess"] = isSuccess;
                var model = mapper.Map<FurnizorViewModel>(furnizor);
                return View(model);
            }

            return View();
        }

        public IActionResult StergeFurnizor(Guid furnizorId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var furnizor = dataContext.Furnizori.FirstOrDefault(f => f.FurnizorId == furnizorId && !f.Inactiv);

            if (furnizor != null)
            {
                furnizor.Inactiv = true;
                dataContext.Furnizori.Update(furnizor);
                dataContext.SaveChanges(true);
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult AdaugaFurnizor(FurnizorUpdateViewModel model)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var item = mapper.Map<Furnizor>(model);
            dataContext.Add(item);
            dataContext.SaveChanges(true);

            return RedirectToAction("DetaliiFurnizor", new { @furnizorId = item.FurnizorId });
        }

        [HttpPost]
        public ActionResult EditeazaFurnizor(Guid furnizorId, FurnizorUpdateViewModel model)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if(furnizorId == Guid.Empty)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("DetaliiFurnizor", new { @furnizorId = furnizorId, @isUpdate = true });
            }

            var item = dataContext.Furnizori.FirstOrDefault(q => q.FurnizorId == furnizorId);

            if (item == null)
            {
                return RedirectToAction("DetaliiFurnizor", new { @furnizorId = furnizorId });
            }

            item = mapper.Map(model, item);

            dataContext.Update(item);
            dataContext.SaveChanges(true);

            return RedirectToAction("DetaliiFurnizor", new { @furnizorId = furnizorId, @isUpdate = true, @isSuccess = true });
        }

        #endregion

        #region Comenzi de Transport

        public async Task<IActionResult> DetaliiComandaTransportAsync(Guid contractTransportId, Guid? comandaTransportId = null, bool isUpdate = false, bool isSuccess = false)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (comandaTransportId.HasValue && comandaTransportId.Value != Guid.Empty)
            {
                ViewData["ComandaTransportId"] = comandaTransportId.ToString();

                var comandaTransport = dataContext.ComandaTransport.Include(x => x.ContractTransport).ThenInclude(x => x.Furnizor).FirstOrDefault(f => f.ComandaTransportId == comandaTransportId && !f.Inactiv);

                if (comandaTransport != null)
                {
                    var comandaTransportViewModel = mapper.Map<ComandaTransportViewModel>(comandaTransport);
                    ViewData["ComandaTransport"] = comandaTransportViewModel;
                    ViewData["IsUpdate"] = isUpdate;
                    ViewData["IsSuccess"] = isSuccess;
                    ViewData["IsCreated"] = true;
                    return View(comandaTransportViewModel);
                }
            }
            else
            {
                var model = new ComandaTransportViewModel();
                var contractTransport = dataContext.ContracteTransport
                    .Include(x => x.ComenziTransport)
                    .FirstOrDefault(q => q.ContractTransportId == contractTransportId);

                model.ContractTransport = mapper.Map<ContractTransportViewModel>(contractTransport);
                model.ContractTransportId = contractTransportId;
                model.CantitateLipsa = 40;
                model.NumarCurse = 1;

                var comenziContract = await dataContext.ComandaTransport.Where(w => !w.Inactiv && w.ContractTransportId == model.ContractTransportId).CountAsync();

                model.NumarComanda = comenziContract + 1;
                model.DataComanda = DateTime.Now;
                model.DataIncarcare = DateTime.Now;
                model.DetaliiPlata = "se va face la data primirii documentelor in original";

                var comandaAnterioara = await dataContext.ComandaTransport.Where(w => w.ContractTransportId == contractTransportId && w.NumarComanda == model.NumarComanda - 1).FirstOrDefaultAsync();
                if(comandaAnterioara != null)
                {
                    model.DetaliiSofer = comandaAnterioara.DetaliiSofer;
                }

                ViewData["ComandaTransport"] = model;
                ViewData["IsUpdate"] = isUpdate;
                ViewData["IsSuccess"] = isSuccess;
                ViewData["IsCreated"] = false;
                return View(model);
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditeazaComandaTransport(ComandaTransportUpdateModel model)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

           
            if (!ModelState.IsValid)
            {
                return RedirectToAction("DetaliiComandaTransport", new { @contractTransportId = model.ContractTransportId, @isUpdate = true, @isSuccess = false });
            }

            var item = dataContext.ComandaTransport.Include(x => x.ContractTransport).FirstOrDefault(q => q.ComandaTransportId == model.ComandaTransportId);

            if (item == null)
            {
                item = mapper.Map(model, item);
                item.ContractTransport = null;
                item.ContractTransportId = model.ContractTransportId;
                await dataContext.ComandaTransport.AddAsync(item);
                await dataContext.SaveChangesAsync(true);

                return RedirectToAction("DetaliiComandaTransport", new { @contractTransportId = model.ContractTransportId, @comandaTransportId = item.ComandaTransportId, @isSuccess = true });
            }

            model.EmailTrimisFurnizor = item.EmailTrimisFurnizor;
            item = mapper.Map(model, item);

            dataContext.Update(item);
            await dataContext.SaveChangesAsync(true);

            return RedirectToAction("DetaliiComandaTransport", new { comandaTransportId = model.ComandaTransportId, @isSuccess = true, @isUpdate = true });
        }

        #endregion

        #region Contracte de Transport 

        public async Task<IActionResult> DetaliiContractTransportAsync(Guid furnizorId, Guid? contractTransportId = null,bool isUpdate = false, bool isSuccess = false)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (contractTransportId.HasValue && contractTransportId.Value != Guid.Empty)
            {
                ViewData["ContractTransportId"] = contractTransportId.Value.ToString();

                var contractTransport = dataContext.ContracteTransport.Include(x => x.ComenziTransport.Where(w => !w.Inactiv).OrderByDescending(o => o.NumarComanda)).Include(x => x.Furnizor).FirstOrDefault(f => f.ContractTransportId == contractTransportId.Value && !f.Sters);

                if (contractTransport != null)
                {
                    var contractTransportViewModel = mapper.Map<ContractTransportViewModel>(contractTransport);
                    ViewData["ContractTransport"] = contractTransportViewModel;
                    ViewData["IsUpdate"] = isUpdate;
                    ViewData["IsSuccess"] = isSuccess;
                    return View(contractTransportViewModel);
                }
            }
            else
            {
                var model = new ContractTransportViewModel();
                var furnizor = dataContext.Furnizori.FirstOrDefault(q => q.FurnizorId == furnizorId);
                model.Furnizor = mapper.Map<FurnizorViewModel>(furnizor);
                model.FurnizorId = furnizorId;

                var furnizorContracte = await dataContext.ContracteTransport.Where(w => !w.Sters && w.FurnizorId == model.FurnizorId).AnyAsync() ? 
                    await dataContext.ContracteTransport
                    .Where(w => !w.Sters && w.FurnizorId == model.FurnizorId)
                    .Select(s => s.NumarContract)
                    .MaxAsync() : 0;

                model.NumarContract = furnizorContracte + 1;
                model.DataContract = DateTime.Now;
                model.Created = DateTime.Now;

                ViewData["ContractTransport"] = model;
                ViewData["IsUpdate"] = isUpdate;
                ViewData["IsSuccess"] = isSuccess;
                return View(model);
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditeazaContractTransport(ContractTransportUpdateModel model)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            

            var item = dataContext.ContracteTransport.Include(x => x.Furnizor).FirstOrDefault(q => q.ContractTransportId == model.ContractTransportId);

            if (item == null)
            {
                
                item = mapper.Map(model, item);
                item.Created = DateTime.Now;
                item.FurnizorId = model.FurnizorId;
                await dataContext.ContracteTransport.AddAsync(item);

                await dataContext.SaveChangesAsync(true);

                var previousContractTransports = await dataContext.ContracteTransport
                    .Where(w => !w.Sters && !w.Inactiv && w.FurnizorId == model.FurnizorId && w.ContractTransportId != item.ContractTransportId)
                    .ToListAsync();

                if (previousContractTransports.Any())
                {
                    previousContractTransports.ForEach(f => f.Inactiv = true);
                    dataContext.ContracteTransport.UpdateRange(previousContractTransports);
                }

                await dataContext.SaveChangesAsync(true);

                return RedirectToAction("DetaliiContractTransport", new { furnizorId = model.FurnizorId, @contractTransportId = item.ContractTransportId });
            }

            item = mapper.Map(model, item);

            dataContext.Update(item);
            await dataContext.SaveChangesAsync(true);

            return RedirectToAction("DetaliiContractTransport", new { @contractTransportId = model.ContractTransportId, @isSuccess = true, @isUpdate = true });
        }

        public IActionResult StergeContractTransport(Guid furnizorId, Guid contractTransportId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var contractTransport = dataContext.ContracteTransport.FirstOrDefault(f => f.ContractTransportId == contractTransportId);

            if (contractTransport != null)
            {
                contractTransport.Sters = true;
                dataContext.ContracteTransport.Update(contractTransport);
                dataContext.SaveChanges(true);
            }

            return RedirectToAction("DetaliiFurnizor", new { @furnizorId = furnizorId });
        }

        public IActionResult StergeComandaTransport(Guid furnizorId, Guid comandaTransportId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var comandaTransport = dataContext.ComandaTransport.FirstOrDefault(f => f.ComandaTransportId == comandaTransportId && !f.Inactiv);

            if (comandaTransport != null)
            {
                comandaTransport.Inactiv = true;
                dataContext.ComandaTransport.Update(comandaTransport);
                dataContext.SaveChanges(true);
            }

            return RedirectToAction("DetaliiContractTransport", new { @furnizorId = furnizorId, @contractTransportId = comandaTransport.ContractTransportId });
        }

        #endregion

        #region Contract de Transport PDF Export

        public async Task<IActionResult> ContractTransportPdfReportDownloadAsync(Guid contractTransportId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var contractTransport = await dataContext.ContracteTransport.Include(x => x.Furnizor).FirstOrDefaultAsync(x => x.ContractTransportId == contractTransportId);

            if (contractTransport == null)
            {
                //??
            }

            var model = mapper.Map<ContractTransportViewModel>(contractTransport);

            string fileDownloadName = string.Format("Contract Transport {0} - {1} - {2}", model.Furnizor?.Nume, model.NumarContract, model.DataContract.ToString("dd/MM/yyyy"));

            var result = new Rotativa.AspNetCore.ViewAsPdf("ContractTransportPdfReport", model)
            {
                FileName = string.Format("{0}.pdf", fileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            return result;
        }

        public async Task<IActionResult> ComandaTransportPdfReportDownloadAsync(Guid comandaTransportId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var comandaTransport = await dataContext.ComandaTransport.Include(x => x.ContractTransport).ThenInclude(x => x.Furnizor).FirstOrDefaultAsync(x => x.ComandaTransportId == comandaTransportId);

            if (comandaTransport == null)
            {
                return RedirectToAction("DetaliiComandaTransport", "Home", new { @contractTransportId = comandaTransport?.ContractTransportId, @comandaTransportId = comandaTransportId });
            }

            var model = mapper.Map<ComandaTransportViewModel>(comandaTransport);

            string fileDownloadName = string.Format("Comanda Transport {0} - {1} - {2}", model.ContractTransport?.Furnizor?.Nume, model.NumarComanda, model.DataComanda.ToString("dd/MM/yyyy"));

            //var result = new Rotativa.AspNetCore.ViewAsPdf("ComandaTransportPdfReport", model)
            //{
            //    FileName = string.Format("{0}.pdf", fileDownloadName),
            //    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
            //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
            //    IsLowQuality = true,
            //    MinimumFontSize = 14,
            //    IsJavaScriptDisabled = true,
            //    ContentType = "application/pdf",
            //    //CustomSwitches = "--disable-smart-shrinking"
            //};

            var result = new Rotativa.AspNetCore.ViewAsPdf("ComandaTransportPdfReport", model)
            {
                FileName = string.Format("{0}.pdf", fileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            return result;
        }

        public async Task<IActionResult> TrimiteContractTransportEmail(Guid comandaTransportId, Guid furnizorId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var comandaTransport = await dataContext.ComandaTransport.Include(x => x.ContractTransport).ThenInclude(x => x.Furnizor).FirstOrDefaultAsync(x => x.ComandaTransportId == comandaTransportId);

            if (comandaTransport == null)
            {
                return new JsonResult(new { @success = false });
            }

            var model = mapper.Map<ComandaTransportViewModel>(comandaTransport);

            var furnizor = await dataContext.Furnizori.Where(w => w.FurnizorId == furnizorId).FirstOrDefaultAsync();

            string comandaFileDownloadName = string.Format("Comanda Transport {0} - {1} - {2}.pdf", model.ContractTransport.Furnizor?.Nume, model.NumarContract, model.DataContract.ToString("dd/MM/yyyy"));

            var comandaResult = new Rotativa.AspNetCore.ViewAsPdf("ComandaTransportPdfReport", model)
            {
                FileName = string.Format("{0}.pdf", comandaFileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            var byteArrayComanda = await comandaResult.BuildFile(ControllerContext);
            var base64StringComanda = Convert.ToBase64String(byteArrayComanda);

            string contractFileDownloadName = string.Format("Contract Transport {0} - {1}.pdf", model.NumarContract, model.DataContract.ToString("dd/MM/yyyy"));

            var contractResult = new Rotativa.AspNetCore.ViewAsPdf("ContractTransportPdfReport", model.ContractTransport)
            {
                FileName = string.Format("{0}.pdf", contractFileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            var byteArrayContract = await contractResult.BuildFile(ControllerContext);
            var base64StringContract = Convert.ToBase64String(byteArrayContract);

            var firstLine = "<p>Buna ziua,</p>";
            var secondLine = $"<p>Atasez documentele in vederea transportului.<p>";
            var thirdLine = $"<p>Va rugam sa le retransmiteti semnate si stampilate pe aceasta adresa de e-mail.</p>" +
                $"<p>Documentele originale va rugam sa le transmiteti la adresa de corespondenta <strong>Sos. De Centura nr. 16, Com. Dobroesti, jud. Ilfov, DOAR PRIN CURIER.</strong></p> <br/>";
            var fourthLine = "<p>Va multumim,</p><p>Adyral Truck S.R.L.</p>";

            var htmlMessage = firstLine + secondLine + thirdLine + fourthLine;

            await EmailUtility.SendEmail(
                htmlMessage, 
                $"Contract de Transport - {model.ContractTransport?.Furnizor?.Nume} Nr. {model.NumarContract}", 
                base64StringComanda, 
                comandaFileDownloadName, 
                base64StringContract,
                contractFileDownloadName,
                model.ContractTransport?.Furnizor?.Email);

            comandaTransport.EmailTrimisFurnizor = true;
            dataContext.ComandaTransport.Update(comandaTransport);
            await dataContext.SaveChangesAsync(true);

            return new JsonResult(new { @success = true });
        }

        public async Task<IActionResult> TrimiteContractTransportEmailuri(Guid[] ids)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if(ids == null)
            {
                return new JsonResult(new { @success = false });
            }

            var comenziTransport = await dataContext.ComandaTransport.Include(x => x.ContractTransport).ThenInclude(x => x.Furnizor).Where(x => ids.Contains(x.ComandaTransportId)).ToListAsync();

            if (comenziTransport == null || !comenziTransport.Any())
            {
                return new JsonResult(new { @success = false });
            }

            var models = comenziTransport.Select(s => mapper.Map<ComandaTransportViewModel>(s)).ToList();

            var contract = models.First().ContractTransport;

            var furnizor = await dataContext.Furnizori.Where(w => w.FurnizorId == contract.FurnizorId).FirstOrDefaultAsync();

            string comandaFileDownloadName = string.Format("Comanda Transport {0} - {1} - {2}.pdf", contract.Furnizor?.Nume, contract.NumarContract, contract.DataContract.ToString("dd/MM/yyyy"));

            var base64StringsComenzi = new List<string>();

            foreach(var model in models)
            {
                var comandaResult = new Rotativa.AspNetCore.ViewAsPdf("ComandaTransportPdfReport", model)
                {
                    FileName = string.Format("{0}.pdf", comandaFileDownloadName),
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    IsLowQuality = true,
                    MinimumFontSize = 14,
                    IsJavaScriptDisabled = true,
                    WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
                };

                var byteArrayComanda = await comandaResult.BuildFile(ControllerContext);
                var base64StringComanda = Convert.ToBase64String(byteArrayComanda);
                base64StringsComenzi.Add(base64StringComanda);
            }

            string contractFileDownloadName = string.Format("Contract Transport {0} - {1}.pdf", contract.NumarContract, contract.DataContract.ToString("dd/MM/yyyy"));

            var contractResult = new Rotativa.AspNetCore.ViewAsPdf("ContractTransportPdfReport", contract)
            {
                FileName = string.Format("{0}.pdf", contractFileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            var byteArrayContract = await contractResult.BuildFile(ControllerContext);
            var base64StringContract = Convert.ToBase64String(byteArrayContract);

            var firstLine = "<p>Buna ziua,</p>";
            var secondLine = $"<p>Atasez documentele in vederea transportului.<p>";
            var thirdLine = $"<p>Va rugam sa le retransmiteti semnate si stampilate pe aceasta adresa de e-mail.</p>" +
                $"<p>Documentele originale va rugam sa le transmiteti la adresa de corespondenta <strong>Sos. De Centura nr. 16, Com. Dobroesti, jud. Ilfov, DOAR PRIN CURIER.</strong></p> <br/>";
            var fourthLine = "<p>Va multumim,</p><p>Adyral Truck S.R.L.</p>";

            var htmlMessage = firstLine + secondLine + thirdLine + fourthLine;

            await EmailUtility.SendEmailuri(
                htmlMessage,
                $"Contract de Transport - {contract?.Furnizor?.Nume} Nr. {contract.NumarContract}",
                base64StringsComenzi,
                comandaFileDownloadName,
                base64StringContract,
                contractFileDownloadName,
                contract?.Furnizor?.Email);

            comenziTransport.ForEach(item => { item.EmailTrimisFurnizor = true; });
            dataContext.ComandaTransport.UpdateRange(comenziTransport);
            await dataContext.SaveChangesAsync(true);

            return new JsonResult(new { @success = true });
        }

        public async Task<IActionResult> TrimiteDoarContractTransportEmail(Guid contractTransportId, Guid furnizorId)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var contractTransport = await dataContext.ContracteTransport.Include(x => x.Furnizor).FirstOrDefaultAsync(x => x.ContractTransportId == contractTransportId);

            if (contractTransport == null)
            {
                return new JsonResult(new { @success = false });
            }

            var model = mapper.Map<ContractTransportViewModel>(contractTransport);

            var furnizor = await dataContext.Furnizori.Where(w => w.FurnizorId == furnizorId).FirstOrDefaultAsync();

            string contractFileDownloadName = string.Format("Contract Transport {0} - {1}.pdf", model.NumarContract, model.DataContract.ToString("dd/MM/yyyy"));

            var contractResult = new Rotativa.AspNetCore.ViewAsPdf("ContractTransportPdfReport", model)
            {
                FileName = string.Format("{0}.pdf", contractFileDownloadName),
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                IsLowQuality = true,
                MinimumFontSize = 14,
                IsJavaScriptDisabled = true,
                WkhtmlPath = Path.Combine(hostingEnvironment.WebRootPath, "Rotativa"),
            };

            var byteArrayContract = await contractResult.BuildFile(ControllerContext);
            var base64StringContract = Convert.ToBase64String(byteArrayContract);

            var firstLine = "<p>Buna ziua,</p>";
            var secondLine = $"<p>Atasez contractul in vederea transportului.<p>";
            var thirdLine = $"<p>Va rugam sa il retransmiteti semnat si stampilat pe aceasta adresa de e-mail.</p>" +
                $"<p>Documentele originale va rugam sa le transmiteti la adresa de corespondenta <strong>Sos. De Centura nr. 16, Com. Dobroesti, jud. Ilfov, DOAR PRIN CURIER.</strong></p> <br/>";
            var fourthLine = "<p>Va multumim,</p><p>Adyral Truck S.R.L.</p>";

            var htmlMessage = firstLine + secondLine + thirdLine + fourthLine;

            await EmailUtility.SendDoarContractEmail(
                htmlMessage,
                $"Contract de Transport - {model.Furnizor?.Nume} Nr. {model.NumarContract}",
                base64StringContract,
                contractFileDownloadName,
                model.Furnizor?.Email);

            contractTransport.EmailTrimisFurnizor = true;
            contractTransport.Inactiv = false;
            dataContext.ContracteTransport.Update(contractTransport);
            await dataContext.SaveChangesAsync(true);

            return new JsonResult(new { @success = true });
        }

        #endregion
    }
}