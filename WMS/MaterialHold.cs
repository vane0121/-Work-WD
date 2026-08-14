// MaterialHold.cshtml




@{
    ViewBag.Title = "Hold Materials";
}

<div class="card border-0 shadow-sm">
    <div class="card-header border-bottom px-4 py-3">
    </div>
    <div class="card-body px-4 py-3">

        <div class="table-responsive">
            <table id="MaterialHoldTable" class="table table-hover align-middle w-100">
                <thead class="text-dark bg-light">
                    <tr>
                        <th>Part Number</th>
                        <th>Lot Id</th>
                        <th>Lot Number</th>
                        <th>Workflow</th>
                        <th>Quantity</th>
                        <th>Hold Reason</th>
                        <th>Hold Comments</th>
                        <th>Hold Owner</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>

<div class="modal fade" id="PassedModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-md">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Inspection Passed Details</h5>
                <button type="button" id="close-passed" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <input type="hidden" class="form-control" id="txtPassedPrevOperation" />
                <input type="hidden" class="form-control" id="txtPassedCurOperation" />
                <div class="form-group">
                    <label for="txtPassedLotId">Lot ID:</label>
                    <input type="text" class="form-control" id="txtPassedLotId" readonly />
                </div>
                <div class="form-group">
                    <label for="txtPassedLotNumber">Lot Number:</label>
                    <input type="text" class="form-control" id="txtPassedLotNumber" readonly />
                </div>
                <div class="form-group">
                    <label for="txtPassedRemarks">Additional Comments/Remarks:</label>
                    <textarea class="form-control" placeholder="optional" id="txtPassedRemarks"></textarea>
                </div>
            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">
                @*<div class="custom-control custom-checkbox">
                    <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxPassedByLot">
                    <label for="cbxPassedByLot" class="custom-control-label">Pass all by Pallet ID?</label>
                </div>*@
                <div class="">
                    <button type="button" id="btn-proceed-passed" class="btn btn-success px-4">
                        Release
                    </button>
                    <button type="button" id="cancel-passed" class="btn btn-outline-secondary px-4">
                        Cancel
                    </button>
                </div>


            </div>
        </div>

    </div>
</div>

<div class="modal fade" id="HoldModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-md">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Hold Details</h5>
                <button type="button" id="close-hold" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <input type="hidden" class="form-control" id="txtHoldPrevOperation" />
                <div class="form-group">
                    <label for="txtHoldLotId">Lot ID:</label>
                    <input type="text" class="form-control" id="txtHoldLotId" readonly />
                </div>
                <div class="form-group">
                    <label for="txtHoldLotNumber">Lot Number:</label>
                    <input type="text" class="form-control" id="txtHoldLotNumber" readonly />
                </div>

                <div class="form-group">
                    <label for="txtHoldReason">Hold Reason:</label>
                    <textarea class="form-control" placeholder="..." id="txtHoldReason"></textarea>
                </div>
            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">
                @*<div class="custom-control custom-checkbox">
                    <input class="custom-control-input custom-control-input-primary" type="checkbox" id="cbxHoldByLot">
                    <label for="cbxHoldByLot" class="custom-control-label">Hold all by Lot Number?</label>
                </div>*@
                <div>
                    <button type="button" id="btn-proceed-hold" class="btn btn-warning px-4">
                        Hold
                    </button>
                    <button type="button" id="cancel-hold" class="btn btn-outline-secondary px-4">
                        Cancel
                    </button>
                </div>


            </div>
        </div>

    </div>
</div>

<div class="modal fade" id="ScrapModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-lg">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Scrap Details</h5>
                <button type="button" id="close-scrap" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <input type="hidden" class="form-control" id="txtScrapPrevOperation" />
                <div class="row">
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="txtScrapLotId">Lot ID:</label>
                            <input type="text" class="form-control" id="txtScrapLotId" readonly />
                            <input type="hidden" class="form-control" id="txtScrapLotStep" />
                        </div>
                    </div>
                    <div class="col-lg-6">

                        <div class="form-group">
                            <label for="txtScrapLotNumber">Lot Number:</label>
                            <input type="text" class="form-control" id="txtScrapLotNumber" readonly />
                        </div>

                    </div>
                </div>


                <div class="row">
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="cmbScrapDefectCode">Defect Code:</label>
                            <select class="form-control" id="cmbScrapDefectCode"></select>
                            <text id="txtScrapDefectDesc"></text>
                        </div>
                    </div>

                </div>


                <div class="form-group">
                    <label for="txtScrapComment">Comments:</label>
                    <textarea class="form-control" placeholder="..." id="txtScrapComment"></textarea>
                </div>
            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">
                <button type="button" id="btn-proceed-scrap" class="btn btn-danger px-4">
                    Scrap
                </button>
                <button type="button" id="cancel-scrap" class="btn btn-outline-secondary px-4">
                    Cancel
                </button>

            </div>
        </div>

    </div>
