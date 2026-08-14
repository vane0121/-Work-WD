// CamstarTransactionRepository.cs
using DocumentFormat.OpenXml.Wordprocessing;
using M2OSS.DTO.WMS;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using WDHelpers.Mitecs3Helper;



namespace M2OSS.Repository.Repository
{
    public class CamstarTransactionRepository : ICamstarTransactionRepository
    {

        private readonly IMitecs3SecureData _mitecs3SecureData;
        private readonly IMitecs3Data _mitecs3Data;



        public CamstarTransactionRepository(IMitecs3SecureData mitecs3SecureData, IMitecs3Data mitecs3Data)
        {

            _mitecs3SecureData = mitecs3SecureData;
            _mitecs3Data = mitecs3Data;

        }

        // Mitecs signals a successful transaction in one of two ways: either by returning
        // a null / empty / whitespace message, or by returning a message that contains
        // the word 'success' (matches 'success' and 'successful', case-insensitive).
        // Anything else is treated as a failure.
        //
        // Benign Camstar error codes that should NOT abort the calling flow:
        //   - LotModifyAttrs_E0020 : "There are no changes to modify for <lot>".
        //     Emitted by SetLotAttribute when the supplied values already match
        //     the lot's current attributes - effectively a no-op success from
        //     our point of view, so we swallow it here rather than forcing
        //     every caller to wrap the call in its own try/catch.
        private static readonly string[] BenignErrorCodes =
        {
            "LotModifyAttrs_E0020",
        };

