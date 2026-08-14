// MaterialIssuance.cshtml




@{
    ViewBag.Title = "Material Issuance";
}

<div class="card border-0 shadow-sm">
    <div class="card-header border-bottom px-4 py-3">
    </div>
    <div class="card-body px-4 py-3">

        <div class="table-responsive">
            <table id="MaterialTicketTable" class="table table-hover align-middle w-100">
                <thead class="text-dark bg-light">
                    <tr>
                        <th>Ticket Number</th>
                        <th>Type</th>
                        <th>Sub Area</th>
                        <th>Requestor ID</th>
                        <th>Planner ID</th>
                        <th>Date Request</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>



@section Scripts {
    <script>
        const AppUrls = {
            getTickets: '@Url.Action("GetApprovedTickets", "MaterialIssuance", new { area = "WMS" })',


        };

        const getPartNumberListUrl = '@Url.Action("MaterialPartNumberPerTicket", "MaterialIssuance")';
        const getTicketNumberListUrl = '@Url.Action("Index", "MaterialIssuance")';
        const currentUser = '@ViewBag.CurrentUser.EmployeeId';
        const currentUserName = '@ViewBag.CurrentUser.DisplayName';
        const site = '@ViewBag.CurrentUser.Site';
    </script>

    <script src="~/Scripts/WMS/materialIssuance.js"></script>

}

