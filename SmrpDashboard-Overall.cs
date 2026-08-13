// Index.cshtml
@model M2OSS.DTO.S_MRP.SmrpDashboardDTO
@using System.Web.Mvc
@{
    ViewBag.Title = "Index";
}

<div class="container-fluid">
    <main role="main" class="col-12 px-4">
        <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
            <h1 class="h2">MOSS S-MRP Decision Cockpit</h1>
            <div class="btn-toolbar mb-2 mb-md-0">
                <div class="btn-group mr-2">
                    <button type="button"
                            class="btn btn-sm btn-outline-secondary"
                            id="openDigitalWorkersBtn">
                        <i class="fas fa-robot mr-1"></i>
                        Digital Workers
                    </button>
                </div>

                <div class="btn-group mr-2 position-relative">

                    <button type="button"
                            class="btn btn-sm btn-outline-secondary"
                            id="calendarBtn"
                            title="Select date">
                        <i class="fas fa-calendar-alt mr-1"></i>
                        Calendar
                    </button>

                    <input type="date"
                           id="dashboardDate"
                           style=" position: absolute; top: 0; left: 0; opacity: 0; width: 100%; height: 100%; z-index: -1; " />
                </div>

                <div class="btn-group mr-2">
                    <button type="button" class="btn btn-sm btn-outline-secondary" id="refreshBtn">
                        <i class="fas fa-sync-alt mr-1 refresh-icon"></i>
                        <span class="refresh-text">Refresh</span>
                    </button>
                </div>
            </div>
        </div>

        <div class="row mb-4">
            <div class="col-xl-2 col-md-4 mb-4">
                <div class="card border-left-danger shadow h-100 py-2">
                    <div class="card-body">
                        <div class="row no-gutters align-items-center">
                            <div class="col mr-2">
                                <div class="text-xs font-weight-bold text-danger text-uppercase mb-1">Live Shortage Risk</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">@Model.ShortageRisk.MaterialCount Materials</div>
                                <div class="mt-1">
                                    <span class="badge badge-danger">@Model.ShortageRisk.CriticalCount Critical</span>
                                    <span class="badge badge-warning">@Model.ShortageRisk.HighCount High</span>
                                </div>
                            </div>
                            <div class="col-auto">
                                <i class="fas fa-exclamation-triangle fa-2x text-gray-300"></i>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-xl-2 col-md-4 mb-4">
            </div>
            <div class="col-xl-2 col-md-4 mb-4">
            </div>
        </div>

        <div class="row">
            <div class="col-lg-6 mb-4">
                <div class="card shadow dashboard-card">
                    <div class="card-header py-3">
                        <div class="d-flex flex-wrap align-items-center justify-content-between">
                            <h6 class="m-0 font-weight-bold text-primary mr-3">
                                Shortage Risk Analysis
                            </h6>
                            <div class="d-flex flex-wrap align-items-center shortage-toolbar">
                                <div class="btn-group mr-2 mb-2 mb-md-0" id="shortageFilterBtns">
                                    <button class="btn btn-outline-secondary btn-sm active" data-filter="All">All</button>
                                    <button class="btn btn-outline-danger btn-sm" data-filter="Critical">Critical</button>
                                    <button class="btn btn-outline-warning btn-sm" data-filter="High">High</button>
                                    <button class="btn btn-outline-info btn-sm" data-filter="Medium">Medium</button>
                                </div>
                                <input type="text"
                                       id="shortageSearch"
                                       class="form-control form-control-sm shortage-search"
                                       placeholder="Search Material..." />

                            </div>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="table-responsive">
                            <table class="table table-sm" id="shortageRiskTable">
                                <thead>
                                    <tr>
                                        <th>No.</th>
                                        <th>Material</th>
                                        <th>Available Supply</th>
                                        <th>Lead Time</th>
                                        <th>Risk Level</th>
                                        <th>DOS</th>
                                        <th>Action</th>
                                        <th>Update Action</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @if (Model.ShortageRiskList != null && Model.ShortageRiskList.Any())
                                    {
                                        int rowNo = 1;
                                        foreach (var risk in Model.ShortageRiskList)
                                        {
                                            string textColor = risk.RiskLevel == "Critical" ? "text-danger" : (risk.RiskLevel == "High" ? "text-warning" : "text-info");
                                            string badgeColor = risk.RiskLevel == "Critical" ? "badge-danger" : (risk.RiskLevel == "High" ? "badge-warning" : "badge-info");
                                            string btnColor = risk.RiskLevel == "Critical" ? "btn-danger" : (risk.RiskLevel == "High" ? "btn-warning" : "btn-info");
                                            string btnLabel = risk.RiskLevel == "Critical" ? "Urgent PO" : (risk.RiskLevel == "High" ? "Expedite" : "Monitor");

                                            <tr data-risk="@risk.RiskLevel">
                                                <td>@rowNo</td>
                                                <td>@risk.Material</td>
                                                <td><span class="@textColor font-weight-bold">@risk.CurrentStock</span></td>
                                                <td>@risk.LeadTimeDays days</td>
                                                <td><span class="badge @badgeColor">@risk.RiskLevel</span></td>
                                                <td>
                                                    <span class="badge @(risk.DaysToShortage <= 5 ? "badge-danger" : (risk.DaysToShortage <= 15 ? "badge-warning" : "badge-info"))">
                                                        @risk.DaysToShortage
                                                    </span>
                                                </td>
                                                <td>@risk.Action</td>

                                                <td class="text-nowrap">

                                                    @if (risk.Action == "Create PR")
                                                    {
                                                        <button class="btn btn-outline-primary btn-sm create-pr-btn shortage-action-btn"
                                                                data-material="@risk.Material"
                                                                data-risk="@risk.RiskLevel"
                                                                data-stock="@risk.CurrentStock"
                                                                data-days="@risk.DaysToShortage">
                                                            Done PR
                                                        </button>
                                                    }
                                                    else if (risk.Action == "Create RFQ")
                                                    {
                                                        <button class="btn btn-outline-danger btn-sm create-rfq-btn shortage-action-btn"
                                                                data-material="@risk.Material"
                                                                data-risk="@risk.RiskLevel"
                                                                data-stock="@risk.CurrentStock"
                                                                data-days="@risk.DaysToShortage">
                                                            Done RFQ
                                                        </button>
                                                    }
                                                    else if (risk.Action == "Expedite delivery")
                                                    {
                                                        <span class="shortage-action-text">
                                                            Expedite
                                                        </span>
                                                    }
                                                    else if (risk.Action == "Review Planning Profile")
                                                    {
                                                        <span class="shortage-action-text">
                                                            Review Profile
                                                        </span>
                                                    }
                                                    else
                                                    {
                                                        <span class="shortage-action-text">No Action</span>
                                                    }
                                                </td>
                                            </tr>
                                            rowNo++;
                                        }
                                    }
                                    else
                                    {
                                        <tr id="noDataRow">
                                            <td colspan="9" class="text-center text-muted">No data available</td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>

                        <div id="shortageNoResults" class="text-center text-muted py-3" style="display:none;">
                            <i class="fas fa-check-circle fa-2x text-success mb-2"></i>
                            <p class="mb-0">No <span id="shortageNoResultsLabel"></span> items found.</p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-6 mb-4">
                <div class="card shadow dashboard-card">
                    <div class="card-header py-3 d-flex justify-content-between align-items-center">
                        <h6 class="m-0 font-weight-bold text-primary">Digital Worker Activity Log</h6>
                    </div>
                    <div class="card-body">
                        <div class="activity-timeline" id="activityTimeline">
                            @if (Model.RecentDigitalWorkerActions != null && Model.RecentDigitalWorkerActions.Any())
                            {
                                foreach (var action in Model.RecentDigitalWorkerActions.OrderByDescending(a => a.Timestamp))
                                {
                                    <div class="activity-item mb-3 pb-3 border-bottom">
                                        <div class="d-flex align-items-start">
                                            <div class="activity-icon mr-3">
                                                <i class="fas fa-@(GetActionIcon(action.ActionType)) text-@(GetActionColor(action.ActionType))"></i>
                                            </div>
                                            <div class="activity-content flex-grow-1">
                                                <div class="d-flex justify-content-between align-items-start">
                                                    <div>
                                                        <h6 class="mb-1 font-weight-bold">@action.Description</h6>
                                                        <small class="text-muted">Target: @action.Target</small>
                                                    </div>
                                                    <div class="text-right">
                                                        <span class="badge badge-@(GetStatusColor(action.Status))">@action.Status</span>
                                                        <br />
                                                        <small class="text-muted">@action.Timestamp.ToString("HH:mm:ss")</small>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                }
                            }
                            else
                            {
                                <div class="text-center text-muted py-4">
                                    <i class="fas fa-robot fa-3x mb-3"></i>
                                    <p>No recent digital worker activity</p>
                                </div>
                            }
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>

    <!-- PR Modal -->
    <div class="modal fade pr-modern-modal" id="prModal" tabindex="-1" role="dialog" aria-labelledby="prModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header border-0">
                    <div class="d-flex align-items-center">
                        <div class="pr-modal-icon mr-3">
                            <i class="fas fa-file-signature"></i>
                        </div>
                        <div>
                            <h5 class="modal-title font-weight-bold mb-1" id="prModalLabel">Create Purchase Requisition</h5>
                        </div>
                    </div>
                    <button type="button"
                            class="pr-modal-close"
                            data-dismiss="modal"
                            aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <form id="prForm">
                        <div class="pr-context-card mb-4">
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <div class="pr-section-title">Request Context</div>
                                <span class="pr-context-badge">
                                    <i class="fas fa-bolt mr-1"></i>Auto-filled
                                </span>
                            </div>

                            <div class="row">
                                <div class="col-md-6 mb-3 mb-md-0">
                                    <label class="pr-label">Material</label>
                                    <input type="text"
                                           class="form-control pr-input pr-input-readonly"
                                           id="prMaterial"
                                           readonly>
                                </div>

                                <div class="col-md-6">
                                    <label class="pr-label">Plant</label>
                                    <input type="text"
                                           class="form-control pr-input pr-input-readonly"
                                           id="prPlant"
                                           readonly>
                                </div>
                            </div>
                        </div>

                        <div class="pr-form-card">
                            <div class="pr-section-title mb-3">Requisition Details</div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label class="pr-label">Quantity</label>
                                        <div class="pr-input-wrap">
                                            <i class="fas fa-cubes pr-input-icon"></i>
                                            <input type="number"
                                                   class="form-control pr-input pr-input-with-icon"
                                                   id="prQuantity"
                                                   placeholder="Enter quantity">
                                        </div>
                                    </div>
                                </div>

                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label class="pr-label">Unit Cost</label>
                                        <div class="pr-input-wrap">
                                            <span class="pr-input-icon pr-currency-symbol">₱</span>
                                            <input type="number"
                                                   class="form-control pr-input pr-input-with-icon"
                                                   id="prUnitCost"
                                                   placeholder="Enter unit cost">
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="form-group mb-0">
                                <label class="pr-label">Reason</label>
                                <textarea class="form-control pr-input pr-textarea"
                                          rows="3"
                                          id="prReason"
                                          placeholder="Describe the reason for this requisition">Shortage detected by S‑MRP</textarea>
                            </div>
                        </div>
                    </form>
                </div>

                <div class="modal-footer border-0">
                    <button type="button" class="btn pr-btn pr-btn-light" data-dismiss="modal">
                        Cancel
                    </button>
                    <button type="button"
                            id="createPrBtn"
                            class="btn pr-btn pr-btn-primary"
                            onclick="submitPr()">
                        <i class="fas fa-paper-plane mr-2"></i>Create PR
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- PO Modal -->
    <div class="modal fade pr-modern-modal" id="poModal" tabindex="-1"
         role="dialog" aria-labelledby="poModalLabel" aria-hidden="true">

        <div class="modal-dialog modal-dialog-centered modal-lg" role="document">
            <div class="modal-content">

                <div class="modal-header border-0">
                    <div class="d-flex align-items-center">
                        <div class="pr-modal-icon mr-3" style="background:linear-gradient(135deg,#e74a3b,#be2617)">
                            <i class="fas fa-truck-loading"></i>
                        </div>
                        <div>
                            <h5 class="modal-title font-weight-bold mb-1" id="poModalLabel">
                                Create Purchase Order
                            </h5>
                            <small class="text-muted">
                                Critical shortage – immediate procurement action
                            </small>
                        </div>
                    </div>
                    <button type="button"
                            class="pr-modal-close"
                            data-dismiss="modal"
                            aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <form id="poForm">

                        <div class="pr-context-card mb-4">
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <div class="pr-section-title">Request Context</div>
                                <span class="badge badge-danger">
                                    Critical Only
                                </span>
                            </div>

                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <label class="pr-label">Material</label>
                                    <input type="text"
                                           class="form-control pr-input pr-input-readonly"
                                           id="poMaterial"
                                           readonly>
                                </div>

                                <div class="col-md-6">
                                    <label class="pr-label">Plant</label>
                                    <input type="text"
                                           class="form-control pr-input pr-input-readonly"
                                           id="poPlant"
                                           readonly>
                                </div>
                            </div>
                        </div>

                        <div class="pr-form-card">
                            <div class="pr-section-title mb-3">Purchase Order Details</div>

                            <div class="row">
                                <div class="col-md-6">
                                    <label class="pr-label">Supplier</label>
                                    <input type="text"
                                           class="form-control pr-input"
                                           id="poSupplier"
                                           value="SUP-001">
                                </div>

                                <div class="col-md-6">
                                    <label class="pr-label">Quantity</label>
                                    <input type="number"
                                           class="form-control pr-input"
                                           id="poQuantity">
                                </div>
                            </div>

                            <div class="row mt-3">
                                <div class="col-md-6">
                                    <label class="pr-label">Unit Cost</label>
                                    <input type="number"
                                           class="form-control pr-input"
                                           id="poUnitCost">
                                </div>

                                <div class="col-md-6">
                                    <label class="pr-label">Requested Delivery Date</label>
                                    <input type="date"
                                           class="form-control pr-input"
                                           id="poDeliveryDate">
                                </div>
                            </div>

                            <div class="form-group mt-3 mb-0">
                                <label class="pr-label">Justification</label>
                                <textarea class="form-control pr-input pr-textarea"
                                          rows="3"
                                          id="poReason">Critical shortage – immediate supply required</textarea>
                            </div>
                        </div>

                    </form>
                </div>

                <div class="modal-footer border-0">
                    <button type="button"
                            class="btn pr-btn pr-btn-light"
                            data-dismiss="modal">
                        Cancel
                    </button>

                    <button type="button"
                            id="createPoBtn"
                            class="btn pr-btn pr-btn-primary"
                            style="background:linear-gradient(135deg,#e74a3b,#be2617)"
                            onclick="submitPo()">
                        <i class="fas fa-bolt mr-2"></i>Create PO
                    </button>
                </div>

            </div>
        </div>
    </div>

    <!-- Digital Workers Modal -->
    <div class="modal fade pr-modern-modal" id="dwModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">

                <div class="modal-header border-0">
                    <div class="d-flex align-items-center">
                        <div class="pr-modal-icon mr-3" style="background:linear-gradient(135deg,#1cc88a,#17a673)">
                            <i class="fas fa-robot"></i>
                        </div>
                        <div>
                            <h5 class="modal-title font-weight-bold mb-1">
                                Digital Workers
                            </h5>
                            <small class="text-muted">
                                Trigger and monitor automated agents
                            </small>
                        </div>
                    </div>
                    <button type="button"
                            class="pr-modal-close"
                            data-dismiss="modal"
                            aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <div class="dw-card" data-worker="ShortagePredictor">
                                <div class="dw-icon bg-danger">
                                    <i class="fas fa-exclamation-triangle"></i>
                                </div>
                                <div class="dw-content">
                                    <h6>Shortage Predictor</h6>
                                    <small class="text-muted">Detect supply risks early</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="dw-card" data-worker="InventoryAccuracy">
                                <div class="dw-icon bg-info">
                                    <i class="fas fa-balance-scale"></i>
                                </div>
                                <div class="dw-content">
                                    <h6>Inventory Accuracy</h6>
                                    <small class="text-muted">Oracle vs WMS validation</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="dw-card" data-worker="ShelfLife">
                                <div class="dw-icon bg-warning">
                                    <i class="fas fa-hourglass-half"></i>
                                </div>
                                <div class="dw-content">
                                    <h6>Shelf Life Analyzer</h6>
                                    <small class="text-muted">Expiry and aging detection</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="dw-card" data-worker="InventoryAging">
                                <div class="dw-icon bg-secondary">
                                    <i class="fas fa-chart-area"></i>
                                </div>
                                <div class="dw-content">
                                    <h6>Inventory Aging</h6>
                                    <small class="text-muted">Analyze slow-moving stock</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-0">
                    <button type="button" class="btn pr-btn pr-btn-light" data-dismiss="modal">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- Shortage Predictor Modal -->
    <div class="modal fade pr-modern-modal" id="shortagePredictorModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">

                <div class="modal-header border-0">
                    <div class="d-flex align-items-center">
                        <div class="pr-modal-icon mr-3 bg-danger">
                            <i class="fas fa-exclamation-triangle"></i>
                        </div>
                        <div>
                            <h5 class="modal-title font-weight-bold">
                                Run Shortage Predictor
                            </h5>
                            <small class="text-muted">
                                Choose Scan All Materials or enter one material per line.
                            </small>
                        </div>
                    </div>
                    <button type="button"
                            class="pr-modal-close"
                            data-dismiss="modal"
                            aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <form id="shortagePredictorForm">
                        <div class="pr-form-card mb-4" id="spMaterialsCard">

                            <div class="pr-section-title mb-3">
                                Scan Scope
                            </div>

                            <div class="custom-control custom-radio mb-2">
                                <input type="radio"
                                       id="spAllMaterials"
                                       name="spScope"
                                       class="custom-control-input"
                                       value="ALL"
                                       checked>
                                <label class="custom-control-label" for="spAllMaterials">
                                    Scan All Materials
                                </label>
                            </div>
                            <div class="custom-control custom-radio">
                                <input type="radio"
                                       id="spSelectedMaterials"
                                       name="spScope"
                                       class="custom-control-input"
                                       value="SELECTED">
                                <label class="custom-control-label" for="spSelectedMaterials">
                                    Scan Specific Materials
                                </label>
                            </div>
                        </div>
                        <div class="pr-section-title mb-3">
                            Material / Part Number
                        </div>
                        <div class="form-group mb-0">
                            <textarea id="spMaterials"
                                      class="form-control pr-input pr-textarea"
                                      rows="4"
                                      disabled
                                      placeholder="Enter one material per line
                                    MAT-001
                                    MAT-002
                                    MAT-003"></textarea>
                        </div>

                        <div class="pr-form-card">
                            <div class="d-flex justify-content-between align-items-center">
                                <div class="pr-section-title">
                                    Execution Status
                                </div>
                                <div id="spStatus" class="text-muted">
                                    Ready to execute
                                </div>
                            </div>
                        </div>
                    </form>
                </div>

                <div class="modal-footer border-0">
                    <button class="btn pr-btn pr-btn-light" id="cancelShortagePredictorBtn" data-dismiss="modal">
                        Cancel
                    </button>

                    <button class="btn pr-btn pr-btn-primary" id="runShortagePredictorBtn">
                        <i class="fas fa-play mr-1"></i>
                        Run Worker
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- Confirmation Modal -->
    <div class="modal fade pr-modern-modal" id="confirmActionModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">

                <div class="modal-header border-0">
                    <div class="d-flex align-items-center">
                        <div class="pr-modal-icon mr-3" style="background:linear-gradient(135deg,#f6c23e,#dda20a)">
                            <i class="fas fa-exclamation"></i>
                        </div>
                        <div>
                            <h5 class="modal-title font-weight-bold mb-1">
                                Confirm Action
                            </h5>
                            <small class="text-muted">
                                Please confirm your action
                            </small>
                        </div>
                    </div>
                    <button type="button"
                            class="pr-modal-close"
                            data-dismiss="modal"
                            aria-label="Close">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <p id="confirmActionText" class="mb-0">
                        Are you sure you want to proceed?
                    </p>
                </div>

                <div class="modal-footer border-0">
                    <button type="button" class="btn pr-btn pr-btn-light" data-dismiss="modal">
                        Cancel
                    </button>

                    <button type="button" id="confirmActionBtn" class="btn pr-btn pr-btn-primary">
                        Yes, Confirm
                    </button>
                </div>

            </div>
        </div>
    </div>
</div>

@section Styles {
    <link href="~/Content/S-MRP/smrpDashboard.css" rel="stylesheet" />
}

@section Scripts {
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <script>
        window.SMRPDashboardConfig = {
            consumptionData: @Html.Raw(Json.Encode(Model.ConsumptionChartData)),
            alignmentData: @Html.Raw(Json.Encode(Model.AlignmentChartData)),
            accuracyPercentage: @(Model.InventoryAccuracy?.AccuracyPercentage ?? 0)
    };
    </script>

    <script>
        window.smrpIndexUrl = '@Url.Action("Index", "SmrpDashboard")';
        window.smrpActivityLogUrl = '@Url.Action("GetActivityLog", "SmrpDashboard")';
        window.createPrUrl = '@Url.Action("CreatePurchaseRequisition", "SmrpDashboard", new { area = "S_MRP" })';
        window.markActionUrl = '@Url.Action("MarkActionDone", "SmrpDashboard", new { area = "S_MRP" })';
    </script>

    <script src="~/Scripts/S-MRP/smrpDashboard.js"></script>
}

@functions {
    private string GetActionIcon(string actionType)
    {
        var icons = new Dictionary<string, string>
{
            {"Notify", "bell"},
            {"Trigger", "play-circle"},
            {"Create", "plus-circle"},
            {"Generate", "file-alt"},
            {"Update", "sync-alt"},
            {"Log", "clipboard-list"}
        };
        return icons.ContainsKey(actionType) ? icons[actionType] : "cog";
    }

    private string GetActionColor(string actionType)
    {
        var colors = new Dictionary<string, string>
{
            {"Notify", "primary"},
            {"Trigger", "success"},
            {"Create", "info"},
            {"Generate", "warning"},
            {"Update", "secondary"},
            {"Log", "dark"}
        };
        return colors.ContainsKey(actionType) ? colors[actionType] : "muted";
    }

    private string GetStatusColor(string status)
    {
        var colors = new Dictionary<string, string>
{
            {"Completed", "success"},
            {"In Progress", "warning"},
            {"Failed", "danger"}
        };
        return colors.ContainsKey(status) ? colors[status] : "secondary";
    }
}

// SmrpDashboardController.cs
using M2OSS.DTO.Common;
using M2OSS.DTO.DigitalWorkers;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Service.DigitalWorkers.Interface;
using M2OSS.Service.S_MRP.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace M2OSS.Web.Controllers.S_MRP
{
    public class SmrpDashboardController : BaseController
    {
        private readonly ISmrpDashboardService _dashboardService;
        private readonly IAutoPrPoGenerationService _autoPrPoGenerationService;
        private readonly IDigitalWorkerExecutorService _executor;
        private readonly IDigitalWorkerActionLogRepository _actionLogRepo;

        public SmrpDashboardController(ISmrpDashboardService dashboardService, IAutoPrPoGenerationService autoPrPoGenerationService, IDigitalWorkerExecutorService executor, IDigitalWorkerActionLogRepository actionLogRepo)
        {
            _dashboardService = dashboardService;
            _autoPrPoGenerationService = autoPrPoGenerationService;
            _executor = executor;
            _actionLogRepo = actionLogRepo;
        }

        public async Task<ActionResult> Index(string subMenu, string site, DateTime? selectedDate)
        {
            ViewData["ActiveMenu"] = "SMRP";
            ViewData["ActiveSubMenu"] = "SmrpDashboard";

            var user = Session["User"] as UserDTO;

            site = !string.IsNullOrWhiteSpace(site)
                ? site
                : user?.ViewingSite ?? "PHO";

            var model =
                selectedDate.HasValue
                    ? await _dashboardService.GetDashboardDataByDateAsync(site, selectedDate.Value)
                    : await _dashboardService.GetDashboardDataAsync(site);

            return View("~/Views/S-MRP/SmrpDashboard/Index.cshtml", model);
        }

        [HttpPost]
        public async Task<JsonResult> CreatePurchaseRequisition(GeneratePurchaseDocumentsRequestDTO dto)
        {
            if (dto == null)
                return Json(new { success = false, message = "Invalid request" });

            var result = await _autoPrPoGenerationService.GenerateAsync(dto);
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> RunShortagePredictor(string materialsJson, string plantCode, bool scanAllMaterials)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(plantCode))
                {
                    return Json(new { success = false, message = "PlantCode is required." });
                }

                JArray materialsArray = null;

                if (!string.IsNullOrWhiteSpace(materialsJson))
                {
                    try
                    {
                        materialsArray = JArray.Parse(materialsJson);
                    }
                    catch
                    {
                        return Json(new { success = false, message = "Invalid Materials JSON" });
                    }
                }

                var user = Session["User"] as UserDTO;
                var payload = new JObject
                {
                    ["PlantCode"] = plantCode,
                    ["ScanAllMaterials"] = scanAllMaterials,
                    ["SourceSystem"] = "SMRP_Dashboard",
                    ["RequestedByEmail"] = user?.Email,
                    ["Timestamp"] = DateTime.UtcNow
                };

                if (materialsArray != null)
                    payload["Materials"] = materialsArray;

                System.Diagnostics.Debug.WriteLine(payload.ToString(Newtonsoft.Json.Formatting.Indented));

                var context = new WorkerExecutionContext(
                    workerCode: "SHORTAGE_PREDICTION",
                    payload: payload,
                    requestedBy: "Dashboard",
                    correlationId: Guid.NewGuid().ToString()
                );

                var result = await _executor.ExecuteAsync("SHORTAGE_PREDICTION", context);

                return Json(new
                {
                    success = true,
                    worker = new
                    {
                        result.WorkerCode,
                        result.Status,
                        result.Summary,
                        result.CorrelationId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult MarkActionDone(MarkActionRequestDTO dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Material))
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                _actionLogRepo.InsertLog(
                    "SHORTAGE_PREDICTION",
                    dto.ActionType,
                    dto.ActionType == "DONE_PR"
                        ? $"PR marked complete for {dto.Material}"
                        : $"RFQ marked complete for {dto.Material}",
                    dto.Material,
                    "SUCCESS",
                    Guid.NewGuid().ToString()
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

// MarkActionRequestDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.DigitalWorkers
{
    public class MarkActionRequestDTO
    {
        public string Material { get; set; }
        public string ActionType { get; set; }
    }
}

// SmrpDashboardService.cs
using M2OSS.DTO.DigitalWorkers.AgingAnalyzer;
using M2OSS.DTO.S_MRP;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Service.S_MRP.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Service.S_MRP.Service
{
    public class SmrpDashboardService : ISmrpDashboardService
    {
        private readonly IExecutionAuditRepository _auditRepo;
        private readonly IDigitalWorkerActionLogRepository _actionLogRepo;
        private readonly ICamstarTransactionRepository _camstarRepo;

        private const string ACTION_RFQ = "Create RFQ";
        private const string ACTION_PR = "Create PR";
        private const string ACTION_EXPEDITE = "Expedite delivery";

        public SmrpDashboardService(
            IExecutionAuditRepository auditRepo,
            IDigitalWorkerActionLogRepository actionLogRepo,
            ICamstarTransactionRepository camstarRepo)
        {
            _auditRepo = auditRepo;
            _actionLogRepo = actionLogRepo;
            _camstarRepo = camstarRepo;
        }

        public async Task<SmrpDashboardDTO> GetDashboardDataAsync(string site)
        {
            Dictionary<string, decimal> inventoryMap;

            if (site == "THO")
            {
                inventoryMap = GetMockThoInventory();
            }
            else
            {
                inventoryMap = await GetCamstarInventoryAsync();
            }

            var shortageRiskList = GetConfirmedShortagesFromPredictor(site);
            var shortageRisk = BuildSummaryFromConfirmedShortages(shortageRiskList);
            var executions = _auditRepo.GetRecentExecutions("SHORTAGE_PREDICTION", 50);
            var actionLogs = _actionLogRepo.GetRecentLogs("SHORTAGE_PREDICTION", 2000);

            var actionLogLookup = actionLogs
                .Where(x => x.CorrelationId != Guid.Empty)
                .GroupBy(x => x.CorrelationId.ToString().ToLower())
                .ToDictionary(g => g.Key, g => g.ToList());

            var dynamicActivities = new List<DigitalWorkerAction>();
            var processedLogs = new HashSet<string>();

            var latestExec = GetLatestExecutionForSite(executions, site);

            if (latestExec != null)
            {
                var correlationKey = latestExec.CorrelationId?.ToLower();

                dynamicActivities.Add(new DigitalWorkerAction
                {
                    ActionType = "EXECUTION",
                    Description = "SHORTAGE_PREDICTION_EXECUTED",
                    Timestamp = latestExec.ExecutedAtUtc.DateTime,
                    Status = latestExec.Status,
                    Target = "Shortage Prediction Worker"
                });

                if (!string.IsNullOrEmpty(correlationKey) &&
                    actionLogLookup.ContainsKey(correlationKey))
                {
                    foreach (var log in actionLogLookup[correlationKey].OrderBy(x => x.CreatedAt))
                    {
                        if (!IsBusinessAction(log.ActionType))
                            continue;

                        var uniqueKey = $"{log.CorrelationId}-{log.CreatedAt:O}";
                        processedLogs.Add(uniqueKey);

                        dynamicActivities.Add(new DigitalWorkerAction
                        {
                            ActionType = MapActionType(log.ActionType),
                            Description = GetFriendlyName(log.ActionType),
                            Timestamp = log.CreatedAt,
                            Status = MapStatus(log.Status),
                            Target = log.Target
                        });
                    }
                }
            }

            var agingLogs = _actionLogRepo
                .GetRecentLogs("AGING_ANALYZER", 200);

            var latestAgingCorrelation = agingLogs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CorrelationId.ToString().ToLower())
                .FirstOrDefault();

            var latestAgingLogs = string.IsNullOrEmpty(latestAgingCorrelation)
                ? new List<DigitalWorkerActionLog>()
                : agingLogs
                    .Where(x => x.CorrelationId.ToString().ToLower() == latestAgingCorrelation)
                    .ToList();

            var agingSummaryLog = latestAgingLogs
                .FirstOrDefault(x => x.Target == "AGING_SUMMARY");

            int agingRisk = 0;

            if (agingSummaryLog != null && !string.IsNullOrEmpty(agingSummaryLog.Description))
            {
                agingRisk = ExtractInt(agingSummaryLog.Description, "Risk(>60 days)=");
            }

            var allActivities = dynamicActivities
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            var latestSummaryLog = _actionLogRepo
                .GetRecentLogs("INVENTORY_ACCURACY_MONITOR", 50)
                .Where(x => x.ActionType == "INVENTORY_SUMMARY")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            var shelfLogs = _actionLogRepo
                .GetRecentLogs("SHELF_LIFE_ANALYZER", 200);

            var latestShelfCorrelation = shelfLogs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.CorrelationId.ToString().ToLower())
                .FirstOrDefault();

            var latestShelfLogs = string.IsNullOrEmpty(latestShelfCorrelation)
                ? new List<DigitalWorkerActionLog>()
                : shelfLogs
                    .Where(x => x.CorrelationId.ToString().ToLower() == latestShelfCorrelation)
                    .ToList();

            InventoryAccuracySummaryDTO inventoryAccuracy = null;

            if (latestSummaryLog != null && !string.IsNullOrEmpty(latestSummaryLog.Description))
            {
                var summary = JsonConvert.DeserializeObject<InventoryAccuracySummaryDTO>(
                    latestSummaryLog.Description
                );

                inventoryAccuracy = summary;
            }
            else
            {
                inventoryAccuracy = new InventoryAccuracySummaryDTO();
            }

            // SHELF LIFE ANALYZER -> DASHBOARD
            // Build Expiry Exposure
            var summaryLog = latestShelfLogs
                .FirstOrDefault(x => x.Target == "SHELF_LIFE_SUMMARY");

            int expired = 0;
            int nearExpiry = 0;
            int safe = 0;

            if (summaryLog != null && !string.IsNullOrEmpty(summaryLog.Description))
            {
                var parts = summaryLog.Description;

                expired = ExtractInt(parts, "Expired=");
                nearExpiry = ExtractInt(parts, "NearExpiry=");
                safe = ExtractInt(parts, "Safe=");
            }

            // Create model
            var expiryExposureModel = new ExpiryExposureModel
            {
                Days30 = nearExpiry,
                Days60 = nearExpiry,
                TotalValue = 0 // future enhancement
            };


            // Build Expiry Alerts Table
            var expiryAlerts = latestShelfLogs
                .Where(x => x.ActionType == "DECISION")
                .Select(x =>
                {
                    var desc = x.Description;

                    var days = ExtractInt(desc, "Days=");

                    return new ExpiryAlertDetail
                    {
                        Material = ExtractValue(desc, "Item="),
                        Batch = ExtractValue(desc, "Lot="),
                        DaysLeft = days,
                        ExpiryDate = DateTime.Now.AddDays(days),
                        Value = 0
                    };
                })
                .OrderBy(x => x.DaysLeft)
                .Take(50)
                .ToList();

            int totalItems = 0;

            if (agingSummaryLog != null && !string.IsNullOrEmpty(agingSummaryLog.Description))
            {
                totalItems = ExtractInt(agingSummaryLog.Description, "Total=");
            }

            var inventoryHealthModel = new InventoryHealthModel
            {
                Percentage = totalItems > 0
                    ? Math.Round(((decimal)(totalItems - agingRisk) / totalItems) * 100, 2)
                    : 100,
                HealthyItems = Math.Max(0, totalItems - agingRisk)
            };

            var agingList = latestAgingLogs
                .Where(x => x.ActionType == "DECISION" && x.Target == "AGING_DETAIL")
                .Select(x =>
                {
                    var desc = x.Description;

                    return new AgingDetailDTO
                    {
                        Material = ExtractValue(desc, "Item="),
                        Lot = ExtractValue(desc, "Lot="),
                        AgingDays = ExtractInt(desc, "Aging=")
                    };
                })
                .OrderByDescending(x => x.AgingDays)
                .ToList();

            return new SmrpDashboardDTO
            {
                ShortageRisk = shortageRisk,     // live Camstar
                ShortageRiskList = shortageRiskList, // live Camstar
                InventoryAccuracy = inventoryAccuracy, // Inventory Accuracy Monitor

                // Static/mock — replace later as needed
                ExpiryExposure = expiryExposureModel,
                InventoryHealth = inventoryHealthModel,
                AgingAlertList = agingList,
                ConsumptionRate = new ConsumptionRateModel { Percentage = 12, Trend = "Trending up" },
                OpenPOs = new OpenPOModel { Count = 147, OnTime = 89, Delayed = 58, Value = 2300000 },
                PlanVsSupply = new PlanVsSupplyModel { Percentage = 92 },

                DigitalWorkerActivity = new DigitalWorkerActivityModel
                {
                    ActionsToday = allActivities.Count,
                    ActionsThisWeek = allActivities.Count,
                    LastAction = allActivities.FirstOrDefault()?.Description ?? "No recent activity",
                    LastActionTime = allActivities.FirstOrDefault()?.Timestamp ?? DateTime.Now,
                    IsActive = true
                },

                OpenPOList = new List<OpenPODetail>
                {
                    new OpenPODetail { PONumber = "PO-2024-089", Supplier = "TechCorp",          DueDate = new DateTime(2024,12,15), Status = "Delayed",  Value = 125000 },
                    new OpenPODetail { PONumber = "PO-2024-090", Supplier = "Global Supplies",   DueDate = new DateTime(2024,12,20), Status = "On Track", Value = 89500  },
                    new OpenPODetail { PONumber = "PO-2024-091", Supplier = "Premium Materials", DueDate = new DateTime(2024,12,22), Status = "On Track", Value = 156750 }
                },

                ExpiryAlertList = expiryAlerts,

                RecentDigitalWorkerActions = allActivities,

                ConsumptionChartData = new ChartDataModel
                {
                    Labels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4", "Week 5", "Week 6", "Week 7", "Week 8" },
                    Data = new List<int> { 120, 150, 170, 160, 190, 210, 230, 260 }
                },

                AlignmentChartData = new ChartDataModel
                {
                    Labels = new List<string> { "Perfect Supply", "Over Supply", "Under Supply" },
                    Data = new List<int> { 127, 23, 15 }
                }
            };

        }

        public async Task<SmrpDashboardDTO> GetDashboardDataByDateAsync(string site, DateTime selectedDate)
        {
            var model = await GetDashboardDataAsync(site);
            var shortageRiskList = GetConfirmedShortagesFromPredictor(site, selectedDate);
            var activities = GetActivityLogByDate(site, selectedDate);

            model.ShortageRiskList = shortageRiskList;
            model.ShortageRisk = BuildSummaryFromConfirmedShortages(shortageRiskList);
            model.RecentDigitalWorkerActions = activities;

            model.DigitalWorkerActivity = new DigitalWorkerActivityModel
            {
                ActionsToday = activities.Count,
                ActionsThisWeek = activities.Count,
                LastAction = activities.Any()
                ? activities.First().Description
                : "No recent activity",
                LastActionTime = activities.Any()
                ? activities.First().Timestamp
                : selectedDate,
                IsActive = activities.Any()
            };

            return model;
        }

        // LIVE SHORTAGE RISK CARD - Receives inventoryMap — no extra Camstar call
        private ShortageRiskModel GetLiveShortageRisk(Dictionary<string, decimal> inventoryMap)
        {
            int materialCount = 0;
            int criticalCount = 0;
            int highCount = 0;

            foreach (var kvp in inventoryMap)
            {
                var stockQty = kvp.Value;
                var avgDailyConsumption = stockQty > 0 ? stockQty / 3m : 1m;
                var leadTimeDays = 5m;
                var safetyDays = 5m;
                var reorderPoint = avgDailyConsumption * (leadTimeDays + safetyDays);
                var daysOfSupply = avgDailyConsumption > 0
                                          ? stockQty / avgDailyConsumption
                                          : 0m;

                if (stockQty < reorderPoint)
                {
                    materialCount++;

                    if (daysOfSupply <= leadTimeDays)
                        criticalCount++;
                    else if (daysOfSupply <= leadTimeDays + 2)
                        highCount++;
                }
            }

            return new ShortageRiskModel
            {
                MaterialCount = materialCount,
                CriticalCount = criticalCount,
                HighCount = highCount
            };
        }

        // LIVE SHORTAGE RISK LIST TABLE - Receives inventoryMap — no extra Camstar call
        private List<ShortageRiskDetail> GetLiveShortageRiskList(Dictionary<string, decimal> inventoryMap)
        {
            var result = new List<ShortageRiskDetail>();

            foreach (var kvp in inventoryMap)
            {
                var partNumber = kvp.Key;
                var stockQty = kvp.Value;
                var avgDailyConsumption = stockQty > 0 ? stockQty / 3m : 1m;
                var leadTimeDays = 5m;
                var safetyDays = 5m;
                var reorderPoint = avgDailyConsumption * (leadTimeDays + safetyDays);
                var daysOfSupply = avgDailyConsumption > 0
                                          ? stockQty / avgDailyConsumption
                                          : 0m;

                if (stockQty >= reorderPoint)
                    continue;

                string riskLevel = daysOfSupply <= leadTimeDays ? "Critical"
                                 : daysOfSupply <= leadTimeDays + 2 ? "High"
                                 : "Medium";

                result.Add(new ShortageRiskDetail
                {
                    Material = partNumber,
                    CurrentStock = (int)stockQty,
                    LeadTimeDays = (int)leadTimeDays,
                    RiskLevel = riskLevel,
                    DaysToShortage = (int)Math.Max(0, Math.Floor(daysOfSupply))
                });
            }

            return result
                .OrderBy(x => x.DaysToShortage)
                .ToList();
        }

        // SHARED — fetch inventory from Camstar ONCE
        private async Task<Dictionary<string, decimal>> GetCamstarInventoryAsync()
        {
            const string workflowStep = "PWH_0006";
            var cacheKey = $"CAMSTAR_INV_{workflowStep}";

            // 1. TRY CACHE FIRST
            var cached = MemoryCache.Default.Get(cacheKey) as Dictionary<string, decimal>;
            if (cached != null)
                return cached;

            // 2. FALLBACK TO CAMSTAR (your existing logic)
            var filter = new MaterialDetails { WorkflowStep = workflowStep };

            var materials = await _camstarRepo
                .GetMaterialLotsByFilterAsync(filter, new XDocument());

            var inventoryMap = materials
                .GroupBy(m => m.PartNumber)
                .ToDictionary(
                    g => g.Key,
                    g => (decimal)g.Sum(x => x.Quantity ?? 0)
                );

            // 3. STORE RESULT IN CACHE (short TTL)
            MemoryCache.Default.Set(
                cacheKey,
                inventoryMap,
                DateTimeOffset.Now.AddSeconds(60)
            );

            return inventoryMap;
        }

        // Mappers
        private static string MapActionTypeFromSummary(string summary)
        {
            if (string.IsNullOrEmpty(summary)) return "Log";
            if (summary.Contains("Notify")) return "Notify";
            if (summary.Contains("Trigger")) return "Trigger";
            if (summary.Contains("Create")) return "Create";
            if (summary.Contains("Generate")) return "Generate";
            if (summary.Contains("Update")) return "Update";
            return "Log";
        }

        private static string MapActionType(string actionType)
        {
            if (string.IsNullOrEmpty(actionType)) return "Log";
            if (actionType.Contains("NOTIFICATION")) return "Notify";
            if (actionType.Contains("PURCHASE")) return "Trigger";
            if (actionType.Contains("CREATE")) return "Create";
            if (actionType.Contains("GENERATE")) return "Generate";
            if (actionType.Contains("UPDATE")) return "Update";
            return "Log";
        }

        private static string MapStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return "Completed";
            if (status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase)) return "Completed";
            if (status.Equals("FAILED", StringComparison.OrdinalIgnoreCase)) return "Failed";
            if (status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)) return "In Progress";
            return "Completed";
        }

        private static bool IsDecisionEvaluation(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            return text.Contains("<") || text.Contains(">");
        }

        private Dictionary<string, decimal> GetMockThoInventory()
        {
            var materials = new List<(string PartNumber, decimal Qty)>
            {
                ("TH-MAT-001", 12),
                ("TH-MAT-002", 5),
                ("TH-MAT-003", 22),
                ("TH-MAT-004", 8),
                ("TH-MAT-005", 17)
            };

            return materials.ToDictionary(
                x => x.PartNumber,
                x => x.Qty
            );
        }

        private List<ShortageRiskDetail> GetConfirmedShortagesFromPredictor(string site)
        {
            var executions = _auditRepo
                .GetRecentExecutions("SHORTAGE_PREDICTION", 100);

            var latest = GetLatestExecutionForSite(executions, site);

            if (latest == null || string.IsNullOrWhiteSpace(latest.Result))
                return new List<ShortageRiskDetail>();

            dynamic result = JsonConvert.DeserializeObject(latest.Result);

            if (result == null || result.Data == null || result.Data.items == null)
                return new List<ShortageRiskDetail>();

            var items = result.Data.items;

            var list = new List<ShortageRiskDetail>();

            var actionLogs = _actionLogRepo.GetRecentLogs("SHORTAGE_PREDICTION", 200);

            var completedMaterials = new HashSet<string>(
                actionLogs
                    .Where(x => x.ActionType == "DONE_PR" || x.ActionType == "DONE_RFQ")
                    .Select(x => x.Target?.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
            );

            var actionLookup = actionLogs
                .Where(x => x.ActionType == "CREATE_RFQ"
                         || x.ActionType == "CREATE_PURCHASE_REQUISITION")
                .GroupBy(x => x.Target?.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First().ActionType
                );

            foreach (var item in items)
            {
                decimal days = item.metrics?.DaysOfSupply ?? 0;

                string material = item.item?.ToString();

                if (!string.IsNullOrEmpty(material) && completedMaterials.Contains(material))
                    continue;

                string logAction = null;

                if (!string.IsNullOrEmpty(material) && actionLookup.ContainsKey(material))
                {
                    logAction = actionLookup[material];
                }

                //string normalizedAction =
                //    logAction == "CREATE_RFQ" ? "Create RFQ" :
                //    logAction == "CREATE_PURCHASE_REQUISITION" ? "Create PR" :
                //    "Expedite delivery";


                string normalizedAction;

                if (actionLogs.Any(x =>
                    x.ActionType == "EXPIRED_NO_PROFILE" &&
                    string.Equals(x.Target, material, StringComparison.OrdinalIgnoreCase)))
                {
                    normalizedAction = "Review Planning Profile";
                }
                else
                {
                    normalizedAction =
                        logAction == "CREATE_RFQ" ? "Create RFQ" :
                        logAction == "CREATE_PURCHASE_REQUISITION" ? "Create PR" :
                        "Expedite delivery";
                }



                list.Add(new ShortageRiskDetail
                {
                    Material = material,
                    CurrentStock = (int)(item.metrics?.AvailableSupply ?? 0),
                    LeadTimeDays = 5,
                    RiskLevel =
                        days <= 5 ? "Critical" :
                        days <= 7 ? "High" :
                        "Medium",
                    DaysToShortage = (int)Math.Floor(days),
                    Action = normalizedAction
                });
            }

            return list;
        }

        private List<ShortageRiskDetail> GetConfirmedShortagesFromPredictor(string site, DateTime selectedDate)
        {
            var executions = _auditRepo.GetRecentExecutions("SHORTAGE_PREDICTION", 100);

            var execution = GetExecutionForSiteAndDate(executions, site, selectedDate);

            if (execution == null || string.IsNullOrWhiteSpace(execution.Result))
                return new List<ShortageRiskDetail>();

            dynamic result = JsonConvert.DeserializeObject(execution.Result);

            if (result == null || result.Data == null || result.Data.items == null)
                return new List<ShortageRiskDetail>();

            var items = result.Data.items;

            var list = new List<ShortageRiskDetail>();

            var actionLogs = _actionLogRepo.GetRecentLogs("SHORTAGE_PREDICTION", 200);

            var completedMaterials = new HashSet<string>(
                actionLogs
                    .Where(x => x.ActionType == "DONE_PR" ||
                                x.ActionType == "DONE_RFQ")
                    .Select(x => x.Target?.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
            );

            var actionLookup = actionLogs
                .Where(x => x.ActionType == "CREATE_RFQ"
                         || x.ActionType == "CREATE_PURCHASE_REQUISITION")
                .GroupBy(x => x.Target?.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt)
                          .First()
                          .ActionType
                );

            foreach (var item in items)
            {
                decimal days = item.metrics?.DaysOfSupply ?? 0;

                string material = item.item?.ToString();

                if (!string.IsNullOrEmpty(material) &&
                    completedMaterials.Contains(material))
                {
                    continue;
                }

                string logAction = null;

                if (!string.IsNullOrEmpty(material) &&
                    actionLookup.ContainsKey(material))
                {
                    logAction = actionLookup[material];
                }

                string normalizedAction =
                    logAction == "CREATE_RFQ"
                        ? ACTION_RFQ
                    : logAction == "CREATE_PURCHASE_REQUISITION"
                        ? ACTION_PR
                    : ACTION_EXPEDITE;

                list.Add(new ShortageRiskDetail
                {
                    Material = material,
                    CurrentStock = (int)(item.metrics?.AvailableSupply ?? 0),
                    LeadTimeDays = 5,
                    RiskLevel =
                        days <= 5 ? "Critical" :
                        days <= 7 ? "High" :
                        "Medium",
                    DaysToShortage = (int)Math.Floor(days),
                    Action = normalizedAction
                });
            }

            return list;
        }

        private ShortageRiskModel BuildSummaryFromConfirmedShortages(List<ShortageRiskDetail> list)
        {
            return new ShortageRiskModel
            {
                MaterialCount = list.Count,
                CriticalCount = list.Count(x => x.RiskLevel == "Critical"),
                HighCount = list.Count(x => x.RiskLevel == "High")
            };
        }

        private static bool IsBusinessAction(string actionType)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                return false;

            var allowed = new[]
            {
                "CREATE_PURCHASE_REQUISITION",
                "CREATE_RFQ",
                "SEND_NOTIFICATION",
                "CREATE_SHORTAGE_EVENT",
                "GENERATE_REPORT",
                "GENERATE_SHORTAGE_REPORT",
                "UPDATE_DASHBOARD",
                "LOG_SHORTAGE_PREDICTION"
            };

            return allowed.Any(a =>
                actionType.Equals(a, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetFriendlyName(string actionType)
        {
            switch (actionType)
            {
                case "CREATE_PURCHASE_REQUISITION": return "Purchase Requisition Created";
                case "CREATE_RFQ": return "RFQ Created";
                case "SEND_NOTIFICATION": return "Notification Sent";
                case "CREATE_SHORTAGE_EVENT": return "Shortage Event Created";
                case "GENERATE_REPORT": return "Report Generated";
                case "UPDATE_DASHBOARD": return "Dashboard Updated";
                case "LOG_SHORTAGE_PREDICTION": return "Prediction Logged";
                default: return actionType;
            }
        }

        private int ExtractInt(string text, string key)
        {
            var start = text.IndexOf(key);
            if (start == -1) return 0;

            start += key.Length;
            var end = text.IndexOf(",", start);

            var value = end == -1
                ? text.Substring(start)
                : text.Substring(start, end - start);

            return int.TryParse(value.Trim(), out var result) ? result : 0;
        }

        private string ExtractValue(string text, string key)
        {
            var start = text.IndexOf(key);
            if (start == -1) return "";

            start += key.Length;
            var end = text.IndexOf(",", start);

            return end == -1
                ? text.Substring(start).Trim()
                : text.Substring(start, end - start).Trim();
        }

        private WorkerExecutionAudit GetLatestExecutionForSite(IEnumerable<WorkerExecutionAudit> executions, string site)
        {
            return executions
                .Where(x => x.Status == "Success")
                .Where(x => string.Equals(x.RequestedBy, "WINDOWS_SERVICE", StringComparison.OrdinalIgnoreCase))
                .Where(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Payload))
                        return false;

                    var payload = JObject.Parse(x.Payload);

                    return string.Equals(
                        payload["PlantCode"]?.ToString(),
                        site,
                        StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(x => x.ExecutedAtUtc)
                .FirstOrDefault();
        }

        private WorkerExecutionAudit GetExecutionForSiteAndDate(IEnumerable<WorkerExecutionAudit> executions, string site, DateTime selectedDate)
        {
            return executions
                .Where(x => x.Status == "Success")
                .Where(x =>
                    string.Equals(
                        x.RequestedBy,
                        "WINDOWS_SERVICE",
                        StringComparison.OrdinalIgnoreCase))
                .Where(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Payload))
                        return false;

                    var payload = JObject.Parse(x.Payload);

                    return string.Equals(
                        payload["PlantCode"]?.ToString(),
                        site,
                        StringComparison.OrdinalIgnoreCase);
                })
                .Where(x => x.ExecutedAtUtc.Date == selectedDate.Date)
                .OrderByDescending(x => x.ExecutedAtUtc)
                .FirstOrDefault();
        }

        private List<DigitalWorkerAction> GetActivityLogByDate(string site, DateTime selectedDate)
        {
            var executions = _auditRepo.GetRecentExecutions("SHORTAGE_PREDICTION", 100);
            var actionLogs = _actionLogRepo.GetRecentLogs("SHORTAGE_PREDICTION", 2000);
            var execution = GetExecutionForSiteAndDate(executions, site, selectedDate);

            if (execution == null) return new List<DigitalWorkerAction>();

            var activities = new List<DigitalWorkerAction>();

            activities.Add(new DigitalWorkerAction
            {
                ActionType = "EXECUTION",
                Description = "SHORTAGE_PREDICTION_EXECUTED",
                Timestamp = execution.ExecutedAtUtc.DateTime,
                Status = execution.Status,
                Target = "Shortage Prediction Worker"
            });

            var correlationId = execution.CorrelationId?.ToLower();

            if (string.IsNullOrEmpty(correlationId))
                return activities;

            var executionLogs = actionLogs
                .Where(x =>
                    x.CorrelationId.ToString().ToLower()
                    == correlationId)
                .OrderBy(x => x.CreatedAt)
                .ToList();

            foreach (var log in executionLogs)
            {
                if (!IsBusinessAction(log.ActionType))
                    continue;

                activities.Add(new DigitalWorkerAction
                {
                    ActionType = MapActionType(log.ActionType),
                    Description = GetFriendlyName(log.ActionType),
                    Timestamp = log.CreatedAt,
                    Status = MapStatus(log.Status),
                    Target = log.Target
                });
            }

            return activities
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }
}

// ISmrpDashboardService.cs
using M2OSS.DTO.S_MRP;
using System;
using System.Threading.Tasks;

namespace M2OSS.Service.S_MRP.Interface
{
    public interface ISmrpDashboardService
    {
        Task<SmrpDashboardDTO> GetDashboardDataAsync(string site);
        Task<SmrpDashboardDTO> GetDashboardDataByDateAsync(string site, DateTime selectedDate);
    }
}

// SmrpDashboardDTO.cs
using M2OSS.DTO.DigitalWorkers.AgingAnalyzer;
using System;
using System.Collections.Generic;

namespace M2OSS.DTO.S_MRP
{
    public class SmrpDashboardDTO
    {
        // Key Metrics
        public ShortageRiskModel ShortageRisk { get; set; }
        public ExpiryExposureModel ExpiryExposure { get; set; }
        public InventoryHealthModel InventoryHealth { get; set; }
        public ConsumptionRateModel ConsumptionRate { get; set; }
        public OpenPOModel OpenPOs { get; set; }
        public PlanVsSupplyModel PlanVsSupply { get; set; }

        // Digital Worker Activity
        public DigitalWorkerActivityModel DigitalWorkerActivity { get; set; }


        // Inventory Accuracy Monitor (Digital Worker)
        public InventoryAccuracySummaryDTO InventoryAccuracy { get; set; }

        // Lists for Tables
        public List<OpenPODetail> OpenPOList { get; set; }
        public List<ExpiryAlertDetail> ExpiryAlertList { get; set; }
        public List<ShortageRiskDetail> ShortageRiskList { get; set; }
        public List<DigitalWorkerAction> RecentDigitalWorkerActions { get; set; }
        public List<AgingDetailDTO> AgingAlertList { get; set; }

        // Chart Data
        public ChartDataModel ConsumptionChartData { get; set; }
        public ChartDataModel AlignmentChartData { get; set; }
    }

    // Sub-models for Metrics
    public class ShortageRiskModel { public int MaterialCount { get; set; } public int CriticalCount { get; set; } public int HighCount { get; set; } }
    public class ExpiryExposureModel { public int Days30 { get; set; } public int Days60 { get; set; } public decimal TotalValue { get; set; } }
    public class InventoryHealthModel { public decimal Percentage { get; set; } public int HealthyItems { get; set; } }
    public class ConsumptionRateModel { public decimal Percentage { get; set; } public string Trend { get; set; } }
    public class OpenPOModel { public int Count { get; set; } public int OnTime { get; set; } public int Delayed { get; set; } public decimal Value { get; set; } }
    public class PlanVsSupplyModel { public decimal Percentage { get; set; } }

    public class DigitalWorkerActivityModel
    {
        public int ActionsToday { get; set; }
        public int ActionsThisWeek { get; set; }
        public string LastAction { get; set; }
        public DateTime LastActionTime { get; set; }
        public bool IsActive { get; set; }
    }

    // Sub-models for Tables
    public class OpenPODetail { public string PONumber { get; set; } public string Supplier { get; set; } public DateTime DueDate { get; set; } public string Status { get; set; } public decimal Value { get; set; } }
    public class ExpiryAlertDetail { public string Material { get; set; } public string Batch { get; set; } public DateTime ExpiryDate { get; set; } public int DaysLeft { get; set; } public decimal Value { get; set; } }
    public class ShortageRiskDetail { public string Material { get; set; } public int CurrentStock { get; set; } public int LeadTimeDays { get; set; } public string RiskLevel { get; set; } public int DaysToShortage { get; set; } public string Action { get; set; } }

    public class DigitalWorkerAction
    {
        public string ActionType { get; set; } // "Notify", "Trigger", "Create", "Generate", "Update", "Log"
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } // "Completed", "In Progress", "Failed"
        public string Target { get; set; } // "Production Planning Team", "Procurement Team", "System", "Report", "Dashboard", "Prediction Results"
    }

    // Sub-models for Charts
    public class ChartDataModel { public List<string> Labels { get; set; } public List<int> Data { get; set; } }
}

// InventoryAccuracySummaryDTO.cs
using System;

namespace M2OSS.DTO.S_MRP
{
    public class InventoryAccuracySummaryDTO
    {
        public int TotalEvaluated { get; set; }
        public int AccurateRecords { get; set; }
        public int QuantityMismatches { get; set; }
        public int LocationDiscrepancies { get; set; }
        public int UnusableInventory { get; set; }
        public int NearExpiryAdvisory { get; set; }
        public decimal OracleQoh { get; set; }
        public decimal WmsQoh { get; set; }

        public decimal AccuracyPercentage =>
            TotalEvaluated == 0 ? 0 :
            Math.Round((decimal)AccurateRecords / TotalEvaluated * 100, 2);
    }
}

// smrpDashboard.js
document.addEventListener("DOMContentLoaded", function () {

    const refreshBtn = document.getElementById("refreshBtn");

    if (refreshBtn) {
        refreshBtn.addEventListener("click", function () {
            const btn = this;
            const text = btn.querySelector(".refresh-text");

            btn.classList.add("refreshing");
            text.innerText = "Refreshing...";

            setTimeout(() => {

                const site =
                    document.getElementById("cmbSite")?.value || "PHO";

                window.location.href =
                    window.smrpIndexUrl + "?site=" + encodeURIComponent(site);

            }, 800);
        });
    }

    const dwBtn = document.getElementById("openDigitalWorkersBtn");

    if (dwBtn) {
        dwBtn.addEventListener("click", function () {
            $('#dwModal').modal('show');
        });
    }

    const runBtn = document.getElementById("runShortagePredictorBtn");

    if (runBtn) {
        runBtn.addEventListener("click", runShortagePredictor);
    }

    initCharts();
    initAccuracyGauge();
    initRefreshButtons();
    initShortageFilter();
    initPurchaseButtons();
    initDigitalWorkers();
    initConfirmActionHandler();
    initShortageSearch();
    initShortagePredictorScope();
    hookGlobalSiteSelector();
    initDashboardCalendar();
});

// REFRESH BUTTONS 
function initRefreshButtons() {

    const autoRefreshBtn = document.getElementById('autoRefreshBtn');
    let autoRefreshTimer = null;
    let isAutoRefreshing = false;

    if (autoRefreshBtn) {
        autoRefreshBtn.addEventListener('click', function () {

            if (isAutoRefreshing) {
                clearInterval(autoRefreshTimer);
                autoRefreshTimer = null;
                isAutoRefreshing = false;

                autoRefreshBtn.innerHTML = '<i class="fas fa-sync-alt mr-1"></i>Auto Refresh';
                autoRefreshBtn.classList.remove('btn-success');
                autoRefreshBtn.classList.add('btn-outline-success');

            } else {
                fetchActivityLog();

                autoRefreshTimer = setInterval(function () {
                    fetchActivityLog();
                }, 15000);

                isAutoRefreshing = true;

                autoRefreshBtn.innerHTML = '<i class="fas fa-stop mr-1"></i>Stop Auto Refresh';
                autoRefreshBtn.classList.remove('btn-outline-success');
                autoRefreshBtn.classList.add('btn-success');
            }
        });
    }
}

// FETCH ACTIVITY LOG VIA AJAX 
function fetchActivityLog() {

    var timeline = document.getElementById('activityTimeline');
    if (!timeline) return;

    timeline.innerHTML =
        '<div class="text-center text-muted py-3">' +
        '  <i class="fas fa-sync-alt fa-spin mr-1"></i> Refreshing...' +
        '</div>';

    fetch(window.smrpActivityLogUrl)
        .then(function (response) {
            if (!response.ok)
                throw new Error('HTTP ' + response.status);
            return response.json();
        })
        .then(function (activities) {
            renderActivityLog(activities);
        })
        .catch(function (err) {
            timeline.innerHTML =
                '<div class="text-center text-danger py-3">' +
                '  <i class="fas fa-exclamation-circle mr-1"></i> Failed to refresh: ' + err.message +
                '</div>';
        });
}

// RENDER ACTIVITY LOG HTML
function renderActivityLog(activities) {

    var timeline = document.getElementById('activityTimeline');
    if (!timeline) return;

    if (!activities || activities.length === 0) {
        timeline.innerHTML =
            '<div class="text-center text-muted py-4">' +
            '  <i class="fas fa-robot fa-3x mb-3"></i>' +
            '  <p>No recent digital worker activity</p>' +
            '</div>';
        return;
    }

    var html = '';

    activities.forEach(function (action) {

        var actionType = action.ActionType || action.actionType || '';
        var description = action.Description || action.description || '';
        var target = action.Target || action.target || '';
        var status = action.Status || action.status || '';
        var timestamp = action.Timestamp || action.timestamp || null;

        var icon = getActionIcon(actionType);
        var iconColor = getActionColor(actionType);
        var statusColor = getStatusColor(status);
        var timeStr = parseMvcDate(timestamp);

        html +=
            '<div class="activity-item mb-3 pb-3 border-bottom">' +
            '  <div class="d-flex align-items-start">' +
            '    <div class="activity-icon mr-3">' +
            '      <i class="fas fa-' + icon + ' text-' + iconColor + '"></i>' +
            '    </div>' +
            '    <div class="activity-content flex-grow-1">' +
            '      <div class="d-flex justify-content-between align-items-start">' +
            '        <div>' +
            '          <h6 class="mb-1 font-weight-bold">' + escapeHtml(description) + '</h6>' +
            '          <small class="text-muted">Target: ' + escapeHtml(target) + '</small>' +
            '        </div>' +
            '        <div class="text-right">' +
            '          <span class="badge badge-' + statusColor + '">' + escapeHtml(status) + '</span>' +
            '          <br />' +
            '          <small class="text-muted">' + timeStr + '</small>' +
            '        </div>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>';
    });

    timeline.innerHTML = html;
}

function parseMvcDate(value) {
    if (!value) return '--';

    var mvcMatch = /\/Date\((-?\d+)([+-]\d{4})?\)\//.exec(value);
    if (mvcMatch) {
        var ticks = parseInt(mvcMatch[1], 10);
        var date = new Date(ticks);
        return formatTime(date);
    }

    var iso = new Date(value);
    if (!isNaN(iso.getTime())) {
        return formatTime(iso);
    }

    return '--';
}

function formatTime(date) {
    if (!date || isNaN(date.getTime())) return '--';

    var hh = String(date.getHours()).padStart(2, '0');
    var mm = String(date.getMinutes()).padStart(2, '0');
    var ss = String(date.getSeconds()).padStart(2, '0');
    return hh + ':' + mm + ':' + ss;
}

// SHORTAGE RISK FILTER 
function initShortageFilter() {

    const filterBtns = document.querySelectorAll('#shortageFilterBtns [data-filter]');
    const table = document.getElementById('shortageRiskTable');
    const noResults = document.getElementById('shortageNoResults');
    const noResultLbl = document.getElementById('shortageNoResultsLabel');

    if (!filterBtns.length || !table) return;

    filterBtns.forEach(function (btn) {
        btn.addEventListener('click', function () {

            var filter = btn.getAttribute('data-filter');

            filterBtns.forEach(function (b) {
                b.classList.remove('active');
                b.className = b.className
                    .replace('btn-danger', 'btn-outline-danger')
                    .replace('btn-warning', 'btn-outline-warning')
                    .replace('btn-info', 'btn-outline-info')
                    .replace('btn-secondary', 'btn-outline-secondary');
            });

            btn.classList.add('active');

            if (filter === 'Critical') btn.className = btn.className.replace('btn-outline-danger', 'btn-danger');
            else if (filter === 'High') btn.className = btn.className.replace('btn-outline-warning', 'btn-warning');
            else if (filter === 'Medium') btn.className = btn.className.replace('btn-outline-info', 'btn-info');
            else btn.className = btn.className.replace('btn-outline-secondary', 'btn-secondary');

            var rows = table.querySelectorAll('tbody tr[data-risk]');
            var visibleCount = 0;

            rows.forEach(function (row) {
                if (filter === 'All' || row.getAttribute('data-risk') === filter) {
                    row.style.display = '';
                    visibleCount++;
                } else {
                    row.style.display = 'none';
                }
            });

            if (noResults && noResultLbl) {
                noResultLbl.textContent = filter;
                noResults.style.display = visibleCount === 0 ? 'block' : 'none';
            }
        });
    });
}

// CHARTS 
function initCharts() {

    const cfg = window.SMRPDashboardConfig;

    if (!cfg || !cfg.consumptionData) {
        console.warn("Missing consumption data", cfg);
        return;
    }

    if (document.getElementById('consumptionChart')) {
        new Chart(document.getElementById('consumptionChart').getContext('2d'), {
            type: 'line',
            data: {
                labels: cfg.consumptionData.Labels || [],
                datasets: [{
                    label: 'Material Consumption',
                    data: cfg.consumptionData.Data || [],
                    borderColor: '#17a2b8',
                    backgroundColor: 'rgba(23,162,184,0.2)',
                    borderWidth: 2,
                    fill: true
                }]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });
    }

    if (!cfg.alignmentData) {
        console.warn("Missing alignment data", cfg);
        return;
    }

    if (document.getElementById('alignmentChart')) {
        new Chart(document.getElementById('alignmentChart').getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: cfg.alignmentData.Labels || [],
                datasets: [{
                    data: cfg.alignmentData.Data || [],
                    backgroundColor: ['#28a745', '#ffc107', '#dc3545']
                }]
            },
            options: { responsive: true, maintainAspectRatio: false }
        });
    }
}

