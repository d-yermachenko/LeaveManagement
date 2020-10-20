using LeaveManagement.Code.CustomLocalization;
using LeaveManagement.Data.Entities;
using LeaveManagement.PasswordGenerator;
using LeaveManagement.Repository.Entity;
using LeaveManagement.ViewModels.Company;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagement.Controllers {
    public class TestExamplesController : Controller {
        private readonly Contracts.ILeaveManagementUnitOfWork _UnitOfWork;
        private readonly UserManager<IdentityUser> _UserManager;
        private readonly IStringLocalizer _StringLocalizer;
        private readonly IPasswordGenerator _PasswordGenerator;
        private readonly IEmailSender _EmailSender;
        private readonly int _FemaileNamesStartIndex;

        private const string CompanyRegistrationForm = "FakeCompanyRegistration";
        #region Constanst
        private string[] _Names = new string[] { "Gabriel", "Raphaël", "Léo", "Louis", "Lucas", "Adam", "Arthur", "Jules", "Hugo", "Paul", "Nathan", "Gabin", "Noé", "Victor", "Martin", "Mathis", "Axel", "Léon", "Antoine", "Marius", "Valentin", "Clément", "Baptiste", "Samuel", "Augustin", "Emma", "Jade", "Louise", "Alice", "Chloé", "Inès", "Lina", "Léa", "Rose", "Léna", "Anna", "Ambre", "Julia", "Manon", "Juliette", "Lou", "Zoé", "Camille", "Eva", "Agathe", "Jeanne", "Lucie", "Sarah", "Romane", "Charlotte" };
        private string[] _Surname = new string[] { "MARTIN","BERNARD","THOMAS","PETIT","ROBERT","RICHARD","DURAND","DUBOIS","MOREAU","LAURENT","SIMON","MICHEL",
"LEFEBVRE","LEROY","ROUX","DAVID","BERTRAND","MOREL","FOURNIER","GIRARD","BONNET","DUPONT","LAMBERT","FONTAINE","ROUSSEAU","VINCENT","MULLER","LEFEVRE","FAURE","ANDRE",
"MERCIER","BLANC","GUERIN","BOYER","GARNIER","CHEVALIER","FRANCOIS","LEGRAND","GAUTHIER","GARCIA","PERRIN","ROBIN","CLEMENT","MORIN","NICOLAS","HENRY","ROUSSEL","MATHIEU",
"GAUTIER","MASSON","MARCHAND","DUVAL","DENIS","DUMONT","MARIE","LEMAIRE","NOEL","MEYER","DUFOUR","MEUNIER","BRUN","BLANCHARD","GIRAUD","JOLY","RIVIERE","LUCAS","BRUNET",
"GAILLARD","BARBIER","ARNAUD","MARTINEZ","GERARD","ROCHE","RENARD","SCHMITT","ROY","LEROUX","COLIN","VIDAL","CARON","PICARD","ROGER","FABRE","AUBERT","LEMOINE","RENAUD",
"DUMAS","LACROIX","OLIVIER","PHILIPPE","BOURGEOIS","PIERRE","BENOIT","REY","LECLERC","PAYET","ROLLAND","LECLERCQ","GUILLAUME","LECOMTE","LOPEZ","JEAN","DUPUY","GUILLOT",
"HUBERT","BERGER","CARPENTIER","SANCHEZ","DUPUIS","MOULIN","LOUIS","DESCHAMPS","HUET","VASSEUR","PEREZ","BOUCHER","FLEURY","ROYER","KLEIN","JACQUET","ADAM","PARIS",
"POIRIER","MARTY","AUBRY","GUYOT","CARRE","CHARLES","RENAULT","CHARPENTIER","MENARD","MAILLARD","BARON","BERTIN","BAILLY","HERVE","SCHNEIDER","FERNANDEZ","LE GALL",
"COLLET","LEGER","BOUVIER","JULIEN","PREVOST","MILLET","PERROT","DANIEL","LE ROUX","COUSIN","GERMAIN","BRETON","BESSON","LANGLOIS","REMY","LE GOFF","PELLETIER","LEVEQUE",
"PERRIER","LEBLANC","BARRE","LEBRUN","MARCHAL","WEBER","MALLET","HAMON","BOULANGER","JACOB","MONNIER","MICHAUD","RODRIGUEZ","GUICHARD","GILLET","ETIENNE","GRONDIN","POULAIN",
"TESSIER","CHEVALLIER","COLLIN","CHAUVIN","DA SILVA","BOUCHET","GAY","LEMAITRE","BENARD","MARECHAL","HUMBERT","REYNAUD","ANTOINE","HOARAU","PERRET","BARTHELEMY","CORDIER",
"PICHON","LEJEUNE","GILBERT","LAMY","DELAUNAY","PASQUIER","CARLIER","LAPORTE"};
        #endregion
        public TestExamplesController(
            Contracts.ILeaveManagementUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            ILeaveManagementCustomLocalizerFactory localizerFactory,
            IPasswordGenerator passwordGenerator,
            IEmailSender emailSender
            ) {
            _UnitOfWork = unitOfWork;
            _UserManager = userManager;
            _StringLocalizer = localizerFactory.Create(this.GetType());
            _PasswordGenerator = passwordGenerator;
            _EmailSender = emailSender;
            _FemaileNamesStartIndex = _Names.ToList().IndexOf("Emma");
        }

        [HttpGet]
        public async Task<ActionResult> CreateTestCompany() {
            CompanyQuickRegistrationVm registrationVm = await Task.FromResult(new CompanyQuickRegistrationVm());
            return View(CompanyRegistrationForm, registrationVm);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTestCompany(CompanyQuickRegistrationVm registrationVm) {
            if((await _UserManager.FindByNameAsync(registrationVm.CompanyAdminUserName)) != null) {
                ModelState.AddModelError("", _StringLocalizer["User with name '{0}' already exists. Please choose another", registrationVm.CompanyAdminUserName]);
                return View(CompanyRegistrationForm, registrationVm);
            }
            if ((await _UserManager.FindByEmailAsync(registrationVm.CompanyEmail)) != null) {
                ModelState.AddModelError("", _StringLocalizer["User with email '{0}' already exists. Please choose another", registrationVm.CompanyEmail]);
                return View(CompanyRegistrationForm, registrationVm);
            }
            Random randomizer = new Random();
            bool result = true;
            StringBuilder messageBuilder = null;
            var company = CreateCompany(registrationVm, randomizer);
            result &= await _UnitOfWork.Companies.CreateAsync(company);
            var leaveTypes = CreateLeaveTypes(company);

            if (result) {
                var employeesData = await RegisterEmployees(company, registrationVm, randomizer);
                if (employeesData.Item1)
                    messageBuilder = employeesData.Item2;
            }
            else {
                ModelState.AddModelError("", _StringLocalizer[""]);
            }
            foreach (LeaveType leaveType in leaveTypes)
                result &= await _UnitOfWork.LeaveTypes.CreateAsync(leaveType);
            if (result)
                result &= await _UnitOfWork.Save();

            if (result) {
                try {
                    await _EmailSender.SendEmailAsync(email: registrationVm.CompanyEmail,
                    subject: _StringLocalizer["Test company registration ({0})", registrationVm.CompanyName],
                    messageBuilder.ToString());
                }
                catch {
                    result &= false;
                }
            }
            if (result)
                return RedirectToAction("Index", "Home");
            else
                return View(CompanyRegistrationForm, registrationVm);

        }

        private Company CreateCompany(CompanyQuickRegistrationVm registrationVm, Random randomizer) {
            string companyZipCode = $"{randomizer.Next(00001, 99999):5}";
            Company company = new Company() {
                Active = true,
                CompanyCreationDate = DateTime.Now.AddMonths(randomizer.Next(3, 120)),
                CompanyEmail = registrationVm.CompanyEmail,
                CompanyName = registrationVm.CompanyName,
                CompanyPostAddress = $"companyZipCode {registrationVm.CompanyName}",
                CompanyRegistrationDate = DateTime.Now,
                CompanyZipCode = companyZipCode,
                TaxId = randomizer.Next().ToString(),
                CompanyState = _StringLocalizer["European state"],
                EnableLockoutForEmployees = false,
                CompanyPublicComment = _StringLocalizer["Automatic registration company"]

            };
            return company;
        }

        private LeaveType[] CreateLeaveTypes(Company company) {
            LeaveType[] result = new LeaveType[] {
            new LeaveType() {LeaveTypeName = _StringLocalizer["Vacation"], DefaultDays = 20, DateCreated = DateTime.Now, Company = company },
            new LeaveType() {LeaveTypeName = _StringLocalizer["RTT"], DefaultDays = 12, DateCreated = DateTime.Now, Company = company },
            new LeaveType() {LeaveTypeName = _StringLocalizer["Ancienity benefits"], DefaultDays = 1, DateCreated = DateTime.Now, Company = company },
            new LeaveType() {LeaveTypeName = _StringLocalizer["Illness"], DefaultDays = 10, DateCreated = DateTime.Now, Company = company },
            };
            return result;
        }

        private async Task<Tuple<Employee, string>> CreateOrdinaryEmployeeAndPassword(Company company, Random randomizer, Employee manager = null) {
            string companyNormalizedName = GetCompanyNormalizedName(company.CompanyName);
            Employee candidate = null;
            string title, firstName, lastName, email;
            do {
                int nameIndex = randomizer.Next(_Names.Length);
                title = nameIndex < _FemaileNamesStartIndex ? _StringLocalizer["M"] : _StringLocalizer["Mme"];
                firstName = _Names[nameIndex];
                lastName = _Surname[randomizer.Next(_Surname.Length)];
                email = $"{firstName[0]}.{lastName.ToLower()}@{companyNormalizedName.ToLower()}.com";
                candidate = await _UnitOfWork.Employees.FindAsync(x => x.Email.Equals(email));
            } while (candidate != null);
            var daysFromCreation = (company.CompanyCreationDate - DateTime.Now).TotalDays;

            Employee result = new Employee() {
                UserName = email,
                Email = email,
                Title = title,
                FirstName = firstName,
                LastName = lastName,
                Company = company,
                Manager = manager,
                EmploymentDate = DateTime.Now.AddDays(randomizer.NextDouble() * daysFromCreation),
                DateOfBirth = DateTime.Now.AddDays(Convert.ToDouble(randomizer.Next(19 * 365, 65 * 365))),
                TaxRate = randomizer.Next().ToString()
            };
            string password = _PasswordGenerator.GeneratePassword();
            return Tuple.Create(result, password);
        }

        private async Task<Tuple<Employee, string>> CreateMasterEmployeeAndPassword(Company company, CompanyQuickRegistrationVm registrationVm, Random randomizer) {
            var daysFromCreation = (company.CompanyCreationDate - DateTime.Now).TotalDays;
            Employee result = new Employee() {
                UserName = registrationVm.CompanyAdminUserName,
                Email = registrationVm.CompanyEmail,
                ContactMail = registrationVm.CompanyEmail,
                Title = _StringLocalizer["Master"],
                FirstName = "Administrator",
                LastName = company.CompanyName,
                Company = company,
                Manager = null,
                EmploymentDate = DateTime.Now.AddDays(randomizer.NextDouble() * daysFromCreation),
                DateOfBirth = DateTime.Now.AddDays(Convert.ToDouble(randomizer.Next(19 * 365, 65 * 365))),
                TaxRate = randomizer.Next().ToString()
            };
            string password = _PasswordGenerator.GeneratePassword();
            return await Task.FromResult(Tuple.Create(result, password));
        }

        private string GetCompanyNormalizedName(string companyName) => companyName?.Replace("&", " And ")?.Replace(' ', '_').ToLower();

        private async Task<Tuple<bool, StringBuilder>> RegisterEmployees(Company company, CompanyQuickRegistrationVm registrationVm, Random randomizer) {
            bool employeesCreationResult = true;
            var managerData = await CreateMasterEmployeeAndPassword(company, registrationVm, randomizer);
            employeesCreationResult &= await _UnitOfWork.Employees.RegisterEmployeeAsync(_UserManager, managerData.Item1, managerData.Item2);
            employeesCreationResult &= employeesCreationResult ? await _UnitOfWork.Employees.SetEmployeesRoles(_UserManager, managerData.Item1, UserRoles.CompanyAdministrator) : false;
            Tuple<Employee, string>[] ordinaryEmployees = new Tuple<Employee, string>[randomizer.Next(2, 8)];
            for (int i = 0; i < ordinaryEmployees.Length && employeesCreationResult; i++) {
                ordinaryEmployees[i] = await CreateOrdinaryEmployeeAndPassword(company, randomizer, manager: managerData.Item1);
                employeesCreationResult &= employeesCreationResult ? await _UnitOfWork.Employees.RegisterEmployeeAsync(_UserManager, ordinaryEmployees[i].Item1, ordinaryEmployees[i].Item2) : employeesCreationResult;
                employeesCreationResult &= employeesCreationResult ? await _UnitOfWork.Employees.SetEmployeesRoles(_UserManager, ordinaryEmployees[i].Item1, UserRoles.Employee) : employeesCreationResult;
            }
            var messageBuilder = GetWelcomeHtmlMessage(managerData, ordinaryEmployees, company);
            return Tuple.Create(employeesCreationResult, messageBuilder);
        }

        /// <summary>
        /// Hard-coded message of wellcome. It is in test puroposes.
        /// </summary>
        /// <param name="companyAdmin"></param>
        /// <param name="employees"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO Replace with something more professional and configurable.
        /// </remarks>
        private StringBuilder GetWelcomeHtmlMessage(Tuple<Employee, string> companyAdmin, IEnumerable<Tuple<Employee, string>> employees, Company company) {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(_StringLocalizer["Hi, Mr/Mrs vacation manager at {0}", company.CompanyName] + "<br/>");
            messageBuilder.AppendLine(_StringLocalizer["Thank you for paying attention to our tool and taking the time to test it"] + "<br/>");
            messageBuilder.AppendLine(_StringLocalizer["You can find some information how to use our application at"] + GetHelpLink() + ".<br/>");
            messageBuilder.AppendLine(_StringLocalizer["Your test userName is: ", companyAdmin.Item1.Email] + "<br/>");
            messageBuilder.AppendLine(_StringLocalizer["Your test password: ", companyAdmin.Item2] + "<br/>");
            messageBuilder.AppendLine("<hr/>");
            messageBuilder.AppendLine($"<h4>{_StringLocalizer["Here the list of your testing employees data"]}</h4>");
            messageBuilder.AppendLine($"<table><thead><tr><td>{_StringLocalizer["Name"]}</td><td>{_StringLocalizer["UserName"]}</td><td>{_StringLocalizer["Password"]}</td></tr></thead><tbody>");
            foreach (var item in employees.OrderBy(x => x.Item1.FormatEmployeeSNT()))
                messageBuilder.AppendLine($"<tr><td>{item.Item1.FormatEmployeeSNT()}</td>{item.Item1.Email}<td></td><td>{item.Item2}</td></tr>");
            messageBuilder.Append("</tbody></table><br/><br/>");
            messageBuilder.Append(_StringLocalizer["Have a nice day and thank you for testing!"]);
            messageBuilder.Append(_StringLocalizer["For all your remarks please dont hesitate to answer to this address"]);
            return messageBuilder;
        }

        private string GetHelpLink() {
            /*var callbackUrl = Url.Page(
                    "Help",
                    pageHandler: null,
                    values: new { controller = "Home", action = "Help" },
                    protocol: Request.Scheme);*/
            var callbackUrl = Url.Action("Help", "Home");
            return callbackUrl;
        }

        #region Disposing

        public new void Dispose() {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        private bool _Disposed = false;

        protected override void Dispose(bool disposing) {
            if (_Disposed)
                return;

            if (disposing) {
                _UnitOfWork.Dispose();
            }
            base.Dispose(disposing);

            _Disposed = true;
        }

        #endregion
    }
}
