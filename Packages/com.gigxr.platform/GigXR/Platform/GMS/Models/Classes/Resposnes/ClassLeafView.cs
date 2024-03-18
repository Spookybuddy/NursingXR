namespace GIGXR.GMS.Models.Classes.Resposnes
{
    using System;

    public class ClassLeafView
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public ClassStatus ClassStatus { get; set; }

        public Guid? InstructorId { get; set; }

        public Guid DepartmentId { get; set; }

        public Guid InstitutionId { get; set; }
    }
}