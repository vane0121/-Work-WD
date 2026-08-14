// GoodsReceivingController.cs
using Autofac.Features.Indexed;
using M2OSS.DTO.WMS;
using M2OSS.Service.WMS.Interface;
using M2OSS.Web.Helper;
using M2OSS.Web.Helper.XmlConverter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace M2OSS.Web.Controllers.WMS
{
    public class GoodsReceivingController : BaseController
    {
        private readonly IGoodsReceivingService _goodsReceivingService;
     
        public GoodsReceivingController(IGoodsReceivingService goodsReceivingService)
        {
            _goodsReceivingService = goodsReceivingService;
        }
        // GET: Receiving
        public ActionResult Index()
        {
            SetPageHeader("Goods Receiving");
            return View("~/Views/WMS/Receiving/Receiving.cshtml");
        }

        public async Task<JsonResult> GetPartNumberList()
        {
            var partNumbers = await _goodsReceivingService.GetMaterialPartNumbersAsync();
            return Json(partNumbers);
        }

        public JsonResult GetPartNumberListInCsv()
        {
            string filePath = Server.MapPath("~/testData/PartNumberSampleData.csv");
            var partNumbers = _goodsReceivingService.GetMaterialPartNumbersInCsvAsync(filePath);
            return Json(partNumbers);
        }


        public async Task<JsonResult> GetOwnerList()
        {
            var owners = await _goodsReceivingService.GetOwnerAsync();
            return Json(owners);
        }

        public async Task<JsonResult> GetFactoryList()
        {
            var factories = await _goodsReceivingService.GetFactoryAsync();
            return Json(factories);
        }

        public async Task<JsonResult> GetVendorList()
        {
            var vendors = await _goodsReceivingService.GetVendorListAsync();
            return Json(vendors);
        }

        public async Task<JsonResult> GetVendorListByPartNumber(string partNumber)
        {
            var vendors = await _goodsReceivingService.GetVendorListByPartNumberAsync(partNumber);
            return Json(vendors);
        }

        public async Task<JsonResult> SaveMaterial(IEnumerable<MaterialDetailsDTO> materialDto)
        {
            int failedInsert = 0;
            List<MaterialDetailsDTO> failedInsertData = new List<MaterialDetailsDTO>();

            var tasks = materialDto.Select(async material =>
            {
              
                bool success = await _goodsReceivingService.CreateMaterialLotAsync(material);
                if (!success)
                {
                    lock (failedInsertData)
                    {
                        failedInsert++;
                        failedInsertData.Add(material);
                    }
                }
            });

            await Task.WhenAll(tasks);


            if (failedInsert > 0)
            {
                return Json(new
                {
                    result = false,
                    data = failedInsertData,
                    count = materialDto.Count() - failedInsertData.Count()
                });
            }
            else
            {
                return Json(new
                {
                    result = true,
                    data = "",
                    count = materialDto.Count()
                }) ;
            }

            
        }

        public async Task<JsonResult> GetPartNumberDetails(string partNumber)
        {
            var pn = await _goodsReceivingService.GetMaterialPartNumberDetailsAsync(partNumber);
            return Json(pn);
        }

        public async Task<JsonResult> GetUomList()
        {
            var uoms = await _goodsReceivingService.GetUomList();
            return Json(uoms);
        }

        public JsonResult GetCategoryList()
        {
            var category = _goodsReceivingService.GetCategoryList();
            return Json(category);
        }


        public async Task<JsonResult> GetUserEmailList(string input)
        {
            var userDetails = await _goodsReceivingService.GetUserDetailsList(LdapHelper.EscapeLdapSearchFilter(input));
            return Json(userDetails);
        }

        // Returns the list of known descriptions for the "no part number" dummies
        // (NP000..NP111). Used to populate the description Select2 on Receiving.cshtml
        // when one of those dummy part numbers is selected.
        public async Task<JsonResult> GetMaterialDescriptions()
        {
            var descriptions = await _goodsReceivingService.GetMaterialDescriptionsAsync();
            return Json(descriptions, JsonRequestBehavior.AllowGet);
        }

        // Persists a new description typed by the operator (Select2 tag input).
        // Idempotent at the repository layer (UNIQUE constraint guards duplicates).
        [HttpPost]
        public async Task<JsonResult> SaveMaterialDescription(string description, string createdBy, string uom)
        {
            var ok = await _goodsReceivingService.AddMaterialDescriptionAsync(description, createdBy, uom);
            return Json(new { result = ok });
        }
    } 
}

// MaterialDetailsDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.DTO.WMS
{
    public class MaterialDetailsDTO
    {
        public string LotId { get; set; }
        public string LotNumber { get; set; }
        public string PartNumber { get; set; }
        public string FactoryName { get; set; }
        public string OwnerName { get; set; }
        public string Workflow { get; set; }
        public string WorkflowStep { get; set; }
        public int? Quantity { get; set; }
        public string Uom { get; set; }
        public string PoNumber { get; set; }
        public string PoLineNumber { get; set; }
        public string Vendor { get; set; }
        public string InvoiceNumber { get; set; }
        public string WaybillNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Category { get; set; }
        public DateTime? DateReceive { get; set; }
        public string Remarks { get; set; }
        public string ReceivingLocation { get; set; }
        public string ReceivedBy { get; set; }
        public string DrNumber { get; set; }
        public string WmsKeyNumber { get; set; }
        public string PalletId { get; set; }
        public bool WithInspection { get; set; }
        public string ParentLotId { get; set; }
        public string OtherRemarks { get; set; }
        public string Description { get; set; }
        public string SupplierLotNum { get; set; }
        public string ReqNotes { get; set; }
        public string RequestorID { get; set; }
        public string TicketNumber { get; set; }
        public string DateRequest { get; set; }
        public string PlannerID { get; set; }
        public string TicketStatus { get; set; }
        public string actionHistory { get; set; }
        public string ApproverNotes { get; set; }
        public string DateApproval { get; set; }
        public string BuStatus { get; set; }
        public string Operation { get; set; }
        public string OwnerEmail { get; set; }
        public string ReceiverName { get; set; }
        public string DefectCode { get; set; }
        public string DeliveryType { get; set; }
        public string PreviousOperation { get; set; }
        public int BoxId { get; set; }
        public string RequestorName { get; set; }
        public string PlannerName { get; set; }
        public int? RequestedQuantity { get; set; }

        public string IssuanceStatus { get; set; }
        public List<string> LotIds { get; set; }
        public string ReferenceLotNumber { get; set; }
    }
}

// LdapHelper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M2OSS.Web.Helper
{
    public class LdapHelper
    {
        public static string EscapeLdapSearchFilter(string input)
        {
            return input
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }
    }
}

