// Index.cshtml
@{
    ViewBag.Title = "Borrow Materials";
}

<div class="card border-0 shadow-sm">
    <div class="card-header border-bottom px-4 py-3 d-flex align-items-center justify-content-between">
        <div>
            <h5 class="m-0">Borrowed Materials - For Return</h5>
            <small class="text-muted">Borrow tickets the user has flagged for return.</small>
        </div>
        <button type="button" id="btnRefreshReturn" class="btn btn-sm btn-outline-secondary">
            <i class="fas fa-sync-alt"></i>&nbsp;Refresh
        </button>
    </div>
    <div class="card-body px-4 py-3">

        <div class="table-responsive">
            <table id="tblBorrowReturn" class="table table-hover align-middle w-100">
                <thead class="text-dark bg-light">
                    <tr>
                        <th>#</th>
                        <th>Ticket Number</th>
                        <th>Sub Area</th>
                        <th>Requestor ID</th>
                        <th>Approver</th>
                        <th>Date Requested</th>
                        <th>Date Issued</th>
                        <th>Status</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>
    </div>
</div>


<!-- Acknowledge Return modal: shows the material lines under the selected
     borrow ticket and a Proceed button that, after a confirm dialog,
     moves the ticket from 'return to wh' to 'returned'. -->
<div class="modal fade" id="modal-acknowledgeReturn" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">
                    Acknowledge Return
                    &nbsp;<span id="ackTicketLabel" class="ticket-no text-muted"></span>
                </h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <p class="text-muted mb-2">
                    Review the borrowed materials before acknowledging the return.
                </p>
                <div class="table-responsive">
                    <table id="tblAckMaterials" class="table table-sm table-bordered align-middle w-100">
                        <thead class="text-dark bg-light">
                            <tr>
                                <th>#</th>
                                <th>Part Number</th>
                                <th>Material Name</th>
                                <th>Requested Qty</th>
                                <th>UoM</th>
                                <th>Lot Number</th>
                                <th>Remarks</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                <button type="button" id="btnAckProceed" class="btn btn-primary">Proceed</button>
            </div>
        </div>
    </div>
</div>


@section Scripts {
    <script>
        const AppUrls = {
            GetReturnToWhTickets: '@Url.Action("GetReturnToWhTickets", "BorrowMaterials", new { area = "WMS" })',
            GetTicketMaterials:   '@Url.Action("GetTicketMaterials",   "BorrowMaterials", new { area = "WMS" })',
            AcknowledgeReturn:    '@Url.Action("AcknowledgeReturn",    "BorrowMaterials", new { area = "WMS" })'
        };

        const currentUser     = '@ViewBag.CurrentUser?.EmployeeId';
        const currentUserName = '@ViewBag.CurrentUser?.DisplayName';
        const site            = '@ViewBag.CurrentUser?.Site';
    </script>

    <script src="~/Scripts/WMS/borrowMaterials.js"></script>
}

// BorrowMaterials.cs
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using M2OSS.Service.WMS.Interface;

namespace M2OSS.Web.Controllers.WMS
{
    // Warehouse-facing landing page for borrow tickets the user has flagged
    // 'return to wh'. Backed by a dedicated WMS service so the borrow-return
    // workflow stays out of the e-PULL user dashboard service.
    public class BorrowMaterialsController : BaseController
    {
        private readonly IBorrowMaterialsService _borrowMaterialsService;

        public BorrowMaterialsController(IBorrowMaterialsService borrowMaterialsService)
        {
            _borrowMaterialsService = borrowMaterialsService;
        }

        public ActionResult Index()
        {
            SetPageHeader("Borrow Materials");
            return View("~/Views/WMS/BorrowMaterials/Index.cshtml");
        }

