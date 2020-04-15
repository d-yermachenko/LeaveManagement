using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Controllers {
    public class LeaveTypesController : Controller {

        private readonly ILogger<LeaveTypesController> _Logger;

        private readonly Contracts.ILeaveTypeRepository _Repository;

        private readonly AutoMapper.IMapper _Mapper;

        public LeaveTypesController(Contracts.ILeaveTypeRepository repository, AutoMapper.IMapper mapper, ILogger<LeaveTypesController> logger) {
            _Repository = repository;
            _Mapper = mapper;
            _Logger = logger;
        }

        #region Reading
        // GET: LeaveTypes
        public ActionResult Index() {
            List<Data.Entities.LeaveType> leaveTypes = _Repository.FindAll().ToList();
            var viewModel = _Mapper.Map<List<Data.Entities.LeaveType>, List<Models.ViewModels.LeaveTypeNavigationViewModel>>(leaveTypes);
            return View(viewModel);
        }

        // GET: LeaveTypes/Details/5
        public ActionResult Details(int id) {
            var entry = _Repository.FindById(id);
            var viewModel = _Mapper.Map<Data.Entities.LeaveType, Models.ViewModels.LeaveTypeNavigationViewModel>(entry);
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
        public ActionResult Create(Models.ViewModels.LeaveTypeCreation creationModel) {
            try {
                if (!ModelState.IsValid) {
                    ModelState.AddModelError("StateInvalid", "Model state invalid");
                    return View();
                }
                var createdLeaveType = _Mapper.Map<Data.Entities.LeaveType>(creationModel);
                createdLeaveType.DateCreated = DateTime.Now;
                if (!_Repository.Create(createdLeaveType)) {
                    ModelState.AddModelError("SavingFailed", "Model state invalid");
                };

                return RedirectToAction(nameof(Index));
            }
            catch {
                ModelState.AddModelError("ExceptionThrown", "Something went wrong");
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
                    var notFoundInfo = new Models.ViewModels.LeaveTypeNotFoundViewModel(id, "Leave type");
                    return LeaveTypeNotFound(notFoundInfo);
                }
                var leaveTypeViewModel = _Mapper.Map<Data.Entities.LeaveType, Models.ViewModels.LeaveTypeCreation>(leaveType);
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
                var leaveTypeToChange= _Repository.FindById(id);
                if(leaveTypeToChange == null) {
                    var notFoundInfo = new Models.ViewModels.LeaveTypeNotFoundViewModel(id, "Leave type");
                    return LeaveTypeNotFound(notFoundInfo);
                }
                if(leaveTypeToChange != null) {
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

        #region Vulnerable edition
        public ActionResult VulnerableEdit(int id) {
            try {
                var leaveTypeModel = _Repository.FindById(id);
                if (leaveTypeModel == null) {
                    var notFoundData = new Models.ViewModels.LeaveTypeNotFoundViewModel(id, "Leave type");
                    return LeaveTypeNotFound(notFoundData);
                }
                var leaveTypeViewModel = _Mapper.Map<Models.ViewModels.LeaveTypeCreation>(leaveTypeModel);
                return VulnerableEdit(leaveTypeViewModel);
            }
            catch(Exception e) {
                ModelState.AddModelError("Exception", e.Message);
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VulnerableEdit(Models.ViewModels.LeaveTypeCreation leaveTypeViewModel) {
            try {
                if (!ModelState.IsValid)
                    return View(leaveTypeViewModel);
                Data.Entities.LeaveType leaveType = _Mapper.Map<Data.Entities.LeaveType>(leaveTypeViewModel);
                bool succeed = _Repository.Update(leaveType);
                if (!succeed) {
                    ModelState.AddModelError("UpdateError", "Writing to database failed");
                    return View(leaveTypeViewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch(Exception e) {
                ModelState.AddModelError("Exception", e.Message);
                return View(leaveTypeViewModel);
            }
        }

        #endregion

        #region Remove
        // GET: LeaveTypes/Delete/5
        public ActionResult Delete(int id) {
            return View();
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection) {
            try {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch {
                return View();
            }
        }
        #endregion

        #region Anomalies handling
        public ActionResult Error(Exception error) {
            return View();
        }
        

        public ActionResult LeaveTypeNotFound(Models.ViewModels.LeaveTypeNotFoundViewModel notFoundMessage) {
            return View(notFoundMessage);
        }
        #endregion
    }
}