// HELPERS 
function getActionIcon(actionType) {
    const icons = { 'Notify': 'bell', 'Trigger': 'play-circle', 'Create': 'plus-circle', 'Generate': 'file-alt', 'Update': 'sync-alt', 'Log': 'clipboard-list' };
    return icons[actionType] || 'cog';
}

function getActionColor(actionType) {
    const colors = { 'Notify': 'primary', 'Trigger': 'success', 'Create': 'info', 'Generate': 'warning', 'Update': 'secondary', 'Log': 'dark' };
    return colors[actionType] || 'muted';
}

function getStatusColor(status) {
    const colors = { 'Completed': 'success', 'In Progress': 'warning', 'Failed': 'danger' };
    return colors[status] || 'secondary';
}

function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

window.SMRPDashboardHelpers = {
    getActionIcon: getActionIcon,
    getActionColor: getActionColor,
    getStatusColor: getStatusColor
};

let pendingAction = null;

function initPurchaseButtons() {

    $(document).on('click', '.shortage-action-btn', function (e) {

        e.preventDefault();

        const btn = $(this);

        const material = btn.data("material");
        let actionType = "";

        if (btn.hasClass("create-pr-btn")) {
            actionType = "DONE_PR";
        }
        else if (btn.hasClass("create-rfq-btn")) {
            actionType = "DONE_RFQ";
        }
        else {
            return;
        }

        pendingAction = {
            material: material,
            actionType: actionType,
            button: btn[0]
        };

        const actionLabel = actionType === "DONE_PR" ? "Done PR" : "Done RFQ";

        $('#confirmActionText').text(
            `Are you sure you want to mark ${material} as ${actionLabel}?`
        );

        console.log("CLICK detected → opening modal");

        $('#confirmActionModal').modal('show');
    });
}

