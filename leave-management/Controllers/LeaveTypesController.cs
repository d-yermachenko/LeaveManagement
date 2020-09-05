using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using LeaveManagement.Code;
using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.ViewModels.LeaveType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Controllers {

    [MiddlewareFilter(typeof(LocalizationPipeline))]
    [Authorize]
    public class LeaveTypesController : Controller {

        private readonly ILogger<LeaveTypesController> _Logger;

        private readonly Contracts.ILeaveTypeRepositoryAsync _Repository;

        private readonly AutoMapper.IMapper _Mapper;

        private readonly ILeaveManagementCustomLocalizerFactory LocalizerFactory;

        private readonly IStringLocalizer ControllerLocalizer;

        private readonly SignInManager<IdentityUser> _SignInManager;


        public LeaveTypesController(Contracts.ILeaveTypeRepositoryAsync repository,
            AutoMapper.IMapper mapper,
            ILogger<LeaveTypesController> logger,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            SignInManager<IdentityUser> signInManager) {
            _Repository = repository;
            _Mapper = mapper;
            _Logger = logger;
            LocalizerFactory = localizerFactory;
            ControllerLocalizer = localizerFactory.Create(typeof(LeaveTypesController));
            _SignInManager = signInManager;
        }

        #region Reading
        // GET: LeaveTypes
        public async Task<ActionResult> Index() {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            List<Data.Entities.LeaveType> leaveTypes = (await _Repository.FindAllAsync()).ToList();
            var viewModel = _Mapper.Map<List<Data.Entities.LeaveType>, List<LeaveTypeNavigationViewModel>>(leaveTypes);
            return View(viewModel);
        }

        // GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int id) {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            var entry = await _Repository.FindByIdAsync(id);
            if (entry == null) {
                string errorMessage = ControllerLocalizer["Impossible to find leave type #{0}. Please check the adress", id];
                return StatusCode(StatusCodes.Status404NotFound, errorMessage);
            }
            var viewModel = _Mapper.Map<Data.Entities.LeaveType, LeaveTypeNavigationViewModel>(entry);
            viewModel.AuthorLastName = $"{entry.Author?.Title} {entry.Author?.FirstName} {entry.Author?.LastName}";
            return View(viewModel);
        }
        #endregion

        #region Create

        // GET: LeaveTypes/Create
        public async Task<ActionResult> Create() {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            return View();
        }


        //POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeEditionViewModel creationModel) {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            string newName = string.Empty;
            try {
                if (!ModelState.IsValid) {
                    HomeController.DisplayProblem(_Logger, this, ControllerLocalizer["Model state invalid"],
                        ControllerLocalizer[ModelState.ValidationState.ToString()]);
                    return View(creationModel);
                }
                newName = creationModel.LeaveTypeName;
                var leaveTypes =  (await _Repository.FindAllAsync()).Where(x => x.LeaveTypeName?.Equals(newName) ?? false);
                if (leaveTypes?.Count() > 0) {
                    HomeController.DisplayProblem(_Logger, this, ControllerLocalizer["Failed to create leave type '{0}'.", newName],
                        ControllerLocalizer["Leave type with name {0} already exists", newName]);
                    return View(creationModel);
                }
                var createdLeaveType = _Mapper.Map<Data.Entities.LeaveType>(creationModel);
                createdLeaveType.DateCreated = DateTime.Now;
                createdLeaveType.AuthorId = (await _SignInManager.UserManager.FindByNameAsync(User.Identity.Name)).Id;
                if (!(await _Repository.CreateAsync(createdLeaveType))) {
                    string errorTitle = ControllerLocalizer["Saving failed"];
                    string errorMessage = ControllerLocalizer["Failed to save leave type '{0}' in repositary", newName];
                    HomeController.DisplayProblem(_Logger, this, errorTitle, errorMessage);
                    return View(creationModel);
                };
                return RedirectToAction(nameof(Index));
            }
            catch(DbException dbException) {
                string errorTitle = ControllerLocalizer["Saving failed"];
                string errorMessage = ControllerLocalizer["Repository error while saving leave type '{0}' in repositary", newName];
                HomeController.DisplayProblem(_Logger, this, errorTitle, errorMessage, dbException);
                return View(creationModel);
            }
            catch (Exception error) {
                string errorTitle = ControllerLocalizer["Saving failed"];
                string errorMessage = ControllerLocalizer["Unidentified error while saving leave type '{0}' in repositary", newName];
                HomeController.DisplayProblem(_Logger, this, errorTitle, errorMessage, error);
                return View(creationModel);
            }
        }
        #endregion

        #region Edit
        // GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int id) {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            try {
                var leaveType = await _Repository.FindByIdAsync(id);
                if (leaveType == null) {
                    return NotFound();
                }
                var leaveTypeViewModel = _Mapper.Map<Data.Entities.LeaveType, LeaveTypeEditionViewModel>(leaveType);
                return View(leaveTypeViewModel);
            }
            catch (DbException error) {
                HomeController.DisplayProblem(_Logger, this, ControllerLocalizer[error.ToString()], error.Message);
                return View();
            }
            catch (Exception error) {
                _Logger.LogError(new EventId(error.HResult, error.ToString()), error.Message);
                ViewBag.Message = ControllerLocalizer[error.ToString()] + "\n" + error.Message;
                return View();
            }

        }

        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, LeaveTypeEditionViewModel editionViewModel) {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            string newName = string.Empty;
            try {
                if (!ModelState.IsValid) {
                    return View();
                }
                var leaveTypeToChange = await _Repository.FindByIdAsync(id);
                if (leaveTypeToChange == null) {
                    return NotFound(ControllerLocalizer["Leave type {0} not found", id]);
                }
                newName = editionViewModel.LeaveTypeName;
                var leaveTypes = (await _Repository.FindAllAsync()).Where(x => x.LeaveTypeName?.Equals(newName) ??false &&
                x.Id != id);
                leaveTypeToChange.LeaveTypeName = newName;
                leaveTypeToChange.DefaultDays = editionViewModel.DefaultDays;
                if (!(await _Repository.UpdateAsync(leaveTypeToChange))) {
                    string errorMessage = ControllerLocalizer["Failed to rename this leave type to {0}", newName];
                    ModelState.AddModelError("UpdateFailed", errorMessage);
                    HomeController.DisplayProblem(_Logger, this, ControllerLocalizer["Leave type with name {0} already exists", newName],
                        ControllerLocalizer["Leave type with name {0} already exists", newName]);
                    return View();
                }
                return RedirectToAction(nameof(Index));
            }
            catch(DbUpdateException updateError) {
                HomeController.DisplayProblem(_Logger, this, ControllerLocalizer[updateError.ToString()],
                    updateError.Message);
                return View();
            }
            catch (Exception exception) {
                HomeController.DisplayProblem(_Logger, this, ControllerLocalizer[exception.ToString()],
                    exception.Message);
                return View();
            }
        }
        #endregion

        #region Remove
        // GET: LeaveTypes/Delete/5
        public async Task<ActionResult> Delete(int id) {
            if ((await _SignInManager.UserManager?.IsPrivelegedUser(User)) != true)
                return Forbid();
            try {
                var instanceToDelete = await _Repository.FindByIdAsync(id);
                if (instanceToDelete == null) {
                    string errorMessage = ControllerLocalizer["Leave type with number {0} was not found", id];
                    string errorTitle = ControllerLocalizer["Leave type not found"];
                    HomeController.DisplayProblem(_Logger, this, errorMessage, errorTitle);
                    return View();
                }
                var leaveTypeViewModel = _Mapper.Map<LeaveTypeNavigationViewModel>(instanceToDelete);
                bool isSucceed = await _Repository.DeleteAsync(instanceToDelete);
                if (!isSucceed) {
                    string errorMessage = ControllerLocalizer["Can't delete leave type with id {0}", id];
                    string errorTitle = ControllerLocalizer["Can't delete leave type"];
                    HomeController.DisplayProblem(_Logger, this, errorMessage, errorTitle);
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            catch(DbException dbException) {
                string errorMessage = ControllerLocalizer["Can't delete leave type with id {0}", id];
                string errorTitle = ControllerLocalizer["Can't delete leave type"];
                HomeController.DisplayProblem(_Logger, this, errorMessage, errorTitle, dbException);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}