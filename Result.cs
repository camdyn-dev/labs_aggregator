/*
 * Purpose:
 *  Store specific, wanted results from an individual lab test in a record for manipulation.
 * 
 */

namespace labs_aggregator
{
    public record Result : IComparable<Result>
    {
        public DateTime CollectionDate { get; set; } // date our lab was collected

        /*
        * For now (or possibly forever), I'm going to hardode the individual lab tests I want to aggregate.
        * I don't think making it generic would be helpful at the time; I'm creating this specifically for personal use with QuestLabs PDFs.
        *
        * For general liver function, I want to keep track of;
        *   Albumin, INR, ALP, AST, ALT, GGT, TB, DB, Platelets
        *      
        *  For infections and general blood, I want to keep track of;
        *   ESR, CRP, WBC, RBC
        */

        public string Albumin { get; set; }
        public string TotalBilirubin { get; set; }
        public string DirectBilirubin { get; set; }
        public string ALP { get; set; }
        public string AST { get; set; }
        public string ALT { get; set; }
        public string GGT { get; set; }
        public string INR { get; set; }
        public string ESR { get; set; }
        public string WBC { get; set; }
        public string RBC { get; set; }  
        public string PlateletCount { get; set; }
        public string CRP { get; set; }


        public int CompareTo(Result? other)
        { // Implementing CompareTo so we can sort Results by date
            return DateTime.Compare(this.CollectionDate, other.CollectionDate);
        }
    }
}
