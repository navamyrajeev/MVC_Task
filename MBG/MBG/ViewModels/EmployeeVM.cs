using MBG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MBG.ViewModels
{
    public class EmployeeVM
    {
        public Employee EmpObj { get; set; }

        public List<Employee_Details> EmpDetails { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
    }
}