using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Department
    {
        public PoliceBase.Departments currentDep;
        public string fullName;
        public string shortName;
        public List<Cop> employees;
        
        public Department(string name, string sname, PoliceBase.Departments current_dep)
        {
            fullName = name;
            shortName = sname;
            currentDep = current_dep;
            employees = new List<Cop>();
        }

        public string GetEmployeeString()
        {
            if (employees.Count == 0)
            {
                return "";
            }
            else if (employees.Count == 1)
            {
                return employees[0].Name;
            }
            else
            {
                string result = "";
                for (int i = 0; i < employees.Count - 1; i++)
                {
                    result += employees[i].Name + ", ";
                }
                result += employees[employees.Count - 1].Name;
                return result;
            }
        }

        public bool isCopAnEmployee(string name)
        {
            foreach (Cop cop in employees)
            {
                if (cop.Name == name) return true;
            }
            return false;
        }

        #region Employee actions

        public void Join(Cop cop)
        { 
            if (!employees.Contains(cop))
            {
                employees.Add(cop);
            }
            else
            {
                Debug.WriteLine("Already have employee " + cop.Name);
            }
            cop.currentDepartment = this;
            cop.player.TriggerEvent("PoliceBasics:joinedDepartment", fullName);
        }

        public void Leave(Cop cop)
        {
            if (employees.Contains(cop))
            {
                employees.Remove(cop);
            }
        }

        #endregion
    }
}
