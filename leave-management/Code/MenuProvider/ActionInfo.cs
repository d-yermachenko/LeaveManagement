using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;


namespace LeaveManagement.MenuProvider {
    public class ActionInfo {

        public ActionInfo() {
            SubActions = new List<ActionInfo>();
        }

        public UserRoles AllowedRoles { get; set; }

        public string ActionName { get; set; }

        public RouteValueDictionary ActionData { get; set; }

        public string ActionDisplayName { get; set; }

        public string ActionDescription { get; set; }

        /// <summary>
        /// Need this method to allow disigner define the icon for each action
        /// </summary>
        /// <param name="styles"></param>
        /// <returns></returns>
        public string GetIconCssClass(IDictionary<string, string> styles) {
            if (styles.ContainsKey(ActionName))
                return styles[ActionName];
            else
                return String.Empty;
        }

        public IList<ActionInfo> SubActions { get; private set; }

        public ActionInfo Clone() {
            var result = new ActionInfo() {
                ActionData = new RouteValueDictionary(),
                ActionDescription = this.ActionDescription,
                ActionDisplayName = this.ActionDisplayName,
                ActionName = this.ActionName,
                AllowedRoles = this.AllowedRoles,
                SubActions = new List<ActionInfo>()
            };
            foreach (var pair in this.ActionData)
                result.ActionData.TryAdd(pair.Key, pair.Value);
            if(SubActions.Count > 0) {
                foreach(var subAction in this.SubActions) {
                    result.SubActions.Add(subAction.Clone());
                }
            }
            return result;
        }
    }
}