function initConfirmActionHandler() {

    const confirmBtn = document.getElementById("confirmActionBtn");

    if (!confirmBtn) return;

    confirmBtn.addEventListener("click", function () {

        if (!pendingAction) return;

        const { material, actionType, button } = pendingAction;

        confirmBtn.disabled = true;

        fetch(window.markActionUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Material: material,
                ActionType: actionType
            })
        })
            .then(res => res.json())
            .then(res => {

                if (res.success) {

                    showSuccessToast(
                        `${material} marked as ${actionType === "DONE_PR" ? "Done PR" : "Done RFQ"}`,
                        "Success"
                    );

                    // close modal
                    $('#confirmActionModal').modal('hide');

                    // clear pending action
                    pendingAction = null;

                    setTimeout(() => {
                        location.reload();
                    }, 3500);

                } else {
                    toastr.error(res.message || "Failed");
                }
            })
            .catch(err => {
                console.error(err);
                toastr.error("Error updating action");
            })
            .finally(() => {
                confirmBtn.disabled = false;
            });

    });
}

function extractPurchaseData(button) {
    return {
        material: button.dataset.material,
        risk: button.dataset.risk,
        stock: button.dataset.stock,
        days: button.dataset.days,
        site: document.getElementById('cmbSite').value || 'PHO'
    };
}

