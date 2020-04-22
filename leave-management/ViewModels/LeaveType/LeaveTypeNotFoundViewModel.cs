namespace LeaveManagement.ViewModels.LeaveType {

    public class LeaveTypeNotFoundViewModel {
        public LeaveTypeNotFoundViewModel() {
            ;
        }

        public LeaveTypeNotFoundViewModel(object id, string objectClass) {
            Id = id;
            ObjectClass = objectClass;
        }

        public object Id { get; set; }

        public string ObjectClass { get; set; }

        public string ObjectName { get; set; }

        public string Message {
            get {
                return string.IsNullOrWhiteSpace(ObjectName) ? $"with id {Id}" : $"named {ObjectName}";
            }
        }


    }
}