// GoodsReceivingService.cs
using AutoMapper;
using M2OSS.DTO.Common;
using M2OSS.DTO.Material;
using M2OSS.DTO.WMS;
using M2OSS.Entities.WMS;
using M2OSS.DTO.E_PULL;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.Material.Interface;
using M2OSS.Repository.Vendors.Interface;
using M2OSS.Repository.Material.Repository;
using M2OSS.Service.Common;
using M2OSS.Service.WMS.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace M2OSS.Service.WMS.Service
{
    public class GoodsReceivingService: IGoodsReceivingService
    {
        private readonly IMapper _mapper;
        private readonly ICamstarTransactionRepository _camstarTransactionRepository;
        private readonly IWebConfigurationService _webConfigurationRepository;
        private readonly ILdapService _ldapRepository;
        private readonly IXmlConverterService _xmlConverterRepository;
        private readonly IPhoMaterialRepository _phoMaterialRepository;
        private readonly IPhoVendorRepository _phoVendorRepository;

        
        public GoodsReceivingService(IMapper mapper, ICamstarTransactionRepository camstarTransactionRepository, IWebConfigurationService webConfigurationRepository, ILdapService ldapRepository, IXmlConverterService xmlConverterRepository, IPhoMaterialRepository phoMaterialRepository, IPhoVendorRepository phoVendorRepository)
        {
            _mapper = mapper;
            _camstarTransactionRepository = camstarTransactionRepository;
            _webConfigurationRepository = webConfigurationRepository;
            _ldapRepository = ldapRepository;
            _xmlConverterRepository = xmlConverterRepository;
            _phoMaterialRepository = phoMaterialRepository;
            _phoVendorRepository = phoVendorRepository;
        }

        public async Task<IEnumerable<VendorDTO>> GetVendorListAsync()
        {
            var vendors = await _phoVendorRepository.GetAllVendorsAsync();
            return _mapper.Map<IEnumerable<VendorDTO>>(vendors);
        }

        // TODO: Switch the Receiving page to this once Ref.MaterialVendors is populated, so the dropdown
        // only shows vendors actually mapped to the selected part number.
        public async Task<IEnumerable<VendorDTO>> GetVendorListByPartNumberAsync(string partNumber)
        {
            var vendors = await _phoMaterialRepository.GetVendorsByPartNumberAsync(partNumber);
            return _mapper.Map<IEnumerable<VendorDTO>>(vendors);
        }
        public async Task<IEnumerable<OwnerDTO>> GetOwnerAsync()
        {
            var owners = await _camstarTransactionRepository.GetOwnerAsync();
            return _mapper.Map<IEnumerable<OwnerDTO>>(owners);
        }

        public async Task<IEnumerable<FactoryDTO>> GetFactoryAsync()
        {
            var factories = await _camstarTransactionRepository.GetFactoryAsync();
            return _mapper.Map<IEnumerable<FactoryDTO>>(factories);
        }

        //public async Task<IEnumerable<MaterialPartNumbersDTO>> GetMaterialPartNumbersAsync()
        //{

        //    var materialPnList = await _camstarTransactionRepository.GetMaterialPartNumberAsync();
        //    return _mapper.Map<IEnumerable<MaterialPartNumbersDTO>>(materialPnList);
        //}

        public IEnumerable<MaterialPartNumbersDTO> GetMaterialPartNumbersInCsvAsync(string filepath)
        {

            var materialPartNumberInCsv = _camstarTransactionRepository.ReadCsv(filepath);
            var materialPartNumberList = materialPartNumberInCsv.ToList();
            foreach (var material in materialPartNumberList)
            {
                if (material.MaterialPartNumber.Contains("XCA"))
                {
                    material.InventoryType = "CONSUMABLES";
                }
                else if (material.MaterialPartNumber.Contains("XCC"))
                {
                    material.InventoryType = "CHEMICALS";
                }
                else if (material.MaterialPartNumber.Contains("XJF"))
                {
                    material.InventoryType = "MSPJF";
                }
                else
                {
                    material.InventoryType = "COSTEDIDM";
                }
            }

            return _mapper.Map<IEnumerable<MaterialPartNumbersDTO>>(materialPartNumberList.AsEnumerable());
        }

        public async Task<IEnumerable<PartNumbersDTO>> GetMaterialPartNumbersAsync()
        {
            var partnumbers = await _phoMaterialRepository.GetAllMaterialPartNumbersASync();
            return _mapper.Map<IEnumerable<PartNumbersDTO>>(partnumbers);
        }

        public async Task<MaterialPartNumbersDTO> GetMaterialPartNumberDetailsAsync(string pn)
        {
            var partNumberlist = await _camstarTransactionRepository.GetMaterialPartNumberAsync();
            var partNumber = partNumberlist.Where(w => w.MaterialPartNumber == pn).FirstOrDefault();

            return _mapper.Map<MaterialPartNumbersDTO>(partNumber);
        }

        public async Task<bool> CreateMaterialLotAsync(MaterialDetailsDTO materialDto)
        {
            materialDto.LotId = $"{materialDto.PartNumber}-{DateTime.Now:yyyyMMddhhmmss}";

            var material = _mapper.Map<MaterialDetails>(materialDto);
            var createMaterialXml = _xmlConverterRepository.CreateMaterialXml(material);
            var setAttributesXml = _xmlConverterRepository.CreateMaterialAttributesXml(material);
            var movelotXml = _xmlConverterRepository.MaterialInventoryMoveXml(material);

            var created =  await _camstarTransactionRepository.CreateMaterialLotAsync(material, createMaterialXml);
            
            bool setAttributes;
            if (created.Item2)
                setAttributes = await _camstarTransactionRepository.SetMaterialLotAttributeAsync(material,"PWH_0001", setAttributesXml);
            else
                return created.Item2;


            if (setAttributes)
            {
                var moved = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, movelotXml);
                //if (material.Category != "VMI")
                //    return await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, movelotXml);
                //else 
                //    return true;

                // Audit trail for the dummy NPxxx receivings: once the lot is
                // fully created in Camstar (lot + attributes + inventory move
                // all succeeded), persist one row in Txn.MaterialNoPartNumberLots
                // linking the new LotId to the picked Material Name. Best-effort;
                // a failure here must not roll back the Camstar lot.
                if (moved && DummyPartNumber.IsDummy(materialDto.PartNumber))
                {
                    try
                    {
                        await _phoMaterialRepository.AddMaterialNoPartNumberLotAsync(
                            materialDto.LotId,
                            materialDto.PartNumber,
                            materialDto.Description,
                            materialDto.Quantity,
                            materialDto.LotNumber,
                            materialDto.DateReceive ?? DateTime.Now);
                    }
                    catch
                    {
                        // Swallow: the lot already exists in Camstar; the audit
                        // row is a non-critical follow-up step.
                    }
                }

                return moved;
            }
            else
                return setAttributes;
           
        }

        public async Task<IEnumerable<string>> GetUomList()
        {
            //return _webConfigurationRepository.GetUomList();
            return await _phoMaterialRepository.GetUomAsync();
        }

        public IEnumerable<string> GetCategoryList()
        {
            return _webConfigurationRepository.GetCategoryList();
        }

        public async Task<IEnumerable<UserDTO>> GetUserDetailsList(string input)
        {
            var userDetails = await _ldapRepository.GetEmployeeDetailsAsync(input);
            return _mapper.Map<IEnumerable<UserDTO>>(userDetails);
        }

        public async Task<IEnumerable<string>> GetMaterialDescriptionsAsync()
        {
            return await _phoMaterialRepository.GetMaterialDescriptionsAsync();
        }

        public async Task<bool> AddMaterialDescriptionAsync(string description, string createdBy, string uom)
        {
            if (string.IsNullOrWhiteSpace(description))
                return false;

            var affected = await _phoMaterialRepository.AddMaterialDescriptionAsync(description.Trim(), createdBy, (uom ?? string.Empty).Trim());
            return affected > 0;
        }

    }
}

// IGoodsReceivingService.cs
using M2OSS.DTO.Common;
using M2OSS.DTO.E_PULL;
using M2OSS.DTO.Material;
using M2OSS.DTO.WMS;
using M2OSS.Entities.WMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Service.WMS.Interface
{
    public interface IGoodsReceivingService
    {
        Task<IEnumerable<OwnerDTO>> GetOwnerAsync();

        Task<IEnumerable<FactoryDTO>> GetFactoryAsync();
        Task<IEnumerable<PartNumbersDTO>> GetMaterialPartNumbersAsync();
        Task<bool> CreateMaterialLotAsync(MaterialDetailsDTO materialDto);
        Task<MaterialPartNumbersDTO> GetMaterialPartNumberDetailsAsync(string pn);
        Task<IEnumerable<string>> GetUomList();
        IEnumerable<string> GetCategoryList();
        IEnumerable<MaterialPartNumbersDTO> GetMaterialPartNumbersInCsvAsync(string filepath);
        Task<IEnumerable<UserDTO>> GetUserDetailsList(string input);
        Task<IEnumerable<VendorDTO>> GetVendorListAsync();
        // TODO: Once Ref.MaterialVendors has data, switch the Receiving page to use this filtered list per selected part number.
        Task<IEnumerable<VendorDTO>> GetVendorListByPartNumberAsync(string partNumber);

        // Goods Receiving "no part number" descriptions (dummies NP000..NP111).
        Task<IEnumerable<string>> GetMaterialDescriptionsAsync();
        Task<bool> AddMaterialDescriptionAsync(string description, string createdBy, string uom);
    }
}

// VendorDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.E_PULL
{
    public class VendorDTO
    {
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
    }
}

// OwnerDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.WMS
{
    public class OwnerDTO
    {
        public string OwnerName { get; set; }
        public string OwnerDescription { get; set; }
        public string Revision { get; set; }
        public string Status { get; set; }
    }
}

// FactoryDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.WMS
{
    public class FactoryDTO
    {
        public string FactoryName { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string Status { get; set; }
    }
}

// MaterialPartNumbersDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.WMS
{
    public class MaterialPartNumbersDTO
    {
        public string MaterialPartNumber { get; set; }
        public string MaterialPartDescription { get; set; }
        public string Revision { get; set; }
        public string Status { get; set; }
        public bool WithExpiration { get; set; }
        public bool IsLotControlled { get; set; }
        public bool WithInspection { get; set; }

        public string InventoryType { get; set; }
        public string Uom { get; set; }
    }
}

// PartNumbersDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.Material
{
    public class PartNumbersDTO
    {
        public string PartNumber { get; set; }
        public string MaterialName { get; set; }
        public string Uom { get; set; }
        public string CommodityType { get; set; }
        public string PlannerId { get; set; }
        public string SpendingTreatment { get; set; }
        public string UsageFrequency { get; set; }
        public int UsageFrequencyId { get; set; }
        public int FrequencyValue { get; set; }
        public bool IsVmi { get; set; }
        public int Moq { get; set; }
        public double Allocation { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoIssued { get; set; }
        public bool WithInspection { get; set; }
        public bool WithExpiration { get; set; }
        public bool IsLotControlled { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }

        public string WmsCommodityType { get; set; }
        public string SubInventory { get; set; }
        public string SubInventoryDescription { get; set; }
        public int SubAreaId { get; set; }
        public string SubAreaName { get; set; }
        public double BasicUnit { get; set; }
        public string Location { get; set; }

        public string UsageRatio { get; set; }
        public bool WithHost { get; set; }
        public string WorkflowStep { get; set; }


    }
}

// MaterialDetails.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Entities.WMS
{
    public class MaterialDetails
    {
        public string LotId { get; set; }
        public string LotNumber { get; set; }
        public string PartNumber { get; set; }
        public string FactoryName { get; set; }
        public string OwnerName { get; set; }
        public string Workflow { get; set; }
        public string WorkflowStep { get; set; }
        public int? Quantity { get; set; }
        public string Uom { get; set; }
        public string PoNumber { get; set; }
        public string PoLineNumber { get; set; }
        public string Vendor { get; set; }
        public string InvoiceNumber { get; set; }
        public string WaybillNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Category { get; set; }
        public DateTime? DateReceive { get; set; }
        public string Remarks { get; set; }
        public string ReceivingLocation { get; set; }
        public string ReceivedBy { get; set; }
        public string DrNumber { get; set; }
        public string WmsKeyNumber { get; set; }
        public string PalletId { get; set; }
        public bool WithInspection { get; set; }
        public string ParentLotId { get; set; }
        public string OtherRemarks { get; set; }
        public string Description { get; set; }
        public string SupplierLotNum { get; set; }
        public string ReqNotes { get; set; }
        public string RequestorID { get; set; }
        public string TicketNumber { get; set; }
        public string DateRequest { get; set; }
        public string PlannerID { get; set; }
        public string TicketStatus { get; set; }
        public string actionHistory { get; set; }
        public string ApproverNotes { get; set; }
        public string DateApproval { get; set; }
        public string BuStatus { get; set; }
        public string Operation { get; set; }
        public string RequestorName { get; set; }
        public string PlannerName { get; set; }
        public string OwnerEmail {get; set;}
        public string ReceiverName { get; set; }
        public string DefectCode { get; set; }
        public string DeliveryType { get; set; }
        public string PreviousOperation { get; set; }
        public int BoxId { get; set; }
        public int? RequestedQuantity { get; set; }
        public string IssuanceStatus { get; set; }
        public string ReferenceLotNumber { get; set; }

    }

}

// UserDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.Common
{
    public class UserDTO
    {
        public string EmployeeId { get; set; } = "";
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Department { get; set; } = "";
        public string Site { get; set; } = "";
        public string ViewingSite { get; set; } = "";
        public string NickName { get; set; }
        public string Title { get; set; }
        public string ReportingEmail { get; set; }
        public string ReportingId { get; set; }

        public List<string> Groups { get; set; } = new List<string>();

    }
}

// Receiving.cshtml



@{
    ViewBag.Title = "Goods Receiving";
}
    <div class="card border-0 shadow-sm">
        <div class="card-header border-bottom px-3 py-2">
            <h6>Receiving Details</h6>
        </div>
        <div class="card-body px-4 py-3" style="overflow: visible!important;">

            <div class="form-group d-none">
                <label for="txtLotId">Camstar Lot ID:</label>
                <input type="text" class="form-control" id="txtLotId" readonly />
            </div>
            <div class="row">
                <div class="col-lg">
                    <div class="form-group">
                        <label for="cmbDeliveryType">Delivery Type:</label>
                        <select class="form-control select2" id="cmbDeliveryType">
                            <option value=""></option>
                            <option value="FOREIGN">FOREIGN</option>
                            <option value="LOCAL">LOCAL</option>
                        </select>
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="cmbWorkflow">Workflow:</label>
                        <select class="form-control select2" id="cmbWorkflow">
                        </select>
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtInvoiceNumber">Invoice Number:</label>
                        <input type="text" class="form-control" id="txtInvoiceNumber" />
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtPoNumber">PO Number:</label>
                        <input type="text" class="form-control" id="txtPoNumber" />
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtPoLineNumber">PO Line Number:</label>
                        <input type="text" class="form-control" id="txtPoLineNumber" />
                    </div>
                </div>
                
            </div>

            <div class="row">
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtWaybillNumber">Waybill Number:</label>
                        <input type="text" class="form-control" id="txtWaybillNumber" />
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtDrNumber">DR Number:</label>
                        <input type="text" class="form-control" id="txtDrNumber" />
                    </div>
                </div>

                <div class="col-lg">
                    <div class="form-group">
                        <label for="cmbOwner">Owner:</label>
                        <select class="form-control select2" id="cmbOwner">
                        </select>
                    </div>
                </div>
                <div class="col-lg d-none" id="div-ownerEmail">
                    <div class="form-group">
                        <label for="cmbOwnerEmail">Owner Email:</label>
                        <select class="form-control select2 " id="cmbOwnerEmail">
                        </select>
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="cmbFactory">Factory:</label>
                        <select class="form-control select2" id="cmbFactory">
                        </select>
                    </div>
                </div>
                <div class="col-lg">
                    <div class="form-group">
                        <label for="txtPalletQty">Pallet Qty:</label>
                        <input type="number" min="1" class="form-control" id="txtPalletQty" value="1" />
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-5">
                    <div class="form-group clearfix">
                        <div class="custom-control custom-checkbox">
                            <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxMultiLot">
                            <label for="cbxMultiLot" class="custom-control-label">Common Part Number</label>
                        </div>

                    </div>
                </div>
                <div class="col-lg-5">

                </div>
                <div class="col-lg-2">
                    <div class="custom-control custom-checkbox float-right">
                        <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxWithInspection" disabled>
                        <label for="cbxWithInspection" class="custom-control-label">w/ inspection?</label>
                    </div>
                </div>

            </div>

            <div class="row">
                <div class="col-lg-3">
                    <div class="form-group">
                        <label for="txtPartNumber">Part Number:</label>
                        <input type="text" class="form-control d-none" id="txtPartNumber" />
                        <select class="form-control select2" id="cmbPartNumber">
                        </select>
                        <text id="txtPartNumberDesc"></text>
                        @* Description picker for the "no part number" dummies (NP000..NP111).
                           Hidden by default; the goodsReceiving.js script reveals it when a
                           dummy part number is selected and populates it via
                           GetMaterialDescriptions. New descriptions typed in (Select2 tags)
                           are persisted via SaveMaterialDescription, and the chosen value
                           is mirrored into #txtPartNumberDesc. *@
                        <div id="divPartNumberDescPicker" class="d-none mt-1">
                            <select class="form-control select2" id="cmbPartNumberDesc"></select>
                            <small class="form-text">
                                <a href="#" id="lnkNoPartNumberMatrix" data-toggle="modal" data-target="#NoPartNumberMatrixModal">
                                    <i class="fa fa-info-circle"></i> What does NPxxx mean?
                                </a>
                            </small>
                        </div>
                    </div>
                </div>
                <div class="col-lg-3">
                    <div class="form-group">
                        <label for="cmbVendor">Vendor:</label>
                        <select class="form-control select2" id="cmbVendor">
                            <option value=""></option>
                        </select>
                    </div>
                </div>
                <div class="col-lg-2">
                    <div class="form-group">
                        <label for="txtLotNumber">Lot Number:</label>
                        <input type="text" class="form-control" id="txtLotNumber" />
                    </div>
                </div>
                <div class="col-lg-2">
                    <div class="form-group">
                        <label for="txtExpiryDate">Expiration Date:</label>
                        <input type="date" class="form-control" id="txtExpiryDate" />
                    </div>
                </div>

                <div class="col-lg-1">
                    <div class="form-group">
                        <label for="cmbUom">UOM:</label>
                        <select class="form-control select2" id="cmbUom" disabled>
                            @*<option value=""></option>
                            <option value="BAG">BAG</option>
                            <option value="BOTTLE">BOTTLE</option>
                            <option value="BOX">BOX</option>
                            <option value="BUCKET">BUCKET</option>
                            <option value="CAN">CAN</option>
                            <option value="CARBOY">CARBOY</option>
                            <option value="CYLINDER">CYLINDER</option>
                            <option value="DRUM">DRUM</option>
                            <option value="EACH">EACH</option>
                            <option value="GALLON">GALLON</option>
                            <option value="LITER">LITER</option>
                            <option value="METER">METER</option>
                            <option value="MILLILITER">MILLILITER</option>
                            <option value="PACK">PACK</option>
                            <option value="PACKAGE">PACKAGE</option>
                            <option value="PAIL">PAIL</option>
                            <option value="PAIR">PAIR</option>
                            <option value="PIECE">PIECE</option>
                            <option value="REAM">REAM</option>
                            <option value="ROLL">ROLL</option>
                            <option value="SET">SET</option>
                            <option value="SHEET">SHEET</option>
                            <option value="SPOOL">SPOOL</option>
                            <option value="TUBE">TUBE</option>*@
                        </select>
                    </div>
                </div>
                <div class="col-lg-1">
                    <div class="form-group">
                        <label for="txtQuantity">Quantity:</label>
                        <input type="number" accept="any" class="form-control" id="txtQuantity" />

                    </div>
                </div>
            </div>
            <div class="form-group">
                <label for="txtRemarks">Additional Comments/Remarks: (Optional)</label>
                <textarea class="form-control" placeholder="maximum character is 250." id="txtRemarks"></textarea>
            </div>
        </div>
        <div class="card-footer border-top px-4 py-3 footer-dark-mode">
            <div class="float-left d-none" id="divMaterialAttributes">
                <label>Inventory Type: <span id="txtInvType" style="font-weight:normal"></span></label><br/>
                <label>Rack Location: <span id="txtRackLocation" style="font-weight:normal"></span></label>
                
            </div>
            <div class="float-right">
                <button class="btn btn-primary" id="btn-add" data-mode="add">Add</button>
            </div>

        </div>
    </div>

    <div class="card border-0 shadow-sm">
        <div class="card-header border-bottom px-3 py-2">
            <div class="float-left">
                <ul class="nav nav-tabs" role="tablist" id="dynamicTabs">
                </ul>
            </div>
           
        </div>
        <div class="card-body px-4 py-3" style="overflow: visible!important;">

            <div id="dynamicTabContent" class="tab-content"></div>


        </div>
        <div class="card-footer border-top px-4 py-3 footer-dark-mode">
            <div class="">
                <button type="button" id="btn-receive" class="btn btn-primary btn-block px-4" disabled>
                    Receive
                </button>

            </div>

        </div>
    </div>



    

    

@*/////////////////////////////////////////////////////////////////////////////////////////////////////*@


    @*<div class="card border-0 shadow-sm">
        <div class="card-header border-bottom px-3 py-2">
            <h6>Receiving Details</h6>
        </div>
        <div class="card-body px-4 py-3" style="overflow: visible!important;">
            <div class="row">
                <div class="col-lg-6">
                    <div class="form-group">
                        <label for="cmbWorkflow">Workflow:</label>
                        <select class="form-control select2" id="cmbWorkflow">
                            <option value=""></option>
                            <option value="INVENTORY">INVENTORY</option>
                            <option value="EXPENSE">EXPENSE</option>
                            <option value="NCV">NCV</option>
                            <option value="VMI">VMI</option>
                        </select>
                    </div>
                </div>
                <div class="col-lg-4">
                    <div class="form-group">
                        <label for="txtInvoiceNumber">Invoice Number:</label>
                        <input type="text" class="form-control" id="txtInvoiceNumber" />
                    </div>
                </div>
                <div class="col-lg-2">
                    <div class="form-group" id="div-pallet">
                        <label for="txtPalletId">Pallet Number: </label> <i class="fas fa-exchange-alt fa-xs text-info" style="cursor:pointer" title="Change Pallet ID"></i>
                        <input type="text" class="form-control" id="txtPalletId" readonly/>
                    </div>


                </div>
            </div>
            
            <button type="button" id="btn-add-box" class="btn btn-info px-4" disabled>
                Add Box
            </button>
            <br />
            <br />
            <div class="table-responsive">
                <table id="ReceiveTable" class="table table-hover align-middle w-100 d-none">
                    <thead class="text-dark bg-light">
                        <tr>
                            <th>Pallet Number</th>
                            <th>Box Number</th>
                            <th>Part Number</th>
                            <th>Lot Number</th>
                            <th>PO Number</th>
                            <th>PO Line Number</th>
                            <th>DR Number</th>
                            <th>Waybill Number</th>
                            <th>Quantity</th>
                            <th>Expiration Date</th>
                            <th>Received Date</th>
                            <th>Remarks</th>
                            <th class="d-none">Vendor</th>
                            <th class="d-none">Owner</th>
                            <th class="d-none">Factory</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody id="ReceiveTableBody"></tbody>
                </table>
            </div>
        </div>
        <div class="card-footer border-top px-4 py-3 footer-dark-mode">
            <div class="">
                <button type="button" id="btn-receive" class="btn btn-primary btn-block px-4" disabled>
                    Receive
                </button>
              
            </div>

        </div>
    </div>*@





@*<div class="modal fade" id="ReceivingModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-xl">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Goods Recieving</h5>
                <button type="button" id="close-receiving" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <div class="row">
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="txtGeneratedPalletId">Pallet ID:</label>
                            <input type="text" class="form-control" id="txtGeneratedPalletId" />
                        </div>
                    </div>
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="txtBoxId">Box ID:</label>
                            <input type="text" class="form-control" id="txtBoxId" />
                        </div>
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-lg-5">

                    </div>
                    <div class="col-lg-5">

                    </div>
                    <div class="col-lg-2">
                        <div class="custom-control custom-checkbox float-right">
                            <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxWithInspection" disabled>
                            <label for="cbxWithInspection" class="custom-control-label">w/ inspection?</label>
                        </div>
                    </div>

                </div>


                <div class="row">
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtPoNumber">PO Number:</label>
                            <input type="text" class="form-control" id="txtPoNumber" />
                        </div>
                    </div>
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtPoLineNumber">PO Line Number:</label>
                            <input type="text" class="form-control" id="txtPoLineNumber" />
                        </div>
                    </div>

                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtWaybillNumber">Waybill Number:</label>
                            <input type="text" class="form-control" id="txtWaybillNumber" />
                        </div>
                    </div>
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtDrNumber">DR Number:</label>
                            <input type="text" class="form-control" id="txtDrNumber" />
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtPartNumber">Part Number:</label>
                            <input type="text" class="form-control d-none" id="txtPartNumber" />
                            <select class="form-control select2" id="cmbPartNumber">
                            </select>
                        </div>
                    </div>

                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtLotNumber">Lot Number:</label>
                            <input type="text" class="form-control" id="txtLotNumber" />
                        </div>
                    </div>
                    <div class="col-lg-2">
                        <div class="form-group">
                            <label for="txtExpiryDate">Expiration Date:</label>
                            <input type="date" class="form-control" id="txtExpiryDate" />
                        </div>
                    </div>
                    <div class="col-lg-2">
                        <div class="form-group">
                            <label for="txtQuantity">Quantity:</label>
                            <input type="number" accept="any" class="form-control" id="txtQuantity" />

                        </div>
                    </div>
                    <div class="col-lg-2">
                        <div class="form-group">
                            <label for="cmbUom">UOM:</label>
                            <select class="form-control select2" id="cmbUom" disabled>
                                <option value=""></option>
                                <option value="BAG">BAG</option>
                                <option value="BOTTLE">BOTTLE</option>
                                <option value="BOX">BOX</option>
                                <option value="BUCKET">BUCKET</option>
                                <option value="CAN">CAN</option>
                                <option value="CARBOY">CARBOY</option>
                                <option value="CYLINDER">CYLINDER</option>
                                <option value="DRUM">DRUM</option>
                                <option value="EACH">EACH</option>
                                <option value="GALLON">GALLON</option>
                                <option value="LITER">LITER</option>
                                <option value="METER">METER</option>
                                <option value="MILLILITER">MILLILITER</option>
                                <option value="PACK">PACK</option>
                                <option value="PACKAGE">PACKAGE</option>
                                <option value="PAIL">PAIL</option>
                                <option value="PAIR">PAIR</option>
                                <option value="PIECE">PIECE</option>
                                <option value="REAM">REAM</option>
                                <option value="ROLL">ROLL</option>
                                <option value="SET">SET</option>
                                <option value="SHEET">SHEET</option>
                                <option value="SPOOL">SPOOL</option>
                                <option value="TUBE">TUBE</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="row">


                    <div class="col-lg-4">
                        <div class="form-group">
                            <label for="cmbVendor">Vendor:</label>
                            <select class="form-control select2" id="cmbVendor">
                                <option value=""></option>
                            </select>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="form-group">
                            <label for="cmbOwner">Owner:</label>
                            <select class="form-control select2" id="cmbOwner">
                            </select>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="form-group">
                            <label for="cmbFactory">Factory:</label>
                            <select class="form-control select2" id="cmbFactory">
                            </select>
                        </div>
                    </div>

                </div>
                <div class="form-group">
                    <label for="txtRemarks">Additional Comments/Remarks:</label>
                    <textarea class="form-control" placeholder="optional" id="txtRemarks"></textarea>
                </div>
                <div class="form-group">
                    <label for="txtLotId">Camstar Lot ID:</label>
                    <input type="text" class="form-control" id="txtLotId" readonly />
                </div>
                <div class="" id="div-receivedDetails">
                    <label for="txtReceivedBy">Received by: <span id="txtReceivedBy"> 7351921</span></label>
                    <label for="txtReceivedDate" class="float-right">Received Date:<span id="txtReceivedDate"> 2025-10-07 08:45:33</span></label>
                </div>
                <div class="table-responsive">
                    <table id="ReceiveBoxTable" class="table table-hover align-middle w-100">
                        <thead class="text-dark bg-light">
                            <tr>

                                <th>Part Number</th>
                                <th>Lot Number</th>
                                <th>PO Number</th>
                                <th>PO Line Number</th>
                                <th>DR Number</th>
                                <th>Waybill Number</th>
                                <th>Quantity</th>
                                <th>Expiration Date</th>
                                <th>Received Date</th>
                                <th>Remarks</th>
                                <th class="d-none">Vendor</th>
                                <th class="d-none">Owner</th>
                                <th class="d-none">Factory</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody id="ReceiveBoxTableBody"></tbody>
                    </table>
                </div>

            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-between">
                <div>
                    <div class="custom-control custom-checkbox">
                        <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxMultipleParts">
                        <label for="cbxMultipleParts" class="custom-control-label">Multiple Item Receiving</label>
                    </div>
                </div>
                <div class="float-right">
                    <button type="button" id="btn-add" class="btn btn-primary px-4">
                        Add
                    </button>
                    <button type="button" id="cancel-receiving" class="btn btn-outline-secondary px-4">
                        Clear
                    </button>
                </div>
                

            </div>
        </div>

    </div>
</div>*@




@* Matrix legend for "no part number" dummies (NP000..NP111). The three suffix
   digits encode WithInspection | LotControlled | WithExpiration (1 = yes,
   0 = no). The modal is launched from #lnkNoPartNumberMatrix which is only
   visible while a dummy part number is selected. The current selection is
   highlighted by goodsReceiving.js when the modal opens. *@
<div class="modal fade" id="NoPartNumberMatrixModal" tabindex="-1" role="dialog" aria-labelledby="NoPartNumberMatrixModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="NoPartNumberMatrixModalLabel">No Part Number Code Matrix (NPxxx)</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <p class="text-muted mb-2">
                    The three digits after <strong>NP</strong> encode the material flags
                    (<code>1</code> = yes, <code>0</code> = no) in the order:
                    <strong>WithInspection</strong>, <strong>LotControlled</strong>,
                    <strong>WithExpiration</strong>.
                </p>
                <table class="table table-sm table-bordered table-striped mb-0" id="tblNoPartNumberMatrix">
                    <thead class="thead-light">
                        <tr>
                            <th>Part Number</th>
                            <th class="text-center">WithInspection</th>
                            <th class="text-center">LotControlled</th>
                            <th class="text-center">WithExpiration</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr data-pn="NP000"><td>NP000</td><td class="text-center">0</td><td class="text-center">0</td><td class="text-center">0</td></tr>
                        <tr data-pn="NP100"><td>NP100</td><td class="text-center">1</td><td class="text-center">0</td><td class="text-center">0</td></tr>
                        <tr data-pn="NP010"><td>NP010</td><td class="text-center">0</td><td class="text-center">1</td><td class="text-center">0</td></tr>
                        <tr data-pn="NP001"><td>NP001</td><td class="text-center">0</td><td class="text-center">0</td><td class="text-center">1</td></tr>
                        <tr data-pn="NP110"><td>NP110</td><td class="text-center">1</td><td class="text-center">1</td><td class="text-center">0</td></tr>
                        <tr data-pn="NP101"><td>NP101</td><td class="text-center">1</td><td class="text-center">0</td><td class="text-center">1</td></tr>
                        <tr data-pn="NP011"><td>NP011</td><td class="text-center">0</td><td class="text-center">1</td><td class="text-center">1</td></tr>
                        <tr data-pn="NP111"><td>NP111</td><td class="text-center">1</td><td class="text-center">1</td><td class="text-center">1</td></tr>
                    </tbody>
                </table>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        const AppUrls = {
            getPartNumberListInCsv: '@Url.Action("GetPartNumberListInCsv", "GoodsReceiving", new { area = "WMS" })',
            getPartNumberList: '@Url.Action("GetPartNumberList", "GoodsReceiving", new { area = "WMS" })',
            getOwnerList: '@Url.Action("GetOwnerList", "GoodsReceiving", new { area = "WMS" })',
            getFactoryList: '@Url.Action("GetFactoryList", "GoodsReceiving", new { area = "WMS" })',
            saveMaterial: '@Url.Action("SaveMaterial", "GoodsReceiving", new { area = "WMS" })',
            getPartNumberDetails: '@Url.Action("GetPartNumberDetails", "GoodsReceiving", new { area = "WMS" })',
            getUom: '@Url.Action("GetUomList", "GoodsReceiving", new { area = "WMS" })',
            getCategory: '@Url.Action("GetCategoryList", "GoodsReceiving", new { area = "WMS" })',
            getUseremail:'@Url.Action("GetUserEmailList", "GoodsReceiving", new { area = "WMS" })',
            getVendorList: '@Url.Action("GetVendorList", "GoodsReceiving", new { area = "WMS" })',
            getVendorListByPartNumber: '@Url.Action("GetVendorListByPartNumber", "GoodsReceiving", new { area = "WMS" })',
            getMaterialDescriptions: '@Url.Action("GetMaterialDescriptions", "GoodsReceiving", new { area = "WMS" })',
            saveMaterialDescription: '@Url.Action("SaveMaterialDescription", "GoodsReceiving", new { area = "WMS" })',
        };


        const currentUser = '@ViewBag.CurrentUser.EmployeeId';
        const currentUserName = '@ViewBag.CurrentUser.DisplayName';

    </script>

    <script src="~/Scripts/WMS/goodsReceiving.js"></script>
  
}


