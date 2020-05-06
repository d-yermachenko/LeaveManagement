using System;
using System.Collections.Generic;
using System.Linq;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Controllers {
    public class LeaveTypesController : Controller {

        private readonly ILogger<LeaveTypesController> _Logger;

        private readonly Contracts.ILeaveTypeRepository _Repository;

        private readonly AutoMapper.IMapper _Mapper;

        private readonly IStringLocalizer _Localizer;

        public LeaveTypesController(Contracts.ILeaveTypeRepository repository,
            AutoMapper.IMapper mapper,
            ILogger<LeaveTypesController> logger,
            ILeaveManagementCustomLocalizerFactory localizerFactory) {
            _Repository = repository;
            _Mapper = mapper;
            _Logger = logger;
            _Localizer = localizerFactory.CreateStringLocalizer(typeof(LeaveTypesController));
        }

        #region Reading
        // GET: LeaveTypes
        public ActionResult Index() {
            List<Data.Entities.LeaveType> leaveTypes = _Repository.FindAll().ToList();
            var viewModel = _Mapper.Map<List<Data.Entities.LeaveType>, List<LeaveTypeNavigationViewModel>>(leaveTypes);
            return View(viewModel);
        }

        // GET: LeaveTypes/Details/5
        public ActionResult Details(int id) {
            var entry = _Repository.FindById(id);
            var viewModel = _Mapper.Map<Data.Entities.LeaveType, LeaveTypeNavigationViewModel>(entry);
            return View(viewModel);
        }
        #endregion

        #region Create

        // GET: LeaveTypes/Create
        public ActionResult Create() {
            return View();
        }

        // POST: LeaveTypes/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection) {
        //    try {
        //        Data.Entities.LeaveType leaveType = new Data.Entities.LeaveType() {
        //            Id = 0,
        //            LeaveTypeName = collection["LeaveTypeName"],
        //            DateCreated = DateTime.Now
        //        };
        //        _Repository.Create(leaveType);

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch {
        //        return View();
        //    }
        //}

        //POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LeaveTypeEditionViewModel creationModel) {
            try {
                if (!ModelState.IsValid || (creationModel?.LeaveTypeName?.Equals("Kick me!")??true)) {
                    ModelState.AddModelError("StateInvalid", _Localizer["Model state invalid"]);
                    ViewBag.Message = _Localizer["Model state invalid"];
                    _Logger.LogWarning("StateInvalid");
                    return View();
                }
                var createdLeaveType = _Mapper.Map<Data.Entities.LeaveType>(creationModel);
                createdLeaveType.DateCreated = DateTime.Now;
                if (!_Repository.Create(createdLeaveType)) {
                    ModelState.AddModelError("SavingFailed", _Localizer["Failed to save the leave type {0}", creationModel?.LeaveTypeName]);
                    ViewBag.Message = _Localizer["Model state invalid"];
                    _Logger.LogError("SavingFailed");
                    return View();
                };

                return RedirectToAction(nameof(Index));
            }
            catch (Exception error) {
                ModelState.AddModelError("ExceptionThrown", _Localizer["Something went wrong"]);
                _Logger.LogError(new EventId(error.HResult, error.ToString()), error.Message);
                ViewBag.Message = _Localizer["Model state invalid"];
                return View();
            }
        }
        #endregion

        #region Edit
        // GET: LeaveTypes/Edit/5
        public ActionResult Edit(int id) {
            try {
                var leaveType = _Repository.FindById(id);
                if (leaveType == null) {
                    var notFoundInfo = new LeaveTypeNotFoundViewModel(id, "Leave type");
                    return LeaveTypeNotFound(notFoundInfo);
                }
                var leaveTypeViewModel = _Mapper.Map<Data.Entities.LeaveType, LeaveTypeEditionViewModel>(leaveType);
                return View(leaveTypeViewModel);
            }
            catch (Exception e) {
                return Error(e);
            }

        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection) {
            try {
                if (!ModelState.IsValid) {
                    return View();
                }
                var leaveTypeToChange = _Repository.FindById(id);
                if (leaveTypeToChange == null) {
                    var notFoundInfo = new LeaveTypeNotFoundViewModel(id, "Leave type");
                    return LeaveTypeNotFound(notFoundInfo);
                }
                if (leaveTypeToChange != null) {
                    leaveTypeToChange.LeaveTypeName = collection["LeaveTypeName"];
                    if (!_Repository.Update(leaveTypeToChange)) {
                        ModelState.AddModelError("UpdateFailed", "Something went wrong");
                        return View();
                    }

                }

                return RedirectToAction(nameof(Index));
            }
            catch {
                return View();
            }
        }
        #endregion

        #region Remove
        // GET: LeaveTypes/Delete/5
        public ActionResult Delete(int id) {
            var instanceForDelete = _Repository.FindById(id);
            if (instanceForDelete == null) {
                LeaveTypeNotFoundViewModel notFoundView =
                    new LeaveTypeNotFoundViewModel(id, _Localizer["Leave Type"]);
                return LeaveTypeNotFound(notFoundView);
            }
            var leaveTypeViewModel = _Mapper.Map<LeaveTypeNavigationViewModel>(instanceForDelete);
            if (!ModelState.IsValid) {
                ModelState.AddModelError("ModelInvalid", "Model is not valid");
                return RedirectToAction(nameof(Index));
            }
            bool isSucceed = _Repository.Delete(instanceForDelete);
            if (!isSucceed) {
                ModelState.AddModelError("DeleteFailed", "Operation de suppression planté");
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

       
        #endregion

        #region Anomalies handling
        public ActionResult Error(Exception error) {
            return View();
        }


        public ActionResult LeaveTypeNotFound(LeaveTypeNotFoundViewModel notFoundMessage) {
            return View(notFoundMessage);
        }
        #endregion
    }
}