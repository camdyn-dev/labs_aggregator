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

        // START OF VARIABLES
        public string Albumin { get; set; }
        public string INR { get; set; }
        public string ALP { get; set; }
        public string AST { get; set; }
        public string ALT { get; set; }
        public string GGT { get; set; }
        public string TBilirubin { get; set; }
        public string DBilirubin { get; set; }
        public string Platelets { get; set; }
        public string ESR { get; set; }
        public string CRP { get; set; }
        public string WBC { get; set; }
        public string RBC { get; set; }  
        
        // END OF VARIABLES


        // START OF FUNCTIONS
        public int CompareTo(Result? other)
        { // Implementing CompareTo so we can sort Results by date
            return DateTime.Compare(this.CollectionDate, other.CollectionDate);
        }

        public void StoreResult(string labName, string labValue)
        { // Function we'll use to store lab numbers within this Result

            // Mappings from labName to save function
            var mappings = new Dictionary<string, Action<string>>
            {
                // These two should undoubtedly contain a valid DateTime, but for safety's sake
                ["Collected:"] = input => {
                    this.CollectionDate = DateTime.TryParse(labValue, out var date) ? date : default;
                },
                ["Date of Service:"] = input => {
                    // The reason we have a separate "Date of Service:" in addition to "Collected:" is because of changes in Quest's protocol.
                    // Some old PDF's use "Date of Service:" rather than the new "Collected:"
                    this.CollectionDate = DateTime.TryParse(labValue, out var date) ? date : default; 
                },

                ["ALBUMIN"] = (input) => { this.Albumin = labValue; },
                ["BILIRUBIN, TOTAL"] = (input) => { this.TBilirubin = labValue; },
                ["BILIRUBIN, DIRECT"] = (input) => { this.DBilirubin = labValue; },
                ["ALKALINE PHOSPHATASE"] = (input) => { this.ALP = labValue; },
                ["AST"] = (input) => { this.AST = labValue; },
                ["ALT"] = (input) => { this.ALT = labValue; },
                ["GGT"] = (input) => { this.GGT = labValue; },
                ["INR"] = (input) => { this.INR = labValue; },
                ["SED RATE BY MODIFIED WESTERGREN"] = (input) => { this.ESR = labValue; },
                ["WHITE BLOOD CELL COUNT"] = (input) => { this.WBC = labValue; },
                ["RED BLOOD CELL COUNT"] = (input) => { this.RBC = labValue; },
                ["PLATELET COUNT"] = (input) => { this.Platelets = labValue; },
                ["C-REACTIVE PROTEIN"] = (input) => { this.CRP =    labValue; },
                ["HS CRP"] = (input) => { this.CRP = labValue; },
            };

            // Save/Store the given value
            mappings[labName](labValue);
        }
    }
}
