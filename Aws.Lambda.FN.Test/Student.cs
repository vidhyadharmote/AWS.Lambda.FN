using System;
using System.Collections.Generic;
using System.Text;

namespace AWS.Lambda.FN
{
    public class Student
    {
        #region Constructors

        public Student(){ }

        public Student(string name,string rollNumber,string mobileNo,string address)
        {
            Name = name;
            RollNumber = rollNumber;
            MobileNo = mobileNo;
            Address = address;
        }

        #endregion

        #region Public Properties

        public string Name { get; set; }
        public string RollNumber { get; set; }
        public string MobileNo { get; set; }
        public string Address { get; set; }

        #endregion
        

    }

}
