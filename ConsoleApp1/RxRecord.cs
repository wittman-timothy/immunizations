using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class RxRecord
    {
        public string RecordIdentifierImmunization { get; set; }
        public string NDC { get; set; }
        public string TradeName { get; set; }
        public string CPTCode { get; set; }
        public string BlankCVXCode { get; set; }
        public string VaccineGroup { get; set; }
        public string VaccineDate { get; set; }
        public string AdministrationRouteCode { get; set; }
        public string BodySiteCode { get; set; }
        public string ReactionCode { get; set; }
        public string ManufacturerCode { get; set; }
        public string ImmunizationInformationSource { get; set; }
        public string LotNumber { get; set; }
        public string ProviderName { get; set; }
        public string AdministeredByName { get; set; }
        public string SendingOrganization { get; set; }
        public string VaccineEligibility { get; set; }
        public string FundingType { get; set; }
        public string RecordIdentifierPatient { get; set; }
        public string PatientStatus { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameSuffix { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string MothersFirstName { get; set; }
        public string MothersMaidenLastName { get; set; }
        public string MothersHbsAgStatus { get; set; }
        public string immPatientGender { get; set; }
        public string AmericanIndianOrAlaskaNative { get; set; }
        public string Asian { get; set; }
        public string NativeHawaiianOrOtherPacificIsland { get; set; }
        public string BlackOrAfricanAmerican { get; set; }
        public string White { get; set; }
        public string OtherRace { get; set; }
        public string Ethnicity { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string ContactAllowed { get; set; }
        public string PatientID { get; set; }
        public string MedicaidID { get; set; }
        public string ResponsiblePartyFirstName { get; set; }
        public string ResponsibilePartyMiddleName { get; set; }
        public string ResponsiblePartyLastName { get; set; }
        public string ResponsiblePartyRelationship { get; set; }
        public string StreetAddressLine { get; set; }
        public string OtherAddressLine { get; set; }
        public string POBoxRouteLine { get; set; }
        public string City { get; set; }
        public string immPatientSate { get; set; }
        public string ZipCode { get; set; }
        public string County { get; set; }
        public string Phone { get; set; }
        public string SendingOrg { get; set; }
        public string StoreCode { get; set; }
        public string FillFactKey { get; set; }
        public string AdministeredAmount { get; set; }
        public string AdministeredUnits { get; set; }
        public string SIG { get; set; }
        public string CVX { get; set; }
        public string CVXDescription { get; set; }
        public string RxNum { get; set; }

        /// <summary>
        /// Creates Blank RxRecord
        /// </summary>
        public RxRecord() { }


        public RxRecord(string recordIdentifierImmunization, string nDC, string tradeName, string cPTCode, string blankCVXCode, string vaccineGroup, string vaccineDate, string administrationRouteCode, string bodySiteCode, string reactionCode, string manufacturerCode, string immunizationInformationSource, string lotNumber, string providerName, string administeredByName, string sendingOrganization, string vaccineEligibility, string fundingType, string recordIdentifierPatient, string patientStatus, string firstName, string middleName, string lastName, string nameSuffix, string birthDate, string deathDate, string mothersFirstName, string mothersMaidenLastName, string mothersHbsAgStatus, string immPatientGender, string americanIndianOrAlaskaNative, string asian, string nativeHawaiianOrOtherPacificIsland, string blackOrAfricanAmerican, string white, string otherRace, string ethnicity, string socialSecurityNumber, string contactAllowed, string patientID, string medicaidID, string responsiblePartyFirstName, string responsibilePartyMiddleName, string responsiblePartyLastName, string responsiblePartyRelationship, string streetAddressLine, string otherAddressLine, string pOBoxRouteLine, string city, string immPatientSate, string zipCode, string county, string phone, string sendingOrg, string storeCode, string fillFactKey, string administeredAmount, string administeredUnits, string sig, string cvx, string cvxdescription, string rxNum)
        {
            RecordIdentifierImmunization = recordIdentifierImmunization;
            NDC = nDC;
            TradeName = tradeName;
            CPTCode = cPTCode;
            BlankCVXCode = blankCVXCode;
            VaccineGroup = vaccineGroup;
            VaccineDate = vaccineDate;
            AdministrationRouteCode = administrationRouteCode;
            BodySiteCode = bodySiteCode;
            ReactionCode = reactionCode;
            ManufacturerCode = manufacturerCode;
            ImmunizationInformationSource = immunizationInformationSource;
            LotNumber = lotNumber;
            ProviderName = providerName;
            AdministeredByName = administeredByName;
            SendingOrganization = sendingOrganization;
            VaccineEligibility = vaccineEligibility;
            FundingType = fundingType;
            RecordIdentifierPatient = recordIdentifierPatient;
            PatientStatus = patientStatus;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            NameSuffix = nameSuffix;
            BirthDate = birthDate;
            DeathDate = deathDate;
            MothersFirstName = mothersFirstName;
            MothersMaidenLastName = mothersMaidenLastName;
            MothersHbsAgStatus = mothersHbsAgStatus;
            this.immPatientGender = immPatientGender;
            AmericanIndianOrAlaskaNative = americanIndianOrAlaskaNative;
            Asian = asian;
            NativeHawaiianOrOtherPacificIsland = nativeHawaiianOrOtherPacificIsland;
            BlackOrAfricanAmerican = blackOrAfricanAmerican;
            White = white;
            OtherRace = otherRace;
            Ethnicity = ethnicity;
            SocialSecurityNumber = socialSecurityNumber;
            ContactAllowed = contactAllowed;
            PatientID = patientID;
            MedicaidID = medicaidID;
            ResponsiblePartyFirstName = responsiblePartyFirstName;
            ResponsibilePartyMiddleName = responsibilePartyMiddleName;
            ResponsiblePartyLastName = responsiblePartyLastName;
            ResponsiblePartyRelationship = responsiblePartyRelationship;
            StreetAddressLine = streetAddressLine;
            OtherAddressLine = otherAddressLine;
            POBoxRouteLine = pOBoxRouteLine;
            City = city;
            this.immPatientSate = immPatientSate;
            ZipCode = zipCode;
            County = county;
            Phone = phone;
            SendingOrg = sendingOrg;
            FillFactKey = fillFactKey;
            AdministeredAmount = administeredAmount;
            AdministeredUnits = administeredUnits;
            SIG = sig;
            CVX = cvx;
            CVXDescription = cvxdescription;
            RxNum = rxNum;
        }


        /// <summary>
        /// Give this a line from the | delimited file and it'll parse it and self-fill
        /// </summary>
        /// <param name="FileLine">Line from | file</param>
        public void FillFromFileLine(string FileLine)
        {
            string[] values = FileLine.Split("|".ToCharArray());
            RecordIdentifierImmunization = values[0].Trim();
            NDC = values[1].Trim();
            TradeName = values[2].Trim();
            CPTCode = values[3].Trim();
            BlankCVXCode = values[4].Trim();
            VaccineGroup = values[5].Trim();
            VaccineDate = values[6].Trim();
            AdministrationRouteCode = values[7].Trim();
            BodySiteCode = values[8].Trim();
            ReactionCode = values[9].Trim();
            ManufacturerCode = values[10].Trim();
            ImmunizationInformationSource = values[11].Trim();
            LotNumber = values[12].Trim();
            ProviderName = values[13].Trim();
            AdministeredByName = values[14].Trim();
            SendingOrganization = values[15].Trim();
            VaccineEligibility = values[16].Trim();
            FundingType = values[17].Trim();
            RecordIdentifierPatient = values[18].Trim();
            PatientStatus = values[19].Trim();
            FirstName = values[20].Trim();
            MiddleName = values[21].Trim();
            LastName = values[22].Trim();
            NameSuffix = values[23].Trim();
            BirthDate = values[24].Trim();
            DeathDate = values[25].Trim();
            MothersFirstName = values[26].Trim();
            MothersMaidenLastName = values[27].Trim();
            MothersHbsAgStatus = values[28].Trim();
            this.immPatientGender = values[29].Trim();
            AmericanIndianOrAlaskaNative = values[30].Trim();
            Asian = values[31].Trim();
            NativeHawaiianOrOtherPacificIsland = values[32].Trim();
            BlackOrAfricanAmerican = values[33].Trim();
            White = values[34].Trim();
            OtherRace = values[35].Trim();
            Ethnicity = values[36].Trim();
            SocialSecurityNumber = values[37].Trim();
            ContactAllowed = values[38].Trim();
            PatientID = values[39].Trim();
            MedicaidID = values[40].Trim();
            ResponsiblePartyFirstName = values[41].Trim();
            ResponsibilePartyMiddleName = values[42].Trim();
            ResponsiblePartyLastName = values[43].Trim();
            ResponsiblePartyRelationship = values[44].Trim();
            StreetAddressLine = values[45].Trim();
            OtherAddressLine = values[46].Trim();
            POBoxRouteLine = values[47].Trim();
            City = values[48].Trim();
            this.immPatientSate = values[49].Trim();
            ZipCode = values[50].Trim();
            County = values[51].Trim();
            Phone = values[52].Trim();
            SendingOrg = values[53].Trim();
            StoreCode = values[54].Trim();
            FillFactKey = values[55].Trim();
            AdministeredAmount = values[56].Trim();
            AdministeredUnits = values[57].Trim();
            SIG = values[58].Trim();
            CVX = values[59].Trim();
            CVXDescription = values[60].Trim();
            RxNum = values[61].Trim();
        }
    }
}