</div>

<div class="modal fade" id="RtvModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-lg">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">RTV Details</h5>
                <button type="button" id="close-rtv" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <input type="hidden" class="form-control" id="txtRtvPrevOperation" />
                <div class="row">
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="txtRtvLotId">Lot ID:</label>
                            <input type="text" class="form-control" id="txtRtvLotId" readonly />
                            <input type="hidden" class="form-control" id="txtRtvLotStep" />
                        </div>
                    </div>
                    <div class="col-lg-6">

                        <div class="form-group">
                            <label for="txtRtvLotNumber">Lot Number:</label>
                            <input type="text" class="form-control" id="txtRtvLotNumber" readonly />
                        </div>

                    </div>
                </div>

                <div class="form-group">
                    <label for="txtRtvReason">Comments:</label>
                    <textarea class="form-control" placeholder="..." id="txtRtvReason"></textarea>
                </div>
            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">
                <button type="button" id="btn-proceed-rtv" class="btn btn-info px-4">
                    RTV
                </button>
                <button type="button" id="cancel-rtv" class="btn btn-outline-secondary px-4">
                    Cancel
                </button>

            </div>
        </div>

    </div>
</div>
<div class="modal fade" id="LotSplitModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-md">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Lot Split</h5>
                <button type="button" id="close-lotsplit" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <div class="form-group">
                    <label for="txtSplitLotId">Lot ID:</label>
                    <input type="text" class="form-control" id="txtSplitLotId" readonly />
                    <input type="hidden" class="form-control" id="txtSplitPartNumber" />
                </div>
                <div class="row">
                    <div class="col-lg-5">
                        <div class="form-group">
                            <label for="txtSampleQty">Sample Qty:</label>
                            <input type="number" min="0" value="0" class="form-control" id="txtSampleQty" />
                            <input type="hidden" min="0" value="0" class="form-control" id="txtOriginalQty" />
                        </div>
                    </div>
                    <div class="col-lg-7">
                        <div class="form-group">
                            <label for="cmbSplitSampleUom">UOM:</label>
                            <select class="form-control select2" id="cmbSplitSampleUom">
                            </select>
                        </div>

                    </div>

                </div>



            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">
                <button type="button" id="btn-proceed-split" class="btn btn-info px-4">
                    Lot Split
                </button>
                <button type="button" id="cancel-lotsplit" class="btn btn-outline-secondary px-4">
                    Cancel
                </button>

            </div>
        </div>

    </div>
</div>