function openPrModal(data) {
    document.getElementById('prMaterial').value = data.material;
    document.getElementById('prPlant').value = data.site;

    document.getElementById('prQuantity').value = 100;

    $('#prModal').modal('show');
}

function submitPr() {
    const btn = document.getElementById('createPrBtn');
    btn.disabled = true;

    const payload = {
        MaterialCode: document.getElementById('prMaterial').value,
        PlantCode: document.getElementById('prPlant').value,
        SupplierCode: 'SUP-001',
        Quantity: parseFloat(document.getElementById('prQuantity').value),
        UnitCost: parseFloat(document.getElementById('prUnitCost').value),
        Reason: document.getElementById('prReason').value,
        CreatePurchaseOrder: false
    };

    fetch(window.createPrUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(r => {
            if (!r.ok) throw new Error('HTTP ' + r.status);
            return r.json();
        })
        .then(result => {
            const prNumber =
                result.purchaseRequisitionNumber ||
                result.PurchaseRequisitionNumber;

            showSuccessToast(
                `Purchase Requisition ${prNumber} has been created successfully.`,
                "Create PR"
            );
            $('#prModal').modal('hide');
        })
        .catch(err => {
            console.error(err);
            alert('Error creating PR');
        })
        .finally(() => btn.disabled = false);
}

function openPoModal(data) {
    document.getElementById('poMaterial').value = data.material;
    document.getElementById('poPlant').value = data.site;

    document.getElementById('poQuantity').value = data.stock < 100 ? 100 : data.stock;
    document.getElementById('poUnitCost').value = document.getElementById('prUnitCost')?.value || 10;

    const d = new Date();
    d.setDate(d.getDate() + 7);
    document.getElementById('poDeliveryDate').value = d.toISOString().substring(0, 10);

    $('#poModal').modal('show');
}

function submitPo() {
    const btn = document.getElementById('createPoBtn');
    btn.disabled = true;

    const payload = {
        MaterialCode: document.getElementById('poMaterial').value,
        PlantCode: document.getElementById('poPlant').value,
        SupplierCode: document.getElementById('poSupplier').value,
        Quantity: parseFloat(document.getElementById('poQuantity').value),
        UnitCost: parseFloat(document.getElementById('poUnitCost').value),
        Reason: document.getElementById('poReason').value,
        RequestedDeliveryDate: document.getElementById('poDeliveryDate').value,
        CreatePurchaseOrder: true
    };

    // TODO: replace with real endpoint later
    console.log("PO Payload:", payload);

    showSuccessToast(
        "Purchase Order request has been submitted successfully.",
        "Create PO"
    );

    $('#poModal').modal('hide');
    btn.disabled = false;
}

function showSuccessToast(message, title) {
    toastr.success(message, title, {
        timeOut: 6000,
        extendedTimeOut: 2000,
        closeButton: true,
        progressBar: true
    });
}
function initAccuracyGauge() {
    const pct = window.SMRPDashboardConfig.accuracyPercentage;
    const circle = document.getElementById("accuracyCircle");

    if (!circle) return;

    const circumference = 263.89;
    const offset = circumference * (1 - pct / 100);

    circle.style.strokeDashoffset = offset;
}

function initDigitalWorkers() {

    const cards = document.querySelectorAll('.dw-card');
    if (!cards.length) return;

    cards.forEach(card => {
        card.addEventListener('click', function () {

            const worker = this.dataset.worker;

            // Open specific modal
            if (worker === "ShortagePredictor") {

                $('#dwModal').modal('hide');
                $('#shortagePredictorModal').modal('show');
                return;
            }

            // fallback (future workers)
            showSuccessToast(`${worker} clicked`, "Digital Worker");
        });
    });
}

function runShortagePredictor() {

    const btn = document.getElementById("runShortagePredictorBtn");
    btn.disabled = true;

    const cancelBtn =
        document.getElementById("cancelShortagePredictorBtn");

    if (cancelBtn) {
        cancelBtn.disabled = true;
    }

    const materialsRaw = document.getElementById("spMaterials").value.trim();
    const plant = document.getElementById("cmbSite")?.value;

    if (!plant || plant === "N/A") {
        toastr.error("Please select a site.");
        btn.disabled = false;
        return;
    }

    const selectedScope =
        document.querySelector('input[name="spScope"]:checked').value;

    const scanAllMaterials =
        selectedScope === "ALL";

    let materials = null;

    if (selectedScope === "SELECTED") {

        materials = materialsRaw
            .split(/\r?\n/)
            .map(x => x.trim())
            .filter(x => x.length > 0)
            .map(x => ({
                MaterialCode: x
            }));

        if (materials.length === 0) {

            toastr.error(
                "Please enter at least one material."
            );

            btn.disabled = false;
            return;
        }
    }

    btn.innerHTML = `<i class="fas fa-spinner fa-spin mr-1"></i> Running...`;

    document.getElementById("spStatus").innerHTML =
        "<i class='fas fa-sync-alt fa-spin mr-1'></i>" +
        "Running Shortage Predictor - please do not close or cancel this window.";

    fetch('/SmrpDashboard/RunShortagePredictor', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body:
            "plantCode=" + encodeURIComponent(plant || "") +
            "&scanAllMaterials=" + encodeURIComponent(scanAllMaterials) +
            "&materialsJson=" + encodeURIComponent(
                materials ? JSON.stringify(materials) : ""
            )
    })
        .then(r => r.json())
        .then(res => {

            if (res.success) {
                document.getElementById("spStatus").innerHTML =
                    "<span class='text-success'>" +
                    "<i class='fas fa-check'></i> Completed. " +
                    "Please check your email for the generated shortage report." +
                    "</span>";

                showSuccessToast("Shortage Predictor executed");
            } else {
                document.getElementById("spStatus").innerHTML =
                    "<span class='text-danger'>" + res.message + "</span>";

                toastr.error(res.message);
            }
        })
        .catch(err => {
            document.getElementById("spStatus").innerHTML =
                "<span class='text-danger'>Error running worker</span>";

            console.error(err);
        })
        .finally(() => {
            btn.disabled = false;
            btn.innerHTML = `<i class="fas fa-play mr-1"></i> Run Worker`;

            const cancelBtn =
                document.getElementById("cancelShortagePredictorBtn");

            if (cancelBtn) {
                cancelBtn.disabled = false;
            }
        });
}