// MaterialIssuanceController.cs
using M2OSS.DTO.Common;
using M2OSS.DTO.WMS;
using M2OSS.Service.WMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace M2OSS.Web.Controllers.WMS
{
    public class MaterialIssuanceController : BaseController
    {
        private readonly IMaterialIssuanceService _materialIssuanceService;
        public MaterialIssuanceController(IMaterialIssuanceService materialIssuanceService)
        {
            _materialIssuanceService = materialIssuanceService;
        }
        // GET: MaterialIssuance
        public ActionResult Index()
        {
            SetPageHeader("Material Tickets");
            return View("~/Views/WMS/MaterialIssuance/MaterialIssuance.cshtml");
        }

        public async Task<JsonResult> GetApprovedTickets()
        {
            var tickets = await _materialIssuanceService.GetAllApprovedMaterialTicketsAsync();
            return Json(tickets);
        }

        public ActionResult MaterialPartNumberPerTicket()
        {
            SetPageHeader("Material Part Number Per Ticket");
            return View("~/Views/WMS/MaterialIssuance/MaterialPartNumberPerTicket.cshtml");
        }

        public async Task<JsonResult> GetMaterialPartNumberByTicket(MaterialDetailsDTO ticketDto)
        {
            var partNumbers = await _materialIssuanceService.GetPartNumberByTicketIdAsync(ticketDto);
            return Json(partNumbers);
        }

        // Lightweight metadata endpoint used by the Material Issuance page
        // to decide whether to suppress scan / check / acknowledge. A ticket
        // qualifies when it originated from a borrow but has been converted
        // back into a regular request (BorrowedDate IS NOT NULL AND
        // TransactionType = 'REQUEST'); in that case the materials are
        // already in the requestor's hands so picking/checking are skipped.
        public async Task<JsonResult> GetTicketContext(string ticketNumber)
        {
            var isBorrowConverted = await _materialIssuanceService.IsBorrowConvertedAsync(ticketNumber);
            return Json(new { isBorrowConverted }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> AssignMaterialsPerTicket(List<MaterialDetailsDTO> materialDtoList)
        {
            string filePath = Server.MapPath("~/testData/PartNumberSampleData.csv");
            var pickedMaterials = await _materialIssuanceService.PickMaterialsByTicketNumberAsync(materialDtoList, filePath);


            return Json(pickedMaterials);
        }

        public async Task<JsonResult> GetAssignedMaterialForIssuance(MaterialDetailsDTO ticketDto)
        { 
            var picked = await _materialIssuanceService.GetLotAssignedPerTicket(ticketDto);

            return Json(picked);
        }

        public async Task<JsonResult> PickMaterial(MaterialDetailsDTO materialDto)
        {
            string filePath = Server.MapPath("~/testData/PartNumberSampleData.csv");
            var picked = await _materialIssuanceService.PickedMaterialsAsync(materialDto,filePath);
            return Json(picked);

        }

        public async Task<JsonResult> IssueMaterial(List<MaterialDetailsDTO> materialForIssuance, List<MaterialDetailsDTO> materialDto)
        {
            var userDetails = Session["User"] as UserDTO;
            var issued = await _materialIssuanceService.IssueMaterialsAsync(materialForIssuance, userDetails.DisplayName, materialDto);
            return Json(issued);

        }
        public async Task<JsonResult> CheckMaterial(List<MaterialDetailsDTO> materialDtoList)
        {
            var userDetails = Session["User"] as UserDTO;
            var issued = await _materialIssuanceService.CheckMaterialsAsync(materialDtoList, userDetails.DisplayName);
            
            return Json(issued);

        }

        public async Task<JsonResult> AcknowledgeTicket(string ticket)
        {
            var res = await _materialIssuanceService.AcknowledgeRequestTicketAsync(ticket);
            return Json(res);
        }
    }
}

// MaterialIssuanceService.cs
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using M2OSS.DTO.WMS;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Entities.E_POU;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.Common.Service;
using M2OSS.Repository.E_PULL.Interface;
using M2OSS.Repository.Material.Interface;
using M2OSS.Service.Common;
using M2OSS.Service.DigitalWorkers.Interface;
using M2OSS.Service.WMS.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Service.WMS.Service
{
    public class MaterialIssuanceService: IMaterialIssuanceService
    {
        private readonly IMapper _mapper;
        private readonly ICamstarTransactionRepository _camstarTransactionRepository;
        private readonly IWebConfigurationService _webConfigurationRepository;
        private readonly IXmlConverterService _xmlConverterService;
        private readonly Repository.Common.Interface.ILabelPrintingService _labelPrintingRepository;
        private readonly IDigitalWorkerExecutorService _digitalWorkerExecutorService;
        private readonly IPhoTicketRepository _ticketRepository;
        private readonly IPhoMaterialRepository _materialRepository;
        
        public MaterialIssuanceService(IMapper mapper, ICamstarTransactionRepository camstarTransactionRepository, IWebConfigurationService webConfigurationRepository, IXmlConverterService xmlConverterService, Repository.Common.Interface.ILabelPrintingService labelPrintingRepository, IDigitalWorkerExecutorService digitalWorkerExecutorService, IPhoTicketRepository ticketRepository, IPhoMaterialRepository materialRepository)
        {
            _mapper = mapper;
            _camstarTransactionRepository = camstarTransactionRepository;
            _webConfigurationRepository = webConfigurationRepository;
            _xmlConverterService = xmlConverterService;
            _labelPrintingRepository = labelPrintingRepository;
            _digitalWorkerExecutorService = digitalWorkerExecutorService;
            _ticketRepository = ticketRepository;
            _materialRepository = materialRepository;
            
        }

        public async Task<IEnumerable<MaterialTicketDTO>> GetAllApprovedMaterialTicketsAsync()
        {
            // Include 'partial' alongside 'approved' so tickets that have
            // had SOME of their lines issued (but not all) stay visible on
            // the warehouse issuance grid until the remaining lines are
            // finished. 'closed' tickets drop off as before.
            var tickets = await _ticketRepository.GetTicketsByStatusAsync("approved", "partial");
            return _mapper.Map<IEnumerable<MaterialTicketDTO>>(tickets);


            //var tickets = await _camstarTransactionRepository.GetAllApprovedMaterialTicketAsync();

            //string[] steps = { "PWH_0006", "PWH_0007", "PWH_0008", "PWH_0009" };
            //return _mapper.Map<IEnumerable<MaterialTicketDTO>>(tickets.Where(w=> steps.Contains(w.WorkflowStep)));
        }

        public async Task<IEnumerable<MaterialDetailsDTO>> GetPartNumberByTicketIdAsync(MaterialDetailsDTO materialDto)
        {
            //var material = _mapper.Map<MaterialDetails>(materialDto);
            //// Allocated lots are now persisted locally in Txn.TicketMaterialLots
            //// during AllocateLot, so read from the DB by ticket instead of
            //// round-tripping to Camstar.
            //var materialLots = await _ticketRepository.GetTicketMaterialLotsByTicketAsync(material.TicketNumber);

            string wfs = "PWH_0007,PWH_0008,PWH_0009";
            var workflowSteps = string.IsNullOrWhiteSpace(wfs)
                 ? new[] { string.Empty }
                 : wfs
                     .Split(',')
                     .Select(s => s.Trim())
                     .Where(s => !string.IsNullOrEmpty(s))
                     .ToArray();

            var stepTasks = workflowSteps.Select(step =>
            {
                // Clone the DTO per step so each request has its own material/filterXml instance.
                var perStepDto = _mapper.Map<MaterialDetailsDTO>(materialDto);
                perStepDto.WorkflowStep = step;

                var perStepMaterial = _mapper.Map<MaterialDetails>(perStepDto);
                var perStepFilterXml = _xmlConverterService.MaterialFilterXml(perStepMaterial);

                return _camstarTransactionRepository.GetMaterialLotsByFilterAsync(perStepMaterial, perStepFilterXml);
            }).ToList();

            var materials = (await Task.WhenAll(stepTasks)).SelectMany(r => r).ToList();
            var matList = materials.Where(w=>w.TicketNumber == materialDto.TicketNumber).ToList();


            #region temp. solution tofetch all lot details


            // Throttle the per-lot attribute SOAP calls so we don't fan out to hundreds
            // of concurrent requests against the external service.
            var gate = new SemaphoreSlim(10);

            var newMaterialDetails = await Task.WhenAll(matList.Select(async mat =>
            {
                await gate.WaitAsync();
                try
                {
                    var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mat.LotId);
                    var merged = MaterialMergeHelper.Merge(mat, materialAttribute);

                    // Camstar returns the literal string "No material part number"
                    // for all NPxxx lots, which is useless on the Issuance UI.
                    // Resolve the actual description the operator picked at
                    // receiving time using the LotId -> Txn.MaterialNoPartNumberLots
                    // -> Ref.MaterialNoPartNumbers chain.
                    if (DummyPartNumber.IsDummy(merged.PartNumber))
                    {
                        // Lot-split during requesting creates new Camstar LotIds
                        // that are not present in Txn.MaterialNoPartNumberLots
                        // (only the original received lot was recorded there).
                        // Fall back to ParentLotId so the split children still
                        // resolve to the originally-picked description.
                        var npDescription = !string.IsNullOrWhiteSpace(merged.LotId)
                            ? await _materialRepository.GetDescriptionByLotIdAsync(merged.LotId)
                            : null;

                        if (string.IsNullOrWhiteSpace(npDescription) && !string.IsNullOrWhiteSpace(merged.ParentLotId))
                        {
                            npDescription = await _materialRepository.GetDescriptionByLotIdAsync(merged.ParentLotId);
                        }

                        if (!string.IsNullOrWhiteSpace(npDescription))
                        {
                            merged.Description = npDescription;
                        }
                    }

                    return merged;
                }
                finally
                {
                    gate.Release();
                }
            }));




            //List<MaterialDetails> newMaterialDetails = new List<MaterialDetails>();
            //var tasks = matList.Select(async mats =>
            //{
            //    var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mats.LotId);
            //    var materialDetails = await _materialRepository.GetMaterialDetailsByPartNumberAsync(mats.PartNumber);
            //    lock (newMaterialDetails)
            //    {
            //        MaterialDetailsDTO md = new MaterialDetailsDTO();

            //        md.LotId = mats.LotId ?? materialAttribute.LotId;
            //        md.PartNumber = mats.PartNumber ?? materialAttribute.PartNumber;
            //        md.WorkflowStep = mats.WorkflowStep ?? materialAttribute.WorkflowStep;
            //        md.OwnerName = mats.OwnerName ?? materialAttribute.OwnerName;
            //        md.Quantity = mats.Quantity != 0 ? mats.Quantity : materialAttribute.Quantity;
            //        md.LotNumber = mats.LotNumber ?? materialAttribute.LotNumber;
            //        md.PoNumber = mats.PoNumber ?? materialAttribute.PoNumber;
            //        md.PoLineNumber = mats.PoLineNumber ?? materialAttribute.PoLineNumber;
            //        md.PoLineNumber = mats.PoLineNumber ?? materialAttribute.PoLineNumber;
            //        md.InvoiceNumber = mats.InvoiceNumber ?? materialAttribute.InvoiceNumber;
            //        md.WaybillNumber = mats.WaybillNumber ?? materialAttribute.WaybillNumber;
            //        md.DrNumber = mats.DrNumber ?? materialAttribute.DrNumber;
            //        md.ReceivingLocation = mats.ReceivingLocation ?? materialAttribute.ReceivingLocation;
            //        md.WmsKeyNumber = mats.WmsKeyNumber ?? materialAttribute.WmsKeyNumber;
            //        md.PalletId = mats.PalletId ?? materialAttribute.PalletId;
            //        md.ExpirationDate = mats.ExpirationDate ?? materialAttribute.ExpirationDate;
            //        md.Category = mats.Category ?? materialAttribute.Category;
            //        md.FactoryName = mats.FactoryName ?? materialAttribute.FactoryName;
            //        md.Vendor = mats.Vendor ?? materialAttribute.Vendor;
            //        md.Remarks = mats.Remarks ?? materialAttribute.Remarks;
            //        md.ParentLotId = mats.ParentLotId ?? materialAttribute.ParentLotId;
            //        md.OtherRemarks = mats.OtherRemarks ?? materialAttribute.OtherRemarks;
            //        md.Uom = materialAttribute.Uom ?? mats.Uom;
            //        md.Vendor = materialAttribute.Vendor ?? mats.Vendor;
            //        md.ReceivedBy = mats.ReceivedBy ?? materialAttribute.ReceivedBy;
            //        md.DateReceive = mats.DateReceive ?? materialAttribute.DateReceive;
            //        md.OwnerEmail = mats.OwnerEmail ?? materialAttribute.OwnerEmail;
            //        md.ReceiverName = mats.ReceiverName ?? materialAttribute.ReceiverName;
            //        md.DefectCode = mats.DefectCode ?? materialAttribute.DefectCode;
            //        md.DeliveryType = mats.DeliveryType ?? materialAttribute.DeliveryType;
            //        md.PreviousOperation = mats.PreviousOperation ?? materialAttribute.PreviousOperation;
            //        md.BoxId = mats.BoxId != 0 ? mats.BoxId : materialAttribute.BoxId;
            //        md.RequestedQuantity = mats.RequestedQuantity != 0 ? mats.RequestedQuantity : materialAttribute.RequestedQuantity;
            //        md.DateRequest = mats.DateRequest ?? materialAttribute.DateRequest;
            //        md.IssuanceStatus = mats.IssuanceStatus ?? materialAttribute.IssuanceStatus;
            //        md.Description = materialDetails.MaterialName;
            //        md.TicketNumber = mats.TicketNumber ?? materialAttribute.TicketNumber;
            //        md.actionHistory = mats.actionHistory ?? materialAttribute.actionHistory;

            //        newMaterialDetails.Add(_mapper.Map<MaterialDetails>(md));
            //    }


            //});

            //await Task.WhenAll(tasks);
            #endregion

            return _mapper.Map<IEnumerable<MaterialDetailsDTO>>(newMaterialDetails);
        }

        public async Task<IEnumerable<MaterialDetailsDTO>> PickMaterialsByTicketNumberAsync(List<MaterialDetailsDTO> materialDtoList,string filePath)
        {
            foreach (var materialDto in materialDtoList)
            {
                var material = _mapper.Map<MaterialDetails>(materialDto);
                int? pickedQuantity = 0;

                //continue looping until requested quantity is achieved.
                while (pickedQuantity < materialDto.RequestedQuantity)
                {
                    //var filterXml = _xmlConverterService.MaterialFilterByTicketXml(materialDto);
                    var filterXml = new XDocument();
                    var pickedMaterials = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(material, filterXml);

                    // Throttle the per-lot attribute SOAP calls so we don't fan out to
                    // hundreds of concurrent requests against the external service.
                    var pickedGate = new SemaphoreSlim(10);
                    var pickedDtos = await Task.WhenAll(pickedMaterials.Select(async mats =>
                    {
                        await pickedGate.WaitAsync();
                        try
                        {
                            var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mats.LotId);
                            return MaterialMergeHelper.Merge(mats, materialAttribute);
                        }
                        finally
                        {
                            pickedGate.Release();
                        }
                    }));
                    List<MaterialDetails> newPickedMaterialDetails = pickedDtos.Select(d => _mapper.Map<MaterialDetails>(d)).ToList();


                    if (materialDto.SupplierLotNum != null)
                    {
                        #region Issuance has specific lot number to issue.
                        
                        //Must follow the box number sequence
                        var picked = newPickedMaterialDetails.Where(w=>w.LotNumber == materialDto.SupplierLotNum).OrderBy(o => o.BoxId).FirstOrDefault();
                        picked.WorkflowStep = "PWH_0007";
                        picked.DateRequest = materialDto.DateRequest;
                        picked.RequestedQuantity = materialDto.RequestedQuantity;
                        picked.DateApproval = materialDto.DateApproval;
                        picked.TicketStatus = materialDto.TicketStatus;
                        picked.PlannerID = materialDto.PlannerID;
                        picked.PlannerName = materialDto.PlannerName;
                        picked.RequestorID = materialDto.RequestorID;
                        picked.RequestorName = materialDto.RequestorName;
                        picked.ReqNotes = materialDto.ReqNotes;
                        picked.TicketNumber = materialDto.TicketNumber;
                        picked.ApproverNotes = materialDto.ApproverNotes;
                        picked.actionHistory = materialDto.actionHistory;
                        picked.IssuanceStatus = "material assigned automatically (requested lot number)";
                        var pickMaterialDto = _mapper.Map<MaterialDetailsDTO>(picked);
                        var moveXml = _xmlConverterService.MaterialInventoryMoveXml(picked);
                        ///move lot as "Lot asssign ticket(PWH_0007)"
                        var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(picked, moveXml);

                        if (move)
                        {
                            var setTicketXml = _xmlConverterService.AssigTicketToLotXml(picked);
                            await _camstarTransactionRepository.SetMaterialLotAttributeAsync(picked, picked.WorkflowStep, setTicketXml);
                            pickedQuantity = picked.Quantity;


                        }

                        #endregion
                    }
                    else
                    {
                        //Get attributes
                        var partNumberAttributes = _camstarTransactionRepository.ReadCsv(filePath).Where(w => w.MaterialPartNumber == materialDto.PartNumber).FirstOrDefault();
                        //
                        #region Follow FEFO if material has expiration
                        if (partNumberAttributes.WithExpiration)
                        {
                           
                            var picked = newPickedMaterialDetails.OrderBy(m => m.ExpirationDate).ThenBy(m => m.BoxId).FirstOrDefault();
                            picked.WorkflowStep = "PWH_0007";
                            picked.DateRequest = materialDto.DateRequest;
                            picked.RequestedQuantity = materialDto.RequestedQuantity;
                            picked.DateApproval = materialDto.DateApproval;
                            picked.TicketStatus = materialDto.TicketStatus;
                            picked.PlannerID = materialDto.PlannerID;
                            picked.PlannerName = materialDto.PlannerName;
                            picked.RequestorID = materialDto.RequestorID;
                            picked.RequestorName = materialDto.RequestorName;
                            picked.ReqNotes = materialDto.ReqNotes;
                            picked.TicketNumber = materialDto.TicketNumber;
                            picked.ApproverNotes = materialDto.ApproverNotes;
                            picked.actionHistory = materialDto.actionHistory;
                            picked.IssuanceStatus = "material assigned automatically(FEFO)";
                            var pickMaterialDto = _mapper.Map<MaterialDetailsDTO>(picked);
                            var moveXml = _xmlConverterService.MaterialInventoryMoveXml(picked);
                            ///move lot as "Lot asssign ticket(PWH_0007)"
                            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(picked, moveXml);

                            if (move)
                            {
                                var setTicketXml = _xmlConverterService.AssigTicketToLotXml(picked);
                                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(picked, picked.WorkflowStep, setTicketXml);
                                pickedQuantity = picked.Quantity;


                            }

                        }
                        #endregion

                        #region Follow FIFO for normal issuance
                        else
                        {
                            var picked = newPickedMaterialDetails.OrderBy(m => m.DateReceive).ThenBy(m => m.BoxId).FirstOrDefault();
                            picked.WorkflowStep = "PWH_0007";
                            picked.DateRequest = materialDto.DateRequest;
                            picked.RequestedQuantity = materialDto.RequestedQuantity;
                            picked.DateApproval = materialDto.DateApproval;
                            picked.TicketStatus = materialDto.TicketStatus ?? "Approved";
                            picked.PlannerID = materialDto.PlannerID;
                            picked.PlannerName = materialDto.PlannerName;
                            picked.RequestorID = materialDto.RequestorID;
                            picked.RequestorName = materialDto.RequestorName;
                            picked.ReqNotes = materialDto.ReqNotes;
                            picked.TicketNumber = materialDto.TicketNumber;
                            picked.ApproverNotes = materialDto.ApproverNotes;
                            picked.actionHistory = materialDto.actionHistory;
                            picked.IssuanceStatus = "material assigned automatically(FIFO)";
                            if ((pickedQuantity + picked.Quantity) > materialDto.RequestedQuantity)
                            {
                                picked.IssuanceStatus = "Split";
                            }

                            var pickMaterialDto = _mapper.Map<MaterialDetailsDTO>(picked);
                            var moveXml = _xmlConverterService.MaterialInventoryMoveXml(picked);
                            ///move lot as "Lot asssign ticket(PWH_0007)"
                            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(picked, moveXml);

                            if (move)
                            {
                                var setTicketXml = _xmlConverterService.AssigTicketToLotXml(picked);
                                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(picked, picked.WorkflowStep, setTicketXml);
                                pickedQuantity += picked.Quantity;
                            }
                        }
                        #endregion
                    }
                }   
            }


            #region Removal of assigned ticket(E-PULL) from lots under PWH_0006(storage) after picking the right materials base on FEFO/FIFO
         
            MaterialDetailsDTO dto = new MaterialDetailsDTO();
            
            dto.WorkflowStep = "PWH_0006";
            dto.TicketNumber = materialDtoList.Select(s => s.TicketNumber).FirstOrDefault();
            var mat = _mapper.Map<MaterialDetails>(dto);
            var ePullAssignedMaterialsXml = _xmlConverterService.MaterialFilterByTicketAssignedByEPullXml(mat);

            
            var epullAssignedMaterials = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(mat, ePullAssignedMaterialsXml);

            foreach (var item in epullAssignedMaterials)
            {
                var emptyTicketDTO = new MaterialDetailsDTO();
                emptyTicketDTO.LotId = item.LotId;
                emptyTicketDTO.WorkflowStep = "PWH_0006";


                var emptyTicket = _mapper.Map<MaterialDetails>(emptyTicketDTO);
                var removeTicketXml = _xmlConverterService.AssigTicketToLotXml(emptyTicket);
                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(emptyTicket, emptyTicketDTO.WorkflowStep, removeTicketXml);
            }
            #endregion


            

            ///this one is for removal
            ///return the lots assigned for this ticket.
            MaterialDetailsDTO LotAssignedDto = new MaterialDetailsDTO();
            LotAssignedDto.WorkflowStep = "PWH_0007";
            LotAssignedDto.TicketNumber = materialDtoList.Select(s => s.TicketNumber).FirstOrDefault();
            var LotAssigned = _mapper.Map<MaterialDetails>(LotAssignedDto);
            var filterPickedMaterialsXml = _xmlConverterService.MaterialFilterByTicketXml(LotAssigned);

            
            var materials = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(LotAssigned, filterPickedMaterialsXml);


            #region temp. solution tofetch all lot details
            // Throttle the per-lot attribute SOAP calls so we don't fan out to
            // hundreds of concurrent requests against the external service.
            var lotAssignedGate = new SemaphoreSlim(10);
            var lotAssignedDtos = await Task.WhenAll(materials.Select(async mats =>
            {
                await lotAssignedGate.WaitAsync();
                try
                {
                    var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mats.LotId);
                    return MaterialMergeHelper.Merge(mats, materialAttribute);
                }
                finally
                {
                    lotAssignedGate.Release();
                }
            }));
            var newLotAssignedMaterialDetails = lotAssignedDtos.Select(d => _mapper.Map<MaterialDetails>(d)).ToList();
            #endregion
            return _mapper.Map<IEnumerable<MaterialDetailsDTO>>(newLotAssignedMaterialDetails);
        }
        public async Task<IEnumerable<MaterialDetailsDTO>> GetLotAssignedPerTicket(MaterialDetailsDTO materialDto)
        {
            string[] steps = {"PWH_0007","PWH_0008","PWH_0009" };
            List<MaterialDetails> newLotAssignedMaterialDetails = new List<MaterialDetails>();

            foreach (string step in steps)
            {
                materialDto.WorkflowStep = step;
                var material = _mapper.Map<MaterialDetails>(materialDto);
                var filterPickedMaterialsXml = _xmlConverterService.MaterialFilterByTicketXml(material);

                
                var lotAssigned = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(material, filterPickedMaterialsXml);
                #region temp. solution tofetch all lot details

                // Throttle the per-lot attribute SOAP calls so we don't fan out to
                // hundreds of concurrent requests against the external service.
                var gate = new SemaphoreSlim(10);
                var stepDtos = await Task.WhenAll(lotAssigned.Select(async mats =>
                {
                    await gate.WaitAsync();
                    try
                    {
                        var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mats.LotId);
                        return MaterialMergeHelper.Merge(mats, materialAttribute);
                    }
                    finally
                    {
                        gate.Release();
                    }
                }));
                newLotAssignedMaterialDetails.AddRange(stepDtos.Select(d => _mapper.Map<MaterialDetails>(d)));
                #endregion
            }


            return _mapper.Map<IEnumerable<MaterialDetailsDTO>>(newLotAssignedMaterialDetails);
        }

        public async Task<(bool result, bool printResult,string message)> PickedMaterialsAsync(MaterialDetailsDTO materialDto,string filePath)
        {
           
            var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(materialDto.LotId);
                
            MaterialDetails newMaterialDetails = new MaterialDetails();

            newMaterialDetails.LotId = materialDto.LotId ?? materialAttribute.LotId;
            newMaterialDetails.PartNumber = materialDto.PartNumber ?? materialAttribute.PartNumber;
            newMaterialDetails.WorkflowStep = "PWH_0008";
            newMaterialDetails.OwnerName = materialDto.OwnerName ?? materialAttribute.OwnerName;
            newMaterialDetails.Quantity = materialDto.Quantity != 0 ? materialDto.Quantity : materialAttribute.Quantity;
            newMaterialDetails.LotNumber = materialDto.LotNumber ?? materialAttribute.LotNumber;
            newMaterialDetails.PoNumber = materialDto.PoNumber ?? materialAttribute.PoNumber;
            newMaterialDetails.PoLineNumber = materialDto.PoLineNumber ?? materialAttribute.PoLineNumber;
            newMaterialDetails.PoLineNumber = materialDto.PoLineNumber ?? materialAttribute.PoLineNumber;
            newMaterialDetails.InvoiceNumber = materialDto.InvoiceNumber ?? materialAttribute.InvoiceNumber;
            newMaterialDetails.WaybillNumber = materialDto.WaybillNumber ?? materialAttribute.WaybillNumber;
            newMaterialDetails.DrNumber = materialDto.DrNumber ?? materialAttribute.DrNumber;
            newMaterialDetails.ReceivingLocation = materialDto.ReceivingLocation ?? materialAttribute.ReceivingLocation;
            newMaterialDetails.WmsKeyNumber = materialDto.WmsKeyNumber ?? materialAttribute.WmsKeyNumber;
            newMaterialDetails.PalletId = materialDto.PalletId ?? materialAttribute.PalletId;
            newMaterialDetails.ExpirationDate = materialDto.ExpirationDate ?? materialAttribute.ExpirationDate;
            newMaterialDetails.Category = materialDto.Category ?? materialAttribute.Category;
            newMaterialDetails.FactoryName = materialDto.FactoryName ?? materialAttribute.FactoryName;
            newMaterialDetails.Vendor = materialDto.Vendor ?? materialAttribute.Vendor;
            newMaterialDetails.Remarks = materialDto.Remarks ?? materialAttribute.Remarks;
            newMaterialDetails.ParentLotId = materialDto.ParentLotId ?? materialAttribute.ParentLotId;
            newMaterialDetails.OtherRemarks = materialDto.OtherRemarks ?? materialAttribute.OtherRemarks;
            newMaterialDetails.Uom = materialAttribute.Uom ?? materialDto.Uom;
            newMaterialDetails.Vendor = materialAttribute.Vendor ?? materialDto.Vendor;
            newMaterialDetails.ReceivedBy = materialDto.ReceivedBy ?? materialAttribute.ReceivedBy;
            newMaterialDetails.DateReceive = materialDto.DateReceive ?? materialAttribute.DateReceive;
            newMaterialDetails.OwnerEmail = materialDto.OwnerEmail ?? materialAttribute.OwnerEmail;
            newMaterialDetails.ReceiverName = materialDto.ReceiverName ?? materialAttribute.ReceiverName;
            newMaterialDetails.DefectCode = materialDto.DefectCode ?? materialAttribute.DefectCode;
            newMaterialDetails.DeliveryType = materialDto.DeliveryType ?? materialAttribute.DeliveryType;
            newMaterialDetails.PreviousOperation = materialDto.PreviousOperation ?? materialAttribute.PreviousOperation;
            newMaterialDetails.BoxId = materialDto.BoxId != 0 ? materialDto.BoxId : materialAttribute.BoxId;
            newMaterialDetails.RequestedQuantity = materialDto.RequestedQuantity != 0 ? materialDto.RequestedQuantity : materialAttribute.RequestedQuantity;
            newMaterialDetails.DateRequest = materialDto.DateRequest ?? materialAttribute.DateRequest;
            newMaterialDetails.IssuanceStatus = materialDto.IssuanceStatus;
            newMaterialDetails.RequestorID = materialDto.RequestorID;
            newMaterialDetails.TicketNumber = materialDto.TicketNumber;
            newMaterialDetails.PlannerID = materialDto.PlannerID;
            newMaterialDetails.ApproverNotes = materialDto.ApproverNotes ?? materialAttribute.ApproverNotes;
            newMaterialDetails.DateApproval = materialDto.DateApproval;
            newMaterialDetails.ReqNotes = materialDto.ReqNotes ?? materialAttribute.ReqNotes;
            newMaterialDetails.actionHistory = materialDto.actionHistory ?? materialAttribute.actionHistory;

            var partNumberAttribute = await _materialRepository.GetMaterialDetailsByPartNumberAsync(newMaterialDetails.PartNumber);
            newMaterialDetails.Description = partNumberAttribute.MaterialName;

            var printerConnection = await _labelPrintingRepository.IsPrinterConnectedAsync();

            if (newMaterialDetails.actionHistory != null)
            {
                if (newMaterialDetails.actionHistory.ToLower().Contains("split"))
                {
                    if (printerConnection)
                    {
                        string inboundLabel = OutboundBarcode90LabelZplCommand(newMaterialDetails, 0, "", false);
                        await _labelPrintingRepository.SendZplToNetworkPrinter(inboundLabel);
                    }
                }
            }
           
           

            var moveXml = _xmlConverterService.MaterialInventoryMoveXml(newMaterialDetails);
            ///move lot as "Picked(PWH_0008)"
            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(newMaterialDetails, moveXml);

            if (move)
            {
                var setTicketXml = _xmlConverterService.MaterialIssuanceStatusXml(newMaterialDetails);
                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(newMaterialDetails, newMaterialDetails.WorkflowStep, setTicketXml);

            }

            return (move, printerConnection,"success");
        



            
           
        }

        public async Task<int> IssueMaterialsAsync(List<MaterialDetailsDTO> materialForIssuance, string user, List<MaterialDetailsDTO> materialDtoList)
        {
            // Issuance routes lots to different post-issuance workflow steps
            // depending on which family of ticket allocated them:
            //   REQUEST -> PWH_0010 (normal issuance)
            //   BORROW  -> PWH_0011 (borrowed-out, awaiting return)
            //
            // We resolve REQUEST vs BORROW from Txn.TicketNumbers.TransactionType
            // (the canonical column) and cache the lookup per TicketNumber so
            // a batch issuance of N lots costs at most one DB hit per distinct
            // ticket. The BTN- prefix is kept only as a last-resort fallback
            // for the edge case where TicketNumber is present but the row
            // somehow lacks a TransactionType value (legacy / not-yet-backfilled
            // data).
            var transactionTypeByTicket = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            int count = 0;
            foreach (var materialDto in materialForIssuance)
            {
                var material = _mapper.Map<MaterialDetails>(materialDto);

                string txnType = await ResolveTransactionTypeAsync(materialDto.TicketNumber, transactionTypeByTicket);
                bool isBorrow = !string.IsNullOrEmpty(txnType)
                                && txnType.Equals("BORROW", StringComparison.OrdinalIgnoreCase);
                material.WorkflowStep = isBorrow ? "PWH_0011" : "PWH_0010";
                material.IssuanceStatus = $"Issued by {user}";
                var moveXml = _xmlConverterService.MaterialInventoryMoveXml(material);
                ///move lot as "Issued(PWH_0009)"
                var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveXml);

                if (move)
                {
                    var setIssuanceXml = _xmlConverterService.MaterialIssuanceStatusXml(material);
                    var setAttr = await _camstarTransactionRepository.SetMaterialLotAttributeAsync(material, material.WorkflowStep, setIssuanceXml);

                    if (setAttr)
                    { 
                        count++;
                    }

                }
            }
            #region Call shortage prediction worker
            // Shortage prediction only makes sense for REQUEST issuances:
            // a BORROW ticket means the materials are going out temporarily
            // and will come back, so feeding those lots into the predictor
            // would skew the on-hand projection. We skip the worker entirely
            // when every issued lot in this batch came from a BORROW ticket;
            // otherwise we feed only the REQUEST-side part numbers in.
            var requestPartNumbers = materialForIssuance
                .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                .Where(x =>
                {
                    var txn = transactionTypeByTicket.TryGetValue(x.TicketNumber ?? string.Empty, out var t) ? t : null;
                    return string.IsNullOrEmpty(txn)
                        || txn.Equals("REQUEST", StringComparison.OrdinalIgnoreCase);
                })
                .Select(x => x.PartNumber)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (count > 0 && requestPartNumbers.Count > 0)
            {
                var plantCode = "PHO"; // or derive dynamically

                var materialsPayload = requestPartNumbers
                    .Select(p => new { MaterialCode = p })
                    .ToList();

                var payload = new JObject
                {
                    ["Materials"] = JArray.FromObject(materialsPayload),
                    ["PlantCode"] = plantCode,
                    ["SourceSystem"] = "WMS-Issuance",
                    ["Timestamp"] = DateTime.UtcNow
                };

                var executionContext = new WorkerExecutionContext(
                    "SHORTAGE_PREDICTION",
                    payload,
                    null,
                    Guid.NewGuid().ToString()
                );

                //await _digitalWorkerExecutorService.ExecuteAsync("SHORTAGE_PREDICTION", executionContext);

                _ = Task.Run(() => _digitalWorkerExecutorService.ExecuteAsync("SHORTAGE_PREDICTION", executionContext));
            }

            #endregion

            #region Update ticket status to "Issuance"
            // Three-way state machine driven by the issuance counts:
            //   all lines issued      -> ticket is fully "closed"
            //   some lines issued     -> ticket is "partial" so the warehouse
            //                            grid keeps surfacing it until the
            //                            remaining lines are handled
            //   no lines issued yet   -> leave the status untouched
            //
            // Important: the previous implementation inspected the incoming
            // materialDtoList items' IssuanceStatus, but those DTOs are the
            // ones the controller bound from the client - they are NOT updated
            // inside the loop above (we mutate a freshly-mapped MaterialDetails,
            // not the source DTO). For lines being issued for the first time
            // their IssuanceStatus stayed null, allIssued/anyIssued were both
            // false, and the ticket status update never ran.
            //
            // We now derive the state from authoritative counts:
            //   previouslyIssued = lines the client already saw as "Issued"
            //   newlyIssued      = count returned by the Camstar loop above
            int totalLines = materialDtoList?.Count ?? 0;
            int previouslyIssued = materialDtoList == null ? 0 : materialDtoList.Count(x =>
                !string.IsNullOrEmpty(x.IssuanceStatus) &&
                x.IssuanceStatus.IndexOf("Issued", StringComparison.OrdinalIgnoreCase) >= 0);
            int totalIssued = previouslyIssued + count;

            bool allIssued = totalLines > 0 && totalIssued >= totalLines;
            bool anyIssued = totalIssued > 0;

            string ticketNumber = materialDtoList.Select(s => s.TicketNumber).FirstOrDefault();

            // Terminal status diverges by ticket family:
            //   REQUEST -> 'closed'    (existing behavior)
            //   BORROW  -> 'borrowed'  (lots are out of WH but the workflow
            //                           isn't complete until the borrower
            //                           returns the materials or converts
            //                           the borrow into a real request)
            // 'partial' is shared by both flows.
            string ticketTxnType = await ResolveTransactionTypeAsync(ticketNumber, transactionTypeByTicket);
            bool isBorrowTicket = !string.IsNullOrEmpty(ticketTxnType)
                                  && ticketTxnType.Equals("BORROW", StringComparison.OrdinalIgnoreCase);

            string newStatus = allIssued ? (isBorrowTicket ? "borrowed" : "closed")
                             : anyIssued ? "partial"
                             : null;

            if (newStatus != null)
            {
                var ticket = new MaterialTicket();
                ticket.TicketNumber = ticketNumber;
                ticket.TicketStatus = newStatus;

                // Await rather than fire-and-forget: the repository is scoped
                // to the request, so a Task.Run continuation can race the DI
                // container disposing its connection and silently swallow the
                // update. This is the second half of the original bug.
                await _ticketRepository.UpdateTicketStatusAsync(ticket);
            }

            #endregion



            return count;
        }

        // Resolves and caches the TransactionType for a TicketNumber so a
        // batch of N lots costs at most one DB hit per distinct ticket.
        // Falls back to the BTN- prefix only when the DB lookup yields
        // nothing (defensive against legacy / not-yet-backfilled rows).
        private async Task<string> ResolveTransactionTypeAsync(string ticketNumber, IDictionary<string, string> cache)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber)) return null;

            if (cache.TryGetValue(ticketNumber, out var cached))
            {
                return cached;
            }

            var txnType = await _ticketRepository.GetTransactionTypeAsync(ticketNumber);

            if (string.IsNullOrEmpty(txnType))
            {
                txnType = ticketNumber.StartsWith("BTN", StringComparison.OrdinalIgnoreCase)
                    ? "BORROW"
                    : "REQUEST";
            }

            cache[ticketNumber] = txnType;
            return txnType;
        }

        public async Task<int> CheckMaterialsAsync(List<MaterialDetailsDTO> materialDtoList, string user)
        {
            int count = 0;
            foreach (var materialDto in materialDtoList)
            {
                var material = _mapper.Map<MaterialDetails>(materialDto);
                material.WorkflowStep = "PWH_0009";
                material.IssuanceStatus = $"Checked by {user}";
                var moveXml = _xmlConverterService.MaterialInventoryMoveXml(material);
                
                var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveXml);

                if (move)
                {
                    var setIssuanceXml = _xmlConverterService.MaterialIssuanceStatusXml(material);
                    var setAttr = await _camstarTransactionRepository.SetMaterialLotAttributeAsync(material, material.WorkflowStep, setIssuanceXml);

                    if (setAttr)
                    {
                        count++;
                    }


                }
            }

            return count;
        }

        public async Task<bool> AcknowledgeRequestTicketAsync(string ticket)
        {
            var res = await _ticketRepository.AcknowledgeRequestTicketAsync(ticket);
            return res > 0;
        }

        public Task<bool> IsBorrowConvertedAsync(string ticketNumber)
        {
            return _ticketRepository.IsBorrowConvertedAsync(ticketNumber);
        }

        private string OutboundBarcode90LabelZplCommand(MaterialDetails material, int packagingQty, string packagingType, bool isLotSplit)
        {
            string expDate = material.ExpirationDate == null ? "N/A" : material.ExpirationDate.ToString().Substring(0, 10);
            string details = isLotSplit ? $"Lot splitted from Camstar Lot ID: {material.LotId}" : $"{packagingType.ToUpper()} {material.BoxId} of {packagingQty}";
            
            string zpl = $"^XA" +  //start of label
                        $"^PO N" +  //start of label
                        $"^PW1200" + // label width
                        $"^LL1050" + // label height
                        $"^CI28" + // Use UTF-8 compatible character set (prevents encoding shift)
                        $"^LH2,2" + // origin position

                        $"^FO1140,30" +
                        $"^A0R,35,35" +
                        $"^FH^FDWestern Digital®^FS" + // Western Digital logo

                        $"^FO1020,70" +
                        $"^BY3,2,65" +
                        $"^BCR,65,Y,N,N" +
                        $"^FD{material.LotId}^FS" +  //Camstar Lot ID in BarCode

                        //$"^FO950,30" +
                        //$"^GB5,960,2,B,0^FS" + // Vertical line

                        //$"^FO870,30" +
                        //$"^A0R,35,35" +
                        //$"^FDPart Number:^FS" + // Part Number Label

                        //$"^FO870,270" +
                        //$"^FB500,1,7,L" +
                        //$"^A0R,45,45" +
                        //$"^FD{material.PartNumber}^FS" + // Part Number

                        $"^FO820,30" +
                        $"^GB5,960,2,B,0^FS" + // Vertical line

                        $"^FO730,30" +
                        $"^FB180,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDRequest Ticket No.:^FS" +// Ticket Number label

                        $"^FO750,200" +
                        $"^FB500,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.TicketNumber}^FS" + // Ticket Number


                        $"^FO730,600" +
                        $"^FB150,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDPart Number:^FS" +// Part Number  new location label

                        $"^FO750,750" +
                        $"^FB500,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.PartNumber}^FS" + // Part Number  new location


                        $"^FO720,30" +
                        $"^GB5,960,2,B,0^FS" + // Vertical line

                        $"^FO630,30" +
                        $"^FB190,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDIssuance Date:^FS" +//issuance date label

                        $"^FO650,220" +
                        $"^FB550,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.DateApproval.Replace("T"," ").Substring(0,19)}^FS" +  //issuance date

                        $"^FO620,30" +
                        $"^GB5,960,2,B,0^FS" +  //Vertical Line

                        $"^FO530,30" +
                        $"^FB230,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDItem Description:^FS" + // Description Label

                        $"^FO510,250" +
                        $"^FB700,2,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.Description}^FS" +  // Description

                        $"^FO520,30" +
                        $"^GB5,960,2,B,0^FS" +  //Vertical Line

                        $"^FO450,30" +
                        $"^A0R,35,35" +
                        $"^FDLot Number:^FS" + // Lot Label

                        $"^FO450,250" +
                        $"^FB600,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.LotNumber}^FS" + // Lot Number

                        $"^FO420,30" +
                        $"^GB5,960,2,B,0^FS" +  //Vertical Line

                        $"^FO330,30" +
                        $"^FB180,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDExpiration Date:^FS" + // expiration Label

                        $"^FO350,200" +
                        $"^FB600,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{expDate}^FS" + // Expiration

                        $"^FO330,580" +
                        $"^FB180,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FDQty:^FS" + // quantity Label

                        $"^FO350,700" +
                        $"^FB600,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.Quantity}  {material.Uom}^FS" + // qty + uom

                        $"^FO320,30" +
                        $"^GB5,960,2,B,0^FS" +  //Vertical Line

                        $"^FO270,30" +
                        $"^FB400,1,7,L" +
                        $"^A0R,35,35" +
                        $"^FDRemarks:^FS" + // remarks label

                        $"^FO225,210" +
                        $"^FB850,2,7,L" +
                        $"^A0R,35,35" +
                        $"^FD{material.ReqNotes}^FS" +  // remarks

                        $"^FO220,30" +
                        $"^GB5,960,2,B,0^FS" +  //Vertical Line

                        $"^FO170,30" +
                        $"^FB400,1,7,L" +
                        $"^A0R,30,30" +
                        $"^FDRequestor:^FS" +

                        $"^FO150,200" +
                        $"^FB600,1,7,L" +
                        $"^A0R,45,45" +
                        $"^FD{material.RequestorID}^FS" + // Requestor Name

                        $"^XZ"//end of label
                        ;
            return zpl;

        }

        
    }
}