<div class="modal fade" id="ReceivingModal" aria-modal="true" role="dialog" data-backdrop="static" data-keyboard="false">
    <div class="modal-dialog modal-xl">
        <div class="modal-content border-0 shadow-sm">
            <div class="modal-header bg-dark border-bottom px-4 py-3">
                <h5 class="modal-title text-light font-weight-semibold">Receiving Details</h5>
                <button type="button" id="close-receiving" class="close text-dark" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>

            <div class="modal-body px-4 py-3">
                <input type="hidden" class="form-control" id="txtId" value="0" />
                <div class="form-group">
                    <label for="txtLotId">Camstar Lot ID:</label>
                    <input type="text" class="form-control" id="txtLotId" readonly />
                </div>
                <div class="form-group">
                    <label for="txtDescription">Description:</label>
                    <input type="text" class="form-control" id="txtDescription" readonly />
                </div>
                <div class="row">
                    <div class="col-lg-6">
                        <div class="form-group">
                            <label for="cmbWorkflow">Workflow:</label>
                            <select class="form-control select2" id="cmbWorkflow">
                               
                            </select>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="form-group d-none" id="div-workflowStep">
                            <label for="txtWorkFlowStep">Workflow Step:</label>
                            <input type="text" class="form-control" id="txtWorkFlowStep" readonly />
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
                            <label for="txtInvoiceNumber">Invoice Number:</label>
                            <input type="text" class="form-control" id="txtInvoiceNumber" />
                        </div>
                    </div>
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtWaybillNumber">Waybill Number:</label>
                            <input type="text" class="form-control" id="txtWaybillNumber" />
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtPartNumber">Part Number:</label>
                            <input type="text" class="form-control" id="txtPartNumber" />

                        </div>
                    </div>

                    <div class="col-lg-3">
                        <div class="form-group">
                            <label for="txtSupplierLotNumber">Lot Number:</label>
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
                            <select class="form-control select2" id="cmbUom">
                                
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
                                <option value="Vendor1">Vendor1</option>
                                <option value="Vendor2">Vendor2</option>
                                <option value="Vendor3">Vendor3</option>

                            </select>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="form-group">
                            <label for="txtOwner">Owner:</label>
                            <input type="text" class="form-control" id="txtOwner" />
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="form-group">
                            <label for="txtFactory">Factory:</label>
                            <input type="text" class="form-control" id="txtFactory" />
                        </div>
                    </div>

                </div>
                <div class="form-group">
                    <label for="txtRemarks">Additional Comments/Remarks:</label>
                    <textarea class="form-control" placeholder="optional" id="txtRemarks"></textarea>
                </div>

                <div class="" id="div-receivedDetails">
                    <label for="txtReceivedBy">Received by: <span id="txtReceivedBy"> 7351921</span></label>
                    <label for="txtReceivedDate" class="float-right">Received Date:<span id="txtReceivedDate"> 2025-10-07 08:45:33</span></label>
                </div>
            </div>
            <div class="modal-footer border-top bg-light px-4 py-3 d-flex justify-content-end">

                <button type="button" id="cancel-receiving" class="btn btn-outline-secondary px-4">
                    Close
                </button>

            </div>
        </div>

    </div>
</div>

@section Scripts {
    <script>
        const AppUrls = {
            getHoldMaterials: '@Url.Action("GetHoldMaterials", "MaterialHold", new { area = "WMS" })',
            getUom: '@Url.Action("GetUomList", "MaterialHold", new { area = "WMS" })',
            getCategory: '@Url.Action("GetCategoryList", "MaterialHold", new { area = "WMS" })',
            lotSplit: '@Url.Action("PerformLotSplit", "MaterialHold", new { area = "WMS" })',
            lotRelease: '@Url.Action("ReleasedMaterial", "MaterialHold", new { area = "WMS" })',
            lotRtv: '@Url.Action("RtvMaterial", "MaterialHold", new { area = "WMS" })',
            lotHold: '@Url.Action("HoldMaterial", "MaterialHold", new { area = "WMS" })',
            lotScrap: '@Url.Action("ScrapMaterial", "MaterialHold", new { area = "WMS" })',
            getHoldCategory: '@Url.Action("GetHoldReasonList", "MaterialHold", new { area = "WMS" })',
            getDefectCode: '@Url.Action("GetDefectCodeList", "MaterialHold", new { area = "WMS" })',

        };
        const currentUser = '@ViewBag.CurrentUser.EmployeeId';
    </script>

    <script src="~/Scripts/WMS/materialHold.js"></script>
  
}