        private static bool IsSuccessMessage(string outMessage)
        {
            if (string.IsNullOrWhiteSpace(outMessage))
                return true;

            if (outMessage.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            foreach (var benign in BenignErrorCodes)
            {
                if (outMessage.IndexOf(benign, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }



        public Task<bool> AuthenticateAsync(string id, string key)
        {
            try
            {
                var authenticated = _mitecs3SecureData.Authenticate(id, key, out Mitecs3User user, out string message);
                if (!string.IsNullOrEmpty(message) && message.Contains("not found"))
                    authenticated = false;
                return Task.FromResult(authenticated);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IEnumerable<string>> GetRegisteredOperationsAsync()
        {
            var result = _mitecs3Data.GetRegisteredOperations();
            return Task.FromResult(result);
        }

        public Task<DataSet> GetLotDetailsAsync(string lotNumber)
        {
            var lotDetails = _mitecs3Data.GetLotDetails(lotNumber, DateTime.Now, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetLotDetails error for lot {lotNumber}: {outMessage}");
            }

            return Task.FromResult(lotDetails);
        }

        public Task<DataSet> GetProductsAsync()
        {
            var products = _mitecs3Data.GetProducts(out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetProducts error: {outMessage}");
            }

            return Task.FromResult(products);
        }

        public Task<IEnumerable<MaterialPartNumbers>> GetMaterialPartNumberAsync()
        {
            var rawValue = ConfigurationManager.AppSettings["UomList"];
            string[] uoms = rawValue.Split(',').Where(w => w != "").Select(x => x.Trim()).ToArray();

            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialPartNumbers, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialPartNumbers) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialPartNumbers>>(
                    Enumerable.Empty<MaterialPartNumbers>());
            }

            DataTable table = data.Tables[0];
            Random rand = new Random();

            IEnumerable<MaterialPartNumbers> result = table.AsEnumerable()
                .Where(w => w.Field<string>("MATERIALPART_NAME").Contains("XC"))
                .Select(row => new MaterialPartNumbers
                {
                    MaterialPartNumber = row.Field<string>("MATERIALPART_NAME"),
                    MaterialPartDescription = row.Field<string>("MATERIALPART_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status"),
                    WithExpiration = Convert.ToBoolean(rand.Next(0, 2)),
                    WithInspection = Convert.ToBoolean(rand.Next(0, 2)),
                    IsLotControlled = Convert.ToBoolean(rand.Next(0, 2)),
                    Uom = uoms[rand.Next(0, uoms.Count())]
                })
                .ToList();

            return Task.FromResult(result);
        }

        public IEnumerable<MaterialPartNumbers> ReadCsv(string filePath)
        {

            var lines = File.ReadAllLines(filePath);

            var result = new List<MaterialPartNumbers>();

            if (lines.Length <= 1)
                return result; // empty or no data

            // First line contains headers
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

            var props = typeof(MaterialPartNumbers).GetProperties();

            foreach (var line in lines.Skip(1))
            {
                var cleanLine = line.Replace(",", "");
                if (string.IsNullOrWhiteSpace(cleanLine))
                    continue;

                var values = line.Split(',');

                MaterialPartNumbers obj = new MaterialPartNumbers();

                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    var header = headers[i];
                    var value = values[i].Trim();

                    var prop = props.FirstOrDefault(p =>
                        p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

                    if (prop != null && !string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            object convertedValue;

                            if (prop.PropertyType == typeof(bool))
                            {
                                // Handle common boolean text patterns
                                if (value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("1"))
                                {
                                    convertedValue = true;
                                }
                                else if (value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("No", StringComparison.OrdinalIgnoreCase) ||
                                         value.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                                         value.Equals("0"))
                                {
                                    convertedValue = false;
                                }
                                else
                                {
                                    // fallback to standard parsing (true/false)
                                    convertedValue = Convert.ToBoolean(value);
                                }
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value.ToUpper(), prop.PropertyType);
                            }

                            prop.SetValue(obj, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            continue;
                            // handle invalid conversions gracefully (e.g. non-numeric Age)
                        }
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        public Task<IEnumerable<Owner>> GetOwnerAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.Owner, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(Owner) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Owner>>(Enumerable.Empty<Owner>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Owner> result = table.AsEnumerable()
                .Where(r => r.Field<string>("OWNER_DESCRIPTION").ToUpper() == "WMS")
                .Select(row => new Owner
                {
                    OwnerName = row.Field<string>("OWNER"),
                    OwnerDescription = row.Field<string>("OWNER_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }



        public Task<IEnumerable<Factory>> GetFactoryAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.Factory, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(Factory) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Factory>>(Enumerable.Empty<Factory>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Factory> result = table.AsEnumerable().Select(row => new Factory
            {
                FactoryName = row.Field<string>("FactoryName"),
                Description = row.Field<string>("Description"),
                Revision = row.Field<string>("Revision"),
                Status = row.Field<string>("Status")
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Hold>> GetHoldReasonAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.HoldCategory, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(HoldCategory) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Hold>>(Enumerable.Empty<Hold>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Hold> result = table.AsEnumerable()
                .Where(r => r.Field<string>("HOLD_CATEGORY_NAME").ToUpper().Contains("WMS"))
                .Select(row => new Hold
                {
                    HoldCategory = row.Field<string>("HOLD_CATEGORY_NAME"),
                    HoldDescription = row.Field<string>("HOLD_CATEGORY_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Defect>> GetDefectCodeAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.TerminateReasons, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(TerminateReasons) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Defect>>(Enumerable.Empty<Defect>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Defect> result = table.AsEnumerable()
                .Where(r => r.Field<string>("TERMINATE_CATEGORY_NAME").ToUpper().Contains("WMS"))
                .Select(row => new Defect
                {
                    DefectCode = row.Field<string>("TERMINATE_CATEGORY_NAME"),
                    DefectDescription = row.Field<string>("TERMINATE_CATEGORY_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public Task<(string, bool)> CreateMaterialLotAsync(MaterialDetails material, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.CreateIndirectMaterialLot(xml.ToString(), out string outMessage);
            return Task.FromResult((outMessage, res));
        }

        public Task<(string message, bool result)> AdjustLotQuantityAsync(MaterialDetails material, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.AdjustMaterialLotQuantity(material.LotId, xml.ToString(), out string outMessage);
            return Task.FromResult<(string message, bool result)>((outMessage, res));
        }

        public Task<bool> SetMaterialLotAttributeAsync(MaterialDetails material, string step, XDocument xml)
        {
            bool res = _mitecs3Data.SetLotAttribute(step, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"SetLotAttribute error for lot {material?.LotId} at step {step}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        public Task<bool> MaterialInventoryMoveAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.MaterialInventoryMove("", xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"MaterialInventoryMove error for lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        // Adds additional wash tray (carrier) lots to an existing virtual lot via Mitecs RePack.
        // The xml payload must follow the schema documented in ICamstarTransactionRepository.RePackAsync.
        public Task<bool> RePackAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.RePack(xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"RePack error for virtual lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }



        public Task<MaterialDetails> GetMaterialLotByLotIdAsync(MaterialDetails material, XDocument xml)
        {
            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for lot {material?.LotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            MaterialDetails materials = dt.AsEnumerable()
                .Where(w => w.Field<string>("CONTAINERNAME").ToString() == material.LotId)
                .Select(row =>
                {
                        

                        return new MaterialDetails
                        {
                            LotId = row["CONTAINERNAME"].ToString(),
                            Quantity = Convert.ToInt32(row["QTY"]),
                            WorkflowStep = row["SPECNAME"].ToString(),
                            PartNumber = row["PRODUCTNAME"].ToString(),
                            OwnerName = row["OWNERNAME"].ToString(),
                            Uom = row["UOMNAME"].ToString(),
                            PoNumber = row["WMSPONUMBER"].ToString(),
                            PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                            InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                            WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                            DrNumber = row["WMSDRNUMBER"].ToString(),
                            LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                            ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                            WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                            PalletId = row["WMSPALLETID"].ToString(),
                            ExpirationDate = row["WMSEXPIRATIONDATE"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                            Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                            Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                            ParentLotId = row["WMSPARENTLOTID"].ToString(),
                            OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                            FactoryName = row["FACTORYNAME"].ToString(),
                            Vendor = row["VENDORNAME"].ToString(),
                            Description = row["DESCRIPTION"]?.ToString(),
                            SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                            TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                            RequestedQuantity = row["WMSRequestQty"].ToString() == "" ? 0 : Convert.ToInt32(row["WMSRequestQty"]),
                            DateRequest = row["WMSDateRequest"].ToString() == "" ? DateTime.MinValue.ToString("yyyy-MM-dd hh:mm:ss") : row["WMSDateRequest"].ToString().Substring(0, 19),
                            TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                            RequestorID = row["WMSRequestorId"].ToString(),
                            PlannerID = row["WMSPlannerId"].ToString(),
                            actionHistory = row["WMSActionHistory"]?.ToString(),


                        };

                }).FirstOrDefault();

            return Task.FromResult(materials);
        }

        public Task<IEnumerable<MaterialDetails>> GetMaterialLotsByFilterAsync(MaterialDetails material, XDocument xml)
        {
            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for part {material?.PartNumber} at step {material?.WorkflowStep}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            IEnumerable<MaterialDetails> materials = dt.AsEnumerable().Select(row =>
            {
                        //DateTime expirationDate;
                        //DateTime.TryParse(row["WMSExpirationDate"]?.ToString(), out expirationDate);

                        return new MaterialDetails
                        {
                            LotId = row["CONTAINERNAME"].ToString(),
                            Quantity = Convert.ToInt32(row["QTY"]),
                            WorkflowStep = row["SPECNAME"].ToString(),
                            PartNumber = row["PRODUCTNAME"].ToString(),
                            OwnerName = row["OWNERNAME"].ToString(),
                            Uom = row["UOMNAME"].ToString(),
                            PoNumber = row["WMSPONUMBER"].ToString(),
                            PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                            InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                            WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                            DrNumber = row["WMSDRNUMBER"].ToString(),
                            LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                            ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                            WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                            PalletId = row["WMSPALLETID"].ToString(),
                            ExpirationDate = row["WMSEXPIRATIONDATE"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                            Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                            Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                            ParentLotId = row["WMSPARENTLOTID"].ToString(),
                            OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                            FactoryName = row["FACTORYNAME"].ToString(),
                            Vendor = row["VENDORNAME"].ToString(),
                            Description = row["DESCRIPTION"]?.ToString(),
                            SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                            TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                            RequestedQuantity = row["WMSRequestQty"].ToString() == "" ? 0 : Convert.ToInt32(row["WMSRequestQty"]),
                            DateRequest = row["WMSDateRequest"].ToString() == "" ? DateTime.MinValue.ToString("yyyy-MM-dd hh:mm:ss") : row["WMSDateRequest"].ToString().Substring(0, 19),
                            TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                            RequestorID = row["WMSRequestorId"].ToString(),
                            PlannerID = row["WMSPlannerId"].ToString(),
                            actionHistory = row["WMSActionHistory"].ToString(),



                        };

            }).ToList();

            return Task.FromResult(materials);
        }

        public Task<MaterialDetails> GetMaterialLotAttributeAsync(string lotId)
        {
            string poAttribute = ConfigurationManager.AppSettings["wmsPoAttribute"];
            string poLineAttribute = ConfigurationManager.AppSettings["wmsPoLineAttribute"];
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string waybillAttribute = ConfigurationManager.AppSettings["wmsWaybillAttribute"];
            string drAttribute = ConfigurationManager.AppSettings["wmsDrNumberAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string locationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string receiptKeyAttribute = ConfigurationManager.AppSettings["wmsReceiptKeyAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];
            string expirationAttribute = ConfigurationManager.AppSettings["wmsExpirationAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string remarksAttribute = ConfigurationManager.AppSettings["wmsRemarksAttribute"];
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string uomAttribute = ConfigurationManager.AppSettings["wmsUomAttribute"];
            string vendorAttribute = ConfigurationManager.AppSettings["wmsVendorAttribute"];
            string factoryAttribute = ConfigurationManager.AppSettings["wmsFactoryAttribute"];
            string picAttribute = ConfigurationManager.AppSettings["wmsPicAttribute"];
            string receiveDateAttribute = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string picNameAttribute = ConfigurationManager.AppSettings["wmsPicNameAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string defectCodeAttribute = ConfigurationManager.AppSettings["wmsDefectCodeAttribute"];
            string deliveryTypeAttribute = ConfigurationManager.AppSettings["wmsDeliveryTypeAttribute"];
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string requestedQtyAttribute = ConfigurationManager.AppSettings["epullRequestedQtyAttribute"];
            string requestedNoteAttribute = ConfigurationManager.AppSettings["epullRequestedNoteNumberAttribute"];
            string issuanceStatusAttribute = ConfigurationManager.AppSettings["wmsIssuanceStatusAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];
            string referenceLotAttribute = ConfigurationManager.AppSettings["referenceLotAttribute"];

            DataSet data = _mitecs3Data.GetLotAttributes("PWH_0001", lotId, out string outMessage);

            if (!outMessage.Contains("successful"))
            {
                throw new InvalidOperationException(
                    $"GetLotAttributes error for lot {lotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            var propertyMap = new Dictionary<string, string>
            {
                            { "PalletId", palletAttribute.Substring(0, palletAttribute.IndexOf(';'))},
                            { "LotNumber", lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';'))},
                            { "PoNumber", poAttribute.Substring(0, poAttribute.IndexOf(';')) },
                            { "PoLineNumber", poLineAttribute.Substring(0, poLineAttribute.IndexOf(';'))},
                            { "InvoiceNumber", invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';')) },
                            { "WaybillNumber", waybillAttribute.Substring(0, waybillAttribute.IndexOf(';')) },
                            { "DrNumber", drAttribute.Substring(0, drAttribute.IndexOf(';')) },
                            { "ReceivingLocation", locationAttribute.Substring(0, locationAttribute.IndexOf(';')) },
                            { "WmsKeyNumber", receiptKeyAttribute.Substring(0, receiptKeyAttribute.IndexOf(';'))},
                            { "ExpirationDate", expirationAttribute.Substring(0, expirationAttribute.IndexOf(';'))},
                            { "Category", categoryAttribute.Substring(0, categoryAttribute.IndexOf(';')) },
                            { "FactoryName", factoryAttribute.Substring(0, factoryAttribute.IndexOf(';')) },
                            { "Remarks", remarksAttribute.Substring(0, remarksAttribute.IndexOf(';'))},
                            { "OtherRemarks",dispositionRemarksAttribute.Substring(0, dispositionRemarksAttribute.IndexOf(';')) },
                            { "ParentLotId",parentLotAttribute.Substring(0, parentLotAttribute.IndexOf(';')) },
                            { "Uom", uomAttribute.Substring(0, uomAttribute.IndexOf(';'))},
                            { "Vendor", vendorAttribute.Substring(0, vendorAttribute.IndexOf(';')) },
                            { "ReceivedBy", picAttribute.Substring(0, picAttribute.IndexOf(';')) },
                            { "DateReceive", receiveDateAttribute.Substring(0, receiveDateAttribute.IndexOf(';')) },
                            { "OwnerEmail", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';')) },
                            { "ReceiverName", picNameAttribute.Substring(0, picNameAttribute.IndexOf(';')) },
                            { "DefectCode", defectCodeAttribute.Substring(0, defectCodeAttribute.IndexOf(';')) },
                            { "DeliveryType", deliveryTypeAttribute.Substring(0, deliveryTypeAttribute.IndexOf(';')) },
                            { "PreviousOperation", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';')) },
                            { "BoxId", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';')) },
                            { "RequestedQuantity", requestedQtyAttribute.Substring(0, requestedQtyAttribute.IndexOf(';')) },
                            { "ReqNotes", requestedNoteAttribute.Substring(0, requestedNoteAttribute.IndexOf(';')) },
                            { "IssuanceStatus", issuanceStatusAttribute.Substring(0, issuanceStatusAttribute.IndexOf(';')) },
                            { "actionHistory", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';')) },
                { "ReferenceLotNumber", referenceLotAttribute.Substring(0, referenceLotAttribute.IndexOf(';')) }
            };

            MaterialDetails material = MapAttributesToModel<MaterialDetails>(dt, propertyMap);

            return Task.FromResult(material);
        }

        public Task<(bool result, string message)> SplitLotAsync(MaterialDetails sourceMaterial, MaterialDetails newMaterial, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.LotSplit("", xml.ToString(), out string outMessage);
            return Task.FromResult<(bool result, string message)>((res, outMessage));
        }

        public Task<bool> SellLotAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.SellLots(xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"SellLots error for lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        public Task<bool> AdjustQuantityAsync(string lotId, XDocument xml)
        {
            var res = _mitecs3Data.AdjustMaterialLotQuantity(lotId, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"AdjustMaterialLotQuantity error for lot {lotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }
        public Task<IEnumerable<MaterialDetails>> GetMaterialLotByTicketAsync(MaterialDetails material)
        {
            // Synchronous call into the Mitecs helper DLL
            var data = _mitecs3Data.GetMasterLotSetup(
                LotSetupFilterType.MaterialLotByTicket,
                material.TicketNumber,
                out string msg);

            // If msg has a value, treat it as an error
            if (!IsSuccessMessage(msg))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup error for ticket {material.TicketNumber}: {msg}");
            }

            // No data -> return empty
            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialDetails>>(
                    Enumerable.Empty<MaterialDetails>());
            }

            DataTable dt = data.Tables[0];

            // NOTE: The Mitecs helper returns QTY (and other numeric columns) as the driver's
            // native numeric type (typically decimal or string) - NOT as Int32. Using
            // row.Field<int?>("QTY") would attempt a hard unbox and throw InvalidCastException.
            // Convert.ToInt32 is type-tolerant (handles decimal/double/string/DBNull) and
            // matches the pattern used by GetMaterialLotByLotIdAsync / GetMaterialLotsByFilterAsync.
            IEnumerable<MaterialDetails> materials = dt.AsEnumerable().Select(row => new MaterialDetails
            {
                LotId = row["CONTAINERNAME"]?.ToString(),
                Quantity = row["QTY"] == DBNull.Value || row["QTY"].ToString() == "" ? 0 : Convert.ToInt32(row["QTY"]),
                WorkflowStep = row["SPECNAME"]?.ToString(),
                PartNumber = row["PRODUCTNAME"]?.ToString(),
                OwnerName = row["OWNERNAME"]?.ToString(),
                Description = row["DESCRIPTION"]?.ToString(),
                ReqNotes = row["WMSRequestorNotes"]?.ToString(),
                SupplierLotNum = row["WMSSUPPLIERLOTNUMBER"]?.ToString(),
                DateRequest = row["WMSDateRequest"]?.ToString(),
                TicketNumber = material.TicketNumber,
                RequestedQuantity = row["QTY"] == DBNull.Value || row["QTY"].ToString() == "" ? 0 : Convert.ToInt32(row["QTY"]),
                PlannerID = row["WMSPlannerId"]?.ToString(),
                RequestorID = row["WMSRequestorId"]?.ToString(),
            }).ToList();

            return Task.FromResult(materials);
        }


        

        public Task<IEnumerable<MaterialDetails>> GetSubmittedTicketsAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialTickets, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialTickets) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialDetails>>(Enumerable.Empty<MaterialDetails>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<MaterialDetails> result = table.AsEnumerable().Select(row => new MaterialDetails
            {
                TicketNumber = row.Field<string>("WMSMaterialTicket"),
                TicketStatus = row.Field<string>("WMSMaterialTicketStatus"),
                RequestorID = row.Field<string>("WMSRequestorID"),
                PlannerID = row.Field<string>("WMSPlannerID"),
                DateRequest = row.Field<string>("WMSDateRequest"),
                WorkflowStep = row.Field<string>("SPECNAME"),
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<MaterialTicket>> GetAllApprovedMaterialTicketAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialTickets, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialTickets) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialTicket>>(Enumerable.Empty<MaterialTicket>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<MaterialTicket> result = table.AsEnumerable()
                .Where(w => !string.IsNullOrEmpty(w.Field<string>("WMSDATEREQUEST")))
                /*.Where(w => w.Field<string>("SPECNAME") == "PWH_0006" && w.Field<string>("WMSMaterialTicketStatus").ToUpper() == "APPROVED")*/
                .Select(row => new MaterialTicket
                {
                    MaterialTicketId = row.Field<string>("WMSMaterialTicket"),
                    TicketStatus = row.Field<string>("WMSMaterialTicketStatus"),
                    RequestorId = row.Field<string>("WMSRequestorID"),
                    PlannerId = row.Field<string>("WMSPlannerID"),
                    DateRequest = Convert.ToDateTime(row.Field<string>("WMSDateRequest")),
                    WorkflowStep = row.Field<string>("SPECNAME"),
                })
                .ToList();

            return Task.FromResult(result);
        }


        public Task<IEnumerable<LotHistory>> GetLotHistoryByLotIdAsync(string lotId)
        {
            var data = _mitecs3Data.GetLotHistory(lotId, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetLotHistory error for lot {lotId}: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<LotHistory>>(Enumerable.Empty<LotHistory>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<LotHistory> result = table.AsEnumerable().Select(row => new LotHistory
            {

                    Id = Convert.ToInt32(row.Field<string>("No")),
                    LotId = row.Field<string>("LOTID"),
                    Transaction = row.Field<string>("SERVICE"),
                    TransactionDate = row.Field<string>("TXNDATE"),
                    User = row.Field<string>("USER"),
                    FromWorkflowStep = row.Field<string>("FROMSTEP"),
                    ToWorkflowStep = row.Field<string>("TOSTEP"),
                    Equipment = row.Field<string>("EQUIPMENT"),
                    FromQuantity = string.IsNullOrWhiteSpace(row.Field<string>("FROMQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("FROMQTY")),
                    Quantity = string.IsNullOrWhiteSpace(row.Field<string>("QTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("QTY")),
                    FromQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("FROMQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("FROMQTY2")),
                    Quantity2 = string.IsNullOrWhiteSpace(row.Field<string>("QTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("QTY2")),
                    Shift = row.Field<string>("SHIFT"),
                    AttributeModified = row.Field<string>("ATTR_MODIFIED"),
                    AttributeNewValue = row.Field<string>("ATTR_NEWVALUE"),
                    AttributeOldValue = row.Field<string>("ATTR_OLDVALUE"),
                    ReasonName = row.Field<string>("REASONNAME"),
                    LotStatus = row.Field<string>("LOTSTATUS"),
                    TargetLot = row.Field<string>("TARGETLOT"),
                    TargetLotQuantity = string.IsNullOrWhiteSpace(row.Field<string>("TARGETLOTQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("TARGETLOTQTY")),
                    TargetLotQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("TARGETLOTQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("TARGETLOTQTY2")),
                    SourceLot = row.Field<string>("SOURCELOT"),
                    SourceLotQuantity = string.IsNullOrWhiteSpace(row.Field<string>("SOURCELOTQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("SOURCELOTQTY")),
                SourceLotQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("SOURCELOTQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("SOURCELOTQTY2")),
            }).ToList();

            return Task.FromResult(result);
        }


        public static T MapAttributesToModel<T>(DataTable table, Dictionary<string, string> map) where T : new()
        {
            T model = new T();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                if (!map.TryGetValue(prop.Name, out var attributeName))
                    continue;

                var row = table.AsEnumerable()
                               .FirstOrDefault(r => string.Equals(r.Field<string>("ATTRIBUTE_NAME"), attributeName, StringComparison.OrdinalIgnoreCase));

                if (row == null)
                    continue;

                var val = row.Field<string>("ATTRIBUTE_VALUE");
                if (string.IsNullOrEmpty(val))
                    continue;

                try
                {

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object converted = null;

                    if (val == null)
                    {
                        converted = null;
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        DateTime dt;
                        string[] formats = { "MM/dd/yyyy",
                                              "MM/d/yyyy",
                                              "M/dd/yyyy",

                                              "M/dd/yyyy hh:mm:ss tt",
                                              "M/dd/yyyy h:mm:ss tt",

                                              "MM/d/yyyy hh:mm:ss tt",
                                              "MM/d/yyyy h:mm:ss tt",

                                              "MM/dd/yyyy hh:mm:ss tt",
                                              "MM/dd/yyyy h:mm:ss tt",

                                              "MM/d/yyyy hh:mm:ss tt",
                                              "MM/d/yyyy h:mm:ss tt",

                                              "M/d/yyyy hh:mm:ss tt",
                                              "M/d/yyyy h:mm:ss tt",

                                              "M/dd/yyyy h:mm:ss",
                                              "MM/d/yyyy h:mm:ss",
                                              "MM/dd/yyyy h:mm:ss",
                                              "MM/dd/yyyy hh:mm:ss" };
                        if (DateTime.TryParseExact(val.ToString(),
                                formats,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out dt))
                            converted = dt;
                        else
                            converted = null; // or throw error if you prefer
                    }
                    else
                    {
                        converted = Convert.ChangeType(val, targetType);
                    }

                    prop.SetValue(model, converted);
                    //var converted = Convert.ChangeType(val, prop.PropertyType);
                    prop.SetValue(model, converted);
                }
                catch (Exception ex)
                {
                    //optional: log conversion error
                }
            }

            return model;
        }


        public Task<IEnumerable<EquipmentFamily>> GetEquipmentFamilyList()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.BackgrindEquipmentFamily, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(BackgrindEquipmentFamily) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<EquipmentFamily>>(Enumerable.Empty<EquipmentFamily>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<EquipmentFamily> result = table.AsEnumerable().Select(row => new EquipmentFamily
            {
                EquipmentFamilyName = row.Field<string>("BACKGRIND_EQUIPMENTFAMILY_NAME")
            }).ToList();

            return Task.FromResult(result);
        }


        public Task<IEnumerable<EquipmentTools>> GetEquipmentByFamily(string equiptmentFamily)
        {
            var data = _mitecs3Data.GetMasterLotSetup(
                LotSetupFilterType.BackgrindEquipmentByFamily,
                equiptmentFamily,
                out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(BackgrindEquipmentByFamily) error for family {equiptmentFamily}: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<EquipmentTools>>(Enumerable.Empty<EquipmentTools>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<EquipmentTools> result = table.AsEnumerable()
                .Where(row => row.Field<string>("RESOURCEFAMILYNAME") == equiptmentFamily)
                .Select(row => new EquipmentTools
                {
                    ResourceId = row.Field<string>("RESOURCEID"),
                    ResourceName = row.Field<string>("RESOURCENAME"),
                    ResourceFamilyName = row.Field<string>("RESOURCEFAMILYNAME")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public async Task<bool> ProcessMaterialWithdrawalByLotIdAsync(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "1";
                    var res = _mitecs3Data.MaterialWithdrawal(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<bool> LotCombine(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "1";
                    var res = _mitecs3Data.LotCombine(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }


        public async Task<bool> SetupIDMaterialsToEquipment(string lotNumber, int lotQty, string equipment)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string outMessage;

                    var res = _mitecs3Data.SetupIDMaterialsToEquipment(
                        lotNumber,
                        lotQty,
                        equipment,
                        out outMessage
                    );

                    return res;
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> ConsumeMaterials(XDocument xml)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string outMessage;
                    var res = _mitecs3Data.ConsumeMaterials(xml.ToString(), out outMessage);
                    return res;
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> MaterialInventoryMove(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "";
                    var res = _mitecs3Data.MaterialInventoryMove(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public Task<MaterialDetails> GetMaterialLotAttributeInControllerReceivingAsync(string lotId)
        {
            string poAttribute = ConfigurationManager.AppSettings["wmsPoAttribute"];
            string poLineAttribute = ConfigurationManager.AppSettings["wmsPoLineAttribute"];
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string waybillAttribute = ConfigurationManager.AppSettings["wmsWaybillAttribute"];
            string drAttribute = ConfigurationManager.AppSettings["wmsDrNumberAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string locationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string receiptKeyAttribute = ConfigurationManager.AppSettings["wmsReceiptKeyAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];
            string expirationAttribute = ConfigurationManager.AppSettings["wmsExpirationAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string remarksAttribute = ConfigurationManager.AppSettings["wmsRemarksAttribute"];
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string uomAttribute = ConfigurationManager.AppSettings["wmsUomAttribute"];
            string vendorAttribute = ConfigurationManager.AppSettings["wmsVendorAttribute"];
            string factoryAttribute = ConfigurationManager.AppSettings["wmsFactoryAttribute"];
            string picAttribute = ConfigurationManager.AppSettings["wmsPicAttribute"];
            string receiveDateAttribute = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string picNameAttribute = ConfigurationManager.AppSettings["wmsPicNameAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string defectCodeAttribute = ConfigurationManager.AppSettings["wmsDefectCodeAttribute"];
            string deliveryTypeAttribute = ConfigurationManager.AppSettings["wmsDeliveryTypeAttribute"];
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string requestedQtyAttribute = ConfigurationManager.AppSettings["epullRequestedQtyAttribute"];
            string requestedNoteAttribute = ConfigurationManager.AppSettings["epullRequestedNoteNumberAttribute"];
            string issuanceStatusAttribute = ConfigurationManager.AppSettings["wmsIssuanceStatusAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];
            string referenceLotAttribute = ConfigurationManager.AppSettings["referenceLotAttribute"];

            DataSet data = _mitecs3Data.GetLotAttributes("PWH_0010", lotId, out string outMessage);

            if (!outMessage.Contains("successful"))
            {
                throw new InvalidOperationException(
                    $"GetLotAttributes error for lot {lotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            var propertyMap = new Dictionary<string, string>
            {
                            { "PalletId", palletAttribute.Substring(0, palletAttribute.IndexOf(';'))},
                            { "LotNumber", lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';'))},
                            { "PoNumber", poAttribute.Substring(0, poAttribute.IndexOf(';')) },
                            { "PoLineNumber", poLineAttribute.Substring(0, poLineAttribute.IndexOf(';'))},
                            { "InvoiceNumber", invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';')) },
                            { "WaybillNumber", waybillAttribute.Substring(0, waybillAttribute.IndexOf(';')) },
                            { "DrNumber", drAttribute.Substring(0, drAttribute.IndexOf(';')) },
                            { "ReceivingLocation", locationAttribute.Substring(0, locationAttribute.IndexOf(';')) },
                            { "WmsKeyNumber", receiptKeyAttribute.Substring(0, receiptKeyAttribute.IndexOf(';'))},
                            { "ExpirationDate", expirationAttribute.Substring(0, expirationAttribute.IndexOf(';'))},
                            { "Category", categoryAttribute.Substring(0, categoryAttribute.IndexOf(';')) },
                            { "FactoryName", factoryAttribute.Substring(0, factoryAttribute.IndexOf(';')) },
                            { "Remarks", remarksAttribute.Substring(0, remarksAttribute.IndexOf(';'))},
                            { "OtherRemarks",dispositionRemarksAttribute.Substring(0, dispositionRemarksAttribute.IndexOf(';')) },
                            { "ParentLotId",parentLotAttribute.Substring(0, parentLotAttribute.IndexOf(';')) },
                            { "Uom", uomAttribute.Substring(0, uomAttribute.IndexOf(';'))},
                            { "Vendor", vendorAttribute.Substring(0, vendorAttribute.IndexOf(';')) },
                            { "ReceivedBy", picAttribute.Substring(0, picAttribute.IndexOf(';')) },
                            { "DateReceive", receiveDateAttribute.Substring(0, receiveDateAttribute.IndexOf(';')) },
                            { "OwnerEmail", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';')) },
                            { "ReceiverName", picNameAttribute.Substring(0, picNameAttribute.IndexOf(';')) },
                            { "DefectCode", defectCodeAttribute.Substring(0, defectCodeAttribute.IndexOf(';')) },
                            { "DeliveryType", deliveryTypeAttribute.Substring(0, deliveryTypeAttribute.IndexOf(';')) },
                            { "PreviousOperation", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';')) },
                            { "BoxId", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';')) },
                            { "RequestedQuantity", requestedQtyAttribute.Substring(0, requestedQtyAttribute.IndexOf(';')) },
                            { "ReqNotes", requestedNoteAttribute.Substring(0, requestedNoteAttribute.IndexOf(';')) },
                            { "IssuanceStatus", issuanceStatusAttribute.Substring(0, issuanceStatusAttribute.IndexOf(';')) },
                            { "actionHistory", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';')) },
                { "ReferenceLotNumber", referenceLotAttribute.Substring(0, referenceLotAttribute.IndexOf(';')) }
            };

            MaterialDetails material = MapAttributesToModel<MaterialDetails>(dt, propertyMap);

            return Task.FromResult(material);
        }

        public async Task<bool> MaterialReturn(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "";
                    var res = _mitecs3Data.MaterialReturn(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }


        public Task<IEnumerable<MaterialDetails>> GetMaterialAllLots(MaterialDetails material, XDocument xml,int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 100 : pageSize;
            var skip = (pageNumber - 1) * pageSize;

            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for part {material?.PartNumber} at step {material?.WorkflowStep}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();
            
            IEnumerable<MaterialDetails> materials = dt.AsEnumerable()
                .Select(row => new MaterialDetails
                {
                    LotId = row["CONTAINERNAME"].ToString(),
                    Quantity = Convert.ToInt32(row["QTY"]),
                    WorkflowStep = row["SPECNAME"].ToString(),
                    PartNumber = row["PRODUCTNAME"].ToString(),
                    OwnerName = row["OWNERNAME"].ToString(),
                    Uom = row["UOMNAME"].ToString(),
                    PoNumber = row["WMSPONUMBER"].ToString(),
                    PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                    InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                    WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                    DrNumber = row["WMSDRNUMBER"].ToString(),
                    LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                    ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                    WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                    PalletId = row["WMSPALLETID"].ToString(),
                    ExpirationDate = string.IsNullOrEmpty(row["WMSEXPIRATIONDATE"].ToString())
                        ? (DateTime?)null
                        : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                    Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                    Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                    ParentLotId = row["WMSPARENTLOTID"].ToString(),
                    OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                    FactoryName = row["FACTORYNAME"].ToString(),
                    Vendor = row["VENDORNAME"].ToString(),
                    Description = row["DESCRIPTION"]?.ToString(),
                    SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                    TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                    RequestedQuantity = string.IsNullOrEmpty(row["WMSRequestQty"].ToString())
                        ? 0
                        : Convert.ToInt32(row["WMSRequestQty"]),
                    DateRequest = string.IsNullOrEmpty(row["WMSDateRequest"].ToString())
                        ? DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
                        : row["WMSDateRequest"].ToString().Substring(0, 19),
                    TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                    RequestorID = row["WMSRequestorId"].ToString(),
                    PlannerID = row["WMSPlannerId"].ToString(),
                    actionHistory = row["WMSActionHistory"].ToString()
                })
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(materials);
        }

        public async Task<bool> UnloadEquipmentMaterials(string lotnumber, string equipment, EquipmentSetupFilterType setupFilterType, string outMessage)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var res = _mitecs3Data.UnloadEquipmentMaterials(lotnumber, equipment, setupFilterType, out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public Task<IEnumerable<ConsumptionHistory>> GetConsumptionHistoryForExportAsync(XDocument xml)
        {
            try
            {
                string outMessage;

                var ds = _mitecs3Data.GetMaterialConsumption(
                    xml.ToString(),
                    out outMessage);

                DataTable dt = (ds != null && ds.Tables.Count > 0)
                    ? ds.Tables[0]
                    : new DataTable();

                IEnumerable<ConsumptionHistory> materials = dt.AsEnumerable()
                    .Select(row => new ConsumptionHistory
                    {
                        LotId = row["MaterialLot"]?.ToString(),
                        PartNumber = row["MaterialPart"]?.ToString(),
                        ConsumedQty = row["ConsumeFactor"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(row["ConsumeFactor"]),
                        OutputQty = row["QtyConsumed"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(row["QtyConsumed"]),
                        ToolNumber = row["Equipment"]?.ToString(),
                        DateTimeTransact = row["TxnDate"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(row["TxnDate"]),
                        TransactBy = row["Username"]?.ToString()
                    })
                    .ToList();

                return Task.FromResult(materials);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"GetConsumptionHistoryForExportAsync Error: {ex.Message}",
                    ex);
            }
        }




    }

}

// ICamstarTransactionRepository.cs
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WDHelpers.Mitecs3Helper;

namespace M2OSS.Repository.Camstar.Interface
{
    public interface ICamstarTransactionRepository
    {
        Task<DataSet> GetLotDetailsAsync(string lotNumber);
        Task<DataSet> GetProductsAsync();
        Task<IEnumerable<string>> GetRegisteredOperationsAsync();
        Task<bool> AuthenticateAsync(string id, string key);
        Task<IEnumerable<Owner>> GetOwnerAsync();
        Task<IEnumerable<Factory>> GetFactoryAsync();
        Task<IEnumerable<MaterialPartNumbers>> GetMaterialPartNumberAsync();
        Task<bool> MaterialInventoryMoveAsync(MaterialDetails material, XDocument xml);
        Task<(string,bool)> CreateMaterialLotAsync(MaterialDetails material, XDocument xml);

        Task<(string message, bool result)> AdjustLotQuantityAsync(MaterialDetails material, XDocument xml);
        Task<bool> SetMaterialLotAttributeAsync(MaterialDetails material, string step, XDocument xml);
        Task<IEnumerable<MaterialDetails>> GetMaterialLotsByFilterAsync(MaterialDetails material, XDocument xml);

        Task<MaterialDetails> GetMaterialLotByLotIdAsync(MaterialDetails material, XDocument xml);
        Task<MaterialDetails> GetMaterialLotAttributeAsync(string lotId);
        Task<(bool result, string message)> SplitLotAsync(MaterialDetails sourceMaterial, MaterialDetails newMaterial, XDocument xml);
        Task<bool> SellLotAsync(MaterialDetails material, XDocument xml);

        IEnumerable<MaterialPartNumbers> ReadCsv(string filePath);
        Task<IEnumerable<MaterialDetails>> GetMaterialLotByTicketAsync(MaterialDetails _materialDetails);
        Task<IEnumerable<MaterialDetails>> GetSubmittedTicketsAsync();
        Task<IEnumerable<Hold>> GetHoldReasonAsync();

        Task<IEnumerable<MaterialTicket>> GetAllApprovedMaterialTicketAsync();
        Task<IEnumerable<Defect>> GetDefectCodeAsync();
        Task<IEnumerable<LotHistory>> GetLotHistoryByLotIdAsync(string lotId);
        Task<bool> AdjustQuantityAsync(string lotId, XDocument xml);

        Task<IEnumerable<EquipmentFamily>> GetEquipmentFamilyList();
        Task<IEnumerable<EquipmentTools>> GetEquipmentByFamily(string equiptmentFamily);
        Task<bool> ProcessMaterialWithdrawalByLotIdAsync(XDocument xml);
        Task<bool> LotCombine(XDocument xml);
        Task<bool> SetupIDMaterialsToEquipment(string lotNumber, int lotQty, string equipment);
        Task<bool> ConsumeMaterials(XDocument xml);
        Task<bool> MaterialInventoryMove(XDocument xml);
        Task<MaterialDetails> GetMaterialLotAttributeInControllerReceivingAsync(string lotId);

        Task<bool> RePackAsync(MaterialDetails material, XDocument xml);
        Task<bool> MaterialReturn(XDocument xml);

        Task<IEnumerable<MaterialDetails>> GetMaterialAllLots(MaterialDetails material, XDocument xml, int pageNumber, int pageSize);
        Task<bool> UnloadEquipmentMaterials(string lotnumber, string equipment, EquipmentSetupFilterType setupFilterType, string outMessage);
        Task<IEnumerable<ConsumptionHistory>> GetConsumptionHistoryForExportAsync(XDocument xml);



    }
}

// CamstarTransactionRepository.cs
using DocumentFormat.OpenXml.Wordprocessing;
using M2OSS.DTO.WMS;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using WDHelpers.Mitecs3Helper;



namespace M2OSS.Repository.Repository
{
    public class CamstarTransactionRepository : ICamstarTransactionRepository
    {

        private readonly IMitecs3SecureData _mitecs3SecureData;
        private readonly IMitecs3Data _mitecs3Data;



        public CamstarTransactionRepository(IMitecs3SecureData mitecs3SecureData, IMitecs3Data mitecs3Data)
        {

            _mitecs3SecureData = mitecs3SecureData;
            _mitecs3Data = mitecs3Data;

        }

        // Mitecs signals a successful transaction in one of two ways: either by returning
        // a null / empty / whitespace message, or by returning a message that contains
        // the word 'success' (matches 'success' and 'successful', case-insensitive).
        // Anything else is treated as a failure.
        //
        // Benign Camstar error codes that should NOT abort the calling flow:
        //   - LotModifyAttrs_E0020 : "There are no changes to modify for <lot>".
        //     Emitted by SetLotAttribute when the supplied values already match
        //     the lot's current attributes - effectively a no-op success from
        //     our point of view, so we swallow it here rather than forcing
        //     every caller to wrap the call in its own try/catch.
        private static readonly string[] BenignErrorCodes =
        {
            "LotModifyAttrs_E0020",
        };

        private static bool IsSuccessMessage(string outMessage)
        {
            if (string.IsNullOrWhiteSpace(outMessage))
                return true;

            if (outMessage.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            foreach (var benign in BenignErrorCodes)
            {
                if (outMessage.IndexOf(benign, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }



        public Task<bool> AuthenticateAsync(string id, string key)
        {
            try
            {
                var authenticated = _mitecs3SecureData.Authenticate(id, key, out Mitecs3User user, out string message);
                if (!string.IsNullOrEmpty(message) && message.Contains("not found"))
                    authenticated = false;
                return Task.FromResult(authenticated);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<IEnumerable<string>> GetRegisteredOperationsAsync()
        {
            var result = _mitecs3Data.GetRegisteredOperations();
            return Task.FromResult(result);
        }

        public Task<DataSet> GetLotDetailsAsync(string lotNumber)
        {
            var lotDetails = _mitecs3Data.GetLotDetails(lotNumber, DateTime.Now, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetLotDetails error for lot {lotNumber}: {outMessage}");
            }

            return Task.FromResult(lotDetails);
        }

        public Task<DataSet> GetProductsAsync()
        {
            var products = _mitecs3Data.GetProducts(out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetProducts error: {outMessage}");
            }

            return Task.FromResult(products);
        }

        public Task<IEnumerable<MaterialPartNumbers>> GetMaterialPartNumberAsync()
        {
            var rawValue = ConfigurationManager.AppSettings["UomList"];
            string[] uoms = rawValue.Split(',').Where(w => w != "").Select(x => x.Trim()).ToArray();

            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialPartNumbers, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialPartNumbers) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialPartNumbers>>(
                    Enumerable.Empty<MaterialPartNumbers>());
            }

            DataTable table = data.Tables[0];
            Random rand = new Random();

            IEnumerable<MaterialPartNumbers> result = table.AsEnumerable()
                .Where(w => w.Field<string>("MATERIALPART_NAME").Contains("XC"))
                .Select(row => new MaterialPartNumbers
                {
                    MaterialPartNumber = row.Field<string>("MATERIALPART_NAME"),
                    MaterialPartDescription = row.Field<string>("MATERIALPART_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status"),
                    WithExpiration = Convert.ToBoolean(rand.Next(0, 2)),
                    WithInspection = Convert.ToBoolean(rand.Next(0, 2)),
                    IsLotControlled = Convert.ToBoolean(rand.Next(0, 2)),
                    Uom = uoms[rand.Next(0, uoms.Count())]
                })
                .ToList();

            return Task.FromResult(result);
        }

        public IEnumerable<MaterialPartNumbers> ReadCsv(string filePath)
        {

            var lines = File.ReadAllLines(filePath);

            var result = new List<MaterialPartNumbers>();

            if (lines.Length <= 1)
                return result; // empty or no data

            // First line contains headers
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();

            var props = typeof(MaterialPartNumbers).GetProperties();

            foreach (var line in lines.Skip(1))
            {
                var cleanLine = line.Replace(",", "");
                if (string.IsNullOrWhiteSpace(cleanLine))
                    continue;

                var values = line.Split(',');

                MaterialPartNumbers obj = new MaterialPartNumbers();

                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    var header = headers[i];
                    var value = values[i].Trim();

                    var prop = props.FirstOrDefault(p =>
                        p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

                    if (prop != null && !string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            object convertedValue;

                            if (prop.PropertyType == typeof(bool))
                            {
                                // Handle common boolean text patterns
                                if (value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("1"))
                                {
                                    convertedValue = true;
                                }
                                else if (value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                                    value.Equals("No", StringComparison.OrdinalIgnoreCase) ||
                                         value.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                                         value.Equals("0"))
                                {
                                    convertedValue = false;
                                }
                                else
                                {
                                    // fallback to standard parsing (true/false)
                                    convertedValue = Convert.ToBoolean(value);
                                }
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value.ToUpper(), prop.PropertyType);
                            }

                            prop.SetValue(obj, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            continue;
                            // handle invalid conversions gracefully (e.g. non-numeric Age)
                        }
                    }
                }

                result.Add(obj);
            }

            return result;
        }

        public Task<IEnumerable<Owner>> GetOwnerAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.Owner, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(Owner) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Owner>>(Enumerable.Empty<Owner>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Owner> result = table.AsEnumerable()
                .Where(r => r.Field<string>("OWNER_DESCRIPTION").ToUpper() == "WMS")
                .Select(row => new Owner
                {
                    OwnerName = row.Field<string>("OWNER"),
                    OwnerDescription = row.Field<string>("OWNER_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }



        public Task<IEnumerable<Factory>> GetFactoryAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.Factory, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(Factory) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Factory>>(Enumerable.Empty<Factory>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Factory> result = table.AsEnumerable().Select(row => new Factory
            {
                FactoryName = row.Field<string>("FactoryName"),
                Description = row.Field<string>("Description"),
                Revision = row.Field<string>("Revision"),
                Status = row.Field<string>("Status")
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Hold>> GetHoldReasonAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.HoldCategory, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(HoldCategory) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Hold>>(Enumerable.Empty<Hold>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Hold> result = table.AsEnumerable()
                .Where(r => r.Field<string>("HOLD_CATEGORY_NAME").ToUpper().Contains("WMS"))
                .Select(row => new Hold
                {
                    HoldCategory = row.Field<string>("HOLD_CATEGORY_NAME"),
                    HoldDescription = row.Field<string>("HOLD_CATEGORY_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Defect>> GetDefectCodeAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.TerminateReasons, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException($"GetMasterLotSetup(TerminateReasons) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<Defect>>(Enumerable.Empty<Defect>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<Defect> result = table.AsEnumerable()
                .Where(r => r.Field<string>("TERMINATE_CATEGORY_NAME").ToUpper().Contains("WMS"))
                .Select(row => new Defect
                {
                    DefectCode = row.Field<string>("TERMINATE_CATEGORY_NAME"),
                    DefectDescription = row.Field<string>("TERMINATE_CATEGORY_DESCRIPTION"),
                    Revision = row.Field<string>("Revision"),
                    Status = row.Field<string>("Status")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public Task<(string, bool)> CreateMaterialLotAsync(MaterialDetails material, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.CreateIndirectMaterialLot(xml.ToString(), out string outMessage);
            return Task.FromResult((outMessage, res));
        }

        public Task<(string message, bool result)> AdjustLotQuantityAsync(MaterialDetails material, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.AdjustMaterialLotQuantity(material.LotId, xml.ToString(), out string outMessage);
            return Task.FromResult<(string message, bool result)>((outMessage, res));
        }

        public Task<bool> SetMaterialLotAttributeAsync(MaterialDetails material, string step, XDocument xml)
        {
            bool res = _mitecs3Data.SetLotAttribute(step, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"SetLotAttribute error for lot {material?.LotId} at step {step}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        public Task<bool> MaterialInventoryMoveAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.MaterialInventoryMove("", xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"MaterialInventoryMove error for lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        // Adds additional wash tray (carrier) lots to an existing virtual lot via Mitecs RePack.
        // The xml payload must follow the schema documented in ICamstarTransactionRepository.RePackAsync.
        public Task<bool> RePackAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.RePack(xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"RePack error for virtual lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }



        public Task<MaterialDetails> GetMaterialLotByLotIdAsync(MaterialDetails material, XDocument xml)
        {
            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for lot {material?.LotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            MaterialDetails materials = dt.AsEnumerable()
                .Where(w => w.Field<string>("CONTAINERNAME").ToString() == material.LotId)
                .Select(row =>
                {
                        

                        return new MaterialDetails
                        {
                            LotId = row["CONTAINERNAME"].ToString(),
                            Quantity = Convert.ToInt32(row["QTY"]),
                            WorkflowStep = row["SPECNAME"].ToString(),
                            PartNumber = row["PRODUCTNAME"].ToString(),
                            OwnerName = row["OWNERNAME"].ToString(),
                            Uom = row["UOMNAME"].ToString(),
                            PoNumber = row["WMSPONUMBER"].ToString(),
                            PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                            InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                            WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                            DrNumber = row["WMSDRNUMBER"].ToString(),
                            LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                            ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                            WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                            PalletId = row["WMSPALLETID"].ToString(),
                            ExpirationDate = row["WMSEXPIRATIONDATE"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                            Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                            Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                            ParentLotId = row["WMSPARENTLOTID"].ToString(),
                            OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                            FactoryName = row["FACTORYNAME"].ToString(),
                            Vendor = row["VENDORNAME"].ToString(),
                            Description = row["DESCRIPTION"]?.ToString(),
                            SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                            TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                            RequestedQuantity = row["WMSRequestQty"].ToString() == "" ? 0 : Convert.ToInt32(row["WMSRequestQty"]),
                            DateRequest = row["WMSDateRequest"].ToString() == "" ? DateTime.MinValue.ToString("yyyy-MM-dd hh:mm:ss") : row["WMSDateRequest"].ToString().Substring(0, 19),
                            TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                            RequestorID = row["WMSRequestorId"].ToString(),
                            PlannerID = row["WMSPlannerId"].ToString(),
                            actionHistory = row["WMSActionHistory"]?.ToString(),


                        };

                }).FirstOrDefault();

            return Task.FromResult(materials);
        }

        public Task<IEnumerable<MaterialDetails>> GetMaterialLotsByFilterAsync(MaterialDetails material, XDocument xml)
        {
            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for part {material?.PartNumber} at step {material?.WorkflowStep}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            IEnumerable<MaterialDetails> materials = dt.AsEnumerable().Select(row =>
            {
                        //DateTime expirationDate;
                        //DateTime.TryParse(row["WMSExpirationDate"]?.ToString(), out expirationDate);

                        return new MaterialDetails
                        {
                            LotId = row["CONTAINERNAME"].ToString(),
                            Quantity = Convert.ToInt32(row["QTY"]),
                            WorkflowStep = row["SPECNAME"].ToString(),
                            PartNumber = row["PRODUCTNAME"].ToString(),
                            OwnerName = row["OWNERNAME"].ToString(),
                            Uom = row["UOMNAME"].ToString(),
                            PoNumber = row["WMSPONUMBER"].ToString(),
                            PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                            InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                            WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                            DrNumber = row["WMSDRNUMBER"].ToString(),
                            LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                            ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                            WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                            PalletId = row["WMSPALLETID"].ToString(),
                            ExpirationDate = row["WMSEXPIRATIONDATE"].ToString() == "" ? (DateTime?)null : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                            Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                            Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                            ParentLotId = row["WMSPARENTLOTID"].ToString(),
                            OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                            FactoryName = row["FACTORYNAME"].ToString(),
                            Vendor = row["VENDORNAME"].ToString(),
                            Description = row["DESCRIPTION"]?.ToString(),
                            SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                            TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                            RequestedQuantity = row["WMSRequestQty"].ToString() == "" ? 0 : Convert.ToInt32(row["WMSRequestQty"]),
                            DateRequest = row["WMSDateRequest"].ToString() == "" ? DateTime.MinValue.ToString("yyyy-MM-dd hh:mm:ss") : row["WMSDateRequest"].ToString().Substring(0, 19),
                            TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                            RequestorID = row["WMSRequestorId"].ToString(),
                            PlannerID = row["WMSPlannerId"].ToString(),
                            actionHistory = row["WMSActionHistory"].ToString(),



                        };

            }).ToList();

            return Task.FromResult(materials);
        }

        public Task<MaterialDetails> GetMaterialLotAttributeAsync(string lotId)
        {
            string poAttribute = ConfigurationManager.AppSettings["wmsPoAttribute"];
            string poLineAttribute = ConfigurationManager.AppSettings["wmsPoLineAttribute"];
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string waybillAttribute = ConfigurationManager.AppSettings["wmsWaybillAttribute"];
            string drAttribute = ConfigurationManager.AppSettings["wmsDrNumberAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string locationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string receiptKeyAttribute = ConfigurationManager.AppSettings["wmsReceiptKeyAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];
            string expirationAttribute = ConfigurationManager.AppSettings["wmsExpirationAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string remarksAttribute = ConfigurationManager.AppSettings["wmsRemarksAttribute"];
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string uomAttribute = ConfigurationManager.AppSettings["wmsUomAttribute"];
            string vendorAttribute = ConfigurationManager.AppSettings["wmsVendorAttribute"];
            string factoryAttribute = ConfigurationManager.AppSettings["wmsFactoryAttribute"];
            string picAttribute = ConfigurationManager.AppSettings["wmsPicAttribute"];
            string receiveDateAttribute = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string picNameAttribute = ConfigurationManager.AppSettings["wmsPicNameAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string defectCodeAttribute = ConfigurationManager.AppSettings["wmsDefectCodeAttribute"];
            string deliveryTypeAttribute = ConfigurationManager.AppSettings["wmsDeliveryTypeAttribute"];
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string requestedQtyAttribute = ConfigurationManager.AppSettings["epullRequestedQtyAttribute"];
            string requestedNoteAttribute = ConfigurationManager.AppSettings["epullRequestedNoteNumberAttribute"];
            string issuanceStatusAttribute = ConfigurationManager.AppSettings["wmsIssuanceStatusAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];
            string referenceLotAttribute = ConfigurationManager.AppSettings["referenceLotAttribute"];

            DataSet data = _mitecs3Data.GetLotAttributes("PWH_0001", lotId, out string outMessage);

            if (!outMessage.Contains("successful"))
            {
                throw new InvalidOperationException(
                    $"GetLotAttributes error for lot {lotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            var propertyMap = new Dictionary<string, string>
            {
                            { "PalletId", palletAttribute.Substring(0, palletAttribute.IndexOf(';'))},
                            { "LotNumber", lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';'))},
                            { "PoNumber", poAttribute.Substring(0, poAttribute.IndexOf(';')) },
                            { "PoLineNumber", poLineAttribute.Substring(0, poLineAttribute.IndexOf(';'))},
                            { "InvoiceNumber", invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';')) },
                            { "WaybillNumber", waybillAttribute.Substring(0, waybillAttribute.IndexOf(';')) },
                            { "DrNumber", drAttribute.Substring(0, drAttribute.IndexOf(';')) },
                            { "ReceivingLocation", locationAttribute.Substring(0, locationAttribute.IndexOf(';')) },
                            { "WmsKeyNumber", receiptKeyAttribute.Substring(0, receiptKeyAttribute.IndexOf(';'))},
                            { "ExpirationDate", expirationAttribute.Substring(0, expirationAttribute.IndexOf(';'))},
                            { "Category", categoryAttribute.Substring(0, categoryAttribute.IndexOf(';')) },
                            { "FactoryName", factoryAttribute.Substring(0, factoryAttribute.IndexOf(';')) },
                            { "Remarks", remarksAttribute.Substring(0, remarksAttribute.IndexOf(';'))},
                            { "OtherRemarks",dispositionRemarksAttribute.Substring(0, dispositionRemarksAttribute.IndexOf(';')) },
                            { "ParentLotId",parentLotAttribute.Substring(0, parentLotAttribute.IndexOf(';')) },
                            { "Uom", uomAttribute.Substring(0, uomAttribute.IndexOf(';'))},
                            { "Vendor", vendorAttribute.Substring(0, vendorAttribute.IndexOf(';')) },
                            { "ReceivedBy", picAttribute.Substring(0, picAttribute.IndexOf(';')) },
                            { "DateReceive", receiveDateAttribute.Substring(0, receiveDateAttribute.IndexOf(';')) },
                            { "OwnerEmail", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';')) },
                            { "ReceiverName", picNameAttribute.Substring(0, picNameAttribute.IndexOf(';')) },
                            { "DefectCode", defectCodeAttribute.Substring(0, defectCodeAttribute.IndexOf(';')) },
                            { "DeliveryType", deliveryTypeAttribute.Substring(0, deliveryTypeAttribute.IndexOf(';')) },
                            { "PreviousOperation", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';')) },
                            { "BoxId", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';')) },
                            { "RequestedQuantity", requestedQtyAttribute.Substring(0, requestedQtyAttribute.IndexOf(';')) },
                            { "ReqNotes", requestedNoteAttribute.Substring(0, requestedNoteAttribute.IndexOf(';')) },
                            { "IssuanceStatus", issuanceStatusAttribute.Substring(0, issuanceStatusAttribute.IndexOf(';')) },
                            { "actionHistory", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';')) },
                { "ReferenceLotNumber", referenceLotAttribute.Substring(0, referenceLotAttribute.IndexOf(';')) }
            };

            MaterialDetails material = MapAttributesToModel<MaterialDetails>(dt, propertyMap);

            return Task.FromResult(material);
        }

        public Task<(bool result, string message)> SplitLotAsync(MaterialDetails sourceMaterial, MaterialDetails newMaterial, XDocument xml)
        {
            // outMessage is returned to the caller as part of the tuple, so we do not throw on it here.
            var res = _mitecs3Data.LotSplit("", xml.ToString(), out string outMessage);
            return Task.FromResult<(bool result, string message)>((res, outMessage));
        }

        public Task<bool> SellLotAsync(MaterialDetails material, XDocument xml)
        {
            var res = _mitecs3Data.SellLots(xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"SellLots error for lot {material?.LotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }

        public Task<bool> AdjustQuantityAsync(string lotId, XDocument xml)
        {
            var res = _mitecs3Data.AdjustMaterialLotQuantity(lotId, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"AdjustMaterialLotQuantity error for lot {lotId}: {outMessage}");
            }

            return Task.FromResult(res);
        }
        public Task<IEnumerable<MaterialDetails>> GetMaterialLotByTicketAsync(MaterialDetails material)
        {
            // Synchronous call into the Mitecs helper DLL
            var data = _mitecs3Data.GetMasterLotSetup(
                LotSetupFilterType.MaterialLotByTicket,
                material.TicketNumber,
                out string msg);

            // If msg has a value, treat it as an error
            if (!IsSuccessMessage(msg))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup error for ticket {material.TicketNumber}: {msg}");
            }

            // No data -> return empty
            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialDetails>>(
                    Enumerable.Empty<MaterialDetails>());
            }

            DataTable dt = data.Tables[0];

            // NOTE: The Mitecs helper returns QTY (and other numeric columns) as the driver's
            // native numeric type (typically decimal or string) - NOT as Int32. Using
            // row.Field<int?>("QTY") would attempt a hard unbox and throw InvalidCastException.
            // Convert.ToInt32 is type-tolerant (handles decimal/double/string/DBNull) and
            // matches the pattern used by GetMaterialLotByLotIdAsync / GetMaterialLotsByFilterAsync.
            IEnumerable<MaterialDetails> materials = dt.AsEnumerable().Select(row => new MaterialDetails
            {
                LotId = row["CONTAINERNAME"]?.ToString(),
                Quantity = row["QTY"] == DBNull.Value || row["QTY"].ToString() == "" ? 0 : Convert.ToInt32(row["QTY"]),
                WorkflowStep = row["SPECNAME"]?.ToString(),
                PartNumber = row["PRODUCTNAME"]?.ToString(),
                OwnerName = row["OWNERNAME"]?.ToString(),
                Description = row["DESCRIPTION"]?.ToString(),
                ReqNotes = row["WMSRequestorNotes"]?.ToString(),
                SupplierLotNum = row["WMSSUPPLIERLOTNUMBER"]?.ToString(),
                DateRequest = row["WMSDateRequest"]?.ToString(),
                TicketNumber = material.TicketNumber,
                RequestedQuantity = row["QTY"] == DBNull.Value || row["QTY"].ToString() == "" ? 0 : Convert.ToInt32(row["QTY"]),
                PlannerID = row["WMSPlannerId"]?.ToString(),
                RequestorID = row["WMSRequestorId"]?.ToString(),
            }).ToList();

            return Task.FromResult(materials);
        }


        

        public Task<IEnumerable<MaterialDetails>> GetSubmittedTicketsAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialTickets, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialTickets) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialDetails>>(Enumerable.Empty<MaterialDetails>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<MaterialDetails> result = table.AsEnumerable().Select(row => new MaterialDetails
            {
                TicketNumber = row.Field<string>("WMSMaterialTicket"),
                TicketStatus = row.Field<string>("WMSMaterialTicketStatus"),
                RequestorID = row.Field<string>("WMSRequestorID"),
                PlannerID = row.Field<string>("WMSPlannerID"),
                DateRequest = row.Field<string>("WMSDateRequest"),
                WorkflowStep = row.Field<string>("SPECNAME"),
            }).ToList();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<MaterialTicket>> GetAllApprovedMaterialTicketAsync()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.MaterialTickets, out string outMessage, "");

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(MaterialTickets) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<MaterialTicket>>(Enumerable.Empty<MaterialTicket>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<MaterialTicket> result = table.AsEnumerable()
                .Where(w => !string.IsNullOrEmpty(w.Field<string>("WMSDATEREQUEST")))
                /*.Where(w => w.Field<string>("SPECNAME") == "PWH_0006" && w.Field<string>("WMSMaterialTicketStatus").ToUpper() == "APPROVED")*/
                .Select(row => new MaterialTicket
                {
                    MaterialTicketId = row.Field<string>("WMSMaterialTicket"),
                    TicketStatus = row.Field<string>("WMSMaterialTicketStatus"),
                    RequestorId = row.Field<string>("WMSRequestorID"),
                    PlannerId = row.Field<string>("WMSPlannerID"),
                    DateRequest = Convert.ToDateTime(row.Field<string>("WMSDateRequest")),
                    WorkflowStep = row.Field<string>("SPECNAME"),
                })
                .ToList();

            return Task.FromResult(result);
        }


        public Task<IEnumerable<LotHistory>> GetLotHistoryByLotIdAsync(string lotId)
        {
            var data = _mitecs3Data.GetLotHistory(lotId, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetLotHistory error for lot {lotId}: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<LotHistory>>(Enumerable.Empty<LotHistory>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<LotHistory> result = table.AsEnumerable().Select(row => new LotHistory
            {

                    Id = Convert.ToInt32(row.Field<string>("No")),
                    LotId = row.Field<string>("LOTID"),
                    Transaction = row.Field<string>("SERVICE"),
                    TransactionDate = row.Field<string>("TXNDATE"),
                    User = row.Field<string>("USER"),
                    FromWorkflowStep = row.Field<string>("FROMSTEP"),
                    ToWorkflowStep = row.Field<string>("TOSTEP"),
                    Equipment = row.Field<string>("EQUIPMENT"),
                    FromQuantity = string.IsNullOrWhiteSpace(row.Field<string>("FROMQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("FROMQTY")),
                    Quantity = string.IsNullOrWhiteSpace(row.Field<string>("QTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("QTY")),
                    FromQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("FROMQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("FROMQTY2")),
                    Quantity2 = string.IsNullOrWhiteSpace(row.Field<string>("QTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("QTY2")),
                    Shift = row.Field<string>("SHIFT"),
                    AttributeModified = row.Field<string>("ATTR_MODIFIED"),
                    AttributeNewValue = row.Field<string>("ATTR_NEWVALUE"),
                    AttributeOldValue = row.Field<string>("ATTR_OLDVALUE"),
                    ReasonName = row.Field<string>("REASONNAME"),
                    LotStatus = row.Field<string>("LOTSTATUS"),
                    TargetLot = row.Field<string>("TARGETLOT"),
                    TargetLotQuantity = string.IsNullOrWhiteSpace(row.Field<string>("TARGETLOTQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("TARGETLOTQTY")),
                    TargetLotQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("TARGETLOTQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("TARGETLOTQTY2")),
                    SourceLot = row.Field<string>("SOURCELOT"),
                    SourceLotQuantity = string.IsNullOrWhiteSpace(row.Field<string>("SOURCELOTQTY")) ? (int?)null : Convert.ToInt32(row.Field<string>("SOURCELOTQTY")),
                SourceLotQuantity2 = string.IsNullOrWhiteSpace(row.Field<string>("SOURCELOTQTY2")) ? (int?)null : Convert.ToInt32(row.Field<string>("SOURCELOTQTY2")),
            }).ToList();

            return Task.FromResult(result);
        }


        public static T MapAttributesToModel<T>(DataTable table, Dictionary<string, string> map) where T : new()
        {
            T model = new T();
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                if (!map.TryGetValue(prop.Name, out var attributeName))
                    continue;

                var row = table.AsEnumerable()
                               .FirstOrDefault(r => string.Equals(r.Field<string>("ATTRIBUTE_NAME"), attributeName, StringComparison.OrdinalIgnoreCase));

                if (row == null)
                    continue;

                var val = row.Field<string>("ATTRIBUTE_VALUE");
                if (string.IsNullOrEmpty(val))
                    continue;

                try
                {

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object converted = null;

                    if (val == null)
                    {
                        converted = null;
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        DateTime dt;
                        string[] formats = { "MM/dd/yyyy",
                                              "MM/d/yyyy",
                                              "M/dd/yyyy",

                                              "M/dd/yyyy hh:mm:ss tt",
                                              "M/dd/yyyy h:mm:ss tt",

                                              "MM/d/yyyy hh:mm:ss tt",
                                              "MM/d/yyyy h:mm:ss tt",

                                              "MM/dd/yyyy hh:mm:ss tt",
                                              "MM/dd/yyyy h:mm:ss tt",

                                              "MM/d/yyyy hh:mm:ss tt",
                                              "MM/d/yyyy h:mm:ss tt",

                                              "M/d/yyyy hh:mm:ss tt",
                                              "M/d/yyyy h:mm:ss tt",

                                              "M/dd/yyyy h:mm:ss",
                                              "MM/d/yyyy h:mm:ss",
                                              "MM/dd/yyyy h:mm:ss",
                                              "MM/dd/yyyy hh:mm:ss" };
                        if (DateTime.TryParseExact(val.ToString(),
                                formats,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out dt))
                            converted = dt;
                        else
                            converted = null; // or throw error if you prefer
                    }
                    else
                    {
                        converted = Convert.ChangeType(val, targetType);
                    }

                    prop.SetValue(model, converted);
                    //var converted = Convert.ChangeType(val, prop.PropertyType);
                    prop.SetValue(model, converted);
                }
                catch (Exception ex)
                {
                    //optional: log conversion error
                }
            }

            return model;
        }


        public Task<IEnumerable<EquipmentFamily>> GetEquipmentFamilyList()
        {
            var data = _mitecs3Data.GetMasterLotSetup(LotSetupFilterType.BackgrindEquipmentFamily, out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(BackgrindEquipmentFamily) error: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<EquipmentFamily>>(Enumerable.Empty<EquipmentFamily>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<EquipmentFamily> result = table.AsEnumerable().Select(row => new EquipmentFamily
            {
                EquipmentFamilyName = row.Field<string>("BACKGRIND_EQUIPMENTFAMILY_NAME")
            }).ToList();

            return Task.FromResult(result);
        }


        public Task<IEnumerable<EquipmentTools>> GetEquipmentByFamily(string equiptmentFamily)
        {
            var data = _mitecs3Data.GetMasterLotSetup(
                LotSetupFilterType.BackgrindEquipmentByFamily,
                equiptmentFamily,
                out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMasterLotSetup(BackgrindEquipmentByFamily) error for family {equiptmentFamily}: {outMessage}");
            }

            if (data == null || data.Tables.Count == 0)
            {
                return Task.FromResult<IEnumerable<EquipmentTools>>(Enumerable.Empty<EquipmentTools>());
            }

            DataTable table = data.Tables[0];

            IEnumerable<EquipmentTools> result = table.AsEnumerable()
                .Where(row => row.Field<string>("RESOURCEFAMILYNAME") == equiptmentFamily)
                .Select(row => new EquipmentTools
                {
                    ResourceId = row.Field<string>("RESOURCEID"),
                    ResourceName = row.Field<string>("RESOURCENAME"),
                    ResourceFamilyName = row.Field<string>("RESOURCEFAMILYNAME")
                })
                .ToList();

            return Task.FromResult(result);
        }

        public async Task<bool> ProcessMaterialWithdrawalByLotIdAsync(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "1";
                    var res = _mitecs3Data.MaterialWithdrawal(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<bool> LotCombine(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "1";
                    var res = _mitecs3Data.LotCombine(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }


        public async Task<bool> SetupIDMaterialsToEquipment(string lotNumber, int lotQty, string equipment)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string outMessage;

                    var res = _mitecs3Data.SetupIDMaterialsToEquipment(
                        lotNumber,
                        lotQty,
                        equipment,
                        out outMessage
                    );

                    return res;
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> ConsumeMaterials(XDocument xml)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string outMessage;
                    var res = _mitecs3Data.ConsumeMaterials(xml.ToString(), out outMessage);
                    return res;
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> MaterialInventoryMove(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "";
                    var res = _mitecs3Data.MaterialInventoryMove(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public Task<MaterialDetails> GetMaterialLotAttributeInControllerReceivingAsync(string lotId)
        {
            string poAttribute = ConfigurationManager.AppSettings["wmsPoAttribute"];
            string poLineAttribute = ConfigurationManager.AppSettings["wmsPoLineAttribute"];
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string waybillAttribute = ConfigurationManager.AppSettings["wmsWaybillAttribute"];
            string drAttribute = ConfigurationManager.AppSettings["wmsDrNumberAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string locationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string receiptKeyAttribute = ConfigurationManager.AppSettings["wmsReceiptKeyAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];
            string expirationAttribute = ConfigurationManager.AppSettings["wmsExpirationAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string remarksAttribute = ConfigurationManager.AppSettings["wmsRemarksAttribute"];
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string uomAttribute = ConfigurationManager.AppSettings["wmsUomAttribute"];
            string vendorAttribute = ConfigurationManager.AppSettings["wmsVendorAttribute"];
            string factoryAttribute = ConfigurationManager.AppSettings["wmsFactoryAttribute"];
            string picAttribute = ConfigurationManager.AppSettings["wmsPicAttribute"];
            string receiveDateAttribute = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string picNameAttribute = ConfigurationManager.AppSettings["wmsPicNameAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string defectCodeAttribute = ConfigurationManager.AppSettings["wmsDefectCodeAttribute"];
            string deliveryTypeAttribute = ConfigurationManager.AppSettings["wmsDeliveryTypeAttribute"];
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string requestedQtyAttribute = ConfigurationManager.AppSettings["epullRequestedQtyAttribute"];
            string requestedNoteAttribute = ConfigurationManager.AppSettings["epullRequestedNoteNumberAttribute"];
            string issuanceStatusAttribute = ConfigurationManager.AppSettings["wmsIssuanceStatusAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];
            string referenceLotAttribute = ConfigurationManager.AppSettings["referenceLotAttribute"];

            DataSet data = _mitecs3Data.GetLotAttributes("PWH_0010", lotId, out string outMessage);

            if (!outMessage.Contains("successful"))
            {
                throw new InvalidOperationException(
                    $"GetLotAttributes error for lot {lotId}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();

            var propertyMap = new Dictionary<string, string>
            {
                            { "PalletId", palletAttribute.Substring(0, palletAttribute.IndexOf(';'))},
                            { "LotNumber", lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';'))},
                            { "PoNumber", poAttribute.Substring(0, poAttribute.IndexOf(';')) },
                            { "PoLineNumber", poLineAttribute.Substring(0, poLineAttribute.IndexOf(';'))},
                            { "InvoiceNumber", invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';')) },
                            { "WaybillNumber", waybillAttribute.Substring(0, waybillAttribute.IndexOf(';')) },
                            { "DrNumber", drAttribute.Substring(0, drAttribute.IndexOf(';')) },
                            { "ReceivingLocation", locationAttribute.Substring(0, locationAttribute.IndexOf(';')) },
                            { "WmsKeyNumber", receiptKeyAttribute.Substring(0, receiptKeyAttribute.IndexOf(';'))},
                            { "ExpirationDate", expirationAttribute.Substring(0, expirationAttribute.IndexOf(';'))},
                            { "Category", categoryAttribute.Substring(0, categoryAttribute.IndexOf(';')) },
                            { "FactoryName", factoryAttribute.Substring(0, factoryAttribute.IndexOf(';')) },
                            { "Remarks", remarksAttribute.Substring(0, remarksAttribute.IndexOf(';'))},
                            { "OtherRemarks",dispositionRemarksAttribute.Substring(0, dispositionRemarksAttribute.IndexOf(';')) },
                            { "ParentLotId",parentLotAttribute.Substring(0, parentLotAttribute.IndexOf(';')) },
                            { "Uom", uomAttribute.Substring(0, uomAttribute.IndexOf(';'))},
                            { "Vendor", vendorAttribute.Substring(0, vendorAttribute.IndexOf(';')) },
                            { "ReceivedBy", picAttribute.Substring(0, picAttribute.IndexOf(';')) },
                            { "DateReceive", receiveDateAttribute.Substring(0, receiveDateAttribute.IndexOf(';')) },
                            { "OwnerEmail", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';')) },
                            { "ReceiverName", picNameAttribute.Substring(0, picNameAttribute.IndexOf(';')) },
                            { "DefectCode", defectCodeAttribute.Substring(0, defectCodeAttribute.IndexOf(';')) },
                            { "DeliveryType", deliveryTypeAttribute.Substring(0, deliveryTypeAttribute.IndexOf(';')) },
                            { "PreviousOperation", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';')) },
                            { "BoxId", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';')) },
                            { "RequestedQuantity", requestedQtyAttribute.Substring(0, requestedQtyAttribute.IndexOf(';')) },
                            { "ReqNotes", requestedNoteAttribute.Substring(0, requestedNoteAttribute.IndexOf(';')) },
                            { "IssuanceStatus", issuanceStatusAttribute.Substring(0, issuanceStatusAttribute.IndexOf(';')) },
                            { "actionHistory", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';')) },
                { "ReferenceLotNumber", referenceLotAttribute.Substring(0, referenceLotAttribute.IndexOf(';')) }
            };

            MaterialDetails material = MapAttributesToModel<MaterialDetails>(dt, propertyMap);

            return Task.FromResult(material);
        }

        public async Task<bool> MaterialReturn(XDocument xml)
        {
            return await Task.Run(() =>
            {
                string outMessage = "";
                try
                {
                    var hostMAC = "";
                    var res = _mitecs3Data.MaterialReturn(hostMAC, xml.ToString(), out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }


        public Task<IEnumerable<MaterialDetails>> GetMaterialAllLots(MaterialDetails material, XDocument xml,int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 100 : pageSize;
            var skip = (pageNumber - 1) * pageSize;

            DataSet data = _mitecs3Data.GetMaterialLotsByFilterDetails(material.WorkflowStep, material.PartNumber, xml.ToString(), out string outMessage);

            if (!IsSuccessMessage(outMessage))
            {
                throw new InvalidOperationException(
                    $"GetMaterialLotsByFilterDetails error for part {material?.PartNumber} at step {material?.WorkflowStep}: {outMessage}");
            }

            DataTable dt = (data != null && data.Tables.Count > 0) ? data.Tables[0] : new DataTable();
            
            IEnumerable<MaterialDetails> materials = dt.AsEnumerable()
                .Select(row => new MaterialDetails
                {
                    LotId = row["CONTAINERNAME"].ToString(),
                    Quantity = Convert.ToInt32(row["QTY"]),
                    WorkflowStep = row["SPECNAME"].ToString(),
                    PartNumber = row["PRODUCTNAME"].ToString(),
                    OwnerName = row["OWNERNAME"].ToString(),
                    Uom = row["UOMNAME"].ToString(),
                    PoNumber = row["WMSPONUMBER"].ToString(),
                    PoLineNumber = row["WMSPOLINENUMBER"].ToString(),
                    InvoiceNumber = row["WMSINVOICENUMBER"].ToString(),
                    WaybillNumber = row["WMSWAYBILLNUMBER"].ToString(),
                    DrNumber = row["WMSDRNUMBER"].ToString(),
                    LotNumber = row["WMSSUPPLIERLOTNUMBER"].ToString(),
                    ReceivingLocation = row["WMSRECEIVINGLOCATION"].ToString(),
                    WmsKeyNumber = row["WMSRECEIPTKEYNUMBER"].ToString(),
                    PalletId = row["WMSPALLETID"].ToString(),
                    ExpirationDate = string.IsNullOrEmpty(row["WMSEXPIRATIONDATE"].ToString())
                        ? (DateTime?)null
                        : Convert.ToDateTime(row["WMSEXPIRATIONDATE"].ToString().Substring(0, 10)),
                    Category = row["WMSRECEIVINGCATEGORY"].ToString(),
                    Remarks = row["WMSRECEIVINGREMARKS"].ToString(),
                    ParentLotId = row["WMSPARENTLOTID"].ToString(),
                    OtherRemarks = row["WMSOTHERREMARKS"].ToString(),
                    FactoryName = row["FACTORYNAME"].ToString(),
                    Vendor = row["VENDORNAME"].ToString(),
                    Description = row["DESCRIPTION"]?.ToString(),
                    SupplierLotNum = row["WMSSupplierLotNumber"]?.ToString(),
                    TicketNumber = row["WMSMaterialTicket"]?.ToString(),
                    RequestedQuantity = string.IsNullOrEmpty(row["WMSRequestQty"].ToString())
                        ? 0
                        : Convert.ToInt32(row["WMSRequestQty"]),
                    DateRequest = string.IsNullOrEmpty(row["WMSDateRequest"].ToString())
                        ? DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
                        : row["WMSDateRequest"].ToString().Substring(0, 19),
                    TicketStatus = row["WMSMaterialTicketStatus"]?.ToString(),
                    RequestorID = row["WMSRequestorId"].ToString(),
                    PlannerID = row["WMSPlannerId"].ToString(),
                    actionHistory = row["WMSActionHistory"].ToString()
                })
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(materials);
        }

        public async Task<bool> UnloadEquipmentMaterials(string lotnumber, string equipment, EquipmentSetupFilterType setupFilterType, string outMessage)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var res = _mitecs3Data.UnloadEquipmentMaterials(lotnumber, equipment, setupFilterType, out outMessage);

                    return res;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public Task<IEnumerable<ConsumptionHistory>> GetConsumptionHistoryForExportAsync(XDocument xml)
        {
            try
            {
                string outMessage;

                var ds = _mitecs3Data.GetMaterialConsumption(
                    xml.ToString(),
                    out outMessage);

                DataTable dt = (ds != null && ds.Tables.Count > 0)
                    ? ds.Tables[0]
                    : new DataTable();

                IEnumerable<ConsumptionHistory> materials = dt.AsEnumerable()
                    .Select(row => new ConsumptionHistory
                    {
                        LotId = row["MaterialLot"]?.ToString(),
                        PartNumber = row["MaterialPart"]?.ToString(),
                        ConsumedQty = row["ConsumeFactor"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(row["ConsumeFactor"]),
                        OutputQty = row["QtyConsumed"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(row["QtyConsumed"]),
                        ToolNumber = row["Equipment"]?.ToString(),
                        DateTimeTransact = row["TxnDate"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(row["TxnDate"]),
                        TransactBy = row["Username"]?.ToString()
                    })
                    .ToList();

                return Task.FromResult(materials);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"GetConsumptionHistoryForExportAsync Error: {ex.Message}",
                    ex);
            }
        }




    }

}

// ICamstarTransactionRepository.cs
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WDHelpers.Mitecs3Helper;

namespace M2OSS.Repository.Camstar.Interface
{
    public interface ICamstarTransactionRepository
    {
        Task<DataSet> GetLotDetailsAsync(string lotNumber);
        Task<DataSet> GetProductsAsync();
        Task<IEnumerable<string>> GetRegisteredOperationsAsync();
        Task<bool> AuthenticateAsync(string id, string key);
        Task<IEnumerable<Owner>> GetOwnerAsync();
        Task<IEnumerable<Factory>> GetFactoryAsync();
        Task<IEnumerable<MaterialPartNumbers>> GetMaterialPartNumberAsync();
        Task<bool> MaterialInventoryMoveAsync(MaterialDetails material, XDocument xml);
        Task<(string,bool)> CreateMaterialLotAsync(MaterialDetails material, XDocument xml);

        Task<(string message, bool result)> AdjustLotQuantityAsync(MaterialDetails material, XDocument xml);
        Task<bool> SetMaterialLotAttributeAsync(MaterialDetails material, string step, XDocument xml);
        Task<IEnumerable<MaterialDetails>> GetMaterialLotsByFilterAsync(MaterialDetails material, XDocument xml);

        Task<MaterialDetails> GetMaterialLotByLotIdAsync(MaterialDetails material, XDocument xml);
        Task<MaterialDetails> GetMaterialLotAttributeAsync(string lotId);
        Task<(bool result, string message)> SplitLotAsync(MaterialDetails sourceMaterial, MaterialDetails newMaterial, XDocument xml);
        Task<bool> SellLotAsync(MaterialDetails material, XDocument xml);

        IEnumerable<MaterialPartNumbers> ReadCsv(string filePath);
        Task<IEnumerable<MaterialDetails>> GetMaterialLotByTicketAsync(MaterialDetails _materialDetails);
        Task<IEnumerable<MaterialDetails>> GetSubmittedTicketsAsync();
        Task<IEnumerable<Hold>> GetHoldReasonAsync();

        Task<IEnumerable<MaterialTicket>> GetAllApprovedMaterialTicketAsync();
        Task<IEnumerable<Defect>> GetDefectCodeAsync();
        Task<IEnumerable<LotHistory>> GetLotHistoryByLotIdAsync(string lotId);
        Task<bool> AdjustQuantityAsync(string lotId, XDocument xml);

        Task<IEnumerable<EquipmentFamily>> GetEquipmentFamilyList();
        Task<IEnumerable<EquipmentTools>> GetEquipmentByFamily(string equiptmentFamily);
        Task<bool> ProcessMaterialWithdrawalByLotIdAsync(XDocument xml);
        Task<bool> LotCombine(XDocument xml);
        Task<bool> SetupIDMaterialsToEquipment(string lotNumber, int lotQty, string equipment);
        Task<bool> ConsumeMaterials(XDocument xml);
        Task<bool> MaterialInventoryMove(XDocument xml);
        Task<MaterialDetails> GetMaterialLotAttributeInControllerReceivingAsync(string lotId);

        Task<bool> RePackAsync(MaterialDetails material, XDocument xml);
        Task<bool> MaterialReturn(XDocument xml);

        Task<IEnumerable<MaterialDetails>> GetMaterialAllLots(MaterialDetails material, XDocument xml, int pageNumber, int pageSize);
        Task<bool> UnloadEquipmentMaterials(string lotnumber, string equipment, EquipmentSetupFilterType setupFilterType, string outMessage);
        Task<IEnumerable<ConsumptionHistory>> GetConsumptionHistoryForExportAsync(XDocument xml);



    }
}

// ILdapService.cs
using M2OSS.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Interface
{
    public interface ILdapService
    {
        Task<User> AuthenticateLdapAsync(string username, string password);
        Task<User> GetUserInformation(string input);
        Task<IEnumerable<User>> GetEmployeeDetailsAsync(string input);
        Task<User> GetEmployeeDetailsByEmployeeIdAsync(string id);
    }
}

// LdapService.cs
using DocumentFormat.OpenXml.Spreadsheet;
using M2OSS.Entities.Common;
using M2OSS.Repository.Common.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Service
{
    public class LdapService : ILdapService
    {
        private readonly string _domain;
        private readonly string _container;
        public LdapService(string domain, string container)
        {
            _domain = domain;
            _container = container;
        }

        public async Task<User> AuthenticateLdapAsync(string username, string password)
        {

            return await Task.Run(() =>
            {
                using (var pc = new PrincipalContext(ContextType.Domain, _domain))
                {
                    if (!pc.ValidateCredentials(username, password, ContextOptions.Negotiate))
                        return null;

                    var userPrincipal = UserPrincipal.FindByIdentity(pc, username);
                    if (userPrincipal == null)
                        return null;

                    // Access DirectoryEntry to get additional attributes
                    DirectoryEntry de = (DirectoryEntry)userPrincipal.GetUnderlyingObject();
                    string department = de.Properties["department"]?.Value?.ToString() ?? "";
                    string jobTitle = de.Properties["title"]?.Value?.ToString() ?? "";
                    string empId = de.Properties["employeeid"].Value?.ToString();
                    string site = de.Properties["adsharedcountrycode"].Value?.ToString();
                    string nickName = de.Properties["givenname"].Value?.ToString();
                    string details = de.Properties["distinguishname"].Value?.ToString();
                    string country = de.Properties["co"].Value?.ToString();
                    string lastName = de.Properties["sn"].Value?.ToString();
                    //using (DirectorySearcher searcher = new DirectorySearcher(de))
                    //{
                    //    searcher.Filter = $"(sAMAccountName={empId})"; // change as needed
                    //    searcher.PropertiesToLoad.Add("*");        // load ALL properties

                    //    SearchResult result = searcher.FindOne();

                    //    if (result != null)
                    //    {
                    //        foreach (string propName in result.Properties.PropertyNames)
                    //        {
                    //            Console.Write($"{propName}: ");

                    //            foreach (var val in result.Properties[propName])
                    //            {
                    //                Console.Write($"{val} ");
                    //            }
                    //            Console.WriteLine();
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("No result found.");
                    //    }
                    //}


                    return new User
                    {
                        Username = empId,
                        DisplayName = userPrincipal.DisplayName ?? username,
                        Email = userPrincipal.EmailAddress ?? "",
                        Department = department ?? "",
                        Site = site.Contains("PH") ? "PHO" : site.Contains("TH") ? "THO":"N/A",
                        ViewingSite = site.Contains("PH") ? "PHO" : site.Contains("TH") ? "THO" : "N/A",
                        EmployeeId= empId,
                        NickName = nickName,
                        Title = jobTitle
                    };
                }
            });
        }

        public async Task<User> GetUserInformation(string userID)
        {
            //userID = "0451610";
            string domain = ConfigurationManager.AppSettings["ADShared"].ToString();
            string domainAndUsername = domain + "\\" + userID;


            //DirectoryEntry e = new DirectoryEntry("LDAP://" + domain, domainAndUsername,);

            DirectorySearcher _drSearcher = new DirectorySearcher();
            _drSearcher.Filter = "(SAMAccountName=" + userID + ")";
            SearchResult adsSearchResult = _drSearcher.FindOne();

            if (adsSearchResult != null)
            {
                DirectoryEntry directoryEntry = adsSearchResult.GetDirectoryEntry();
                //string _fullname = directoryEntry.Properties["mail"][0].ToString();

                return new User
                {
                    Email = directoryEntry.Properties["mail"][0].ToString() ?? "",
                    EmployeeId = userID,
                    DisplayName = directoryEntry.Properties["displayName"][0].ToString() ?? "",
                };
            }

            return null;
        }


        public async Task<IEnumerable<User>> GetEmployeeDetailsAsync(string input)
        {
            return await Task.Run(() =>
            {

                List<User> users = new List<User>();
                using (DirectorySearcher searcher = new DirectorySearcher($"LDAP://OU=Employees,DC={_domain},DC={_container}"))
                {
                    try
                    {//displayName
                        searcher.Filter = $"(&(|(displayName=*{input}*))(adsharedcountrycode=PH))";
                        searcher.PropertiesToLoad.Add("displayname"); // only load what you need
                        searcher.PropertiesToLoad.Add("cn");
                        searcher.PropertiesToLoad.Add("employeeid");
                        searcher.PropertiesToLoad.Add("msexchextensionattribute30");

                        SearchResultCollection results = searcher.FindAll();

                        foreach (SearchResult result in results)
                        {
                            User user = new User
                            {

                                DisplayName = result.Properties["displayName"][0].ToString() ?? "",
                                Email = result.Properties["msexchextensionattribute30"][0].ToString() ?? "",
                                EmployeeId = result.Properties["employeeid"][0].ToString() ?? "",
                                //ReportingId = result.Properties["adsharedreportchainid"][0].ToString() ?? "",

                            };
                            users.Add(user);

                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                   
                    return users;
                }
            });
        }

        public async Task<User> GetEmployeeDetailsByEmployeeIdAsync(string id)
        {
            return await Task.Run(() =>
            {

                
                using (DirectorySearcher searcher = new DirectorySearcher($"LDAP://OU=Employees,DC={_domain},DC={_container}"))
                {
                    try
                    {//displayName
                        searcher.Filter = $"(&(|(employeeid={id}))(adsharedcountrycode=PH))";
                        searcher.PropertiesToLoad.Add("displayname"); // only load what you need
                        searcher.PropertiesToLoad.Add("cn");
                        searcher.PropertiesToLoad.Add("employeeid");
                        searcher.PropertiesToLoad.Add("msexchextensionattribute30");

                        SearchResult result = searcher.FindOne();

                        if (result == null)
                            return null; // or throw custom exception
                        
                        User user = new User
                        {

                            DisplayName = result.Properties["displayName"][0].ToString() ?? "",
                            Email = result.Properties["msexchextensionattribute30"][0].ToString() ?? "",
                            EmployeeId = result.Properties["employeeid"][0].ToString() ?? "",
                            //ReportingId = result.Properties["adsharedreportchainid"][0].ToString() ?? "",

                        };
                        return user;

                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    
                }
            });
        }




    }
}

// IXmlConverterService.cs
using M2OSS.DTO.E_POU;
using M2OSS.DTO.WMS;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Repository.Common.Interface
{
    public interface IXmlConverterService
    {
        XDocument AdjustLotQuantity(MaterialDetails material);
        XDocument CreateMaterialXml(MaterialDetails material);
        XDocument CreateMaterialAttributesXml(MaterialDetails material);
        XDocument SetParentLotAttributeXml(MaterialDetails material);
        XDocument SetDispositionAttributesXml(MaterialDetails material);
        XDocument SetRackLocationAttributesXml(MaterialDetails material);

        // Builds the SLTA payload used by the "Move VMI to Inventory"
        // action on WMS -> Material Inventory. Overwrites two lot
        // attributes:
        //   - WMSReceivingCategory ("VMI" -> "Inventory")
        //   - DateCodeTimestamp    (old receive date -> "now")
        // The lot's workflow step is unchanged (both categories live at
        // PWH_0006), so no material move is issued.
        XDocument TransferVmiToInventoryXml(MaterialDetails material);
        XDocument SetPreviousOperationAttributesXml(MaterialDetails material);
        XDocument MaterialInventoryMoveXml(MaterialDetails material);
        XDocument SplitLotXml(MaterialDetails sourceMaterial, MaterialDetails newMaterial);
        XDocument SellLotXml(MaterialDetails material);
        XDocument MaterialFilterXml(MaterialDetails material);
        List<XDocument> EPULLRequestSetLotAttributeXML(List<MaterialDetailsDTO> _materialDetailsDTO);
        List<XDocument> EPULLApprovalOfRequestSetLotAttributeXML(List<MaterialDetailsDTO> _materialDetails);
        XDocument AssigTicketToLotXml(MaterialDetails material);
        XDocument MaterialFilterByTicketXml(MaterialDetails material);
        XDocument MaterialFilterByTicketAssignedByEPullXml(MaterialDetails material);
        XDocument MaterialIssuanceStatusXml(MaterialDetails material);

        XDocument AdjustQuantityXml(MaterialDetails material);

        XDocument MaterialInventoryMoveToPreviousXml(MaterialDetails material);

        // Builds the XML payload for the Mitecs RePack call. Uses material.LotId as the
        // virtual lot name and material.ReceivedBy as the operator id; one <CarrierName>
        // element is produced per entry in carrierNames.
        XDocument RePackXml(MaterialDetails material, IEnumerable<string> carrierNames);
        XDocument MaterialWithdrawal(MaterialDetails details);
        XDocument CombineMaterials(string newLotId, List<MaterialDetails> lots, string operatorId);
        XDocument ConsumeMaterials(ConsumptionHistoryDto history);
        XDocument ConsumeMaterials(List<MaterialDetails> lots, ConsumptionHistoryDto history);
        XDocument MaterialReturnXml(MaterialDetails material);
        XDocument GetMaterialConsumptionHistory(ConsumptionHistory consumptionHistory);
    }
}

// XmlConverterService.cs
using M2OSS.DTO.E_POU;
using M2OSS.DTO.WMS;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Common.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Repository.Common.Service

{
    public class XmlConverterService : IXmlConverterService
    {

        public XDocument AdjustLotQuantity(MaterialDetails material)
        {
            return new XDocument(
                new XElement("Document",
                    new XElement("AdjustFields",
                        new XElement("NewQty", material.Quantity),
                        new XElement("ServiceAttrsModifyAttrsReason", "SYSTEM")
                    ),
                    new XElement("Operator",
                        new XElement("OperatorID", "CAMSTAR")

                    )
                )
            );
        }
        public XDocument CreateMaterialXml(MaterialDetails material)
        {
            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("MaterialLotInfo",
                        new XElement("MaterialLotName", material.LotId),
                        new XElement("FactoryName", material.FactoryName),
                        new XElement("ProductName", material.PartNumber),
                        new XElement("Qty", material.Quantity),
                        new XElement("Owner", material.OwnerName),
                        new XElement("Uom", material.Uom)
                    )
                )
            );
        }

        public XDocument CreateMaterialAttributesXml(MaterialDetails material)
        {
            string poAttribute = ConfigurationManager.AppSettings["wmsPoAttribute"];
            string poLineAttribute = ConfigurationManager.AppSettings["wmsPoLineAttribute"];
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string waybillAttribute = ConfigurationManager.AppSettings["wmsWaybillAttribute"];
            string drAttribute = ConfigurationManager.AppSettings["wmsDrNumberAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string locationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string receiptKeyAttribute = ConfigurationManager.AppSettings["wmsReceiptKeyAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];
            string expirationAttribute = ConfigurationManager.AppSettings["wmsExpirationAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string remarksAttribute = ConfigurationManager.AppSettings["wmsRemarksAttribute"];
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string uomAttribute = ConfigurationManager.AppSettings["wmsUomAttribute"];
            string vendorAttribute = ConfigurationManager.AppSettings["wmsVendorAttribute"];
            string factoryAttribute = ConfigurationManager.AppSettings["wmsFactoryAttribute"];
            string picAttribute = ConfigurationManager.AppSettings["wmsPicAttribute"];
            string receiveDateAttribute = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string picNameAttribute = ConfigurationManager.AppSettings["wmsPicNameAttribute"];
            string deliveryTypeAttribute = ConfigurationManager.AppSettings["wmsDeliveryTypeAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string ticketNumberAttribute = ConfigurationManager.AppSettings["wmsTicketAttribute"];
            string referenceLotAttribute = ConfigurationManager.AppSettings["referenceLotAttribute"];


            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("SLTAInfo",
                        new XElement("SLTALotName", material.LotId),
                        new XElement("SLTAComment", ""),
                        new XElement("SLTAAdjustReason")
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", poAttribute.Substring(poAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", poAttribute.Substring(0, poAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.PoNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", poLineAttribute.Substring(poLineAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", poLineAttribute.Substring(0, poLineAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.PoLineNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", invoiceAttribute.Substring(invoiceAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.InvoiceNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", waybillAttribute.Substring(waybillAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", waybillAttribute.Substring(0, waybillAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.WaybillNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", drAttribute.Substring(drAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", drAttribute.Substring(0, drAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.DrNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", lotNumberAttribute.Substring(lotNumberAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.LotNumber)
                    ),
                        new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", receiptKeyAttribute.Substring(receiptKeyAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", receiptKeyAttribute.Substring(0, receiptKeyAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.WmsKeyNumber)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", palletAttribute.Substring(palletAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", palletAttribute.Substring(0, palletAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.PalletId)
                    ),

                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", expirationAttribute.Substring(expirationAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", expirationAttribute.Substring(0, expirationAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ExpirationDate == null ? null : material.ExpirationDate?.ToString("yyyy-MM-dd") ?? "")
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", categoryAttribute.Substring(categoryAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", categoryAttribute.Substring(0, categoryAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.Category)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", remarksAttribute.Substring(remarksAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", remarksAttribute.Substring(0, remarksAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.Remarks)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", uomAttribute.Substring(uomAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", uomAttribute.Substring(0, uomAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.Uom)
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", vendorAttribute.Substring(vendorAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", vendorAttribute.Substring(0, vendorAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.Vendor)
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", picAttribute.Substring(picAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", picAttribute.Substring(0, picAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ReceivedBy)
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", receiveDateAttribute.Substring(receiveDateAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", receiveDateAttribute.Substring(0, receiveDateAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.DateReceive == null ? null : material.DateReceive?.ToString("yyyy-MM-dd hh:mm:ss") ?? "")
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", locationAttribute.Substring(locationAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", locationAttribute.Substring(0, locationAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ReceivingLocation == null ? null : material.ReceivingLocation ?? "")
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", ownerEmailAttribute.Substring(ownerEmailAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.OwnerEmail == null ? null : material.OwnerEmail ?? "")
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", picNameAttribute.Substring(picNameAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", picNameAttribute.Substring(0, picNameAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ReceiverName == null ? null : material.ReceiverName ?? "")
                    ),
                      new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", deliveryTypeAttribute.Substring(deliveryTypeAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", deliveryTypeAttribute.Substring(0, deliveryTypeAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.DeliveryType == null ? null : material.DeliveryType ?? "")
                    ),
                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", packagingNumberAttribute.Substring(packagingNumberAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", 1)
                    ),
                     new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", ticketNumberAttribute.Substring(ticketNumberAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", ticketNumberAttribute.Substring(0, ticketNumberAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.TicketNumber)
                    ),
                      new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", referenceLotAttribute.Substring(referenceLotAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName", referenceLotAttribute.Substring(0, referenceLotAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ReferenceLotNumber)
                    ),
                    new XElement("Operator",
                        new XElement("OperatorID", material.ReceivedBy)
                    )
                )
            );

        }

        public XDocument SetParentLotAttributeXml(MaterialDetails material)
        {
            string parentLotAttribute = ConfigurationManager.AppSettings["wmsParentLotAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];


            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("SLTAInfo",
                            new XElement("SLTALotName", material.LotId),
                            new XElement("SLTAComment", ""),
                            new XElement("SLTAAdjustReason")
                        ),
                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", parentLotAttribute.Substring(parentLotAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", parentLotAttribute.Substring(0, parentLotAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.ParentLotId)
                        ),
                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", packagingNumberAttribute.Substring(packagingNumberAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.BoxId)
                        ),
                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", actionHistoryAttribute.Substring(actionHistoryAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.actionHistory)
                        ),
                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )

                    )
            );

        }

        public XDocument AdjustQuantityXml(MaterialDetails material)
        {

            return new XDocument(
                    new XElement("Document",
                        new XElement("AdjustFields",
                            new XElement("NewQty", material.Quantity),
                            new XElement("ServiceAttrsModifyAttrsReason", "SYSTEM"),
                            new XElement("SLTAAdjustReason")
                        ),

                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )

                    )
            );

        }

        public XDocument SetDispositionAttributesXml(MaterialDetails material)
        {
            string dispositionRemarksAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string ownerEmailAttribute = ConfigurationManager.AppSettings["wmsOwnerEmailAttribute"];
            string defectCodeAttribute = ConfigurationManager.AppSettings["wmsDefectCodeAttribute"];
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("SLTAInfo",
                            new XElement("SLTALotName", material.LotId),
                            new XElement("SLTAComment", ""),
                            new XElement("SLTAAdjustReason")
                        ),

                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", dispositionRemarksAttribute.Substring(dispositionRemarksAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", dispositionRemarksAttribute.Substring(0, dispositionRemarksAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.OtherRemarks)
                        ),
                         new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", ownerEmailAttribute.Substring(ownerEmailAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", ownerEmailAttribute.Substring(0, ownerEmailAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.OwnerEmail)
                        ),
                         new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", defectCodeAttribute.Substring(defectCodeAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", defectCodeAttribute.Substring(0, defectCodeAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.DefectCode)
                        ),
                         new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", prevOperationAttribute.Substring(prevOperationAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.PreviousOperation)
                        ),
                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )

                    )
            );


        }

        public XDocument AssigTicketToLotXml(MaterialDetails material)
        {
            string ticketNumberAttribute = ConfigurationManager.AppSettings["wmsDispositionRemarksAttribute"];
            string actionHistoryAttribute = ConfigurationManager.AppSettings["wmsActionHistoryAttribute"];
            return new XDocument(
               new XElement("DocumentElement",
                   new XElement("SLTAinfo",
                       new XElement("SLTALotName", material.LotId),
                       new XElement("SLTAComment", ""),
                       new XElement("SLTAAdjustReason", "")
                   ),
                   new XElement("AttributeMetadata",
                       new XElement("ColumnIndex", "7"),
                       new XElement("DisplayName", "WMSMaterialTicket"),
                       new XElement("AttributeValues", material.TicketNumber),

                       new XElement("ColumnIndex", "88"),
                       new XElement("DisplayName", "WMSRequestQty"),
                       new XElement("AttributeValues", material.RequestedQuantity),

                       new XElement("ColumnIndex", "89"),
                       new XElement("DisplayName", "WMSRequestorID"),
                       new XElement("AttributeValues", material.RequestorID),

                       new XElement("ColumnIndex", "72"),
                       new XElement("DisplayName", "WMSDateRequest"),
                       new XElement("AttributeValues", material.DateRequest == null ? "" : material.DateRequest.Replace("T", " ").Substring(0, 19)),

                       new XElement("ColumnIndex", "83"),
                       new XElement("DisplayName", "WMSPlannerID"),
                       new XElement("AttributeValues", material.PlannerID),

                       new XElement("ColumnIndex", "76"),
                       new XElement("DisplayName", "WMSMaterialTicketStatus"),
                       new XElement("AttributeValues", material.TicketStatus),

                       new XElement("ColumnIndex", "90"),
                       new XElement("DisplayName", "WMSRequestorNotes"),
                       new XElement("AttributeValues", material.ReqNotes),

                       new XElement("ColumnIndex", "23"),
                       new XElement("DisplayName", "ExternalComments"),
                       new XElement("AttributeValues", material.IssuanceStatus)

                   //new XElement("AttributeMetadata",
                   //     new XElement("ColumnIndex", actionHistoryAttribute.Substring(actionHistoryAttribute.IndexOf(';') + 1)),
                   //     new XElement("DisplayName", actionHistoryAttribute.Substring(0, actionHistoryAttribute.IndexOf(';'))),
                   //     new XElement("AttributeValues", material.actionHistory)
                   // )
                   )
               )

            );
        }

        public XDocument MaterialIssuanceStatusXml(MaterialDetails material)
        {
            string issuanceStatusAttribute = ConfigurationManager.AppSettings["wmsIssuanceStatusAttribute"];

            return new XDocument(
               new XElement("DocumentElement",
                   new XElement("SLTAinfo",
                       new XElement("SLTALotName", material.LotId),
                       new XElement("SLTAComment", ""),
                       new XElement("SLTAAdjustReason", "")
                   ),
                   new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", issuanceStatusAttribute.Substring(issuanceStatusAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", issuanceStatusAttribute.Substring(0, issuanceStatusAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.IssuanceStatus)
                        )
               )

            );
        }



        public XDocument SetRackLocationAttributesXml(MaterialDetails material)
        {
            string rackLocationAttribute = ConfigurationManager.AppSettings["wmsLocationAttribute"];
            string packagingNumberAttribute = ConfigurationManager.AppSettings["wmsPackagingNumberAttribute"];

            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("SLTAInfo",
                            new XElement("SLTALotName", material.LotId),
                            new XElement("SLTAComment", ""),
                            new XElement("SLTAAdjustReason")
                        ),

                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", rackLocationAttribute.Substring(rackLocationAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", rackLocationAttribute.Substring(0, rackLocationAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.ReceivingLocation)
                        ),
                        // new XElement("AttributeMetadata",
                        //    new XElement("ColumnIndex", packagingNumberAttribute.Substring(packagingNumberAttribute.IndexOf(';') + 1)),
                        //    new XElement("DisplayName", packagingNumberAttribute.Substring(0, packagingNumberAttribute.IndexOf(';'))),
                        //    new XElement("AttributeValues", material.BoxId)
                        //),
                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )

                    )
            );


        }

        // SLTA payload for the "Move VMI to Inventory" action on
        // WMS -> Material Inventory. Touches:
        //   - WMSReceivingCategory  (VMI -> Inventory)
        //   - DateCodeTimestamp     (now)
        //   - WMSReceivingLocation  (rack the operator assigns at transfer)
        // No lot move; the destination step (PWH_0006) is the same as
        // the source. Format matches SetRackLocationAttributesXml.
        public XDocument TransferVmiToInventoryXml(MaterialDetails material)
        {
            string categoryAttribute      = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string dateReceiveAttribute   = ConfigurationManager.AppSettings["wmsReceiveAttribute"];
            string rackLocationAttribute  = ConfigurationManager.AppSettings["wmsLocationAttribute"];

            // Camstar's DateCodeTimestamp field expects a datetime; use
            // the same ISO layout used elsewhere in the receiving XML
            // (SetMaterialAttributesXml formats dates as yyyy-MM-dd,
            // but this attribute is a timestamp so include time too).
            var nowStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("SLTAInfo",
                        new XElement("SLTALotName", material.LotId),
                        new XElement("SLTAComment", "VMI -> Inventory transfer"),
                        new XElement("SLTAAdjustReason")
                    ),

                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", categoryAttribute.Substring(categoryAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName",  categoryAttribute.Substring(0, categoryAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", "Inventory")
                    ),

                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", dateReceiveAttribute.Substring(dateReceiveAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName",  dateReceiveAttribute.Substring(0, dateReceiveAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", nowStamp)
                    ),

                    new XElement("AttributeMetadata",
                        new XElement("ColumnIndex", rackLocationAttribute.Substring(rackLocationAttribute.IndexOf(';') + 1)),
                        new XElement("DisplayName",  rackLocationAttribute.Substring(0, rackLocationAttribute.IndexOf(';'))),
                        new XElement("AttributeValues", material.ReceivingLocation)
                    ),

                    new XElement("Operator",
                        new XElement("OperatorID", material.ReceivedBy)
                    )
                )
            );
        }

        public XDocument SetPreviousOperationAttributesXml(MaterialDetails material)
        {
            string prevOperationAttribute = ConfigurationManager.AppSettings["wmsPrevOperationAttribute"];

            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("SLTAInfo",
                            new XElement("SLTALotName", material.LotId),
                            new XElement("SLTAComment", ""),
                            new XElement("SLTAAdjustReason")
                        ),

                        new XElement("AttributeMetadata",
                            new XElement("ColumnIndex", prevOperationAttribute.Substring(prevOperationAttribute.IndexOf(';') + 1)),
                            new XElement("DisplayName", prevOperationAttribute.Substring(0, prevOperationAttribute.IndexOf(';'))),
                            new XElement("AttributeValues", material.PreviousOperation
                        ),

                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )

                    )
                )
            );


        }

        public XDocument MaterialInventoryMoveXml(MaterialDetails material)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("LotInfo",
                            new XElement("LotName", material.LotId)
                        ),
                        new XElement("LotInfo",
                            new XElement("LotName", material.LotId)
                        ),
                        new XElement("MateriaInfo",
                            new XElement("ToStep", material.WorkflowStep),
                            new XElement("ToWorkFlow", ConfigurationManager.AppSettings["idmWorkFlow"]),
                            new XElement("Rev", "1"),
                            new XElement("UseROR", "true")
                        )
                    )
            );
        }

        public XDocument MaterialInventoryMoveToPreviousXml(MaterialDetails material)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("LotInfo",
                            new XElement("LotName", material.LotId)
                        ),
                        new XElement("LotInfo",
                            new XElement("LotName", material.LotId)
                        ),
                        new XElement("MateriaInfo",
                            new XElement("ToStep", material.PreviousOperation),
                            new XElement("ToWorkFlow", ConfigurationManager.AppSettings["idmWorkFlow"]),
                            new XElement("Rev", "1"),
                            new XElement("UseROR", "true")
                        )
                    )
            );
        }


        public XDocument SplitLotXml(MaterialDetails sourceMaterial, MaterialDetails newMaterial)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("SourceLotInfo",
                            new XElement("LotName", sourceMaterial.LotId),
                            new XElement("LotQty", sourceMaterial.Quantity)
                        ),
                        new XElement("LotInfo",
                            new XElement("LotName", newMaterial.LotId),
                            new XElement("LotQty", newMaterial.Quantity)
                        ),
                        new XElement("Operator",
                            new XElement("OperatorID", sourceMaterial.ReceivedBy)
                        )
                    )
            );
        }

        // Builds the XML payload for Mitecs RePack (adds wash tray / carrier lots to an
        // existing virtual lot). Schema:
        //   <DocumentElement>
        //     <VirtualLot><VirtualLotName>{material.LotId}</VirtualLotName></VirtualLot>
        //     <Carrier>
        //       <CarrierName>{carrier 1}</CarrierName>
        //       <CarrierName>{carrier 2}</CarrierName>
        //       ...
        //     </Carrier>
        //     <Operator><OperatorID>{material.ReceivedBy}</OperatorID></Operator>
        //   </DocumentElement>
        public XDocument RePackXml(MaterialDetails material, IEnumerable<string> carrierNames)
        {
            // Defensive: drop nulls/blank entries so we never emit empty <CarrierName/>.
            var carrierElements = (carrierNames ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => new XElement("CarrierName", name.Trim()))
                .ToList();

            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("VirtualLot",
                        new XElement("VirtualLotName", material?.LotId)
                    ),
                    new XElement("Carrier", carrierElements),
                    new XElement("Operator",
                        new XElement("OperatorID", material?.ReceivedBy)
                    )
                )
            );
        }

        public XDocument SellLotXml(MaterialDetails material)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("LotList",
                            new XElement("LotName", material.LotId)
                        ),
                         new XElement("SellInfo",
                            new XElement("SellAccount", "ESD-HGA"),
                            new XElement("SellReason", "Customer")
                        ),
                          new XElement("Comment",
                            new XElement("Comments", "Lot Terminate")

                        ),
                        new XElement("Operator",
                            new XElement("OperatorID", material.ReceivedBy)
                        )
                    )
            );
        }

        public XDocument MaterialFilterXml(MaterialDetails material)
        {
            var filters = new List<XElement>();
            string invoiceAttribute = ConfigurationManager.AppSettings["wmsInvoiceAttribute"];
            string lotNumberAttribute = ConfigurationManager.AppSettings["wmsLotNumberAttribute"];
            string categoryAttribute = ConfigurationManager.AppSettings["wmsCategoryAttribute"];
            string palletAttribute = ConfigurationManager.AppSettings["wmsPalletAttribute"];

            void AddFilter(string name, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {

                    if (value.Contains(","))
                    {

                        string[] newVal = value.Split(',');

                        XElement xml = new XElement("Filters");

                        foreach (string val in newVal)
                        {
                            XElement filterXmlName = new XElement("AttributeName", name);
                            XElement filterXmlVal = new XElement("AttributeValue", val);

                            xml.Add(filterXmlName, filterXmlVal);
                        }

                        filters.Add(xml);
                    }
                    else
                    {
                        filters.Add(
                        new XElement("Filters",
                            new XElement("AttributeName", name),
                            new XElement("AttributeValue", value)
                        )
                    );
                    }


                }
            }

            AddFilter(invoiceAttribute.Substring(0, invoiceAttribute.IndexOf(';')), material.InvoiceNumber);
            AddFilter(lotNumberAttribute.Substring(0, lotNumberAttribute.IndexOf(';')), material.LotNumber);
            AddFilter(categoryAttribute.Substring(0, categoryAttribute.IndexOf(';')), material.Category);
            AddFilter(palletAttribute.Substring(0, palletAttribute.IndexOf(';')), material.PalletId);

            return new XDocument(
                new XElement("Document", filters)
            );


        }

        public XDocument MaterialFilterByTicketXml(MaterialDetails material)
        {
            var filters = new List<XElement>();
            string materialTicketAttribute = ConfigurationManager.AppSettings["wmsTicketAttribute"];

            void AddFilter(string name, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {

                    if (value.Contains(","))
                    {

                        string[] newVal = value.Split(',');

                        XElement xml = new XElement("Filters");

                        foreach (string val in newVal)
                        {
                            XElement filterXmlName = new XElement("AttributeName", name);
                            XElement filterXmlVal = new XElement("AttributeValue", val);

                            xml.Add(filterXmlName, filterXmlVal);
                        }

                        filters.Add(xml);
                    }
                    else
                    {
                        filters.Add(
                            new XElement("Filters",
                                new XElement("AttributeName", name),
                                new XElement("AttributeValue", value)
                            )
                        );
                    }


                }
            }

            AddFilter(materialTicketAttribute.Substring(0, materialTicketAttribute.IndexOf(';')), material.TicketNumber);

            return new XDocument(
                new XElement("Document", filters)
            );


        }

        public XDocument MaterialFilterByTicketAssignedByEPullXml(MaterialDetails material)
        {
            var filters = new List<XElement>();
            string materialTicketAttribute = ConfigurationManager.AppSettings["wmsTicketAttribute"];

            void AddFilter(string name, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {

                    if (value.Contains(","))
                    {

                        string[] newVal = value.Split(',');

                        XElement xml = new XElement("Filters");

                        foreach (string val in newVal)
                        {
                            XElement filterXmlName = new XElement("AttributeName", name);
                            XElement filterXmlVal = new XElement("AttributeValue", val);

                            xml.Add(filterXmlName, filterXmlVal);
                        }

                        filters.Add(xml);
                    }
                    else
                    {
                        filters.Add(
                        new XElement("Filters",
                            new XElement("AttributeName", name),
                            new XElement("AttributeValue", value)
                        )
                    );
                    }


                }
            }

            AddFilter(materialTicketAttribute.Substring(0, materialTicketAttribute.IndexOf(';')), material.TicketNumber);

            return new XDocument(
                new XElement("Document", filters)
            );


        }


        public List<XDocument> EPULLRequestSetLotAttributeXML(List<MaterialDetailsDTO> _materialDetailsDTO)
        {
            List<XDocument> xml = new List<XDocument>();

            foreach (var item in _materialDetailsDTO)
            {
                //string actionHistory = string.Format("issuticket={0},qty={1},requestor={2},date={3},approver={4},status={5},notes={6};",
                //    IssueTicket, qty, currentUser, _dateSubmit, approver, status, notes);
                string actionHistory = string.Format("Last user={0}", item.RequestorID);

                XDocument _xml = new XDocument(
               new XElement("DocumentElement",
                   new XElement("SLTAinfo",
                       new XElement("SLTALotName", item.LotId),
                       new XElement("SLTAComment", ""),
                       new XElement("SLTAAdjustReason", "")
                   ),
                   new XElement("AttributeMetadata",
                       new XElement("ColumnIndex", "7"),
                       new XElement("DisplayName", "WMSMaterialTicket"),
                       new XElement("AttributeValues", item.TicketNumber),

                       new XElement("ColumnIndex", "88"),
                       new XElement("DisplayName", "WMSRequestQty"),
                       new XElement("AttributeValues", item.Quantity),

                       new XElement("ColumnIndex", "89"),
                       new XElement("DisplayName", "WMSRequestorID"),
                       new XElement("AttributeValues", item.RequestorID),

                       new XElement("ColumnIndex", "72"),
                       new XElement("DisplayName", "WMSDateRequest"),
                       new XElement("AttributeValues", item.DateRequest),

                       new XElement("ColumnIndex", "83"),
                       new XElement("DisplayName", "WMSPlannerID"),
                       new XElement("AttributeValues", item.PlannerID),

                       new XElement("ColumnIndex", "76"),
                       new XElement("DisplayName", "WMSMaterialTicketStatus"),
                       new XElement("AttributeValues", item.TicketStatus),

                       new XElement("ColumnIndex", "90"),
                       new XElement("DisplayName", "WMSRequestorNotes"),
                       new XElement("AttributeValues", item.ReqNotes),

                       new XElement("ColumnIndex", "60"),
                       new XElement("DisplayName", "WMSActionHistory"),
                       new XElement("AttributeValues", actionHistory)
                   )
               )
               );
                xml.Add(_xml);
            }

            return xml;
        }


        public List<XDocument> EPULLApprovalOfRequestSetLotAttributeXML(List<MaterialDetailsDTO> _materialDetails)
        {
            List<XDocument> xml = new List<XDocument>();

            foreach (var item in _materialDetails)
            {
                //string actionHistory = string.Format("issuticket={0},qty={1},requestor={2},date={3},approver={4},status={5},notes={6};",
                //    IssueTicket, qty, currentUser, _dateSubmit, approver, status, notes);
                string actionHistory = string.Format("Last user={0}", item.RequestorID);

                XDocument _xml = new XDocument(
               new XElement("DocumentElement",
                   new XElement("SLTAinfo",
                       new XElement("SLTALotName", item.LotId)
                   ),
                   new XElement("AttributeMetadata",

                       new XElement("ColumnIndex", "68"),
                       new XElement("DisplayName", "WMSApproverNotes"),
                       new XElement("AttributeValues", item.ApproverNotes),

                       new XElement("ColumnIndex", "77"),
                       new XElement("DisplayName", "WMSMaterialTicketStatus"),
                       new XElement("AttributeValues", item.TicketStatus),

                       new XElement("ColumnIndex", "72"),
                       new XElement("DisplayName", "WMSDateApprover"),
                       new XElement("AttributeValues", item.DateApproval)
                   )
               )
               );
                xml.Add(_xml);
            }

            return xml;
        }


        public XDocument MaterialWithdrawal(MaterialDetails details)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("LotInfo",
                            new XElement("LotName", details.LotId)
                        ),
                        new XElement("MateriaInfo",
                            new XElement("ToStep", details.WorkflowStep),
                            new XElement("ToWorkFlow", ConfigurationManager.AppSettings["idmWipWF"]),
                            new XElement("UseROR", "true")
                        )
                    )
            );
        }

        public XDocument CombineMaterials(string newLotId, List<MaterialDetails> lots, string operatorId)
        {
            return new XDocument(
                new XElement("DocumentElement",

                    new XElement("NewLotName",
                        new XElement("LotName", newLotId)
                    ),

                    lots.Select(lot =>
                        new XElement("LotInfo",
                            new XElement("LotName", lot.LotId),
                            new XElement("LotQty", lot.Quantity)
                        )
                    ),

                    new XElement("Operator",
                        new XElement("OperatorID", operatorId)
                    )
                )
            );
        }


        public XDocument ConsumeMaterials(ConsumptionHistoryDto history)
        {
            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("MaterialLotName", history.LotId),
                    new XElement("Equipment", history.ToolNumber),
                    new XElement("ServiceDetails",
                        new XElement("Item",
                            new XElement("MaterialLotName", history.LotId),
                            new XElement("MaterialPart", history.PartNumber),
                            new XElement("QtyToConsume", history.ConsumedQty)
                        )
                    )
                )
            );
        }

        public XDocument ConsumeMaterials(List<MaterialDetails> lots, ConsumptionHistoryDto history)
        {

            return new XDocument(
                new XElement("DocumentElement",
                    new XElement("MaterialLotName", lots.FirstOrDefault().LotId),
                    new XElement("Equipment", history.ToolNumber),
                    new XElement("ServiceDetails",
                        new XElement("Item",
                            new XElement("MaterialLotName", lots.FirstOrDefault().LotId),
                            new XElement("MaterialPart", history.PartNumber),
                            new XElement("QtyToConsume", history.ConsumedQty)
                        )
                    )
                )
            );
        }

        public XDocument MaterialReturnXml(MaterialDetails material)
        {
            return new XDocument(
                    new XElement("DocumentElement",
                        new XElement("LotInfo",
                            new XElement("LotName", material.LotId)
                        ),
                        new XElement("MateriaInfo",
                            new XElement("ToStep", material.WorkflowStep),
                            new XElement("ToWorkflow", ConfigurationManager.AppSettings["idmWorkFlow"]),
                            new XElement("ToWorkflowRev", "1"),
                            new XElement("UseROR", "true")
                        )
                    )
            );
        }


        public XDocument GetMaterialConsumptionHistory(
            ConsumptionHistory consumptionHistory)
        {
            return new XDocument(
                new XElement("Document",
                    new XElement("LOTID",
                        string.IsNullOrEmpty(consumptionHistory.LotId)
                            ? "%"
                            : consumptionHistory.LotId),
                    new XElement("SPEC", "%"),
                    new XElement("MATERIALPART",
                        string.IsNullOrEmpty(consumptionHistory.PartNumber)
                            ? "%"
                            : consumptionHistory.PartNumber),
                    new XElement("MATERIALLOT", "%"),
                    new XElement("EQUIPMENT",
                        string.IsNullOrEmpty(consumptionHistory.ToolNumber)
                            ? "%"
                            : consumptionHistory.ToolNumber),
                    new XElement("DATESTART",
                        consumptionHistory.Start.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("DATEEND",
                        consumptionHistory.End.ToString("yyyy-MM-dd HH:mm:ss"))
                )
            );
        }


    }
}


// IPhoMaterialRepository.cs
using M2OSS.DTO.Material;
using M2OSS.Entities.AreaSubArea;
using M2OSS.Entities.E_PULL;
using M2OSS.Entities.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Material.Interface
{
    public interface IPhoMaterialRepository
    {
        Task<IEnumerable<PartNumbers>> GetAllMaterialPartNumbersASync();
        Task<IEnumerable<string>> GetUomAsync();
        Task<PartNumbers> GetMaterialDetailsByPartNumberAsync(string partnumber);
        Task<IEnumerable<EmailRecipient>> GetDistinctPlannersAsync();
        Task<IEnumerable<PartNumbers>> GetPartNumbersByPlannerAsync(string plannerId);

        Task<IEnumerable<PartNumbers>> GetSubAreaUsageByPartNumberAsync(string[] partnumbers);
        Task<IEnumerable<PartNumbers>> GetActiveMaterialPartNumbersAsync();

        Task<IEnumerable<string>> GetCommodityTypeAsync();
        Task<IEnumerable<UsageFrequency>> GetAllUsageFrequencyAsync();
        Task<int> AddPartNumberAsync(PartNumbers partnumber);
        Task<int> UpdatePartNumberAsync(PartNumbers partnumber);
        Task<int> AddMaterialPerSubAreaAsync(string partNumber, List<SubArea> subareas);
        Task<IEnumerable<SubArea>> GetSubAreaUsageByPartNumberAsync(string partNumber);
        Task<IEnumerable<Vendor>> GetVendorsByPartNumberAsync(string partNumber);
        Task<int> AddVendorsPerPartNumberAsync(string partNumber, List<Vendor> vendors);

        // Lookup table used by Goods Receiving when the part number is one of the
        // "no-PN" dummies (NP000..NP111). The operator picks an existing description
        // or types a new one (Select2 tag) which is persisted via AddMaterialDescriptionAsync.
        Task<IEnumerable<string>> GetMaterialDescriptionsAsync();
        Task<int> AddMaterialDescriptionAsync(string description, string createdBy, string uom);

        // List (with Id) and delete operations exposed via the Material Part
        // Number management screen so admins can curate the dummy-PN catalog.
        Task<IEnumerable<MaterialNoPartNumberDTO>> GetAllMaterialNoPartNumbersAsync();
        Task<int> DeleteMaterialNoPartNumberAsync(int id);
        // Used after a real part number is created with a Material Name that
        // came from the no-PN catalog: the catalog row is removed so it isn't
        // offered again on Goods Receiving's NPxxx description picker.
        Task<int> DeleteMaterialNoPartNumberByNameAsync(string materialName);

        // Inserts one row into Txn.MaterialNoPartNumberLots for a Camstar lot
        // that was just created against an NPxxx dummy part number. The
        // DescriptionId is resolved server-side from Ref.MaterialNoPartNumbers
        // by MaterialName so callers don't need to do a separate lookup.
        Task<int> AddMaterialNoPartNumberLotAsync(
            string lotId,
            string partNumber,
            string materialName,
            int? quantity,
            string lotNumber,
            DateTime? dateReceive);

        // Returns the distinct material descriptions (Id + MaterialName) that
        // have at least one received lot under the given NPxxx part number.
        // Backs the description picker on the E-PULL Request screen.
        Task<IEnumerable<MaterialNoPartNumberDTO>> GetMaterialNoPartNumberDescriptionsByPartNumberAsync(string partNumber);

        // Returns the Camstar LotIds that were received under the given
        // DescriptionId (Ref.MaterialNoPartNumbers.Id). Used by E-PULL lot
        // allocation to constrain the warehouse-inventory pool to lots that
        // match the operator's chosen material description for NPxxx requests.
        Task<IEnumerable<string>> GetLotIdsByDescriptionIdAsync(int descriptionId);

        // Subtracts the issued quantity from Txn.MaterialNoPartNumberLots for
        // the given received-lot id, clamped at 0. Rows are NOT deleted when
        // they reach 0 - they're retained as historical reference of the
        // descriptions a part number has carried. GetLotIdsByDescriptionIdAsync
        // is responsible for excluding empty rows from future allocations.
        Task<int> DecrementMaterialNoPartNumberLotQuantityAsync(string lotId, int quantity);

        // Resolves a Camstar LotId back to the NP MaterialName the operator
        // picked at receiving time so Material Issuance can show a meaningful
        // description for NPxxx lots (Camstar returns only the generic text
        // "No material part number" for those).
        Task<string> GetDescriptionByLotIdAsync(string lotId);

        // Like GetDescriptionByLotIdAsync but also returns the UOM the operator
        // picked at receiving time, so callers (e.g. Material Inventory) can
        // display both a meaningful description and UOM for NPxxx lots without a
        // second round-trip. Returns null when the lot has no NPxxx catalog row.
        Task<MaterialNoPartNumberDTO> GetNoPartNumberDetailsByLotIdAsync(string lotId);
    }
}


// PhoMaterialRepository.cs
using Dapper;
using M2OSS.DTO.Material;
using M2OSS.Entities.AreaSubArea;
using M2OSS.Entities.E_PULL;
using M2OSS.Entities.Material;
using M2OSS.Repository.Material.Interface;
using M2OSS.Repository.RepositoryBases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Material.Repository
{
    public class PhoMaterialRepository:RepositoryBase, IPhoMaterialRepository
    {
        public PhoMaterialRepository(IDatabaseRepository db) : base(db)
        {
            
        }

        public async Task<IEnumerable<PartNumbers>> GetAllMaterialPartNumbersASync()
        {
            var query = @"  SELECT a.PartNumber,
                                 a.MaterialName,
                                 a.Uom,
                                 a.Moq,
                                 a.IsAutoIssued,
                                 a.WithInspection,
                                 a.WithExpiration,
                                 a.IsLotControlled,
                                 a.WMSCommodityType AS CommodityType,
                                 a.Allocation,
                                 a.PlannerId,
                                 a.IsVmi,
                                 a.IsActive,
                                 a.SpendingTreatment,
                                 a.FrequencyId AS UsageFrequencyId,
                                 a.UsageRatio,
                                 a.WithHostOrTap AS WithHost,
                                 a.WorkflowStep,
                                 b.FrequencyValue
                            FROM Ref.MaterialPartNumbers a
                            LEFT JOIN Ref.UsageFrequency b
                                ON a.FrequencyId = b.Id";

            return await _db.QueryAsync<PartNumbers>(query, null, CommandType.Text);
        }

        public async Task<IEnumerable<string>> GetUomAsync()
        {
            var query = @"SELECT DISTINCT UOM      
                            FROM Ref.MaterialPartNumbers";

            return await _db.QueryAsync<string>(query, null, CommandType.Text);
        }
        public async Task<PartNumbers> GetMaterialDetailsByPartNumberAsync(string partnumber)
        {
            var query = @"SELECT PartNumber,
                                 MaterialName,
                                 Uom,
                                 Moq,
                                 IsAutoIssued,
                                 WithInspection,
                                 WithExpiration,
                                 IsLotControlled ,
                                WMSCommodityType           
                            FROM Ref.MaterialPartNumbers
                            WHERE PartNumber=@PartNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@PartNumber",partnumber);

            return await _db.QueryFirstOrDefaultAsync<PartNumbers>(query, parameters, CommandType.Text);
        }
        public async Task<IEnumerable<EmailRecipient>> GetDistinctPlannersAsync()
        {
            var query = @"SELECT DISTINCT a.PlannerId AS EmployeeId,b.EmailAddress , b.Name
                            FROM Ref.MaterialPartNumbers a 
                            LEFT JOIN Ref.EmailRecipients b 
                                ON a.PlannerId = b.EmployeeId 
                            WHERE a.PlannerId <> '-' 
                                AND b.IsPlanner = 1";
            return await _db.QueryAsync<EmailRecipient>(query,null,CommandType.Text);
        }

        public async Task<IEnumerable<PartNumbers>> GetPartNumbersByPlannerAsync(string plannerId)
        {
            var query = @"SELECT PartNumber,MaterialName,Uom,Allocation,IsAutoIssued,FrequencyValue FROM Ref.MaterialPartNumbers WHERE PlannerId = @PlannerId";
            var parameters = new DynamicParameters();
            parameters.Add("@PlannerId",plannerId);

            return await _db.QueryAsync<PartNumbers>(query,parameters,CommandType.Text);
        }

        public async Task<IEnumerable<PartNumbers>> GetSubAreaUsageByPartNumberAsync(string[] partnumbers)
        {
            var query = @"SELECT a.PartNumber,
		                            b.SubAreaId,
		                            b.SubAreaName 
                            FROM Ref.MaterialSubAreaIUsage a
                            LEFT JOIN Ref.SubArea b
	                            ON a.SubAreaId = b.SubAreaId
                            WHERE a.PartNumber IN @Partnumbers";
            var parameters = new DynamicParameters();
            parameters.Add("@Partnumbers", partnumbers);

            return await _db.QueryAsync<PartNumbers>(query, parameters, CommandType.Text);

        }

        public async Task<IEnumerable<PartNumbers>> GetActiveMaterialPartNumbersAsync()
        {
            var query = @"SELECT a.PartNumber,
                                 a.MaterialName,
                                 a.Uom,
                                 a.PlannerId,
                                 a.Moq,
                                 b.FrequencyValue
                          FROM Ref.MaterialPartNumbers a
                          LEFT JOIN Ref.UsageFrequency b
                            ON a.FrequencyId = b.Id
                          WHERE a.IsActive = 1";

            return await _db.QueryAsync<PartNumbers>(query, null, CommandType.Text);
        }

        public async Task<IEnumerable<string>> GetCommodityTypeAsync()
        {
            var query = @"SELECT DISTINCT WMSCommodityType FROM Ref.MaterialPartNumbers WHERE WMSCommodityType IS NOT NULL";
            return await _db.QueryAsync<string>(query, null, CommandType.Text);
        }

        public async Task<IEnumerable<UsageFrequency>> GetAllUsageFrequencyAsync()
        {
            var query = @"SELECT * FROM Ref.UsageFrequency ORDER BY FrequencyValue";
            return await _db.QueryAsync<UsageFrequency>(query, null, CommandType.Text);
        }

        public async Task<int> AddPartNumberAsync(PartNumbers partnumber)
        {
            var query = @"INSERT INTO Ref.MaterialPartNumbers(PartNumber,MaterialName,Uom,WMSCommodityType,PlannerId,SpendingTreatment,IsVmi,Moq,Allocation,IsActive,IsAutoIssued,FrequencyId,WithInspection,WithExpiration,IsLotControlled,UsageRatio,WithHostOrTap,WorkflowStep)VALUES(@PartNumber,@MaterialName,@Uom,@CommodityType,@PlannerId,@SpendingTreatment,@IsVmi,@Moq,@Allocation,@IsActive,@IsAutoIssued,@UsageFrequencyId,0,0,0,@UsageRatio,@WithHost,@WorkflowStep)";
            var parameters = new DynamicParameters(partnumber);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> UpdatePartNumberAsync(PartNumbers partnumber)
        {
            var query = @"UPDATE Ref.MaterialPartNumbers SET MaterialName=@MaterialName, Uom=@Uom, WMSCommodityType=@CommodityType, PlannerId=@PlannerId, SpendingTreatment=@SpendingTreatment, IsVmi=@IsVmi, Moq=@Moq, Allocation=@Allocation, FrequencyId=@UsageFrequencyId, IsActive=@IsActive, IsAutoIssued=@IsAutoIssued, UsageRatio = @UsageRatio, WithHostOrTap = @WithHost,WorkflowStep = @WorkflowStep WHERE PartNumber=@PartNumber";
            var parameters = new DynamicParameters(partnumber);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> AddMaterialPerSubAreaAsync(string partNumber, List<SubArea> subareas)
        {
            var delQuery = @"DELETE FROM Ref.MaterialSubAreaIUsage WHERE PartNumber=@PartNumber";
            var delParam = new DynamicParameters();
            delParam.Add("@PartNumber", partNumber);
            await _db.ExecuteWithResultAsync(delQuery, delParam, CommandType.Text);

            if (subareas == null || subareas.Count == 0)
            {
                return 0;
            }

            var sb = new StringBuilder();
            sb.Append("INSERT INTO Ref.MaterialSubAreaIUsage (PartNumber, SubAreaId) VALUES ");

            var parameters = new DynamicParameters();
            for (int i = 0; i < subareas.Count; i++)
            {
                sb.Append($"(@PartNumber{i}, @SubAreaId{i})");
                if (i < subareas.Count - 1)
                    sb.Append(", ");

                parameters.Add($"@PartNumber{i}", partNumber);
                parameters.Add($"@SubAreaId{i}", subareas[i].SubAreaId);
            }

            return await _db.ExecuteWithResultAsync(sb.ToString(), parameters, CommandType.Text);
        }

        public async Task<IEnumerable<SubArea>> GetSubAreaUsageByPartNumberAsync(string partNumber)
        {
            var query = @"SELECT * FROM Ref.MaterialSubAreaIUsage WHERE PartNumber=@PartNumber";
            var parameter = new DynamicParameters();
            parameter.Add("@PartNumber", partNumber);
            return await _db.QueryAsync<SubArea>(query, parameter, CommandType.Text);
        }

        public async Task<IEnumerable<M2OSS.Entities.E_PULL.Vendor>> GetVendorsByPartNumberAsync(string partNumber)
        {
            var query = @"SELECT b.VendorCode, b.VendorName
                            FROM Ref.MaterialVendors a
                            LEFT JOIN Ref.Vendor b
                                ON a.VendorCode = b.VendorCode
                            WHERE a.PartNumber = @PartNumber";
            var parameter = new DynamicParameters();
            parameter.Add("@PartNumber", partNumber);
            return await _db.QueryAsync<M2OSS.Entities.E_PULL.Vendor>(query, parameter, CommandType.Text);
        }

        public async Task<int> AddVendorsPerPartNumberAsync(string partNumber, List<M2OSS.Entities.E_PULL.Vendor> vendors)
        {
            var delQuery = @"DELETE FROM Ref.MaterialVendors WHERE PartNumber=@PartNumber";
            var delParam = new DynamicParameters();
            delParam.Add("@PartNumber", partNumber);
            await _db.ExecuteWithResultAsync(delQuery, delParam, CommandType.Text);

            if (vendors == null || vendors.Count == 0)
            {
                return 0;
            }

            var sb = new StringBuilder();
            sb.Append("INSERT INTO Ref.MaterialVendors (PartNumber, VendorCode) VALUES ");

            var parameters = new DynamicParameters();
            for (int i = 0; i < vendors.Count; i++)
            {
                sb.Append($"(@PartNumber{i}, @VendorCode{i})");
                if (i < vendors.Count - 1)
                    sb.Append(", ");

                parameters.Add($"@PartNumber{i}", partNumber);
                parameters.Add($"@VendorCode{i}", vendors[i].VendorCode);
            }

            return await _db.ExecuteWithResultAsync(sb.ToString(), parameters, CommandType.Text);
        }

        // --------------------------------------------------------------------
        // Material descriptions for the "no part number" dummies (NP000..NP111).
        //
        // Required columns on Ref.MaterialNoPartNumbers:
        //   Id              INT IDENTITY PK
        //   MaterialName    NVARCHAR(255)  NOT NULL UNIQUE
        //   CreatedBy       NVARCHAR(100)  NULL
        //   CreatedDate     DATETIME       NOT NULL DEFAULT (GETDATE())
        //   WithPartnumber  BIT            NOT NULL DEFAULT (0)  -- set to 1
        //                                                          when a real
        //                                                          part number
        //                                                          is created
        //                                                          for the row
        //   IsActive        BIT            NOT NULL DEFAULT (1)  -- set to 0 to
        //                                                          soft-delete
        // --------------------------------------------------------------------
        public async Task<IEnumerable<string>> GetMaterialDescriptionsAsync()
        {
            // Goods Receiving's NPxxx picker only shows active rows.
            var query = @"SELECT MaterialName
                            FROM Ref.MaterialNoPartNumbers
                           WHERE IsActive = 1
                           ORDER BY MaterialName";

            return await _db.QueryAsync<string>(query, null, CommandType.Text);
        }

        public async Task<int> AddMaterialDescriptionAsync(string description, string createdBy, string uom)
        {
            // Four cases the UNIQUE(MaterialName) constraint forces us to
            // handle here:
            //   1. Row does not exist                            -> INSERT a
            //                                                       fresh
            //                                                       active row.
            //   2. Row exists, IsActive = 0, WithPartnumber = 0  -> Reactivate
            //                                                       it (the
            //                                                       operator is
            //                                                       bringing
            //                                                       back a
            //                                                       previously
            //                                                       soft-
            //                                                       deleted
            //                                                       description).
            //   3. Row exists, IsActive = 0, WithPartnumber = 1  -> No-op: the
            //                                                       description
            //                                                       was already
            //                                                       promoted to
            //                                                       a real part
            //                                                       number, so
            //                                                       it must
            //                                                       stay out of
            //                                                       the picker.
            //   4. Row exists, IsActive = 1                      -> No-op
            //                                                       (already in
            //                                                       the
            //                                                       picker).
            var query = @"IF NOT EXISTS (SELECT 1 FROM Ref.MaterialNoPartNumbers
                                          WHERE MaterialName = @MaterialName)
                          BEGIN
                              INSERT INTO Ref.MaterialNoPartNumbers (MaterialName, Uom, CreatedBy, CreatedDate)
                              VALUES (@MaterialName, @Uom, @CreatedBy, GETDATE());
                          END
                          ELSE
                          BEGIN
                              UPDATE Ref.MaterialNoPartNumbers
                                 SET IsActive = 1,
                                     Uom = COALESCE(NULLIF(@Uom, ''), Uom)
                               WHERE MaterialName = @MaterialName
                                 AND IsActive = 0
                                 AND WithPartnumber = 0;
                          END";

            var parameters = new DynamicParameters();
            parameters.Add("@MaterialName", description);
            parameters.Add("@Uom", uom ?? string.Empty);
            parameters.Add("@CreatedBy", createdBy);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<MaterialNoPartNumberDTO>> GetAllMaterialNoPartNumbersAsync()
        {
            // The admin management modal lists only active rows; rows soft-
            // deleted via DeleteMaterialNoPartNumberAsync (IsActive = 0) are
            // hidden but kept in the table for traceability.
            var query = @"SELECT Id, MaterialName, CreatedBy, CreatedDate
                            FROM Ref.MaterialNoPartNumbers
                           WHERE IsActive = 1
                           ORDER BY MaterialName";

            return await _db.QueryAsync<MaterialNoPartNumberDTO>(query, null, CommandType.Text);
        }

        public async Task<int> DeleteMaterialNoPartNumberAsync(int id)
        {
            // Soft delete: flip IsActive to 0 instead of removing the row so
            // history is preserved. Only flips rows that are still active so
            // the affected-row count reflects real changes.
            var query = @"UPDATE Ref.MaterialNoPartNumbers
                             SET IsActive = 0
                           WHERE Id = @Id
                             AND IsActive = 1";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> DeleteMaterialNoPartNumberByNameAsync(string materialName)
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                return 0;
            }

            // Promotion (not deletion): the matching row is marked as having a
            // real part number assigned (WithPartnumber = 1) AND deactivated
            // (IsActive = 0) so it no longer shows up on the description
            // picker or the admin management modal, while still being kept in
            // the table for traceability. Only flips rows that aren't already
            // promoted.
            var query = @"UPDATE Ref.MaterialNoPartNumbers
                             SET WithPartnumber = 1,
                                 IsActive       = 0
                           WHERE MaterialName = @MaterialName
                             AND WithPartnumber = 0";

            var parameters = new DynamicParameters();
            parameters.Add("@MaterialName", materialName);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        // --------------------------------------------------------------------
        // Records every Camstar lot that was created against one of the dummy
        // NPxxx part numbers, so the no-part-number receivings can be audited
        // and traced back to the Material Name the operator picked.
        //
        // Expected DDL (already created in MOSSDB):
        //   CREATE TABLE Txn.MaterialNoPartNumberLots (
        //       LotId         NVARCHAR(100) NOT NULL,
        //       PartNumber    NVARCHAR(50)  NOT NULL,
        //       DescriptionId INT           NOT NULL,
        //       Quantity      INT           NULL,
        //       LotNumber     NVARCHAR(100) NULL,
        //       DateReceive   DATETIME      NULL
        //   );
        //
        // DescriptionId is resolved inside the same query from
        // Ref.MaterialNoPartNumbers so the service layer doesn't need a
        // separate roundtrip. When the supplied MaterialName has no matching
        // catalog row (defensive: shouldn't happen at the call site because
        // SaveMaterialDescription is invoked first) the INSERT...SELECT yields
        // zero rows and the method returns 0 instead of throwing.
        // --------------------------------------------------------------------
        public async Task<int> AddMaterialNoPartNumberLotAsync(
            string lotId,
            string partNumber,
            string materialName,
            int? quantity,
            string lotNumber,
            DateTime? dateReceive)
        {
            if (string.IsNullOrWhiteSpace(lotId) ||
                string.IsNullOrWhiteSpace(partNumber) ||
                string.IsNullOrWhiteSpace(materialName))
            {
                return 0;
            }

            var query = @"INSERT INTO Txn.MaterialNoPartNumberLots
                              (LotId, PartNumber, DescriptionId, Quantity, LotNumber, DateReceive)
                          SELECT @LotId, @PartNumber, Id, @Quantity, @LotNumber, @DateReceive
                            FROM Ref.MaterialNoPartNumbers
                           WHERE MaterialName = @MaterialName";

            var parameters = new DynamicParameters();
            parameters.Add("@LotId", lotId);
            parameters.Add("@PartNumber", partNumber);
            parameters.Add("@MaterialName", materialName);
            parameters.Add("@Quantity", quantity);
            parameters.Add("@LotNumber", lotNumber);
            parameters.Add("@DateReceive", dateReceive);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<MaterialNoPartNumberDTO>> GetMaterialNoPartNumberDescriptionsByPartNumberAsync(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
            {
                return Enumerable.Empty<MaterialNoPartNumberDTO>();
            }

            // DISTINCT collapses the case where the same description has
            // multiple received lots under the same NPxxx part number; the
            // E-PULL Request picker only needs one entry per description.
            // The catalog row is required to still be active, since a soft-
            // deleted description must not surface in new requests.
            // Only surface descriptions that still have at least one lot with
            // remaining qty - zero-qty rows are retained in the table as
            // historical reference but shouldn't appear as selectable options
            // on a new request.
            var query = @"SELECT DISTINCT r.Id, r.MaterialName, r.Uom
                            FROM Txn.MaterialNoPartNumberLots l
                            JOIN Ref.MaterialNoPartNumbers   r ON r.Id = l.DescriptionId
                           WHERE l.PartNumber       = @PartNumber
                             AND r.IsActive         = 1
                             AND ISNULL(l.Quantity, 0) > 0
                           ORDER BY r.MaterialName";

            var parameters = new DynamicParameters();
            parameters.Add("@PartNumber", partNumber);

            return await _db.QueryAsync<MaterialNoPartNumberDTO>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<string>> GetLotIdsByDescriptionIdAsync(int descriptionId)
        {
            if (descriptionId <= 0)
            {
                return Enumerable.Empty<string>();
            }

            // Zero-quantity rows are retained as historical reference of which
            // descriptions a part number has ever carried; filter them out
            // here so allocation never tries to pull from an empty lot.
            var query = @"SELECT LotId
                            FROM Txn.MaterialNoPartNumberLots
                           WHERE DescriptionId  = @DescriptionId
                             AND ISNULL(Quantity, 0) > 0";

            var parameters = new DynamicParameters();
            parameters.Add("@DescriptionId", descriptionId);

            return await _db.QueryAsync<string>(query, parameters, CommandType.Text);
        }

        public async Task<string> GetDescriptionByLotIdAsync(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return null;
            }

            // Resolve a Camstar LotId back to the MaterialName the operator
            // picked at receiving time. Used by Material Issuance to display a
            // meaningful description for NPxxx lots (Camstar carries only the
            // generic "No material part number" text on those rows).
            var query = @"SELECT TOP 1 r.MaterialName
                            FROM Txn.MaterialNoPartNumberLots l
                            JOIN Ref.MaterialNoPartNumbers   r ON r.Id = l.DescriptionId
                           WHERE l.LotId = @LotId";

            var parameters = new DynamicParameters();
            parameters.Add("@LotId", lotId);

            return await _db.ExecuteScalarAsync<string>(query, parameters, CommandType.Text);
        }

        public async Task<MaterialNoPartNumberDTO> GetNoPartNumberDetailsByLotIdAsync(string lotId)
        {
            if (string.IsNullOrWhiteSpace(lotId))
            {
                return null;
            }

            // Same LotId -> NPxxx catalog resolution as GetDescriptionByLotIdAsync,
            // but also returns the UOM. Camstar reports only the generic
            // "No material part number" text and no meaningful UOM for dummy
            // lots, so both are pulled from Ref.MaterialNoPartNumbers (the values
            // the operator picked at receiving time) in a single round-trip.
            var query = @"SELECT TOP 1 r.Id, r.MaterialName, r.Uom
                            FROM Txn.MaterialNoPartNumberLots l
                            JOIN Ref.MaterialNoPartNumbers   r ON r.Id = l.DescriptionId
                           WHERE l.LotId = @LotId";

            var parameters = new DynamicParameters();
            parameters.Add("@LotId", lotId);

            var result = await _db.QueryAsync<MaterialNoPartNumberDTO>(query, parameters, CommandType.Text);
            return result.FirstOrDefault();
        }

        public async Task<int> DecrementMaterialNoPartNumberLotQuantityAsync(string lotId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(lotId) || quantity <= 0)
            {
                return 0;
            }

            // Zero-quantity rows are intentionally retained as historical
            // reference of which descriptions a part number has carried; the
            // GREATEST(...,0) clamp prevents going negative on overruns. The
            // GetLotIdsByDescriptionIdAsync side filters out qty <= 0 so
            // allocation will never pick from an empty lot.
            var query = @"UPDATE Txn.MaterialNoPartNumberLots
                             SET Quantity = CASE
                                              WHEN ISNULL(Quantity, 0) - @Quantity < 0 THEN 0
                                              ELSE ISNULL(Quantity, 0) - @Quantity
                                            END
                           WHERE LotId = @LotId;";

            var parameters = new DynamicParameters();
            parameters.Add("@LotId", lotId);
            parameters.Add("@Quantity", quantity);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }
    }
}

// IPhoVendorRepository.cs
using M2OSS.Entities.E_PULL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Vendors.Interface
{
    public interface IPhoVendorRepository
    {
        Task<IEnumerable<M2OSS.Entities.E_PULL.Vendor>> GetAllVendorsAsync();
        Task<int> AddVendorsAsync(M2OSS.Entities.E_PULL.Vendor vendor);
        Task<int> UpdateVendorByCodeAsync(M2OSS.Entities.E_PULL.Vendor vendor);
    }
}


// PhoVendorRepository.cs
using Dapper;
using M2OSS.Entities.E_PULL;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.RepositoryBases;
using M2OSS.Repository.Vendors.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Vendors.Repository
{
    public class PhoVendorRepository : RepositoryBase, IPhoVendorRepository
    {
        public PhoVendorRepository(IDatabaseRepository db) : base(db)
        {
        }

        public async Task<IEnumerable<M2OSS.Entities.E_PULL.Vendor>> GetAllVendorsAsync()
        {
            var query = @"SELECT * FROM Ref.Vendor";
            return await _db.QueryAsync<M2OSS.Entities.E_PULL.Vendor>(query, null, CommandType.Text);
        }

        public async Task<int> AddVendorsAsync(M2OSS.Entities.E_PULL.Vendor vendor)
        {
            // Same contract as the THO repo: return 2 when the VendorCode already
            // exists, the affected row count when inserted, 0 when nothing happened.
            var existQuery = @"SELECT CASE WHEN EXISTS (SELECT 2 FROM Ref.Vendor WHERE VendorCode = @VendorCode) THEN 2 ELSE 0 END";
            var existParam = new DynamicParameters();
            existParam.Add("@VendorCode", vendor.VendorCode);

            var exists = await _db.ExecuteScalarAsync<int>(existQuery, existParam, CommandType.Text);

            if (exists == 0)
            {
                var query = @"INSERT INTO Ref.Vendor(VendorCode,VendorName) VALUES(@VendorCode,@VendorName)";
                var parameters = new DynamicParameters(vendor);
                return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
            }
            else
            {
                return exists;
            }
        }

        public async Task<int> UpdateVendorByCodeAsync(M2OSS.Entities.E_PULL.Vendor vendor)
        {
            var query = @"UPDATE Ref.Vendor SET VendorName=@VendorName WHERE VendorCode=@VendorCode";
            var parameters = new DynamicParameters(vendor);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }
    }
}

// IWebConfigurationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Interface
{
    public interface IWebConfigurationService
    {
        IEnumerable<string> GetUomList();
        IEnumerable<string> GetCategoryList();
        string GetBaseUrl();
        IEnumerable<string> GetInputPlanProgramNames();
    }
}

// WebConfigurationService.cs
using M2OSS.Repository.Common.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Service

{
    public class WebConfigurationService : IWebConfigurationService
    {
        public IEnumerable<string> GetUomList()
        {
            var rawValue = ConfigurationManager.AppSettings["UomList"];
            if (string.IsNullOrWhiteSpace(rawValue))
                return Enumerable.Empty<string>();

            return rawValue.Split(',').Select(x => x.Trim());
        }

        public IEnumerable<string> GetCategoryList()
        {
            var rawValue = ConfigurationManager.AppSettings["idmCategory"];
            if (string.IsNullOrWhiteSpace(rawValue))
                return Enumerable.Empty<string>();

            return rawValue.Split(',').Select(x => x.Trim());
        }
        public string GetBaseUrl()
        {
            return ConfigurationManager.AppSettings["BaseUrl"];
        }

        public IEnumerable<string> GetInputPlanProgramNames()
        {
            var rawValue = ConfigurationManager.AppSettings["InputPlanProgramNames"];
            if (string.IsNullOrWhiteSpace(rawValue))
                return Enumerable.Empty<string>();

            return rawValue.Split(';').Select(x => x.Trim());
        }
        
    }

}

// ILabelPrintingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Interface
{
    public interface ILabelPrintingService
    {
        Task SendZplToNetworkPrinter(string zpl);
        Task<(bool, string)> SendZplToNetworkPrinterWithResult(string zpl);
        Task<bool> IsPrinterConnectedAsync();
    }
}

// LabelPrintingService.cs
using M2OSS.DTO.WMS;
using M2OSS.Repository.Common.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.Common.Service
{
    public class LabelPrintingService : ILabelPrintingService
    {
        private readonly string _printerIp;
        private readonly int _printerPort;
        public LabelPrintingService(string printerIp,int printerPort)
        {
            _printerIp = printerIp;
            _printerPort = printerPort;
        }
        public async Task<(bool,string)> SendZplToNetworkPrinterWithResult(string zpl)
        {

            return await Task.Run(() =>
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        client.Connect(_printerIp, _printerPort);
                        using (var stream = client.GetStream())
                        {
                            //byte[] zplBytes = Encoding.ASCII.GetBytes(zpl);
                            byte[] zplBytes = Encoding.UTF8.GetBytes(zpl);
                            stream.Write(zplBytes, 0, zplBytes.Length);
                        }
                    }
                    return (true,"success"); // Successfully sent data
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Error sending ZPL: {ex.Message}");
                    return (false,ex.ToString());
                }

            });
            
        }

        public async Task SendZplToNetworkPrinter(string zpl)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(_printerIp, _printerPort);

                    using (var stream = client.GetStream())
                    {
                        byte[] zplBytes = Encoding.UTF8.GetBytes(zpl);

                        await stream.WriteAsync(zplBytes, 0, zplBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                // log the error
                throw;
            }






            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        using (var client = new TcpClient())
            //        {
            //            client.Connect(_printerIp, _printerPort);
            //            using (var stream = client.GetStream())
            //            {
            //                //byte[] zplBytes = Encoding.ASCII.GetBytes(zpl);
            //                byte[] zplBytes = Encoding.UTF8.GetBytes(zpl);
            //                stream.Write(zplBytes, 0, zplBytes.Length);
            //            }
            //        }
            //        //return (true, "success"); // Successfully sent data
            //    }
            //    catch (Exception ex)
            //    {
            //        //Console.WriteLine($"Error sending ZPL: {ex.Message}");
            //        //return (false, ex.ToString());
            //    }

            //});

        }

        public async Task<bool> IsPrinterConnectedAsync()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(_printerIp, _printerPort);

                    // Timeout after 5 seconds
                    var completedTask = await Task.WhenAny(
                        connectTask,
                        Task.Delay(5000)
                    );

                    return completedTask == connectTask && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }


    }
}

// IPhoTicketRepository.cs
using M2OSS.DTO.E_PULL;
using M2OSS.Entities.WMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.E_PULL.Interface
{
    public interface IPhoTicketRepository
    {
        Task<string> GetLastTicketGeneratedAsync(string prefix);
        // transactionType is written directly into Txn.TicketNumbers; callers
        // pass "REQUEST" for the regular request flow and "BORROW" for the
        // Borrow flow. The column no longer has a DB default, so a value
        // must always be supplied.
        Task<int> CreateMaterialRequestTicketAsync(IEnumerable<MaterialTicket> materialTicket, string ticketNumber, string approver, string ticketRemarks, int subareaId, string transactionType);
        Task<int> UpdateTicketStatusAsync(MaterialTicket ticket);
        // Flips a borrow ticket back into a regular material request:
        // TicketStatus -> 'requested', TransactionType -> 'REQUEST',
        // DateRequest -> NOW. BorrowedDate is preserved.
        Task<int> ConvertBorrowToRequestAsync(string ticketNumber);

        // True when the ticket is a regular request that originated from a
        // borrow (BorrowedDate IS NOT NULL AND TransactionType = 'REQUEST').
        // The WMS Material Issuance page uses this flag to skip the
        // scan/check stage - the materials are already in the requestor's
        // hands so picking/checking are not applicable.
        Task<bool> IsBorrowConvertedAsync(string ticketNumber);

        // Reads the Txn.TicketNumbers row for a single ticket. Used by the
        // partial Create-Request-from-Borrow flow to clone the source's
        // header into the new ticket. Returns null if not found.
        Task<MaterialTicket> GetTicketHeaderAsync(string ticketNumber);

        // Stamps Txn.TicketNumbers.BorrowedDate for a ticket. The regular
        // CreateMaterialRequestTicketAsync does not populate BorrowedDate
        // (it is only meaningful for borrow-origin tickets), so the partial
        // convert flow calls this right after creating the new REQUEST
        // ticket to carry the source's borrow timestamp forward.
        Task<int> SetBorrowedDateAsync(string ticketNumber, DateTime? borrowedDate);

        // Updates the RequestedQuantity on a single Txn.TicketMaterials
        // row. Used by the partial Create-Request-from-Borrow flow to
        // decrement the SOURCE ticket's borrowed quantity by the amount
        // converted into the new request ticket - otherwise the source
        // ticket's material list keeps showing the original borrowed
        // qty, which is misleading once part of it has been re-routed.
        Task<int> AdjustTicketMaterialQuantityAsync(int ticketMaterialId, int newQuantity);
        // Returns the TransactionType ('REQUEST' / 'BORROW') for a ticket.
        // Used by the warehouse issuance flow to route lots into the correct
        // post-issuance workflow step (PWH_0010 vs PWH_0011).
        Task<string> GetTransactionTypeAsync(string ticketNumber);
        Task<int> DeleteTicketAsync(string ticketNumber);
        Task<IEnumerable<MaterialTicket>> GetTicketsByStatusAsync(string status);
        Task<IEnumerable<MaterialTicket>> GetTicketsByStatusAsync(params string[] statuses);
        Task<IEnumerable<MaterialTicket>> GetMaterialsbyTicketNumberAsync(string ticket);
        Task<int> AcknowledgeRequestTicketAsync(string ticket);
        Task<MaterialTicket> GetMaterialLastRequestAsync(string partNumber, int subArea);

        Task<IEnumerable<MaterialTicket>> GetAllAutoRequestTicketByDateAsync();
        Task<MaterialTicket> GetApprovedTicketByPlannerIdAndSubAreaIdAsync(string plannerId, int subAreaId);

        Task InsertAutoRequestLogs(string transactionId, string ticketReference, string partNumber, string remarks);

        Task<(string Status, DateTime? Timestamp)> GetAutoRequestTrigger();
        Task UpdateAutoRequestTrigger(string status);
        Task AddAutoRequestTrigger();
        Task<IEnumerable<AutoRequestLogs>> GetAutoRequestLogsByReferenceIdAsync(string reference);
        Task<int> DeleteAutoRequestLogsByReferenceIdAsync(string reference);

        // Inserts one Txn.TicketMaterialLots row linking an allocated Camstar lot to its
        // parent Txn.TicketMaterials row.
        Task<int> AddTicketMaterialLotAsync(int ticketMaterialId, string lotId, int quantity);

        // Returns the allocated lots persisted in Txn.TicketMaterialLots for a ticket,
        // joined with Txn.TicketMaterials to surface PartNumber / Uom / LotNumber.
        Task<IEnumerable<M2OSS.Entities.WMS.MaterialDetails>> GetTicketMaterialLotsByTicketAsync(string ticketNumber);

        // Batched TicketNumber -> SubAreaName lookup used by the WMS
        // Material Return page. The lot itself only carries a ticket
        // reference from Camstar; the sub-area is resolved via
        // Txn.TicketNumbers.SubAreaId -> Ref.SubArea.SubAreaName.
        //
        // Missing / unknown tickets are simply absent from the returned
        // dictionary so callers can decide how to render them.
        Task<IDictionary<string, string>> GetSubAreaNamesByTicketsAsync(IEnumerable<string> ticketNumbers);
    }
}

// PhoTicketRepository.CS
using CsvHelper.Configuration;
using Dapper;
using M2OSS.DTO.E_PULL;
using M2OSS.Entities.AreaSubArea;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using M2OSS.Repository.E_PULL.Interface;
using M2OSS.Repository.RepositoryBases;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.E_PULL.Repository
{
    public class PhoTicketRepository: RepositoryBase,IPhoTicketRepository
    {
        public PhoTicketRepository(IDatabaseRepository db) : base(db)
        {
                
        }

        public async Task<string> GetLastTicketGeneratedAsync(string prefix)
        {
            var query = @"SELECT TOP 1 TicketNumber FROM Txn.TicketNumbers WHERE TicketNumber LIKE @Prefix + '-%' ORDER BY DateRequest DESC";
            var parameters = new DynamicParameters();
            parameters.Add("@Prefix", prefix);
            return await _db.ExecuteScalarAsync<string>(query, parameters, CommandType.Text);
        }

        public async Task<int> CreateMaterialRequestTicketAsync(IEnumerable<MaterialTicket> materialTicket,string ticketNumber,string approver,string ticketRemarks,int subareaId, string transactionType)
        {
            /// additional parameters are use for the auto request, in case IEnumerable<MaterialTicket> has no data, we can still have required details to create a ticket. This ticket will be displayed so that users can notice and able to check what is the reason before deleting the ticket. 


            // TransactionType is now mandatory (no DB default) and identifies
            // the ticket family: 'REQUEST' for regular material requests,
            // 'BORROW' for borrow tickets. The shared service decides which.
            var queryInsertTicket = @"INSERT INTO Txn.TicketNumbers(TicketNumber,RequestorId,ApproverId,SubAreaId,ProgramName,DateRequest,TicketStatus,TicketRemarks,TransactionType)
                VALUES(@TicketNumber,@RequestorId,@ApproverId,@SubAreaId,@ProgramName,@DateRequest,@TicketStatus,@TicketRemarks,@TransactionType);";


            // DescriptionId is populated only for NPxxx ("no part number") requests
            // so downstream processes can resolve the dummy PN back to the
            // catalog row (Ref.MaterialNoPartNumbers) the requestor picked.
            // Stays null for real part numbers.
            var queryInsertTicketMaterials = @"INSERT INTO Txn.TicketMaterials(TicketNumber,RequestedPartNumber,Uom,RequestedQuantity,LotNumber,Remarks,DescriptionId)VALUES(@TicketNumber,@RequestedPartNumber,@Uom,@RequestedQuantity,@LotNumber,@Remarks,@DescriptionId);SELECT CAST(SCOPE_IDENTITY() AS INT);";


            int lastinsertedId = 0;
            int saId = materialTicket.Count() > 0 ? materialTicket.Select(s => s.SubAreaId).FirstOrDefault() : subareaId;
            var result = await _db.ExecuteWithTransactionAsync(async (connection, transaction) =>
            {
                

                var ticketNumberParameters = new DynamicParameters();
                ticketNumberParameters.Add("@TicketNumber",ticketNumber);
                ticketNumberParameters.Add("@RequestorId", materialTicket.Select(s => s.RequestorId).FirstOrDefault() ?? "System");
                ticketNumberParameters.Add("@ApproverId", materialTicket.Select(s => s.ApproverId).FirstOrDefault() ?? approver);
                ticketNumberParameters.Add("@SubAreaId", saId);
                ticketNumberParameters.Add("@DateRequest", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                ticketNumberParameters.Add("@TicketStatus", "requested");
                ticketNumberParameters.Add("@TicketRemarks", materialTicket.Select(s => s.TicketRemarks).FirstOrDefault() ?? ticketRemarks);
                ticketNumberParameters.Add("@ProgramName", materialTicket.Select(s => s.ProgramName).FirstOrDefault());
                ticketNumberParameters.Add("@TransactionType", transactionType);


                await connection.ExecuteAsync(
                     queryInsertTicket, ticketNumberParameters, transaction);

                foreach (var material in materialTicket)
                {
                    var materialTicketParameters = new DynamicParameters();
                    materialTicketParameters.Add("@TicketNumber",ticketNumber);
                    materialTicketParameters.Add("@RequestedPartNumber", material.RequestedPartNumber);
                    materialTicketParameters.Add("@Uom", material.Uom);
                    materialTicketParameters.Add("@RequestedQuantity", material.RequestedQuantity);
                    materialTicketParameters.Add("@LotNumber", material.LotNumber);
                    materialTicketParameters.Add("@Remarks", material.Remarks);
                    materialTicketParameters.Add("@DescriptionId", material.DescriptionId);

                    lastinsertedId = await connection.ExecuteScalarAsync<int>(queryInsertTicketMaterials, materialTicketParameters, transaction);
                    // Surface the new Txn.TicketMaterials.Id back to the caller so
                    // downstream allocation logic can FK its rows in Txn.TicketMaterialLots.
                    material.Id = lastinsertedId;
                }

                return lastinsertedId;
            });
            return result;


        }

        public async Task<string> GetTransactionTypeAsync(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber)) return null;

            var query = @"SELECT TOP 1 TransactionType
                            FROM Txn.TicketNumbers
                           WHERE TicketNumber = @TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);
            return await _db.ExecuteScalarAsync<string>(query, parameters, CommandType.Text);
        }

        public async Task<int> UpdateTicketStatusAsync(MaterialTicket ticket)
        {
            var query = @"UPDATE Txn.TicketNumbers
                            SET 
                                TicketStatus = @TicketStatus,
                                TicketRemarks = COALESCE(NULLIF(@TicketRemarks, ''), TicketRemarks),

                                ApprovalDate = CASE 
                                    WHEN @TicketStatus = 'approved'
                                    THEN GETDATE()
                                    ELSE ApprovalDate
                                END,

                                -- 'closed'   = request flow terminal state
                                -- 'borrowed' = borrow  flow terminal state
                                -- Both represent fully issued out of the
                                -- warehouse, so the IssuedDate timestamp
                                -- applies to either status. We only stamp
                                -- on the FIRST transition (IssuedDate IS NULL)
                                -- so that later state changes - e.g. a
                                -- borrowed ticket moving to 'return to wh'
                                -- and back to 'borrowed' - do not overwrite
                                -- the original out-of-warehouse timestamp.
                                IssuedDate = CASE
                                    WHEN @TicketStatus IN ('closed', 'borrowed')
                                         AND IssuedDate IS NULL
                                    THEN GETDATE()
                                    ELSE IssuedDate
                                END,

                                -- BorrowedDate captures the FIRST time the
                                -- ticket entered 'borrowed' status. Kept
                                -- separate from IssuedDate so the borrow
                                -- history survives a later conversion of
                                -- the ticket back to 'requested' (the
                                -- Create-Request-from-Borrow flow bumps
                                -- DateRequest, which would otherwise erase
                                -- when the materials were borrowed).
                                BorrowedDate = CASE
                                    WHEN @TicketStatus = 'borrowed'
                                         AND BorrowedDate IS NULL
                                    THEN GETDATE()
                                    ELSE BorrowedDate
                                END
                            WHERE TicketNumber = @TicketNumber";
            //var query = @"UPDATE Txn.TicketNumber SET TicketStatus = @TicketStatus,TicketRemarks =@TicketRemarks WHERE TicketNumber=@TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketStatus",ticket.TicketStatus);
            parameters.Add("@TicketRemarks", ticket.TicketRemarks);
            parameters.Add("@TicketNumber", ticket.TicketNumber);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }
        // Converts a borrow ticket back into a regular material request.
        // Flips TicketStatus -> 'requested', TransactionType -> 'REQUEST'
        // and resets DateRequest to NOW so the ticket re-enters the
        // requestor queue with a current timestamp. BorrowedDate is left
        // intact so the original borrow history is preserved.
        public async Task<int> ConvertBorrowToRequestAsync(string ticketNumber)
        {
            var query = @"UPDATE Txn.TicketNumbers
                            SET TicketStatus    = 'requested',
                                TransactionType = 'REQUEST',
                                DateRequest     = GETDATE()
                          WHERE TicketNumber    = @TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<bool> IsBorrowConvertedAsync(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber)) return false;

            // A "borrow-converted" ticket is one whose row still bears the
            // original borrow timestamp (BorrowedDate) but has since been
            // flipped back to a regular request (TransactionType='REQUEST').
            // Used by the WMS Material Issuance page to suppress scan/check.
            var query = @"SELECT CASE
                                    WHEN BorrowedDate IS NOT NULL
                                         AND TransactionType = 'REQUEST'
                                    THEN 1 ELSE 0
                                 END
                            FROM Txn.TicketNumbers
                           WHERE TicketNumber = @TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);
            var result = await _db.ExecuteScalarAsync<int?>(query, parameters, CommandType.Text);
            return result.GetValueOrDefault() == 1;
        }

        public async Task<MaterialTicket> GetTicketHeaderAsync(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber)) return null;

            var query = @"SELECT TOP 1 *
                            FROM Txn.TicketNumbers
                           WHERE TicketNumber = @TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);
            return await _db.QuerySingleOrDefaultAsync<MaterialTicket>(query, parameters, CommandType.Text);
        }

        public async Task<int> AdjustTicketMaterialQuantityAsync(int ticketMaterialId, int newQuantity)
        {
            if (ticketMaterialId <= 0) return 0;
            if (newQuantity < 0) newQuantity = 0;

            // Quantity 0 is intentionally kept as a row (not deleted)
            // so any Txn.TicketMaterialLots rows referencing this
            // TicketMaterials.Id remain valid (the FK guard would
            // otherwise reject the delete). The UI is expected to
            // treat 0 as "fully converted" - the source ticket's
            // remaining borrowed quantity for this part number.
            var query = @"UPDATE Txn.TicketMaterials
                             SET RequestedQuantity = @RequestedQuantity
                           WHERE Id = @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", ticketMaterialId);
            parameters.Add("@RequestedQuantity", newQuantity);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> SetBorrowedDateAsync(string ticketNumber, DateTime? borrowedDate)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber)) return 0;

            var query = @"UPDATE Txn.TicketNumbers
                             SET BorrowedDate = @BorrowedDate
                           WHERE TicketNumber = @TicketNumber";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);
            parameters.Add("@BorrowedDate", borrowedDate);
            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> AcknowledgeRequestTicketAsync(string ticket)
        {
            var query = @"UPDATE Txn.TicketNumbers
                            SET 
                                AcknowledgementDate = GETDATE()   
                            WHERE TicketNumber = @TicketNumber";
            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticket);


            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        public async Task<int> DeleteTicketAsync(string ticketNumber)
        {
            // Child-first order required by FK chain:
            //   Txn.TicketMaterialLots -> Txn.TicketMaterials -> Txn.TicketNumbers
            var deleteTicketMaterialLots =
                @"DELETE FROM Txn.TicketMaterialLots
                  WHERE TicketMaterialId IN (
                      SELECT Id FROM Txn.TicketMaterials WHERE TicketNumber = @TicketNumber
                  );";

            var deleteTicket =
                @"DELETE FROM Txn.TicketNumbers
          WHERE TicketNumber=@TicketNumber";

            var deleteTicketMaterials =
                @"DELETE FROM Txn.TicketMaterials
          WHERE TicketNumber=@TicketNumber";

            var result = await _db.ExecuteWithTransactionAsync(async (connection, transaction) =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TicketNumber", ticketNumber);

                // delete lot allocations first (FK -> TicketMaterials)
                await connection.ExecuteAsync(
                    deleteTicketMaterialLots,
                    parameters,
                    transaction);

                // delete materials next
                int deletedMaterials = await connection.ExecuteAsync(
                    deleteTicketMaterials,
                    parameters,
                    transaction);

                // finally delete the ticket header
                int deletedTicket = await connection.ExecuteAsync(
                    deleteTicket,
                    parameters,
                    transaction);

                return deletedTicket;
            });

            return result;
        }

        public async Task<IEnumerable<MaterialTicket>> GetTicketsByStatusAsync(string status)
        {
            // Backward-compatible single-status entrypoint; delegates to the
            // multi-status overload so the SQL and parameter expansion live
            // in exactly one place.
            return await GetTicketsByStatusAsync(new[] { status });
        }

        public async Task<IEnumerable<MaterialTicket>> GetTicketsByStatusAsync(params string[] statuses)
        {
            // Dapper expands @TicketStatuses into the appropriate IN-list
            // parameters when the value is an IEnumerable<string>, so a
            // single query handles both single-status and multi-status reads.
            var query = @"SELECT a.*,b.SubAreaName,c.CostCenter
                            FROM Txn.TicketNumbers a 
                            LEFT JOIN Ref.SubArea b 
	                            ON a.SubAreaId = b.SubAreaId 
                            LEFT JOIN Ref.Area c 
	                            ON b.AreaId = c.AreaId 
                            WHERE TicketStatus IN @TicketStatuses";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketStatuses", statuses);
            return await _db.QueryAsync<MaterialTicket>(query, parameters, CommandType.Text);
        }

        public async Task<IEnumerable<MaterialTicket>> GetMaterialsbyTicketNumberAsync(string ticket)
        {
            // MaterialName is resolved in two steps so a single column carries
            // the correct label for both regimes:
            //   - NPxxx ("no part number") rows have DescriptionId set, so we
            //     prefer the description from Ref.MaterialNoPartNumbers.
            //   - Real part numbers fall back to Ref.MaterialPartNumbers.
            // Without this, Camstar surfaces NPxxx lots as "No material part
            // number", which is unhelpful in the My Ticket Request screen.
            var query = @"SELECT a.*,
                                 COALESCE(np.MaterialName, b.MaterialName) AS MaterialName
                            FROM Txn.TicketMaterials a
                            LEFT JOIN Ref.MaterialPartNumbers   b  ON a.RequestedPartNumber = b.PartNumber
                            LEFT JOIN Ref.MaterialNoPartNumbers np ON np.Id                 = a.DescriptionId
                           WHERE a.TicketNumber = @TicketNumber";
            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticket);
            return await _db.QueryAsync<MaterialTicket>(query, parameters, CommandType.Text);
        }

        public async Task<MaterialTicket> GetMaterialLastRequestAsync(string partNumber, int subArea)
        {
            var query = @"SELECT TOP 1 *  FROM Txn.TicketMaterials a 
                            LEFT JOIN Txn.TicketNumbers b 
	                            ON a.TicketNumber = b.TicketNumber
                            WHERE a.RequestedPartNumber = @PartNumber
                            AND SubAreaId = @subArea
                            ORDER BY DateRequest DESC";

            var parameters = new DynamicParameters();
            parameters.Add("@PartNumber", partNumber);
            parameters.Add("@subArea", subArea);

            return await _db.QuerySingleOrDefaultAsync<MaterialTicket>(query, parameters, CommandType.Text);
        }


        public async Task<IEnumerable<MaterialTicket>> GetAllAutoRequestTicketByDateAsync()
        {
            var query = @"SELECT TicketNumber
                            FROM Txn.TicketNumbers
                            WHERE RequestorId = 'System'
                                AND TicketStatus = 'requested'
                                AND CAST(DateRequest AS DATE) = CAST(GETDATE() AS DATE);";

            return await _db.QueryAsync<MaterialTicket>(query,null,CommandType.Text);
        }

        public async Task<MaterialTicket> GetApprovedTicketByPlannerIdAndSubAreaIdAsync(string plannerId,int subAreaId)
        {
            var query = @"SELECT TicketNumber,ApproverId
                            FROM Txn.TicketNumbers
                            WHERE RequestorId = 'System'
                                AND TicketStatus = 'approved'
                                AND CAST(DateRequest AS DATE) = CAST(GETDATE() AS DATE)
                                AND ApproverId=@PlannerId
                                AND SubAreaId=@SubAreaId;";

            var parameters = new DynamicParameters();
            parameters.Add("@PlannerId", plannerId);
            parameters.Add("@SubAreaId", subAreaId);

            return await _db.QuerySingleOrDefaultAsync<MaterialTicket>(query, parameters, CommandType.Text);
        }



        public async Task InsertAutoRequestLogs(string transactionId,string ticketReference, string partNumber, string remarks)
        {
            var query = @"INSERT INTO Txn.AutoRequestLogs(TransactionId,TicketReference,Partnumber,Remarks,Timestamp)VALUES(@TransactionId,@TicketReference,@PartNumber,@Remarks,@Timestamp)";

            var parameters = new DynamicParameters();
            parameters.Add("@TransactionId", transactionId);
            parameters.Add("@TicketReference", ticketReference);
            parameters.Add("@PartNumber", partNumber);
            parameters.Add("@Remarks", remarks);
            parameters.Add("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


            await _db.ExecuteAsync(query,parameters,CommandType.Text);

        }

        public async Task<(string Status,DateTime? Timestamp)> GetAutoRequestTrigger()
        {
            var query = @"SELECT TOP 1 
                                Status
                                ,Timestamp
                          FROM dbo.AutoRequestStatus";

            
            return await _db.QuerySingleOrDefaultAsync<(string, DateTime)>(query, null, CommandType.Text);
        }

        public async Task UpdateAutoRequestTrigger(string status)
        {
            var query = @"UPDATE dbo.AutoRequestStatus SET Status=@Status,Timestamp=@Timestamp";
            var parameters = new DynamicParameters();
            parameters.Add("@Status",status);
            parameters.Add("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await _db.ExecuteAsync(query, parameters, CommandType.Text);

        }

        public async Task AddAutoRequestTrigger()
        {
            // Only insert when the table is empty so we always keep a single row.
            // Subsequent transitions go through UpdateAutoRequestTrigger.
            var query = @"IF NOT EXISTS (SELECT 1 FROM dbo.AutoRequestStatus)
                            INSERT INTO dbo.AutoRequestStatus(Status,Timestamp)
                            VALUES('ongoing', GETDATE());";
            
            await _db.ExecuteAsync(query, null, CommandType.Text);

        }

        public async Task<IEnumerable<AutoRequestLogs>> GetAutoRequestLogsByReferenceIdAsync (string reference)
        {
            var query = @"SELECT PArtnumber,Remarks,Timestamp FROM  Txn.AutoRequestLogs WHERE TicketReference =@TicketReference";
            var parameters = new DynamicParameters();
            parameters.Add("@TicketReference", reference);

            return await _db.QueryAsync<AutoRequestLogs>(query,parameters,CommandType.Text);
        }

        public async Task<int> DeleteAutoRequestLogsByReferenceIdAsync(string reference)
        {
            var query = @"DELETE FROM  Txn.AutoRequestLogs WHERE TicketReference =@TicketReference";
            var parameters = new DynamicParameters();
            parameters.Add("@TicketReference", reference);

            return await _db.ExecuteWithResultAsync(query, parameters, CommandType.Text);
        }

        // Persists the link between an allocated Camstar lot and the parent
        // Txn.TicketMaterials row. Called once per lot picked by AllocateLot /
        // AllocateLotWithRepack (including the split-lot case, where LotId/Quantity
        // refer to the newly-split sub-lot).
        public async Task<int> AddTicketMaterialLotAsync(int ticketMaterialId, string lotId, int quantity)
        {
            var query = @"INSERT INTO Txn.TicketMaterialLots(TicketMaterialId, LotId, Quantity)
                          VALUES(@TicketMaterialId, @LotId, @Quantity);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketMaterialId", ticketMaterialId);
            parameters.Add("@LotId", lotId);
            parameters.Add("@Quantity", quantity);

            return await _db.ExecuteScalarAsync<int>(query, parameters, CommandType.Text);
        }

        // Returns the allocated lots persisted in Txn.TicketMaterialLots for a ticket.
        // Joins Txn.TicketMaterials so callers get PartNumber / Uom / LotNumber without
        // a second round-trip to Camstar.
        public async Task<IEnumerable<MaterialDetails>> GetTicketMaterialLotsByTicketAsync(string ticketNumber)
        {
            var query = @"SELECT  tml.LotId          AS LotId,
                                  tm.RequestedPartNumber AS PartNumber,
                                  tml.Quantity       AS Quantity,
                                  tm.Uom             AS Uom,
                                  tm.LotNumber       AS LotNumber,
                                  tm.TicketNumber    AS TicketNumber
                          FROM    Txn.TicketMaterialLots tml
                          INNER JOIN Txn.TicketMaterials tm ON tm.Id = tml.TicketMaterialId
                          WHERE   tm.TicketNumber = @TicketNumber;";

            var parameters = new DynamicParameters();
            parameters.Add("@TicketNumber", ticketNumber);

            return await _db.QueryAsync<MaterialDetails>(query, parameters, CommandType.Text);
        }

        // Batched TicketNumber -> SubAreaName resolver. Used by the WMS
        // Material Return page: Camstar lots only carry a TicketNumber
        // reference, and the sub-area lives on the ticket header, so we
        // join Txn.TicketNumbers to Ref.SubArea in one round-trip rather
        // than per-lot.
        public async Task<IDictionary<string, string>> GetSubAreaNamesByTicketsAsync(IEnumerable<string> ticketNumbers)
        {
            var tickets = (ticketNumbers ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (tickets.Count == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var query = @"SELECT  tn.TicketNumber AS TicketNumber,
                                  sa.SubAreaName  AS SubAreaName
                            FROM  Txn.TicketNumbers tn
                       LEFT JOIN  Ref.SubArea sa
                              ON  tn.SubAreaId = sa.SubAreaId
                           WHERE  tn.TicketNumber IN @Tickets;";

            var parameters = new DynamicParameters();
            parameters.Add("@Tickets", tickets);

            var rows = await _db.QueryAsync<(string TicketNumber, string SubAreaName)>(
                query, parameters, CommandType.Text);

            return rows
                .Where(r => !string.IsNullOrWhiteSpace(r.TicketNumber))
                .GroupBy(r => r.TicketNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.SubAreaName).FirstOrDefault(),
                    StringComparer.OrdinalIgnoreCase);
        }

    }
}