        [HttpGet]
        public async Task<JsonResult> GetReturnToWhTickets()
        {
            try
            {
                var tickets = await _borrowMaterialsService.GetReturnToWhTicketsAsync();
                return Json(new { success = true, data = tickets }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Material lines under a borrow ticket - feeds the Acknowledge modal
        // on the WMS Borrow Materials page.
        [HttpGet]
        public async Task<JsonResult> GetTicketMaterials(string ticketNumber)
        {
            try
            {
                var materials = await _borrowMaterialsService.GetTicketMaterialsAsync(ticketNumber);
                return Json(new { success = true, data = materials }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // WH acknowledges receipt of the returned materials - moves the
        // lots back to PWH_0006 in Camstar and closes the ticket.
        [HttpPost]
        public async Task<JsonResult> AcknowledgeReturn(string ticketNumber)
        {
            try
            {
                // Same CSV the LabelPrintingController feeds to PrintLabelAsync;
                // used by the inbound label builder to resolve part description.
                string filePath = Server.MapPath("~/testData/PartNumberSampleData.csv");
                var affected = await _borrowMaterialsService.AcknowledgeReturnAsync(ticketNumber, filePath);
                return Json(new { success = affected > 0, message = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

// BorrowMaterialsService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.E_PULL.Interface;
using M2OSS.Service.WMS.Interface;

namespace M2OSS.Service.WMS.Service
{
    // Warehouse-facing implementation of the Borrow Materials page.
    // Kept distinct from EpullDashboardService so the WMS workflow
    // (list 'return to wh' tickets, acknowledge, move lots back to
    // stock) owns its own dependencies and lifecycle.
    public class BorrowMaterialsService : IBorrowMaterialsService
    {
        private readonly IPhoTicketRepository _phoTicketRepository;
        private readonly ICamstarTransactionRepository _camstarTransactionRepository;
        private readonly IXmlConverterService _xmlConverterService;
        private readonly M2OSS.Service.WMS.Interface.ILabelPrintingService _labelPrintingService;

        public BorrowMaterialsService(
            IPhoTicketRepository phoTicketRepository,
            ICamstarTransactionRepository camstarTransactionRepository,
            IXmlConverterService xmlConverterService,
            M2OSS.Service.WMS.Interface.ILabelPrintingService labelPrintingService)
        {
            _phoTicketRepository = phoTicketRepository;
            _camstarTransactionRepository = camstarTransactionRepository;
            _xmlConverterService = xmlConverterService;
            _labelPrintingService = labelPrintingService;
        }

        public async Task<IEnumerable<MaterialTicket>> GetReturnToWhTicketsAsync()
        {
            // WH queue: every borrow ticket the user has flagged for
            // return. No requestor scoping - WH operates across users.
            var tickets = await _phoTicketRepository.GetTicketsByStatusAsync("return to wh");
            return tickets
                .Where(IsBorrowTicket)
                .OrderByDescending(t => t.DateRequest)
                .ToList();
        }

        public async Task<IEnumerable<MaterialTicket>> GetTicketMaterialsAsync(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                return Enumerable.Empty<MaterialTicket>();

            return await _phoTicketRepository.GetMaterialsbyTicketNumberAsync(ticketNumber);
        }

        public async Task<int> AcknowledgeReturnAsync(string ticketNumber, string filePath)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                return 0;

            // 1) Pull the ticket's lots out of Camstar at PWH_0011
            //    (the "borrowed / pending return" step). The Camstar
            //    filter XML doesn't expose a TicketNumber attribute,
            //    so we widen the query to the step and narrow
            //    client-side - same pattern as
            //    MaterialIssuanceService.GetPartNumberByTicketIdAsync.
            var probe = new MaterialDetails
            {
                WorkflowStep = "PWH_0011",
                TicketNumber = ticketNumber
            };
            var filterXml = _xmlConverterService.MaterialFilterXml(probe);

            var lots = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(probe, filterXml);
            var ticketLots = lots
                .Where(l => string.Equals(l.TicketNumber, ticketNumber, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 2) Move each lot back to PWH_0006. Camstar moves are not
            //    transactional, so we tolerate a partial run and only
            //    close the DB ticket when every lot landed. After each
            //    successful move we reprint the inbound label for the
            //    lot so the WH has something to slap on the box when
            //    it goes back on the shelf. Printer failures are
            //    intentionally swallowed - a missing reprint should
            //    not block the DB close.
            bool allMoved = ticketLots.Count > 0;
            foreach (var lot in ticketLots)
            {
                lot.WorkflowStep = "PWH_0006";
                var moveXml = _xmlConverterService.MaterialInventoryMoveXml(lot);
                var moved = await _camstarTransactionRepository.MaterialInventoryMoveAsync(lot, moveXml);
                if (!moved)
                {
                    allMoved = false;
                    continue;
                }

                // Reprint inbound label. PrintLabelAsync internally
                // calls GetMaterialLotAttributeAsync(lotId) to pull
                // fresh attributes and routes through the same ZPL
                // builder used by goods receiving. We don't know the
                // original packaging context on a reprint, so we use
                // a single-box default - the box-of-N detail line
                // becomes "BOX 1 of 1".
                if (!string.IsNullOrWhiteSpace(lot.LotId) && !string.IsNullOrWhiteSpace(filePath))
                {
                    try
                    {
                        var boxId = lot.BoxId == 0 ? 1 : lot.BoxId;
                        await _labelPrintingService.PrintLabelAsync(
                            lot.LotId,
                            "INBOUND",
                            filePath,
                            "BOX",
                            boxId);
                    }
                    catch
                    {
                        // swallow printer / label errors - move succeeded
                    }
                }
            }

            if (!allMoved)
                return 0;

            // 3) Close the borrow ticket. Stays 'return to wh' on
            //    failure so the WH user can retry from the same row.
            var ticket = new MaterialTicket
            {
                TicketNumber = ticketNumber,
                TicketStatus = "closed"
            };
            return await _phoTicketRepository.UpdateTicketStatusAsync(ticket);
        }

        // Canonical "is this a borrow ticket?" check. Primary signal
        // is Txn.TicketNumbers.TransactionType = 'BORROW'. Fallback
        // to the BTN- prefix covers legacy rows created before
        // TransactionType was wired into the INSERT (column is
        // either null or carries the old 'REQUEST' DB default).
        private static bool IsBorrowTicket(MaterialTicket t)
        {
            if (t == null) return false;

            if (!string.IsNullOrEmpty(t.TransactionType) &&
                t.TransactionType.Equals("BORROW", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrEmpty(t.TicketNumber)
                && t.TicketNumber.StartsWith("BTN", StringComparison.OrdinalIgnoreCase);
        }
    }
}

// IBorrowMaterialsService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using M2OSS.Entities.WMS;

namespace M2OSS.Service.WMS.Interface
{
    // WMS-side service for the Borrow Materials page. Owns the
    // warehouse-facing portion of the borrow-return workflow:
    // listing tickets the user has flagged 'return to wh',
    // exposing the lines under each ticket for the acknowledge
    // modal, and processing the acknowledge action itself
    // (Camstar lot move + DB status close).
    //
    // Intentionally separate from IEpullDashboard so the WMS
    // concern doesn't leak into the user dashboard service.
    public interface IBorrowMaterialsService
    {
        Task<IEnumerable<MaterialTicket>> GetReturnToWhTicketsAsync();

        Task<IEnumerable<MaterialTicket>> GetTicketMaterialsAsync(string ticketNumber);

        // Moves each lot on the ticket from PWH_0011 back to PWH_0006
        // in Camstar, reprints an inbound label per lot so the WH can
        // re-shelve it, and, if every move succeeds, closes the ticket
        // (TicketStatus = 'closed'). filePath is the PartNumber CSV used
        // by the label builder to resolve the part description (same
        // file LabelPrintingController hands to PrintLabelAsync).
        // Returns the number of rows affected by the status update
        // (0 means nothing was closed, either because Camstar reported
        // a partial failure or the ticket no longer existed).
        Task<int> AcknowledgeReturnAsync(string ticketNumber, string filePath);
    }
}