// MaterialHoldController.cs
using M2OSS.DTO.WMS;
using M2OSS.Service.WMS.Interface;
using M2OSS.Web.Helper.XmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace M2OSS.Web.Controllers.WMS
{
    public class MaterialHoldController : BaseController
    {
        private readonly IHoldMaterialService _holdMaterialService;
        public MaterialHoldController(IHoldMaterialService holdMaterialService)
        {
            _holdMaterialService = holdMaterialService;
        }
        // GET: MaterialHold
        public ActionResult Index()
        {
            SetPageHeader("Hold Materials");
            return View("~/Views/WMS/MaterialHold/MaterialHold.cshtml");
        }

        public async Task<JsonResult> GetHoldMaterials(MaterialDetailsDTO materialDto)
        {
            
            var materialList = await _holdMaterialService.GetHoldMaterialLotsAsync(materialDto);
            return Json(materialList);
        }

        public JsonResult GetUomList()
        {
            var uoms = _holdMaterialService.GetUomList();
            return Json(uoms);
        }
        public JsonResult GetCategoryList()
        {
            var category = _holdMaterialService.GetCategoryList();
            return Json(category);
        }
        public async Task<JsonResult> PerformLotSplit(MaterialDetailsDTO sourceMaterialDto, MaterialDetailsDTO newMaterialDto)
        {
           
            var lotSplit = await _holdMaterialService.PerformSplitLotAsync(sourceMaterialDto, newMaterialDto);
            return Json(lotSplit);
        }

        public async Task<JsonResult> ReleasedMaterial(MaterialDetailsDTO materialDto)
        {
          
            var released = await _holdMaterialService.ReleasedLotAsync(materialDto);
            return Json(released);
        }

        public async Task<JsonResult> RtvMaterial(MaterialDetailsDTO materialDto)
        {
           
            var rtv = await _holdMaterialService.RtvLotAsync(materialDto);
            return Json(rtv);
        }
        public async Task<JsonResult> HoldMaterial(MaterialDetailsDTO materialDto)
        {
           
            var hold = await _holdMaterialService.HoldLotAsync(materialDto);
            return Json(hold);

        }
        public async Task<JsonResult> ScrapMaterial(MaterialDetailsDTO materialDto)
        {
            var hold = await _holdMaterialService.ScrapLotAsync(materialDto);
            return Json(hold);
        }

        public async Task<JsonResult> GetHoldReasonList()
        {
            var holdCategory = await _holdMaterialService.GetHoldCategoryList();
            return Json(holdCategory);
        }

        public async Task<JsonResult> GetDefectCodeList()
        {
            var defectCode = await _holdMaterialService.GetDefectCodeList();
            return Json(defectCode);
        }
    }
}

// MaterialHold.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Entities.WMS
{
    public class MaterialHold
    {
        public string LotId { get; set; }
        
    }
}

