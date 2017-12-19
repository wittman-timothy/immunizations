namespace ConsoleApp1
{
    #region Usings
    using Elements.Common;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using NHapi.Model.V251.Message;
    using System.Collections.Generic;
    #endregion

    public class Program
    {
        #region Application KEYS

        #region IA
        static string IAxCertPath = ConfigurationManager.AppSettings["IAxCertPath"].ToString();
        static string IAxDirectoryPath = ConfigurationManager.AppSettings["IAxDirectoryPath"].ToString();
        static string IAxArchivePath = ConfigurationManager.AppSettings["IAxArchivePath"].ToString();
        #endregion

        #region NE
        static string NExDirectoryPath = ConfigurationManager.AppSettings["NExDirectoryPath"].ToString();
        static string NExArchivePath = ConfigurationManager.AppSettings["NExArchivePath"].ToString();
        #endregion

        #region MN
        static string MNxDirectoryPath = ConfigurationManager.AppSettings["MNxDirectoryPath"].ToString();
        static string MNxArchivePath = ConfigurationManager.AppSettings["MNxArchivePath"].ToString();
        #endregion

        #region IL
        static string ILxDirectoryPath = ConfigurationManager.AppSettings["ILxDirectoryPath"].ToString();
        static string ILxArchivePath = ConfigurationManager.AppSettings["ILxArchivePath"].ToString();
        #endregion

        #region KS
        static string KSxDirectoryPath = ConfigurationManager.AppSettings["KSxDirectoryPath"].ToString();
        static string KSxArchivePath = ConfigurationManager.AppSettings["KSxArchivePath"].ToString();
        #endregion

        #region MO
        static string MOxDirectoryPath = ConfigurationManager.AppSettings["MOxDirectoryPath"].ToString();
        static string MOxArchivePath = ConfigurationManager.AppSettings["MOxArchivePath"].ToString();
        #endregion

        #region SD
        static string SDxDirectoryPath = ConfigurationManager.AppSettings["SDxDirectoryPath"].ToString();
        static string SDxArchivePath = ConfigurationManager.AppSettings["SDxArchivePath"].ToString();
        #endregion

        #region WI
        static string WIxDirectoryPath = ConfigurationManager.AppSettings["WIxDirectoryPath"].ToString();
        static string WIxArchivePath = ConfigurationManager.AppSettings["WIxArchivePath"].ToString();
        #endregion

        #region General
        static string EmailRXSupport = ConfigurationManager.AppSettings["EmailRXSupport"].ToString();
        static string ACK_Pharmacy_Rejection_Message = "The following Immunization was rejected and not submitted!"
            + Environment.NewLine + "Please read the following RX Number and correct for it's ACK Error in EnterpriseRx."
            + Environment.NewLine + "After, mark your immunization as sold to resubmit to the Immunization Registry."
            + Environment.NewLine + "Alternatively, you may enter the immunization into the Immunization Registry directly."
            + Environment.NewLine + "If help is needed, reply to Pharmacy Support."
            + Environment.NewLine 
            + Environment.NewLine + "Message Details" 
            + Environment.NewLine;
        static string ACK_Support_Rejection_Message = "The following Immunizations were rejected and not submitted!"
            + Environment.NewLine + "Each store's Pharmacy Mail Group has already been notified of the following ACK Error(s)."
            + Environment.NewLine 
            + Environment.NewLine;
        #endregion

        #endregion

        static string TESTxDirectoryPath = @"C:\Users\tgwittman\Desktop\Immunization\TestDirectory";
        static string TESTxArchivePath = @"C:\Users\tgwittman\Desktop\Immunization\TestArchive";

        public static void Main(string[] args)
        {
            //Test
            NEImmunization();
        }

        //Live
        private static void IAImmunization()
        {
            #region IRIS Client
            IRISCDCService.IS_PortTypeClient IRISClient = new IRISCDCService.IS_PortTypeClient();
            IRISClient.ClientCredentials.UserName.UserName = "hy-vee.com";
            IRISClient.ClientCredentials.UserName.Password = "vxb0SJohtxrLriaw1";
            IRISClient.ClientCredentials.ClientCertificate.Certificate = new X509Certificate2(X509Certificate.CreateFromCertFile(IAxCertPath));
            #endregion

            if (Directory.Exists(IAxDirectoryPath))
            {
                foreach (string ImmunizationFile in Directory.EnumerateFiles(IAxDirectoryPath, "*.txt"))
                {
                    try
                    {
                        #region Read Immunization
                        String[] RxData = File.ReadAllLines(ImmunizationFile);
                        List<VXU_V04> HL7Messages = new List<VXU_V04>();
                        List<string> RxNums = new List<string>();
                        List<string> StoreCodes = new List<string>();
                        foreach (string record in RxData)
                        {
                            RxRecord newRecord = new RxRecord();
                            newRecord.FillFromFileLine(record);
                            RxNums.Add(newRecord.RxNum);
                            StoreCodes.Add(newRecord.StoreCode);
                            HL7Messages.Add(IAVXU_V04(newRecord));
                        }
                        #endregion

                        #region Submit HL7 Messages
                        NHapi.Base.Parser.PipeParser parser = new NHapi.Base.Parser.PipeParser();
                        StringBuilder RXSupportEmailMessage = new StringBuilder();
                        int ApplicationErrors = 0;
                        int repetition = 0;
                        foreach (VXU_V04 HL7message in HL7Messages)
                        {
                            string rsp = IRISClient.submitSingleMessage(IRISClient.ClientCredentials.UserName.UserName, IRISClient.ClientCredentials.UserName.Password, "HPP", parser.Encode(HL7message));

                            if (rsp.Contains("MSA|AE|"))
                            {
                                //"AA" = Acceptance
                                //"AE" = Format or Validation Errors
                                //"AR" = Sending system deals with Error
                                StringBuilder PharmacyEmailMessage = new StringBuilder();
                                PharmacyEmailMessage.AppendLine($"RX Number: {RxNums[repetition]}");
                                PharmacyEmailMessage.AppendLine($"Store Code: {StoreCodes[repetition]}");
                                PharmacyEmailMessage.AppendLine("ACK Error" + Environment.NewLine + rsp);

                                if (!string.IsNullOrEmpty(StoreCodes[repetition]))
                                    Email.Send(EmailRXSupport, $"{StoreCodes[repetition]}pharmacymailgroup@hy-vee.com", "Iowa Immunization - ACK Error", ACK_Pharmacy_Rejection_Message + PharmacyEmailMessage.ToString());

                                RXSupportEmailMessage.AppendLine(PharmacyEmailMessage.ToString() + Environment.NewLine);
                                ApplicationErrors++;
                            }
                            repetition++;
                        }
                        #endregion

                        #region Email RXSupport: ACK Errors
                        if (ApplicationErrors > 0)
                        {
                            RXSupportEmailMessage.AppendLine("File Details");
                            RXSupportEmailMessage.AppendLine($"File Path: {IAxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");
                            RXSupportEmailMessage.AppendLine($"Number of Error Messages: {ApplicationErrors}");
                            Email.Send(EmailRXSupport, EmailRXSupport, "Iowa Immunization - ACK Error", ACK_Support_Rejection_Message + RXSupportEmailMessage.ToString());
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        IRISClient.Close();
                        Console.WriteLine(ex);
                        return;
                    }
                    #region Archive Immunization
                    File.Move(ImmunizationFile, $"{IAxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");

                    if (File.Exists(ImmunizationFile))
                        File.Delete(ImmunizationFile);
                    #endregion
                }
            }
            IRISClient.Close();
        }
        private static VXU_V04 IAVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - message header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = _RxRecord.SendingOrg;
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Error;
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Always;
            #endregion 

            #region PID - Patient Identification Required
            //PID-1 Sequence Number
            HL7VXUMessage251.PID.SetIDPID.Value = "1";
            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number
            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.FirstName));
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.MiddleName));
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.LastName));
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";
            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;
            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = Tools.TrimSpecialCharacters(_RxRecord.StreetAddressLine);
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "C";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";
            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999";
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.NamespaceID.Value = "XX9999";
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = _RxRecord.SendingOrg;
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";
            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";
            //OBX-3.1-3.3
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "vaccination eligibility";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";
            //OBX-5
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V01";
            OBX5_CE.Text.Value = "Not VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;
            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }
        private static void MOImmunization()
        {
            #region SMVDHSSClient
            SMVDHSSMO.ProviderInterface_EXTSoapClient SMVDHSSClient = new SMVDHSSMO.ProviderInterface_EXTSoapClient("ProviderInterface_EXTSoap12");
            SMVDHSSMO.HL7SoapHeader SMVDHSSHeader = new SMVDHSSMO.HL7SoapHeader();
            SMVDHSSHeader.USERID = "hyvee";
            SMVDHSSHeader.PASSWORD1 = "hv1018##";
            SMVDHSSHeader.PASSWORD2 = "hv1018##";
            #endregion

            if (Directory.Exists(MOxDirectoryPath))
            {
                foreach (string ImmunizationFile in Directory.EnumerateFiles(MOxDirectoryPath, "*.txt"))
                {
                    try
                    {
                        #region Read Immunization
                        String[] RxData = File.ReadAllLines(ImmunizationFile);
                        List<VXU_V04> HL7Messages = new List<VXU_V04>();
                        List<string> RxNums = new List<string>();
                        List<string> StoreCodes = new List<string>();
                        List<string> SendingOrgs = new List<string>();
                        foreach (string record in RxData)
                        {
                            RxRecord newRecord = new RxRecord();
                            newRecord.FillFromFileLine(record);
                            RxNums.Add(newRecord.RxNum);
                            StoreCodes.Add(newRecord.StoreCode);
                            SendingOrgs.Add(newRecord.SendingOrg);
                            HL7Messages.Add(MOVXU_V04(newRecord));
                        }
                        #endregion

                        #region Submit HL7 Messages
                        NHapi.Base.Parser.PipeParser parser = new NHapi.Base.Parser.PipeParser();
                        StringBuilder RXSupportEmailMessage = new StringBuilder();
                        int MessageErrorCount = 0;
                        int repetition = 0;
                        foreach (VXU_V04 HL7message in HL7Messages)
                        {
                            SMVDHSSHeader.FACILITYID = SendingOrgs[repetition];
                            SMVDHSSHeader.MESSAGEDATA = parser.Encode(HL7message);
                            string rsp = SMVDHSSClient.SMVHL7(SMVDHSSHeader);

                            if (rsp.Contains("MSA|AE|"))
                            {
                                StringBuilder PharmacyEmailMessage = new StringBuilder();
                                PharmacyEmailMessage.AppendLine($"RX Number: {RxNums[repetition]}");
                                PharmacyEmailMessage.AppendLine($"Store Code: {StoreCodes[repetition]}");
                                PharmacyEmailMessage.AppendLine("ACK Error" + Environment.NewLine + rsp);
                                Email.Send(EmailRXSupport, $"{StoreCodes[repetition]}pharmacymailgroup@hy-vee.com", "Missouri Immunization - ACK Error", ACK_Pharmacy_Rejection_Message + PharmacyEmailMessage.ToString());
                                RXSupportEmailMessage.AppendLine(PharmacyEmailMessage.ToString() + Environment.NewLine);
                                MessageErrorCount++;
                            }
                            repetition++;
                        }
                        #endregion

                        #region Email RXSupport: ACK Errors
                        if (MessageErrorCount > 0)
                        {
                            RXSupportEmailMessage.AppendLine("File Details");
                            RXSupportEmailMessage.AppendLine($"File Path: {MOxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");
                            RXSupportEmailMessage.AppendLine($"Number of Error Messages: {MessageErrorCount}");
                            Email.Send(EmailRXSupport, EmailRXSupport, "Missouri Immunization - ACK Error", ACK_Support_Rejection_Message + RXSupportEmailMessage.ToString());
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        SMVDHSSClient.Close();
                        Console.WriteLine(ex);
                        return;
                    }
                    #region Archive Immunization
                    File.Move(ImmunizationFile, $"{MOxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");

                    if (File.Exists(ImmunizationFile))
                        File.Delete(ImmunizationFile);
                    #endregion
                }
            }
            SMVDHSSClient.Close();
        }
        private static VXU_V04 MOVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - Message Header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-3 Uniquely defines the Sending Application 
            HL7VXUMessage251.MSH.SendingApplication.NamespaceID.Value = _RxRecord.FillFactKey;
            //MSH-4 Assigned to the Sending Facility by MODHSS 
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = _RxRecord.SendingOrg;
            //MSH-5
            HL7VXUMessage251.MSH.ReceivingApplication.NamespaceID.Value = "SHOWMEVAX";
            //MSH-6
            HL7VXUMessage251.MSH.ReceivingFacility.NamespaceID.Value = "MODHSS";
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Error;
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Always;
            #endregion

            #region PID - Patient Identification Required
            //PID-1 Sequence Number
            HL7VXUMessage251.PID.SetIDPID.Value = "1";
            //PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier
            //PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.FirstName));
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.MiddleName));
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.LastName));
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";
            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;
            //PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = Tools.TrimSpecialCharacters(_RxRecord.StreetAddressLine);
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "H";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";
            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999"; //_RxRecord.FillFactKey
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.NamespaceID.Value = "DCS";
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            //RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            //RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            //RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Text.Value = "NEW IMMUNIZATION RECORD";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).NameOfCodingSystem.Value = "NIP001";
            //RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = _RxRecord.SendingOrg;
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";
            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";
            //OBX-3.1-3.3 What Oberservation Referes to, Poses Question for OBX-5
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "VACCINE FUND PGM ELIG CAT";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";
            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";
            //OBX-5 Observation Value, Answers Question Posed by OBX-3
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V01";
            OBX5_CE.Text.Value = "Not VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;
            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }

        //Testing
        private static void NEImmunization()
        {
            #region NESIISCDCService
            NESIISCDCService.IS_PortTypeClient NESIISClient = new NESIISCDCService.IS_PortTypeClient();
            NESIISClient.ClientCredentials.UserName.UserName = "HVPC";
            NESIISClient.ClientCredentials.UserName.Password = "HVPC1";
            #endregion

            if (Directory.Exists(TESTxDirectoryPath))
            {
                foreach (string ImmunizationFile in Directory.EnumerateFiles(TESTxDirectoryPath, "*.txt"))
                {
                    try
                    {
                        #region Read Immunization
                        String[] RxData = File.ReadAllLines(ImmunizationFile);
                        List<VXU_V04> HL7Messages = new List<VXU_V04>();
                        List<string> RxNums = new List<string>();
                        List<string> StoreCodes = new List<string>();
                        foreach (string record in RxData)
                        {
                            RxRecord newRecord = new RxRecord();
                            newRecord.FillFromFileLine(record);
                            RxNums.Add(newRecord.RxNum);
                            StoreCodes.Add(newRecord.StoreCode);
                            HL7Messages.Add(NEVXU_V04(newRecord));
                        }
                        #endregion

                        #region Submit HL7 Messages
                        NHapi.Base.Parser.PipeParser parser = new NHapi.Base.Parser.PipeParser();
                        StringBuilder RXSupportEmailMessage = new StringBuilder();
                        int MessageErrorCount = 0;
                        int repetition = 0;
                        foreach (VXU_V04 HL7message in HL7Messages)
                        {
                            string rsp = NESIISClient.submitSingleMessage(NESIISClient.ClientCredentials.UserName.UserName, NESIISClient.ClientCredentials.UserName.Password, "HVPC", parser.Encode(HL7message));
                            if (rsp.Contains("MSA|AE|"))
                            {
                                StringBuilder PharmacyEmailMessage = new StringBuilder();
                                PharmacyEmailMessage.AppendLine($"RX Number: {RxNums[repetition]}");
                                PharmacyEmailMessage.AppendLine($"Store Code: {StoreCodes[repetition]}");
                                PharmacyEmailMessage.AppendLine("ACK Error" + Environment.NewLine + rsp);
                                //Email.Send(EmailRXSupport, $"{StoreCodes[repetition]}pharmacymailgroup@hy-vee.com", "Nebraska Immunization - ACK Error", ACK_Pharmacy_Rejection_Message + PharmacyEmailMessage.ToString());
                                RXSupportEmailMessage.AppendLine(PharmacyEmailMessage.ToString() + Environment.NewLine);
                                MessageErrorCount++;
                            }
                            repetition++;
                        }
                        #endregion

                        #region Email RXSupport: ACK Errors
                        if (MessageErrorCount > 0)
                        {
                            RXSupportEmailMessage.AppendLine("File Details");
                            RXSupportEmailMessage.AppendLine($"File Path: {TESTxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");
                            RXSupportEmailMessage.AppendLine($"Number of Error Messages: {MessageErrorCount}");
                            //Email.Send(EmailRXSupport, EmailRXSupport, "Nebraska Immunization - ACK Error", ACK_Support_Rejection_Message + RXSupportEmailMessage.ToString());
                            Email.Send(EmailRXSupport, "tgwittman@hy-vee.com", "Nebraska Immunization - ACK Error", ACK_Pharmacy_Rejection_Message + RXSupportEmailMessage.ToString());
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        NESIISClient.Close();
                        Console.WriteLine(ex);
                        return;
                    }
                    #region Archive Immunization
                    File.Move(ImmunizationFile, $"{TESTxArchivePath}\\{Path.GetFileName(ImmunizationFile)}");

                    if (File.Exists(ImmunizationFile))
                        File.Delete(ImmunizationFile);
                    #endregion
                }
            }
            NESIISClient.Close();
        }
        private static VXU_V04 NEVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - message header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-3 Uniquely defines the Sending Application 
            HL7VXUMessage251.MSH.SendingApplication.NamespaceID.Value = _RxRecord.FillFactKey;
            //MSH-4 Assigned to the Sending Facility by NESIIS
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = _RxRecord.SendingOrg;
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMdd");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Error;
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = NHapi.Model.V251.Table.ApplicationAcknowledgementConditions.Always;
            //MSH-21
            HL7VXUMessage251.MSH.GetMessageProfileIdentifier(0).EntityIdentifier.Value = "Z22";
            HL7VXUMessage251.MSH.GetMessageProfileIdentifier(0).NamespaceID.Value = "CDCPHINVS";
            #endregion 

            #region PID - Patient Identification Required
            //PID-1 Sequence Number
            HL7VXUMessage251.PID.SetIDPID.Value = "1";
            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number
            //PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.FirstName));
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.MiddleName));
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = Tools.TrimSpecialCharacters(Tools.TrimWhiteSpace(_RxRecord.LastName));
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";
            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;
            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = Tools.TrimSpecialCharacters(_RxRecord.StreetAddressLine);
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "H";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";
            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999"; //_RxRecord.FillFactKey
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            //RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            //RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            //RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            //RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            //RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = _RxRecord.SendingOrg;
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";
            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";
            //OBX-3.1-3.3
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "VACCINE FUND PGM ELIG CAT";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";
            ////OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";
            //OBX-5
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V01";
            OBX5_CE.Text.Value = "VFC eligibility not determined/unknown";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;
            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }

        //Kansas
        private static VXU_V04 KSVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - Message Header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = "HL70362";
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmm");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = "ER";
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = "AL";
            #endregion 

            #region PID - Patient Identification Required

            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number

            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = _RxRecord.FirstName;
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = _RxRecord.LastName;
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";

            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));

            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;

            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = _RxRecord.StreetAddressLine;
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "C";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";

            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999"; //_RxRecord.FillFactKey
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.NamespaceID.Value = "XX9999";
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.Set(DateTime.Now, "yyyyMMdd");
            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Text.Value = "NEW IMMUNIZATION RECORD";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).NameOfCodingSystem.Value = "NIP001";
            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = "KS1234"; //practice ID
            // RXA-17 Vaccine Manufacturer Code Required
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Identifier.Value = "UNK";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Text.Value = "Unknown manufacturer";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).NameOfCodingSystem.Value = "MVX";
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";
            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";
            //OBX-3.1-3.3 What Oberservation Referes to, Poses Question for OBX-5
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "vaccine fund pgm elig cat";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";
            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";
            //OBX-5 Observation Value, Answers Question Posed by OBX-3
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V04";
            OBX5_CE.Text.Value = "VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;
            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            //OBX-17
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationMethod(0).Identifier.Value = "VXC40";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationMethod(0).Text.Value = "Per Immunization";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationMethod(0).NameOfCodingSystem.Value = "CDCPHINVS ";
            #endregion

            return HL7VXUMessage251;

        }
        
        //South Dakota
        private static VXU_V04 SDVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - Message Header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = "HL70362";
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = "ER";
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = "AL";
            //MSH-21
            HL7VXUMessage251.MSH.GetMessageProfileIdentifier(0).EntityIdentifier.Value = "Z32";
            HL7VXUMessage251.MSH.GetMessageProfileIdentifier(0).NamespaceID.Value = "CDCPHINVS";
            #endregion 

            #region PID - Patient Identification Required
            //PID-1 Set ID
            HL7VXUMessage251.PID.SetIDPID.Value = "1";
            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier

            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number

            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = _RxRecord.FirstName;
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = _RxRecord.LastName;
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";

            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));

            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;

            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = _RxRecord.StreetAddressLine;
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "C";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";
            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999"; //_RxRecord.FillFactKey
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.NamespaceID.Value = "XX9999";
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Text.Value = "NEW IMMUNIZATION RECORD";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).NameOfCodingSystem.Value = "NIP001";
            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = "Hy-Vee Pharmacy (1039)";
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.UniversalID.Value = "90068";
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.UniversalIDType.Value = "PHIS";
            // RXA-17 Vaccine Manufacturer Code Required
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Identifier.Value = "UNK";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Text.Value = "Unknown manufacturer";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).NameOfCodingSystem.Value = "MVX";
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";

            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";

            //OBX-3.1-3.3 What Oberservation Referes to, Poses Question for OBX-5
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "vaccine fund pgm elig cat";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";

            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";

            //OBX-5 Observation Value, Answers Question Posed by OBX-3
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V04";
            OBX5_CE.Text.Value = "VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;

            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }
        private static QBP_Q11 SDQBP_Q11(RxRecord _RxRecord)
        {
            //QBP - Request Immunization History
            //Request Complete Immunization History Query Profile
            //SD responds to QBP messages with Response(RSP) message : RSP^K11^RSP_K11

            //Add Custome Segment

            QBP_Q11 HL7QBPMessage251 = new QBP_Q11();

            #region MSH - Message Header Required
            //MSH-1
            HL7QBPMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7QBPMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-9
            HL7QBPMessage251.MSH.MessageType.MessageCode.Value = "QBP";
            HL7QBPMessage251.MSH.MessageType.TriggerEvent.Value = "Q11";
            HL7QBPMessage251.MSH.MessageType.MessageStructure.Value = "QBP_Q11";
            //MSH-10
            HL7QBPMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7QBPMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7QBPMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-21
            HL7QBPMessage251.MSH.GetMessageProfileIdentifier(0).EntityIdentifier.Value = "Z32";
            HL7QBPMessage251.MSH.GetMessageProfileIdentifier(0).NamespaceID.Value = "CDCPHINVS";
            #endregion

            #region SFT - Software Segment Optional Repeating
            #endregion

            #region QPD - Query Parameter Definition Required
            //QPD-1 Message Query Name
            HL7QBPMessage251.QPD.MessageQueryName.Identifier.Value = "Z34";
            HL7QBPMessage251.QPD.MessageQueryName.Text.Value = "Request Complete Immunization History";
            HL7QBPMessage251.QPD.MessageQueryName.NameOfCodingSystem.Value = "CDCPHINVS";
            //QPD-2 Query Tag Unique Identifier
            HL7QBPMessage251.QPD.QueryTag.Value = _RxRecord.FillFactKey;
            //QPD-3 Unique patient ID  (Medicaid number, Medicare number, Medical record number, etc.)
            NHapi.Model.V251.Datatype.CX QPD3_CX = new NHapi.Model.V251.Datatype.CX(HL7QBPMessage251);
            QPD3_CX.IDNumber.Value = _RxRecord.MedicaidID;
            QPD3_CX.IdentifierTypeCode.Value = "MA";
            HL7QBPMessage251.QPD.UserParametersInsuccessivefields.Data = QPD3_CX;

            //QPD-4 Patient Name
            //NHapi.Model.V251.Datatype.XPN QPD3_XPN = new NHapi.Model.V251.Datatype.XPN(HL7QBPMessage251);
            //QPD3_XPN.GivenName.Value = Tools.TrimSpecialCharacters(_RxRecord.FirstName);
            //QPD3_XPN.SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            //QPD3_XPN.FamilyName.Surname.Value = _RxRecord.LastName;
            //HL7QBPMessage251.QPD.UserParametersInsuccessivefields.Data = QPD3_XPN;
            //QPD-5 Mother’s Maiden Name 
            //NHapi.Model.V251.Datatype.XPN QPD3_XPN_Mother = new NHapi.Model.V251.Datatype.XPN(HL7QBPMessage251);
            //QPD3_XPN_Mother.GivenName.Value = "";
            //QPD3_XPN_Mother.FamilyName.Surname.Value = "";
            //HL7QBPMessage251.QPD.UserParametersInsuccessivefields.Data = QPD3_XPN_Mother;
            //QPD-6 Patient DOB
            //NHapi.Model.V251.Datatype.TS QPD3_TS = new NHapi.Model.V251.Datatype.TS(HL7QBPMessage251);
            //QPD3_TS.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //HL7QBPMessage251.QPD.UserParametersInsuccessivefields.Data = QPD3_TS;
            //QPD-7 Patient Sex 
            //NHapi.Model.V251.Datatype.IS QPD3_IS = new NHapi.Model.V251.Datatype.IS(HL7QBPMessage251,0);
            //QPD3_IS.Value = _RxRecord.immPatientGender;
            //HL7QBPMessage251.QPD.UserParametersInsuccessivefields.Data = QPD3_IS;
            #endregion

            #region RCP - Response Control Parameter Required
            //RCP-1 Query Priority
            HL7QBPMessage251.RCP.QueryPriority.Value = "I";
            //RCP-2 Quantity Limited Request
            HL7QBPMessage251.RCP.QuantityLimitedRequest.Quantity.Value = "LI";
            HL7QBPMessage251.RCP.QuantityLimitedRequest.Units.Identifier.Value = "RD";
            #endregion

            #region DSC - Continuation Pointer Optional
            #endregion

            return HL7QBPMessage251;

        }
        private static void SDRSP_K11(RSP_K11 _RSP)
        {
        }

        //Wisconsin
        private static VXU_V04 WIVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            //Org ID: 43738
            //Site ID: 69634
            //Org Code: HV1392

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - Message Header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = "43738"; //Org ID
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = "ER";
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = "AL";
            #endregion

            #region PID - Patient Identification Required
            //PID-1 Sequence Number
            HL7VXUMessage251.PID.SetIDPID.Value = "1";
            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number
            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = _RxRecord.FirstName;
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = _RxRecord.LastName;
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";
            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;
            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = _RxRecord.StreetAddressLine;
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "C";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";

            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = _RxRecord.FillFactKey;
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            //RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Text.Value = "NEW IMMUNIZATION RECORD";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).NameOfCodingSystem.Value = "NIP001";
            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = "69634"; //Site ID
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";
            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";
            //OBX-3.1-3.3 What Oberservation Referes to, Poses Question for OBX-5
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "Vaccine funding program eligibility category";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";
            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";
            //OBX-5 Observation Value, Answers Question Posed by OBX-3
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V01";
            OBX5_CE.Text.Value = "Not VFC eligible";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;
            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            //OBX-14
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.DateTimeOfTheObservation.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            #endregion

            return HL7VXUMessage251;

        }

        //Waiting in Queue
        private static VXU_V04 MNVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - message header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = "TestEHRApplication";
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss[.s[s[s[s]]]] +/ -zzzz)");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = "ER";
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = "AL";
            #endregion 

            #region PID - Patient Identification Required

            //PID-1 Sequence Number
            HL7VXUMessage251.PID.SetIDPID.Value = "1";

            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier

            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number

            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = _RxRecord.FirstName;
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = _RxRecord.LastName;
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";

            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));

            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;

            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = _RxRecord.StreetAddressLine;
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "H";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";

            //ORC-3 Unique ID
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = "9999"; //_RxRecord.FillFactKey
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";

            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";

            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));

            // RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.Set(DateTime.Now, "yyyyMMdd");

            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";

            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;

            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;

            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";

            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = "TestEHRApplication";
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";

            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";

            //OBX-3.1-3.3
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "funding pgm eligibility";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";

            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";

            //OBX-5
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V04";
            OBX5_CE.Text.Value = "VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;

            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }
        private static VXU_V04 ILVXU_V04(RxRecord _RxRecord)
        {
            //VXU - Unsolicited_Vaccination_Record_Update
            //For sending new and/ or updated patient demographic information and immunizations.
            //VXU may also be used to delete immunizations and may be sent with only patient demographic information.

            VXU_V04 HL7VXUMessage251 = new VXU_V04();

            #region MSH - Message Header Required
            //MSH-1
            HL7VXUMessage251.MSH.FieldSeparator.Value = "|";
            //MSH-2
            HL7VXUMessage251.MSH.EncodingCharacters.Value = @"^~\&";
            //MSH-4 site-specific code
            HL7VXUMessage251.MSH.SendingFacility.NamespaceID.Value = "77700001";
            //MSH-7
            HL7VXUMessage251.MSH.DateTimeOfMessage.Time.Set(DateTime.Now, "yyyyMMddhhmmss[.s[s[s[s]]]] +/ -zzzz)");
            //MSH-9
            HL7VXUMessage251.MSH.MessageType.MessageCode.Value = "VXU";
            HL7VXUMessage251.MSH.MessageType.TriggerEvent.Value = "V04";
            HL7VXUMessage251.MSH.MessageType.MessageStructure.Value = "VXU_V04";
            //MSH-10
            HL7VXUMessage251.MSH.MessageControlID.Value = _RxRecord.FillFactKey;
            //MSH-11
            HL7VXUMessage251.MSH.ProcessingID.ProcessingID.Value = "P";
            //MSH-12
            HL7VXUMessage251.MSH.VersionID.VersionID.Value = "2.5.1";
            //MSH-15
            HL7VXUMessage251.MSH.AcceptAcknowledgmentType.Value = "ER";
            //MSH-16
            HL7VXUMessage251.MSH.ApplicationAcknowledgmentType.Value = "AL";
            #endregion 

            #region PID - Patient Identification Required

            // PID-3 Patient Identifier List 3.1(ID), 3.5(TypeCode) Required
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IDNumber.Value = _RxRecord.PatientID;//set an ID 
            HL7VXUMessage251.PID.GetPatientIdentifierList(0).IdentifierTypeCode.Value = "PI";//set an ID Type Patient Internal Identifier

            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IDNumber.Value = _RxRecord.MedicaidID;//set an ID
            HL7VXUMessage251.PID.GetPatientIdentifierList(1).IdentifierTypeCode.Value = "MA";//set an ID Type Medicaid Number

            // PID-5 Patient Name Required
            HL7VXUMessage251.PID.GetPatientName(0).GivenName.Value = _RxRecord.FirstName;
            HL7VXUMessage251.PID.GetPatientName(0).SecondAndFurtherGivenNamesOrInitialsThereof.Value = _RxRecord.MiddleName;
            HL7VXUMessage251.PID.GetPatientName(0).FamilyName.Surname.Value = _RxRecord.LastName;
            HL7VXUMessage251.PID.GetPatientName(0).NameTypeCode.Value = "L";

            //PID-7 Date of Birth Required
            HL7VXUMessage251.PID.DateTimeOfBirth.Time.SetShortDate(DateTime.ParseExact(_RxRecord.BirthDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));

            // PID-8 Sex (Gender) Required
            HL7VXUMessage251.PID.AdministrativeSex.Value = _RxRecord.immPatientGender;

            // PID-11 Patient Address Strongly Encourage
            HL7VXUMessage251.PID.GetPatientAddress(0).StreetAddress.StreetOrMailingAddress.Value = _RxRecord.StreetAddressLine;
            HL7VXUMessage251.PID.GetPatientAddress(0).City.Value = _RxRecord.City;
            HL7VXUMessage251.PID.GetPatientAddress(0).StateOrProvince.Value = _RxRecord.immPatientSate;
            HL7VXUMessage251.PID.GetPatientAddress(0).ZipOrPostalCode.Value = _RxRecord.ZipCode;
            HL7VXUMessage251.PID.GetPatientAddress(0).Country.Value = "USA";
            HL7VXUMessage251.PID.GetPatientAddress(0).AddressType.Value = "C";
            #endregion

            #region [PD1] Patient Additional Demographic Optional
            //PD1-12
            HL7VXUMessage251.PD1.ProtectionIndicator.Value = "Y"; //Unconsented
            #endregion

            #region [PV1] Patient Visit Segment Required
            //PV1-2
            HL7VXUMessage251.PATIENT.PV1.PatientClass.Value = "R";

            //PV1-20
            HL7VXUMessage251.PATIENT.PV1.GetFinancialClass(0).FinancialClassCode.Value = "V02";
            #endregion

            #region [{NK1}] - Next of Kin / Associated Parties Optional Repeating 
            #endregion

            #region ORC - Order Request Segment Required
            //ORC-1
            HL7VXUMessage251.GetORDER(0).ORC.OrderControl.Value = "RE";

            //ORC-3 Unique ID (16 Characters)
            HL7VXUMessage251.GetORDER(0).ORC.FillerOrderNumber.EntityIdentifier.Value = _RxRecord.FillFactKey;
            #endregion

            #region {RXA} - Pharmacy / Treatment Administration Required Repeating
            //RXA-1 Give Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.GiveSubIDCounter.Value = "0";
            //RXA-2 Administration Sub-ID Counter Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministrationSubIDCounter.Value = "1";
            // RXA-3 Date/Time Start of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeStartOfAdministration.Time.SetShortDate(DateTime.ParseExact(_RxRecord.VaccineDate, "MMddyyyy", System.Globalization.CultureInfo.InvariantCulture));
            // RXA-4 Date/Time End of Administration Required
            HL7VXUMessage251.GetORDER(0).RXA.DateTimeEndOfAdministration.Time.Set(DateTime.Now, "yyyyMMdd");
            // RXA-5 Administered Code Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Identifier.Value = _RxRecord.CVX;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.Text.Value = _RxRecord.CVXDescription;
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredCode.NameOfCodingSystem.Value = "CVX";
            // RXA-6 Administered Amount Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAmount.Value = _RxRecord.AdministeredAmount;
            // RXA-7 Administered Units Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredUnits.Identifier.Value = _RxRecord.AdministeredUnits;
            // RXA-9 Administered Notes Required
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Identifier.Value = "00";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).Text.Value = "NEW IMMUNIZATION RECORD";
            HL7VXUMessage251.GetORDER(0).RXA.GetAdministrationNotes(0).NameOfCodingSystem.Value = "NIP001";
            // RXA-11 Administered-at location Required
            HL7VXUMessage251.GetORDER(0).RXA.AdministeredAtLocation.Facility.NamespaceID.Value = "";
            // RXA-17 Vaccine Manufacturer Code Required
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Identifier.Value = "UNK";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).Text.Value = "Unknown manufacturer";
            HL7VXUMessage251.GetORDER(0).RXA.GetSubstanceManufacturerName(0).NameOfCodingSystem.Value = "MVX";
            #endregion

            #region [RXR] - Pharmacy / Treatment Route (Only one RXR per PXA segment) Optional
            #endregion

            #region [{OBX}] Observation/Result Required Reapeating
            //OBX-1
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.SetIDOBX.Value = "1";

            //OBX-2
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ValueType.Value = "CE";

            //OBX-3.1-3.3 What Oberservation Referes to, Poses Question for OBX-5
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Identifier.Value = "64994-7";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.Text.Value = "vaccine fund pgm elig cat";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationIdentifier.NameOfCodingSystem.Value = "LN";

            //OBX-4
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationSubID.Value = "1";

            //OBX-5 Observation Value, Answers Question Posed by OBX-3
            NHapi.Model.V251.Datatype.CE OBX5_CE = new NHapi.Model.V251.Datatype.CE(HL7VXUMessage251);
            OBX5_CE.Identifier.Value = "V04";
            OBX5_CE.Text.Value = "VFC eligible NA/AN";
            OBX5_CE.NameOfCodingSystem.Value = "HL70064";
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.GetObservationValue(0).Data = OBX5_CE;

            //OBX-11
            HL7VXUMessage251.GetORDER(0).GetOBSERVATION(0).OBX.ObservationResultStatus.Value = "F";
            #endregion

            return HL7VXUMessage251;

        }
    }
}