function initShortageSearch() {

    const searchBox = document.getElementById("shortageSearch");

    if (!searchBox) return;

    searchBox.addEventListener("input", function () {

        const keyword = this.value.toLowerCase().trim();

        const rows = document.querySelectorAll(
            "#shortageRiskTable tbody tr[data-risk]"
        );

        rows.forEach(function (row) {

            const material =
                row.cells[1].textContent.toLowerCase();

            if (material.includes(keyword)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }

        });
    });
}

function initShortagePredictorScope() {

    const allRadio =
        document.getElementById("spAllMaterials");

    const selectedRadio =
        document.getElementById("spSelectedMaterials");

    const materialsBox =
        document.getElementById("spMaterials");

    function toggle() {

        materialsBox.disabled =
            allRadio.checked;

        if (allRadio.checked) {
            materialsBox.value = "";
        }
    }

    allRadio.addEventListener("change", toggle);
    selectedRadio.addEventListener("change", toggle);

    toggle();
}

function hookGlobalSiteSelector() {

    const cmbSite = document.getElementById("cmbSite");

    if (!cmbSite) return;

    cmbSite.addEventListener("change", function () {

        const site = this.value;

        if (!site || site === "N/A")
            return;

        window.location.href =
            window.smrpIndexUrl +
            "?site=" +
            encodeURIComponent(site);
    });
}