// IMaterialIssuanceService.cs
using M2OSS.DTO.WMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Service.WMS.Interface
{
    public interface IMaterialIssuanceService
    {
        Task<IEnumerable<MaterialTicketDTO>> GetAllApprovedMaterialTicketsAsync();
        Task<IEnumerable<MaterialDetailsDTO>> GetPartNumberByTicketIdAsync(MaterialDetailsDTO materialDto);

        Task<IEnumerable<MaterialDetailsDTO>> PickMaterialsByTicketNumberAsync(List<MaterialDetailsDTO> materialDtoList, string filePath);

        Task<IEnumerable<MaterialDetailsDTO>> GetLotAssignedPerTicket(MaterialDetailsDTO materialDto);
        Task<(bool result, bool printResult, string message)> PickedMaterialsAsync(MaterialDetailsDTO materialDto, string filePath);
        Task<int> IssueMaterialsAsync(List<MaterialDetailsDTO> materialForIssuance, string user, List<MaterialDetailsDTO> materialDtoList);
        Task<int> CheckMaterialsAsync(List<MaterialDetailsDTO> materialDtoList, string user);
        Task<bool> AcknowledgeRequestTicketAsync(string ticket);

        // True when the ticket originated from a borrow but has been
        // converted back to a regular request. The Issuance UI uses this
        // to suppress scan / check / acknowledge (materials are already
        // in the requestor's hands) and route Proceed Issuance directly
        // from PWH_0007 -> PWH_0010.
        Task<bool> IsBorrowConvertedAsync(string ticketNumber);
    }
}