// MaterialHoldService.cs
using AutoMapper;
using M2OSS.DTO.WMS;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.Material.Interface;
using M2OSS.Service.Common;
using M2OSS.Service.WMS.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M2OSS.Service.WMS.Service
{
    public class MaterialHoldService: IHoldMaterialService
    {
        private readonly IMapper _mapper;
        private readonly ICamstarTransactionRepository _camstarTransactionRepository;
        private readonly IWebConfigurationService _webConfigurationRepository;
        private readonly IXmlConverterService _xmlConverterRepository;
        // Resolves NPxxx ("no part number") dummy lots back to the description
        // and UOM the operator picked at receiving time. Camstar carries only
        // the generic "No material part number" text and no meaningful UOM on
        // those rows.
        private readonly IPhoMaterialRepository _materialRepository;
        public MaterialHoldService(IMapper mapper, ICamstarTransactionRepository camstarTransactionRepository, IWebConfigurationService webConfigurationRepository, IXmlConverterService xmlConverterRepository, IPhoMaterialRepository materialRepository)
        {
            _mapper = mapper;
            _camstarTransactionRepository = camstarTransactionRepository;
            _webConfigurationRepository = webConfigurationRepository;
            _xmlConverterRepository = xmlConverterRepository;
            _materialRepository = materialRepository;
        }

        public async Task<IEnumerable<MaterialDetailsDTO>> GetHoldMaterialLotsAsync(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var filterXml = _xmlConverterRepository.MaterialFilterXml(material);
            
            var materials = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(material, filterXml);

            // Throttle the per-lot attribute SOAP calls so we don't fan out to
            // hundreds of concurrent requests against the external service.
            var gate = new SemaphoreSlim(10);
            var newMaterialDetails = await Task.WhenAll(materials.Select(async mat =>
            {
                await gate.WaitAsync();
                try
                {
                    var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mat.LotId);
                    var merged = MaterialMergeHelper.Merge(mat, materialAttribute);
                    await ResolveDummyDetailsAsync(merged);
                    return merged;
                }
                finally
                {
                    gate.Release();
                }
            }));

            return _mapper.Map<IEnumerable<MaterialDetailsDTO>>(newMaterialDetails);
        }

        // Camstar returns the literal "No material part number" text (and no
        // meaningful UOM) for every NPxxx lot, so resolve the real description
        // and UOM the operator picked at receiving time via the LotId ->
        // Txn.MaterialNoPartNumberLots -> Ref.MaterialNoPartNumbers chain.
        // Lot-splitting mints new Camstar LotIds that were never recorded
        // there, so fall back to ParentLotId for split children.
        private async Task ResolveDummyDetailsAsync(MaterialDetailsDTO merged)
        {
            if (merged == null || !DummyPartNumber.IsDummy(merged.PartNumber))
                return;

            var npDetails = !string.IsNullOrWhiteSpace(merged.LotId)
                ? await _materialRepository.GetNoPartNumberDetailsByLotIdAsync(merged.LotId)
                : null;

            if (npDetails == null && !string.IsNullOrWhiteSpace(merged.ParentLotId))
                npDetails = await _materialRepository.GetNoPartNumberDetailsByLotIdAsync(merged.ParentLotId);

            if (npDetails == null)
                return;

            if (!string.IsNullOrWhiteSpace(npDetails.MaterialName))
                merged.Description = npDetails.MaterialName;
            if (!string.IsNullOrWhiteSpace(npDetails.Uom))
                merged.Uom = npDetails.Uom;
        }

        public IEnumerable<string> GetUomList()
        {
            return _webConfigurationRepository.GetUomList();
        }
        public IEnumerable<string> GetCategoryList()
        {
            return _webConfigurationRepository.GetCategoryList();
        }

        public async Task<bool> PerformSplitLotAsync(MaterialDetailsDTO sourceMaterialDto, MaterialDetailsDTO newMaterialDto)
        {
            var sourceMaterial = _mapper.Map<MaterialDetails>(sourceMaterialDto);
            var newMaterial = _mapper.Map<MaterialDetails>(newMaterialDto);
            var splitLotXml = _xmlConverterRepository.SplitLotXml(sourceMaterial, newMaterial);
            var setParentXml = _xmlConverterRepository.SetParentLotAttributeXml(newMaterial);


           
            var lotSplit = await _camstarTransactionRepository.SplitLotAsync(sourceMaterial, newMaterial, splitLotXml);
            newMaterial.ParentLotId = sourceMaterial.LotId;

            if (lotSplit.result)
                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(newMaterial, "PWH_0003", setParentXml);

            return lotSplit.result;
        }


        public async Task<bool> ReleasedLotAsync(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var moveLotXml = _xmlConverterRepository.MaterialInventoryMoveToPreviousXml(material);
            var setDispositionXml = _xmlConverterRepository.SetDispositionAttributesXml(material);

            
            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveLotXml);


            //update attribute to move from previous operation
            if (move)
                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(material, material.WorkflowStep, setDispositionXml);


            return move;
        }

        public async Task<bool> RtvLotAsync(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var moveLotXml = _xmlConverterRepository.MaterialInventoryMoveXml(material);
            var setDispositionXml = _xmlConverterRepository.SetDispositionAttributesXml(material);

            
            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveLotXml);

            if (move)
                await _camstarTransactionRepository.SetMaterialLotAttributeAsync(material, material.WorkflowStep, setDispositionXml);


            return move;
        }

        public async Task<bool> HoldLotAsync(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var moveLotXml = _xmlConverterRepository.MaterialInventoryMoveXml(material);
           

           
            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveLotXml);

            //if (move)
            //await _camstarTransactionService.SetMaterialLotAttributeAsync(material, material.WorkflowStep);


            return move;
        }
        public async Task<bool> ScrapLotAsync(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var moveLotXml = _xmlConverterRepository.MaterialInventoryMoveXml(material);
            
            var move = await _camstarTransactionRepository.MaterialInventoryMoveAsync(material, moveLotXml);

            //if (move)
            //await _camstarTransactionService.SetMaterialLotAttributeAsync(material, material.WorkflowStep);


            return move;
        }
        public async Task<IEnumerable<HoldDTO>> GetHoldCategoryList()
        {
            var holdCategoryList = await _camstarTransactionRepository.GetHoldReasonAsync();
            return _mapper.Map<IEnumerable<HoldDTO>>(holdCategoryList);
        }

        public async Task<IEnumerable<DefectDTO>> GetDefectCodeList()
        {
            var defectCodeList = await _camstarTransactionRepository.GetDefectCodeAsync();
            return _mapper.Map<IEnumerable<DefectDTO>>(defectCodeList); ;
        }
    }
}