function resetShortagePredictorModal() {

    document.getElementById("spMaterials").value = "";

    document.getElementById("spStatus").innerHTML = "";

    // document.getElementById("spResult").textContent = "";

    document.getElementById("spAllMaterials").checked = true;

    document.getElementById("spSelectedMaterials").checked = false;

    document.getElementById("runShortagePredictorBtn").disabled = false;

    document.getElementById("runShortagePredictorBtn").innerHTML =
        '<i class="fas fa-play mr-1"></i> Run Worker';

    const cancelBtn =
        document.getElementById("cancelShortagePredictorBtn");

    if (cancelBtn) {
        cancelBtn.disabled = false;
    }

    initShortagePredictorScope();
}

$('#shortagePredictorModal').on('hidden.bs.modal', function () {
    resetShortagePredictorModal();
});

function initDashboardCalendar() {

    const calendarBtn = document.getElementById("calendarBtn");
    const datePicker = document.getElementById("dashboardDate");

    if (!calendarBtn || !datePicker)
        return;

    calendarBtn.addEventListener("click", function () {

        datePicker.focus();

        if (datePicker.showPicker) {
            datePicker.showPicker();
        }
    });

    datePicker.addEventListener("change", function () {

        const selectedDate = this.value;

        if (!selectedDate)
            return;

        const site = document.getElementById("cmbSite")?.value || "PHO";

        window.location.href =
            window.smrpIndexUrl +
            "?site=" + encodeURIComponent(site) +
            "&selectedDate=" + encodeURIComponent(selectedDate);
    });
}

