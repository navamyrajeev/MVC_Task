//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MBG.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Employee_Details
    {
        public int DetailsId { get; set; }
        public int EmpId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}
