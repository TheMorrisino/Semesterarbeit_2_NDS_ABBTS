namespace FullstackRessourcix;

public class Employee
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string name { get; set; } = "";
    public string role { get; set; } = "";
    public int workload { get; set; }
    public double vacationWeeks { get; set; }
    public bool isActive { get; set; } = true;
    public enum Department
    {
      It,
      HumanResources,
      Finance
    }
    public Department department { get; set; } = Department.It;


    public enum Qualification
    {
        GeneralIt,
        GeneralHr,
        GeneralFinance,

        NursingFaGe,              // Fachfrau/Fachmann Gesundheit (FaGe)
        HousekeepingEfz,          // Hauswirtschaft EFZ
        SocialPedagogyHf,         // HF Sozialpädagogik
        NursingAssistanceSbbk,    // Pflegeassistenz SBBK
        Medicine,                 // Arzt/Ärztin
        PhysiotherapyBsc,         // BSc Physiotherapie
        OccupationalTherapyBsc,   // BSc Ergotherapie
        SpitexBasicCourse         // Spitex-spezifischer Grundkurs
    }
    public Qualification education { get; set; } = Qualification.GeneralIt;
}