// smrpDashboard.css

.activity-timeline {
    position: relative;
}

.activity-item {
    position: relative;
    padding-left: 1rem;
}

    .activity-item::before {
        content: '';
        position: absolute;
        left: 0;
        top: 0;
        bottom: 0;
        width: 2px;
        background-color: #e9ecef;
    }

.activity-icon {
    width: 30px;
    height: 30px;
    border-radius: 50%;
    background-color: #f8f9fa;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid #dee2e6;
}

.dashboard-card {
    height: 300px;
    display: flex;
    flex-direction: column;
}

    .dashboard-card .card-body {
        flex: 1;
        overflow: hidden;
        padding: 1rem;
    }

    .dashboard-card .table-responsive {
        height: 100%;
        overflow-y: auto;
        padding-bottom: 1rem;
    }

/* PR / PO MODAL – BASE (LIGHT MODE) */
.pr-modern-modal .modal-dialog {
    max-width: 720px;
}

.pr-modern-modal .modal-content {
    border: 0;
    border-radius: 18px;
    overflow: hidden;
    background: #ffffff;
    box-shadow: 0 20px 60px rgba(31, 45, 61, 0.18);
}

.pr-modern-modal .modal-header {
    padding: 1.5rem 1.5rem 1rem;
    background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.pr-modern-modal .modal-body {
    padding: 0 1.5rem 1.25rem;
}

.pr-modern-modal .modal-footer {
    padding: 1rem 1.5rem 1.5rem;
    background: #fcfdff;
}

.pr-modal-icon {
    width: 48px;
    height: 48px;
    border-radius: 14px;
    background: linear-gradient(135deg, #4e73df 0%, #224abe 100%);
    color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
    box-shadow: 0 10px 22px rgba(78, 115, 223, 0.25);
}

.pr-modern-modal .modal-title {
    color: #2f3a4c;
    font-size: 1.1rem;
}

/*.pr-modal-close {
    font-size: 1.8rem;
    font-weight: 300;
    color: #98a2b3;
    opacity: 1;
}

    .pr-modal-close:hover {
        color: #5a5c69;
    }*/

/* start: modal closing icon */

.pr-modal-close {
    width: 38px;
    height: 38px;
    padding: 0;
    border: 1px solid #e3eaf3;
    border-radius: 50%;
    background: linear-gradient(180deg, #ffffff 0%, #f8fbff 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    color: #667085;
    opacity: 1;
    cursor: pointer;
    box-shadow: 0 2px 8px rgba(15, 23, 42, 0.05);
    transition: all 0.2s ease;
}

    .pr-modal-close i {
        font-size: 14px;
        font-weight: 700;
    }

    .pr-modal-close:hover {
        background: #f3f7fc;
        border-color: #4e73df;
        color: #4e73df;
        box-shadow: 0 4px 12px rgba(78, 115, 223, 0.12);
    }

    .pr-modal-close:focus {
        outline: none;
        box-shadow: 0 0 0 3px rgba(78, 115, 223, 0.15), 0 4px 12px rgba(78, 115, 223, 0.12);
    }

body.dark-mode .pr-modal-close {
    background: #3a3f46;
    border-color: #454b54;
    color: #9aa3b2;
    box-shadow: none;
}

    body.dark-mode .pr-modal-close:hover {
        background: #434952;
        border-color: #58606b;
        color: #ffffff;
    }
/* end: modal closing icon */

/* PR / PO MODAL – CONTENT */
.pr-context-card,
.pr-form-card {
    border: 1px solid #e9eef5;
    border-radius: 16px;
    background: #ffffff;
    box-shadow: 0 8px 24px rgba(15, 23, 42, 0.04);
    padding: 1rem;
}

.pr-context-card {
    background: linear-gradient(180deg, #f8fbff 0%, #fdfefe 100%);
}

.pr-section-title {
    font-size: 0.82rem;
    font-weight: 700;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    color: #4e73df;
}

.pr-context-badge {
    display: inline-flex;
    align-items: center;
    padding: 0.35rem 0.7rem;
    border-radius: 999px;
    background: rgba(54, 185, 204, 0.12);
    color: #1693a5;
    font-size: 0.75rem;
    font-weight: 600;
}

.pr-label {
    margin-bottom: 0.5rem;
    font-size: 0.78rem;
    font-weight: 700;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    color: #858796;
}

.pr-input-wrap {
    position: relative;
}

.pr-input-icon {
    position: absolute;
    top: 50%;
    left: 0.95rem;
    transform: translateY(-50%);
    color: #a0aec0;
    font-size: 0.9rem;
}

.pr-modern-modal .pr-input {
    border-radius: 12px;
    border: 1px solid #dbe3ee;
    min-height: 46px;
    padding: 0.7rem 0.95rem;
    font-size: 0.95rem;
    color: #2f3a4c;
    background-color: #ffffff;
    transition: all 0.2s ease;
}

.pr-modern-modal .pr-input-with-icon {
    padding-left: 2.6rem;
}

.pr-modern-modal .pr-input:focus {
    border-color: #4e73df;
    box-shadow: 0 0 0 0.2rem rgba(78, 115, 223, 0.12);
}

.pr-modern-modal .pr-input-readonly {
    background: #eef3f8;
    border-color: #e1e8f0;
    color: #5a5c69;
    font-weight: 600;
}

.pr-modern-modal .pr-textarea {
    min-height: 100px;
    resize: vertical;
}

/* BUTTONS */
.pr-btn {
    border-radius: 12px;
    min-width: 120px;
    padding: 0.7rem 1.25rem;
    font-size: 0.92rem;
    font-weight: 600;
}

.pr-btn-light {
    background: #f4f6f9;
    border: 1px solid #dbe3ee;
    color: #5a5c69;
}

    .pr-btn-light:hover {
        background: #e9edf3;
    }

.pr-btn-primary {
    border: none;
    color: #fff;
    background: linear-gradient(135deg, #4e73df, #224abe);
}

.shortage-action-btn {
    width: 120px;
    text-align: center;
}

.shortage-action-text {
    display: inline-block;
    width: 120px;
    text-align: center;
    font-weight: 600;
    color: #6c757d;
}

/* DARK MODE – DASHBOARD‑ALIGNED MODAL */
body.dark-mode .pr-modern-modal .modal-content,
body.dark-mode .pr-modern-modal .modal-header,
body.dark-mode .pr-modern-modal .modal-footer {
    background: #2f343a;
    color: #e6e8eb;
}

body.dark-mode .pr-context-card,
body.dark-mode .pr-form-card {
    background: #343a40;
    border-color: #454b54;
}

body.dark-mode .pr-section-title {
    color: #9fb4ff;
}

body.dark-mode .pr-label {
    color: #a5abb3;
}

body.dark-mode .modal-title,
body.dark-mode .pr-modern-modal small {
    color: #e6e8eb;
}

body.dark-mode .pr-modern-modal .pr-input {
    background-color: #3a3f46;
    border-color: #454b54;
    color: #e6e8eb;
}

    body.dark-mode .pr-modern-modal .pr-input::placeholder {
        color: #8f96a0;
    }

body.dark-mode .pr-modern-modal .pr-input-readonly {
    background-color: #424850;
    color: #b8bcc2;
}

body.dark-mode .pr-modern-modal .pr-input:focus {
    border-color: #4e73df;
    box-shadow: 0 0 0 0.15rem rgba(78,115,223,.35);
}

body.dark-mode .pr-modal-close {
    color: #9aa3b2;
}

    body.dark-mode .pr-modal-close:hover {
        color: #ffffff;
    }

/* RESPONSIVE */
@media (max-width: 767.98px) {
    .pr-modern-modal .modal-header,
    .pr-modern-modal .modal-body,
    .pr-modern-modal .modal-footer {
        padding-left: 1rem;
        padding-right: 1rem;
    }

    .pr-modern-modal .modal-dialog {
        margin: 1rem;
    }

    .pr-btn {
        width: 100%;
    }
}

/* Inventory Accuracy Monitor */
.btn-outline-purple {
    color: #6f42c1;
    border-color: #6f42c1;
}

    .btn-outline-purple:hover {
        background: #6f42c1;
        color: #fff;
    }

.accuracy-gauge {
    position: relative;
    width: 80px;
    height: 80px;
}

    .accuracy-gauge svg {
        transform: rotate(-90deg);
    }

.accuracy-gauge-circle-bg {
    fill: none;
    stroke: #e9ecef;
    stroke-width: 8;
}

.accuracy-gauge-circle {
    fill: none;
    stroke: #d1d5db;
    stroke-width: 8;
    stroke-linecap: round;
    transition: stroke-dashoffset 1s ease;
}

.accuracy-gauge-text {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    font-size: 1.1rem;
    font-weight: 700;
    color: #5a5c69;
}

body.dark-mode .accuracy-gauge-text {
    color: #fff !important;
}

/* Metric Tiles */
body.dark-mode .metric-evaluated .metric-value {
    color: #2f3a4c !important;
}

.metric {
    padding: .5rem;
    border-radius: .25rem;
}

.metric-evaluated {
    background: linear-gradient(135deg, #f8f9fc, #eef2f7);
}

.metric-accurate {
    background: linear-gradient(135deg, #f0fff4, #e6ffed);
}

.metric-mismatch {
    background: linear-gradient(135deg, #fff5f5, #ffe6e6);
}

.metric-location {
    background: linear-gradient(135deg, #fffaf0, #fff0e6);
}

.metric-unusable {
    background: linear-gradient(135deg, #f0f8ff, #e6f2ff);
}

.metric-expiring {
    background: linear-gradient(135deg, #fffbe6, #fff5cc);
}

.metric-title {
    font-size: .7rem;
    font-weight: 700;
    text-transform: uppercase;
    color: #5a5c69;
}

body.dark-mode .metric-title {
    color: #5a5c69 !important;
}

.metric-value {
    font-size: 1.25rem;
    font-weight: 700;
}

.metric-oracle {
    background: linear-gradient(135deg, #ede9ff, #dcd3ff);
}

.metric-wms {
    background: linear-gradient(135deg, #e6ffed, #ccf5d6);
}

.text-purple {
    color: #6f42c1;
}

/* Badge */
.badge-group span {
    margin-left: 6px;
}

    .badge-group span:first-child {
        margin-left: 0;
    }

/* Refresh button */
.refreshing .refresh-icon {
    animation: spin 0.8s linear infinite;
}

.refreshing {
    opacity: 0.8;
    pointer-events: none;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}

/* Digital Workers Modal and button */
.dw-launch-btn {
    border-radius: 12px;
}

.dw-card {
    display: flex;
    align-items: center;
    padding: 0.9rem;
    border-radius: 14px;
    border: 1px solid #e3e6f0;
    background: #fff;
    cursor: pointer;
    transition: all 0.2s ease;
}

    .dw-card:hover {
        transform: translateY(-3px);
        box-shadow: 0 10px 25px rgba(0,0,0,0.08);
        border-color: #4e73df;
    }

.dw-icon {
    width: 42px;
    height: 42px;
    border-radius: 10px;
    color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 12px;
    font-size: 1rem;
}

.dw-content h6 {
    margin: 0;
    font-weight: 700;
    color: #2f3a4c;
}

.dw-content small {
    display: block;
    font-size: 0.75rem;
}

#shortagePredictorModal .pr-input {
    min-height: 44px;
}

#shortagePredictorModal textarea {
    font-family: monospace;
}

#spMaterials {
    background: #f8fbff;
    border: 1px solid #dbe3ee;
    border-radius: 10px;
    padding: 10px;
    font-family: Consolas, monospace;
    font-size: 0.85rem;
    white-space: pre;
    overflow-x: auto;
    word-break: normal;
}

.shortage-toolbar {
    gap: 8px;
}

.shortage-search {
    width: 220px;
    min-width: 180px;
}

@media (max-width: 991px) {

    .shortage-toolbar {
        width: 100%;
        margin-top: 10px;
    }

    .shortage-search {
        flex: 1;
        min-width: 100%;
    }
}

/* Shortage Predictor modal */
#shortagePredictorModal .modal-dialog {
    max-height: 80vh;
    margin: auto;
}

#shortagePredictorModal .modal-content {
    max-height: 80vh;
    display: flex;
    flex-direction: column;
}

#shortagePredictorModal .modal-body {
    flex: 1;
    overflow-y: auto;
}

#shortagePredictorModal textarea {
    min-height: 60px;
}
