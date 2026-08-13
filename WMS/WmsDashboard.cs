// WmsDashboard.cshtml
<head>
    <style>
        .timeline > div > i {
            left: 10px;
        }

        .timeline > div > .timeline-item {
            margin-left: 55px;
            max-width: none;
        }
    </style>
</head>
@{
    ViewBag.Title = "WMS - Dashboard";
}

<div class="row">
    <div class="col">
        <div class="card mb-3 text-light" style="background-color:#073EAF">
            <div class="card-body">
                <div class="row">
                    <div class="col-lg-8 col-sm-12">
                        <h1 class="card-title font-weight-bold" style="font-size:24px;" id="txtInventory">---</h1>
                        <p class="card-text">Material in-stock.</p>
                    </div>
                    <div class="col-lg-4 col-sm-0">
                        <div class="float-right">
                            <i class="fa fa-cubes fa-3x" style="opacity:0.4"></i>
                        </div>

                    </div>
                </div>

            </div>
            <div class="card-footer text-bold">
                See Materials Inventory
                <a href="@Url.Action("Index", "MaterialInventory",new {subMenu="WMS",site=@ViewBag.CurrentUser.Site,vmi=0 })"><i class="fas fa-arrow-circle-right text-light"></i></a>
            </div>

        </div>
    </div>
    <div class="col">
        <div class="card mb-3 text-light" style="background-color:#073EAF">
            <div class="card-body">
                <div class="row">
                    <div class="col-lg-8 col-sm-12">
                        <h1 class="card-title font-weight-bold" style="font-size:24px;" id="txtVmiInventory">---</h1>
                        <p class="card-text">VMI Material in-stock.</p>
                    </div>
                    <div class="col-lg-4 col-sm-0">
                        <div class="float-right">
                            <i class="fa fa-cubes fa-3x" style="opacity:0.4"></i>
                        </div>

                    </div>
                </div>

            </div>
            <div class="card-footer text-bold">
                See VMI Inventory
                <a href="@Url.Action("Index", "MaterialInventory",new {subMenu="WMS",site=@ViewBag.CurrentUser.Site,vmi=1 })"><i class="fas fa-arrow-circle-right text-light"></i></a>
            </div>

        </div>
    </div>
    <div class="col">
        <div class="card mb-3" style="background-color:#139DEB">
            <div class="card-body">
                <div class="row">
                    <div class="col-lg-8 col-sm-12">
                        <h1 class="card-title font-weight-bold" style="font-size:24px;" id="txtInspection">--</h1>
                        <p class="card-text">Materials for inspection.</p>
                    </div>
                    <div class="col-lg-4 col-sm-0">
                        <div class="float-right">
                            <i class="fa fa-gavel fa-3x" style="opacity:0.4"></i>
                        </div>
                    </div>
                </div>

            </div>
            <div class="card-footer text-bold">
                See SQE Receiving
                <a href="@Url.Action("Index", "SqeReceiving",new {subMenu="WMS",sm1= "SQE",site=@ViewBag.CurrentUser.Site })"><i class="fas fa-arrow-circle-right text-light"></i></a>
            </div>

        </div>
    </div>
    <div class="col">
        <div class="card mb-3 text-light" style="background-color:#00c0ef">
            <div class="card-body">
                <div class="row">
                    <div class="col-lg-8 col-sm-offset-0">
                        <h1 class="card-title font-weight-bold" style="font-size:24px;" id="txtHold">--</h1>
                        <p class="card-text">Hold materials.</p>
                    </div>
                    <div class="col-lg-4 col-sm-0">
                        <div class="float-right">
                            <i class="fa fa-ban fa-3x" style="opacity:0.4"></i>
                        </div>

                    </div>
                </div>

            </div>
            <div class="card-footer text-bold">
                See on hold materials
                <a href="@Url.Action("Index", "MaterialHold",new {subMenu="WMS",sm1= "SQE",site=@ViewBag.CurrentUser.Site })"><i class="fas fa-arrow-circle-right text-light"></i></a>
            </div>

        </div>
    </div>

</div>
<br />
<hr />

<div class="row">
    <div class="col-lg-12">
        <div class="card border-0 shadow-sm mb-3">
            <div class="card-header border-bottom px-4 py-3 d-flex justify-content-between align-items-center">
                <div>
                    <h6 class="mb-0">WIP Table</h6>
                </div>
                <div class="d-flex align-items-center ml-auto">
                    @* Scrap and RTV are terminal states filtered out of the WIP table above,
                       so their entry points live here as quick links for discoverability. *@
                    <a href="@Url.Action("Index", "MaterialScrap",new {subMenu="WMS",site=@ViewBag.CurrentUser.Site })" class="btn btn-sm btn-outline-secondary mr-2">
                        <i class="fas fa-trash-alt"></i> Scrap Materials
                    </a>
                    <a href="@Url.Action("Index", "MaterialRtv",new {subMenu="WMS",site=@ViewBag.CurrentUser.Site })" class="btn btn-sm btn-outline-secondary mr-2">
                        <i class="fas fa-store-alt"></i> RTV Materials
                    </a>
                    <button class="btn btn-sm btn-outline-primary" type="button" data-toggle="collapse" data-target="#WipTable" id="btnShowWipTable">
                        <i class="fas fa-caret-down" title="See WIP Table"></i>
                    </button>
                </div>
            </div>

            <div class="card-body collapse px-4 py-3" id="WipTable">
                <div class="table-responsive">
                    <table id="MaterialWipTable" class="table table-hover align-middle w-100">
                        <thead class="text-dark bg-light">
                            <tr>
                                <th>Part Number</th>
                                <th>Lot Number</th>
                                <th>Lot Id</th>
                                <th>Packaging Number</th>
                                <th>Category</th>
                                <th>Rack Location</th>
                                <th>Quantity</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <!--<div class="col-lg-12">
        <div class="card border-0 shadow-sm mb-3">
            <div class="card-header border-bottom px-4 py-3 d-flex justify-content-between align-items-center">
                <div>
                    <h6>Lot History & Transaction Logs</h6>
                </div>
                <button class="btn btn-sm btn-outline-primary ml-auto" type="button" data-toggle="collapse" data-target="#LotHistory" id="btnShowLotHistory">
                    <i class="fas fa-caret-down" title="See Lot History & Transaction Logs"></i>
                </button>
            </div>

            <div class="collapse px-4 py-3" id="LotHistory">
                <div class="row">
                    <div class="col-lg-12">
                        <div class="timeline" id="lotIdTimeline">-->
                            @*<div class="time-label" id="divCurrDate">
                                    <span class="bg-gradient-info">@DateTime.Now.ToString("ddd, dd MMM yyy ")</span>
                                </div>*@
                            <!--<div id="divSearch">
                                <i class="fas fa-search bg-primary"></i>
                                <div class="timeline-item">
                                    <h3 class="timeline-header"><input type="text" class="form-control" id="txtSearchLotId" placeholder="Search Camstar Lot ID..." /></h3>
                                </div>
                            </div>-->
                            @*<div>
                                    <i class="fas fa-clock bg-gray"></i>

                                </div>*@
                        <!--</div>
                    </div>
                </div>
            </div>-->
            @*<div class="card-footer bg-light">
                    <button class="btn btn-outline-success d-none"><i class="fas fa-download"></i> Download CSV</button>
                </div>*@

        <!--</div>

    </div>-->
    <div class="col-lg-12">
        <div class="card border-0 shadow-sm mb-3">
            <div class="card-header border-bottom px-4 py-3 d-flex justify-content-between align-items-center">
                <div>
                    <h6>Lot History & Transaction Logs</h6>
                </div>
                <button class="btn btn-sm btn-outline-primary ml-auto" type="button" data-toggle="collapse" data-target="#LotHistory" id="btnShowLotHistory">
                    <i class="fas fa-caret-down" title="See Lot History & Transaction Logs"></i>
                </button>
            </div>

            <div class="card-body px-4 py-3 collapse" id="LotHistory">
                <div class="timeline" id="lotIdTimeline">
                  
                    <div id="divSearch">
                        <i class="fas fa-search bg-primary"></i>
                        <div class="timeline-item">
                            <h3 class="timeline-header"><input type="text" class="form-control" id="txtSearchLotId" placeholder="Search Camstar Lot ID..." /></h3>
                        </div>
                    </div>
                  
                </div>
                <div class="table-responsive">
                    <button class="btn btn-success mb-1" id="btnDownloadLogs">Download CSV</button>
                    <table id="TransactionLogsTable" class="table table-hover table-bordered align-middle w-100">
                        <thead class="text-dark bg-light">
                            <tr id="TransactionLogsTableHeader">
                                <th>Transaction Date</th>
                                <th>Part Number</th>
                                <th>Lot ID</th>
                                <th>Transaction</th>
                                <th>From Operation</th>
                                <th>To Operation</th>
                                <th>Quantity</th>
                                <th>PIC</th>
                                <th>Source Lot</th>
                                <th>Source Lot Qty</th>
                                <th>Target Lot</th>
                                <th>Target Lot Qty</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        const AppUrls = {
            getMaterialCountByStep: '@Url.Action("GetMaterialCountByStep", "WmsDashboard", new { area = "WMS" })',
            getWipMaterials: '@Url.Action("GetWipMaterials", "WmsDashboard", new { area = "WMS" })',
            getLotHistory: '@Url.Action("GetLotHistoryAndTransactionLogs", "WmsDashboard", new { area = "WMS" })',
        };

    </script>

    <script src="~/Scripts/WMS/wmsDashboard.js"></script>
}

// wmsDashboard.js
var WmsDashboardModule = (function () {
    var table;
    var table1;
    var table2;
    var materials;

    var lotHistory;


    function initWmsDashboardData() {

       
        $.ajax({
            url: AppUrls.getMaterialCountByStep,
            type: "POST",
            datatype: "json",
            success: function (response) {
               
                $('#txtInventory').text(response.filter(res => res.workflowStep == "PWH_0006").map(res => res.count));
                $('#txtVmiInventory').text(response.filter(res => res.workflowStep == "PWH_0001").map(res => res.count));
                $('#txtInspection').text(response.filter(res => res.workflowStep == "PWH_0002").map(res => res.count));
                $('#txtHold').text(response.filter(res => res.workflowStep == "PWH_0003").map(res => res.count));
                // Scrap and RTV are no longer KPI cards with counts; they are simple links.
                // Do not populate txtScrap/txtRtv (elements removed from the view).

               
            },
            error: function (xhr, status, errorThrown) {
                let message = "Error: " + xhr.status + " - " + xhr.statusText + ".\n Please call system admin for further support.";
                toastr.error(message, "Unhandled Exception");
             
            }
        });
    }

    function getLotHistory(_lotId) {


        $.ajax({
            url: AppUrls.getLotHistory,
            type: "POST",
            data: {
                lotId: _lotId
            },
            datatype: "json",
            success: function (response) {
                lotHistory = response;
                console.log(lotHistory);
                
                renderLotHistory(lotHistory);
                hidePreloader();

            },
            error: function (xhr, status, errorThrown) {
                hidePreloader();
                let message = "Error: " + xhr.status + " - " + xhr.statusText + ".\n Please call system admin for further support.";
                toastr.error(message, "Unhandled Exception");

            }
        });
    }


    function getWipMaterials() {
        setTimeout(() => {
            var table = $('#MaterialWipTable').DataTable({
                ajax: {
                    url: AppUrls.getWipMaterials,
                    type: 'POST',
                    dataSrc: function (response) {
                        // Scrap (PWH_0004) and RTV (PWH_0005) are terminal,
                        // non-WIP states - they're not material being worked
                        // on, so filter them out before the table renders.
                        // updateWipCounts still receives the filtered list so
                        // the counts agree with what the user actually sees.
                        var excluded = ['PWH_0004', 'PWH_0005'];
                        var filtered = (response || []).filter(function (row) {
                            return excluded.indexOf(row.workflowStep) === -1;
                        });
                        updateWipCounts(filtered);
                        return filtered;
                    },
                    "error": function (xhr) {
                        toastr.error(xhr.responseJSON?.message || "An unexpected error occurred.", "Error");

                    }
                },
                destroy: true,
                pageLength: 10,
                dom:
                    "<'row'<'col-sm-12 col-md-6'B><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",

                buttons: [
                    {
                        extend: 'csvHtml5',
                        text: 'Download CSV',
                        className: 'btn btn-sm btn-success'
                    }
                ],
                columns: [
                    { data: 'partNumber', title: 'Part Number' },
                    { data: 'lotNumber', title: 'Lot Number' },
                    { data: 'lotId', title: 'Camstar Lot ID' },
                    {
                        data: 'workflowStep', title: 'Operation',
                        render: function (data, type, row) {
                            return ConvertWorkflowSteps(data);


                            //if (data == "PWH_0001") {
                            //    return `Goods Receiving`;
                            //} else if (data == "PWH_0002") {
                            //    return `SQE Inspection`;
                            //} else if (data == "PWH_0003") {
                            //    return `Hold`;
                            //} else if (data == "PWH_0004") {
                            //    return `Scrap`;
                            //} else if (data == "PWH_0005") {
                            //    return `RTV`;
                            //} else if (data == "PWH_0006") {
                            //    return `Inventory`;
                            //} else if (data == "PWH_0007") {
                            //    return `Lot assigned`;
                            //} else if (data == "PWH_0008") {
                            //    return `Picked`;
                            //} else if (data == "PWH_0009") {
                            //    return `Checked`;
                            //} else {
                            //    return `N/A`;
                            //}
                        }
                    },
                    { data: 'category', title: 'Category' },
                    { data: 'receivingLocation', title: 'Rack Location' },
                    {
                        data: 'quantity', title: 'Quantity', render: function (data, type, row) {
                            return data + " " + row.uom;
                        }
                    }
                ]
            });
        }, 0);

        


      
    }

    function updateWipCounts(response) {
        var distinctPartCount = function (predicate) {
            var parts = response
                .filter(predicate)
                .map(function (res) { return res.partNumber; });
            return new Set(parts).size;
        };

        var countByStep = function (step) {
            return distinctPartCount(function (res) { return res.workflowStep == step; });
        };

        //$('#txtInventory').text(countByStep("PWH_0006"));

        $('#txtInventory').text(distinctPartCount(function (res) {
            return res.workflowStep == "PWH_0006" && res.category != "VMI";
        }));

        $('#txtVmiInventory').text(distinctPartCount(function (res) {
            return res.workflowStep == "PWH_0006" && res.category == "VMI";
        }));
        $('#txtInspection').text(countByStep("PWH_0002"));
        $('#txtHold').text(countByStep("PWH_0003"));
        // Scrap (PWH_0004) and RTV (PWH_0005) are no longer shown as KPI cards with counts.
        // They are simple links to their dedicated pages. Do not attempt to update removed elements.
    }

    function renderLotHistory(data) {
        const tbody = $('#TransactionLogsTable tbody');
        const headerRow = $('#TransactionLogsTableHeader');

        tbody.empty();

        // 1️⃣ Collect ALL unique attributes
        const attributeSet = new Set();

        data.forEach(item => {
            item.modifiedAttributes.forEach(attr => attributeSet.add(attr));
        });

        const attributes = [...attributeSet];
        console.log(attributes);


        // 2️⃣ Build table header dynamically
        headerRow.find('th:gt(11)').remove(); // remove old dynamic headers

        attributes.forEach(attr => {
            headerRow.append(`<th>${attr}</th>`);
        });

        // 3️⃣ Build rows
        data.forEach(item => {
            //if (item.fromWorkflowStep == item.toWorkflowStep && item.transaction.toLowerCase().includes("movelot")) {
            //    return;
            //}

            if (item.transaction.toLowerCase().includes("current")) {
                return;
            }

            const attrValueMap = {};

            //for (let i = 0; i < item.modifiedAttributes.length; i++) {
            //    attrValueMap[item.modifiedAttributes[i]] = item.newValues[i];
            //}

            console.log(item.modifiedAttributes);
            for (let i = 0; i < attributes.length; i++) {

                if (item.modifiedAttributes.includes(attributes[i])) {
                    const index = item.modifiedAttributes.indexOf(attributes[i]);
                    attrValueMap[attributes[i]] = item.newValues[index];
                } else {
                    attrValueMap[attributes[i]] = ``;
                }

                
            }

            console.log(attrValueMap);

            let rowHtml = `
            <tr>
            <td>${item.transactionDate}</td>
             <td>${item.partNumber}</td>
                <td>${item.lotId}</td>
                <td>${item.transaction}</td>
                <td>${ConvertWorkflowSteps(item.fromWorkflowStep)}</td>
                <td>${ConvertWorkflowSteps(item.toWorkflowStep)}</td>
                <td>${item.quantity}</td>
                <td>${item.user}</td>
                <td>${item.sourceLot}</td>
                <td>${item.sourceLotQuantity}</td>
                <td>${item.targetLot}</td>
                <td>${item.targetLotQuantity}</td>
        `;

            attributes.forEach(attr => {
                rowHtml += `<td>${attrValueMap[attr] ?? ''}</td>`;
            });

            rowHtml += '</tr>';

            tbody.append(rowHtml);
        });
    }


    function renderTimeline(data) {
        const $timeline = $('#lotIdTimeline');
        //$timeline.empty();

        $('#lotIdTimeline')
            .children()
            .not('#divCurrDate, #divSearch')
            .remove();

        let currentDate = '';
        let $lastTimelineItem = null;

        data.forEach(item => {

            const dateOnly = item.transactionDate.slice(0, 10);
            const timeOnly = item.transactionDate.slice(11, 19);

            // Date label
            if (currentDate !== dateOnly) {
                currentDate = dateOnly;
                $timeline.append(`
            <div class="time-label">
                <span class="bg-secondary">${currentDate}</span>
            </div>
        `);

                // reset last item when date changes
                $lastTimelineItem = null;
            }

            // 🔁 MODIFY → append to previous timeline
            if (item.transaction.toLowerCase().includes('modifyattrs') && $lastTimelineItem) {
                $lastTimelineItem.find('.timeline-body').append(`
                    <hr class="my-1">
                    <div>
                       
                        <strong>Attribute:</strong> ${item.attributeModified}<br>
                        <strong>Old Value:</strong> ${item.attributeOldValue}<br>
                        <strong>New Value:</strong> ${item.attributeNewValue}<br>
                         <strong>Modifed by:</strong> ${item.user}<br>
                    </div>
                `);
                return; // IMPORTANT: skip creating new timeline item
            }


            if (item.transaction.toLowerCase().includes('movelot') && item.fromWorkflowStep == item.toWorkflowStep) {
                return; // IMPORTANT: skip creating new timeline item
            }


            if (item.transaction.toLowerCase().includes('splitlot') && item.fromWorkflowStep == item.toWorkflowStep) {

                // 🆕 Create new timeline item
                const $timelineItem = $(`
                <div>
                    <i class="fas fa-random bg-primary"></i>
                    <div class="timeline-item">
                        <span class="time">
                            <i class="fas fa-clock"></i> ${timeOnly}
                        </span>
                        <h3 class="timeline-header">
                            <strong>${item.user}</strong> - ${item.transaction}
                        </h3>
                        <div class="timeline-body">
                            <div><strong>Splitted Lot:</strong> ${item.targetLot}</div>
                            <div><strong>Spliitted Lot Quanity:</strong> ${item.targetLotQuantity}</div>
                            <div><strong>Current Quantity:</strong> ${item.quantity}</div>
                        </div>
                    </div>
                </div>
            `);
                $timeline.append($timelineItem);
                $lastTimelineItem = $timelineItem;
                return; 
            }

            // 🆕 Create new timeline item
            const $timelineItem = $(`
                <div>
                    <i class="fas fa-random bg-primary"></i>
                    <div class="timeline-item">
                        <span class="time">
                            <i class="fas fa-clock"></i> ${timeOnly}
                        </span>
                        <h3 class="timeline-header">
                            <strong>${item.user}</strong> - ${item.transaction}
                        </h3>
                        <div class="timeline-body">
                            <div><strong>From:</strong> ${ConvertWorkflowSteps(item.fromWorkflowStep)}</div>
                            <div><strong>To:</strong> ${ConvertWorkflowSteps(item.toWorkflowStep)}</div>
                            <div><strong>Quantity:</strong> ${item.quantity}</div>
                        </div>
                    </div>
                </div>
            `);

            $timeline.append($timelineItem);
            $lastTimelineItem = $timelineItem;
        });

        // End marker
        $timeline.append(`
        <div>
            <i class="fas fa-clock bg-gray"></i>
            <div class="timeline-item">
                <button class='btn btn-success' id='btnCsv' >Download in CSV</button>       
            </div>
        </div>
    `);
    }
    function csvEscape(value) {
        if (value == null) return '';
        const str = value.toString();
        return `"${str.replace(/"/g, '""')}"`;
    }

    function generateLotHistoryCsv(data) {
        // 1️⃣ Collect ALL unique attributes (same as table)
        const attributeSet = new Set();

        data.forEach(item => {
            item.modifiedAttributes.forEach(attr => attributeSet.add(attr));
        });

        const attributes = [...attributeSet];

        const csvRows = [];

        // 2️⃣ Header row
        const header = [
            'Part Number',
            'Lot ID',
            'Transaction',
            'Transaction Date',
            'From Operation',
            'To Operation', 
            'Quantity',
            'PIC',
            'Source Lot',
            'Source Lot Quantity',
            'Target Lot',
            'Target Lot Quantity',
            ...attributes
        ];
        csvRows.push(header.map(csvEscape).join(','));

        // 3️⃣ Data rows
        data.forEach(item => {

            if (item.transaction.toLowerCase().includes("current")) {
                return;
            }


            const attrValueMap = {};

            //for (let i = 0; i < item.modifiedAttributes.length; i++) {
            //    attrValueMap[item.modifiedAttributes[i]] = item.newValues[i];
            //}

            for (let i = 0; i < attributes.length; i++) {

                if (item.modifiedAttributes.includes(attributes[i])) {
                    const index = item.modifiedAttributes.indexOf(attributes[i]);
                    attrValueMap[attributes[i]] = item.newValues[index];
                } else {
                    attrValueMap[attributes[i]] = ``;
                }


            }

            const row = [
                item.partNumber,
                item.lotId,
                item.transaction,
                item.transactionDate,
                ConvertWorkflowSteps(item.fromWorkflowStep),
                ConvertWorkflowSteps(item.toWorkflowStep),
                item.quantity,
                item.user,
                item.sourceLot,
                item.sourceLotQuantity,
                item.targetLot,
                item.targetLotQuantity,
                ...attributes.map(attr => attrValueMap[attr] ?? '')
            ];

            csvRows.push(row.map(csvEscape).join(','));
        });

        return csvRows.join('\r\n');
    }

    function downloadLotHistoryCsv(data) {
        const csvContent = generateLotHistoryCsv(data);
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });

        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');

        link.href = url;
        link.download = `${$('#txtSearchLotId').val()}_lot_history.csv`; 

        document.body.appendChild(link);
        link.click();

        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }



    function downloadCSV(data, filename = 'data.csv') {
        if (!data || !data.length) return;

        const headers = Object.keys(data[0]);
        const csvRows = [];

        // Header row
        csvRows.push(headers.join(','));

        // Data rows
        for (const row of data) {
            const values = headers.map(header =>
                `"${String(row[header] ?? '').replace(/"/g, '""')}"`
            );
            csvRows.push(values.join(','));
        }

        const csvContent = csvRows.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });

        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    function ConvertWorkflowSteps(step) {
        return WORKFLOW_STEP_MAP[step] ?? "N/A";
    }

    const WORKFLOW_STEP_MAP = {
        PWH_0001: "Goods Receiving",
        PWH_0002: "SQE Receiving/Inspection",
        PWH_0003: "Hold",
        PWH_0004: "Scrap",
        PWH_0005: "RTV",
        PWH_0006: "Storage",
        PWH_0007: "Lot assigned to ticket",
        PWH_0008: "Picked",
        PWH_0009: "Checked",
        PWH_0010: "Issuance",
        PWH_0011: "Borrowed",
        PWH_0012: "Return",
        PWH_0013: "Assigned IDM (w/o HOST)",
        PWH_0014: "Assigned IDM (w/ HOST)"
    };


    function bindEvents() {
        $('#txtSearchLotId').change(function (e) {
            showPreloader();
            getLotHistory($(this).val());


        });

        $(document).on('click', '#btnCsv', function () {

            downloadCSV(lotHistory, `lot_${$('#txtSearchLotId').val()}.csv`);
        });

        $('#btnDownloadLogs').click(function () {
            downloadLotHistoryCsv(lotHistory);


        });
    }


    function getQueryParam(param) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(param);
    }

    function clearForm() {

    }

    return {
        init: function () {
            bindEvents();

            //initWmsDashboardData();
            getWipMaterials();
            
            //disable the selection of site since THO will not have a wms module.
            $('#cmbSite').prop("disabled", true);
        }
    };
})();

$(document).ready(function () {
    WmsDashboardModule.init();
});

// WmsDashboardController
using M2OSS.DTO.WMS;
using M2OSS.Service.WMS.Interface;
using M2OSS.Web.Helper.XmlConverter;
using M2OSS.Web.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace M2OSS.Web.Controllers.WMS
{
    public class WmsDashboardController : BaseController
    {
        private readonly IWmsDashboardService _wmsDashboardService;
        public WmsDashboardController(IWmsDashboardService wmsDashboardService)
        {
            _wmsDashboardService = wmsDashboardService;
        }
        // GET: WmsDashboard
        public ActionResult Index()
        {
            SetPageHeader("Dashboard");
            return View("~/Views/WMS/WmsDashboard/WmsDashboard.cshtml");
        }


        public async Task<JsonResult> GetWipMaterials()
        {
            string[] steps = { "PWH_0001","PWH_0002", "PWH_0003", /*"PWH_0004", "PWH_0005",*/ "PWH_0006", "PWH_0007", "PWH_0008", "PWH_0009", "PWH_0011" };
            string operations = string.Join(",", steps);

            MaterialDetailsDTO materialDto = new MaterialDetailsDTO();
            materialDto.WorkflowStep = operations;

            var wip = await _wmsDashboardService.GetWipMaterialsAsync(materialDto);

            List<MaterialCountByStep> countBySteps = new List<MaterialCountByStep>();
            foreach(string step in steps)
            {
                MaterialCountByStep countByStep = new MaterialCountByStep();

                int count = wip.Where(w => w.WorkflowStep == step).Count();

                countByStep.WorkflowStep = step;
                countByStep.Count = count;
                countBySteps.Add(countByStep);
              
            }

            return Json(wip);

        }

        public async Task<JsonResult> GetMaterialCountByStep()
        {
            string[] steps = {"PWH_0002", "PWH_0003", "PWH_0004", "PWH_0005", "PWH_0006","PWH_0001" };

            string operations = string.Join(",", steps);

            List<MaterialCountByStep> countBySteps = new List<MaterialCountByStep>();

            var tasks = steps.Select(async step =>
            {
                MaterialCountByStep countByStep = new MaterialCountByStep();

                MaterialDetailsDTO dto = new MaterialDetailsDTO();
                dto.WorkflowStep = step;
                if (step == "PWH_0001")
                {
                    dto.Category = "VMI";
                }
               

                int count = await _wmsDashboardService.GetMaterialCountByStep(dto);
                
                lock (countByStep)
                {

                    countByStep.WorkflowStep = step;
                    countByStep.Count = count;
                    countBySteps.Add(countByStep);
                }
               
            });

            await Task.WhenAll(tasks);

            return Json(countBySteps);

        }

        public async Task<JsonResult> GetLotHistoryAndTransactionLogs(string lotId)
        {
            var logs = await _wmsDashboardService.GetLotHistoryAsync(lotId);
            return Json(logs);
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

// MaterialCountByStep.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace M2OSS.Web.Models
{
    public class WmsDashboardViewModel
    {
        
    }
    public class MaterialCountByStep
    {
        public string WorkflowStep { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
    }
}

// WmsDashboardService.cs
using AutoMapper;
using M2OSS.DTO.WMS;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Service.Common;
using M2OSS.Service.WMS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M2OSS.Service.WMS.Service
{
    public class WmsDashboardService: IWmsDashboardService
    {
        private readonly IMapper _mapper;
        private readonly ICamstarTransactionRepository _camstarTransactionRepository;
        private readonly IWebConfigurationService _webConfigurationRepository;
        private readonly IXmlConverterService _xmlConverterRepository;
        public WmsDashboardService(IMapper mapper, ICamstarTransactionRepository camstarTransactionRepository, IWebConfigurationService webConfigurationRepository, IXmlConverterService xmlConverterRepository)
        {
            _mapper = mapper;
            _camstarTransactionRepository = camstarTransactionRepository;
            _webConfigurationRepository = webConfigurationRepository;
            _xmlConverterRepository = xmlConverterRepository;
        }

        public async Task<int> GetMaterialCountByStep(MaterialDetailsDTO materialDto)
        {
            var material = _mapper.Map<MaterialDetails>(materialDto);
            var filterXml = _xmlConverterRepository.MaterialFilterXml(material);

            var materials = await _camstarTransactionRepository.GetMaterialLotsByFilterAsync(material, filterXml);

            return materials
                    .GroupBy(x => x.PartNumber)
                    .Select(g => g.First())
                    .ToList().Count();
        }

        public async Task<IEnumerable<MaterialDetailsDTO>> GetWipMaterialsAsync(MaterialDetailsDTO materialDto)
        {
            // Split the comma-separated WorkflowStep values (e.g. "PWH_0001,PWH_0002,...")
            // and call the external SOAP service once per step in parallel to mitigate slowness
            // when sending all steps in a single request.
            var workflowSteps = string.IsNullOrWhiteSpace(materialDto.WorkflowStep)
                ? new[] { string.Empty }
                : materialDto.WorkflowStep
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
                var perStepFilterXml = _xmlConverterRepository.MaterialFilterXml(perStepMaterial);

                return _camstarTransactionRepository.GetMaterialLotsByFilterAsync(perStepMaterial, perStepFilterXml);
            }).ToList();

            var materials = (await Task.WhenAll(stepTasks)).SelectMany(r => r).ToList();


            //// Throttle the per-lot attribute SOAP calls so we don't fan out to hundreds
            //// of concurrent requests against the external service.
            //var gate = new SemaphoreSlim(10);

            //var newMaterialDetails = await Task.WhenAll(materials.Select(async mat =>
            //{
            //    await gate.WaitAsync();
            //    try
            //    {
            //        var materialAttribute = await _camstarTransactionRepository.GetMaterialLotAttributeAsync(mat.LotId);
            //        return MaterialMergeHelper.Merge(mat, materialAttribute);
            //    }
            //    finally
            //    {
            //        gate.Release();
            //    }
            //}));

            return _mapper.Map<IEnumerable<MaterialDetailsDTO>>(materials);
        }

        // Camstar records the WMS "Move VMI to Inventory" action as a pure
        // SetLotAttribute call (no workflow-step change), so it only shows up
        // as ModifyAttr rows. The original aggregation folded every ModifyAttr
        // into the preceding real transaction, which hid the transfer. This
        // version groups rows by TransactionDate (the exact instant the
        // transaction was performed, so every attribute write in a single SLTA
        // call shares one timestamp) and promotes the VMI -> Inventory
        // attribute batch into its own synthetic row.
        public async Task<IEnumerable<AggregatedLotHistoryDTO>> GetLotHistoryAsync(string lotId)
        {
            var history = await _camstarTransactionRepository.GetLotHistoryByLotIdAsync(lotId);

            var result = new List<AggregatedLotHistoryDTO>();
            AggregatedLotHistoryDTO current = null;

            // Rows sharing the same TransactionDate belong to one Camstar
            // operation. Grouping makes the ModifyAttr-to-transaction pairing
            // order-independent within a batch.
            foreach (var group in history
                                    .OrderBy(r => r.TransactionDate)
                                    .GroupBy(r => r.TransactionDate))
            {
                // Drop no-op MoveLot rows (lot did not actually change step).
                var rows = group
                            .Where(r => !(r.FromWorkflowStep == r.ToWorkflowStep
                                          && r.Transaction.ToLower().Contains("movelot")))
                            .ToList();

                if (rows.Count == 0)
                {
                    continue;
                }

                // The real (non-ModifyAttr) transaction in this batch, if any.
                var anchor = rows.FirstOrDefault(r => !r.Transaction.ToLower().Contains("modifyattr"));

                var attrRows = rows
                                .Where(r => r.Transaction.ToLower().Contains("modifyattr")
                                            && !string.IsNullOrEmpty(r.AttributeModified))
                                .ToList();

                if (anchor != null)
                {
                    // Real transaction: build the row, attach any attribute
                    // edits performed in the same batch.
                    current = BuildAggregatedRow(anchor, anchor.Transaction);
                    result.Add(current);
                    AttachAttributes(current, attrRows);
                    continue;
                }

                // Attribute-only batch. Detect the VMI -> Inventory transfer:
                // the receiving category flips from "VMI" to "Inventory".
                bool isVmiToInventory = attrRows.Any(a =>
                    string.Equals(a.AttributeModified, "WMSReceivingCategory", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(a.AttributeOldValue, "VMI", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(a.AttributeNewValue, "Inventory", StringComparison.OrdinalIgnoreCase));

                if (isVmiToInventory)
                {
                    current = BuildAggregatedRow(attrRows.First(), "VMI to Inventory");
                    result.Add(current);
                    AttachAttributes(current, attrRows);
                }
                else if (current != null)
                {
                    // Any other standalone attribute edit folds into the
                    // previous transaction, preserving the original behavior.
                    AttachAttributes(current, attrRows);
                }
            }

            return result;
        }

        // Builds an aggregated row from a raw history row, using an explicit
        // transaction label so callers can inject a synthetic name (e.g.
        // "VMI to Inventory") while reusing the same field mapping.
        private static AggregatedLotHistoryDTO BuildAggregatedRow(LotHistory row, string transactionLabel)
        {
            return new AggregatedLotHistoryDTO
            {
                LotId = row.LotId,
                PartNumber = row.LotId.Contains("-")
                                ? row.LotId.Substring(0, row.LotId.IndexOf("-"))
                                : row.LotId,
                Transaction = transactionLabel,
                TransactionDate = row.TransactionDate,
                User = row.User,
                FromWorkflowStep = row.FromWorkflowStep,
                ToWorkflowStep = row.ToWorkflowStep,
                FromQuantity = row.FromQuantity,
                Quantity = row.Quantity,
                Shift = row.Shift,
                TargetLot = row.TargetLot,
                TargetLotQuantity = row.TargetLotQuantity,
                SourceLot = row.SourceLot,
                SourceLotQuantity = row.SourceLotQuantity,
            };
        }

        // Appends the ModifyAttr edits (kept as index-aligned parallel lists)
        // onto the given aggregated row.
        private static void AttachAttributes(AggregatedLotHistoryDTO target, IEnumerable<LotHistory> attrRows)
        {
            if (target == null)
            {
                return;
            }

            foreach (var row in attrRows)
            {
                if (string.IsNullOrEmpty(row.AttributeModified))
                {
                    continue;
                }

                target.ModifiedAttributes.Add(row.AttributeModified);
                target.NewValues.Add(row.AttributeNewValue);   // can be null
                target.OldValues.Add(row.AttributeOldValue);   // can be null
            }
        }

        // ---------------------------------------------------------------------
        // ORIGINAL implementation kept as a working reference. Superseded by the
        // grouped version above, which additionally surfaces the VMI -> Inventory
        // transfer as its own row.
        // ---------------------------------------------------------------------
        //public async Task<IEnumerable<AggregatedLotHistoryDTO>> GetLotHistoryAsync(string lotId)
        //{
        //    var history = await _camstarTransactionRepository.GetLotHistoryByLotIdAsync(lotId);

        //    var result = new List<AggregatedLotHistoryDTO>();
        //    AggregatedLotHistoryDTO current = null;

        //    foreach (var row in history.OrderBy(r => r.TransactionDate)) // keep original order
        //    {
        //        if (row.FromWorkflowStep == row.ToWorkflowStep && row.Transaction.ToLower().Contains("movelot"))
        //        {
        //            continue;
        //        }


        //        if (!row.Transaction.ToLower().Contains("modifyattr"))// Start a new row if transaction is not equal to modify attributes
        //        {

        //            current = new AggregatedLotHistoryDTO
        //            {
        //                LotId = row.LotId,
        //                PartNumber = row.LotId.Substring(0,row.LotId.IndexOf("-")),
        //                Transaction = row.Transaction,
        //                TransactionDate = row.TransactionDate,
        //                User = row.User,
        //                FromWorkflowStep = row.FromWorkflowStep,
        //                ToWorkflowStep= row.ToWorkflowStep,
        //                FromQuantity= row.FromQuantity,
        //                Quantity = row.Quantity,
        //                Shift=row.Shift,
        //                TargetLot = row.TargetLot,
        //                TargetLotQuantity=row.TargetLotQuantity,
        //                SourceLot=row.SourceLot,
        //                SourceLotQuantity =row.SourceLotQuantity,

        //};

        //            result.Add(current);
        //            continue;
        //        }

        //        // ModifyAttr → attach to previous service row
        //        if (current != null)
        //        {
        //            if (!string.IsNullOrEmpty(row.AttributeModified))
        //            {
        //                current.ModifiedAttributes.Add(row.AttributeModified);
        //                current.NewValues.Add(row.AttributeNewValue);   // can be null
        //                current.OldValues.Add(row.AttributeOldValue);   // can be null
        //            }
        //        }
        //    }

        //    return result;

        //    //return _mapper.Map<IEnumerable<LotHistoryDTO>>(history.OrderBy(o=>o.TransactionDate).ThenBy(t=>t.Transaction));
        //}
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

// AggregatedLotHistoryDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.WMS
{
    public class AggregatedLotHistoryDTO
    {
        public string LotId { get; set; }
        public string PartNumber { get; set; }
        public string Transaction { get; set; }
        public string TransactionDate { get; set; }
        public List<string> ModifiedAttributes { get; set; } 
        public List<string> NewValues { get; set; }
        public List<string> OldValues { get; set; }

        public string User { get; set; }
        public string FromWorkflowStep { get; set; }
        public string ToWorkflowStep { get; set; }
        public int? FromQuantity { get; set; }
        public int? Quantity { get; set; }
        public string Shift { get; set; }
        public string TargetLot { get; set; }
        public int? TargetLotQuantity { get; set; } 
        public string SourceLot { get; set; }
        public int? SourceLotQuantity { get; set; }

        public AggregatedLotHistoryDTO()
        {
            ModifiedAttributes = new List<string>();
            NewValues = new List<string>();
            OldValues = new List<string>();
        }

    }
}

// IWmsDashboardService.cs
using M2OSS.DTO.WMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Service.WMS.Interface
{
    public interface IWmsDashboardService
    {

        Task<int> GetMaterialCountByStep(MaterialDetailsDTO materialDto);

        Task<IEnumerable<MaterialDetailsDTO>> GetWipMaterialsAsync(MaterialDetailsDTO materialDto);
        Task<IEnumerable<AggregatedLotHistoryDTO>> GetLotHistoryAsync(string lotId);


    }
}


// High-Level Architecture
WmsDashboard.cshtml
        ↓
wmsDashboard.js
        ↓ AJAX
WmsDashboardController
        ↓
IWmsDashboardService
        ↓
WmsDashboardService
        ↓
ICamstarTransactionRepository
        ↓
Camstar SOAP API


// Classic layered architecture:
UI Layer
  ├─ Razor View
  └─ JavaScript

Web Layer
  └─ Controller

Service Layer
  └─ Business Logic

Repository Layer
  └─ Camstar Integration

External System
  └─ Camstar MES/WMS


// Dashboard - The dashboard has only three functions:

A. WIP Summary Cards

These boxes: 
    Material In-Stock
    VMI In-Stock
    Materials for Inspection
    Hold Materials

are not database tables.

They're calculated from:
    getWipMaterials()

    which calls:

    GetWipMaterials()

    which calls:

    GetWipMaterialsAsync()

which retrieves materials from Camstar.

B. WIP Table

This table:
    Part Number
    Lot Number
    Lot ID
    Operation
    Category
    Location
    Quantity

shows all active materials.

The data is: IEnumerable<MaterialDetailsDTO>

C. Lot History

Search by: Camstar Lot ID

Then 'getLotHistory()'

Calls: GetLotHistoryAndTransactionLogs()

which calls: GetLotHistoryAsync() and returns List<AggregatedLotHistoryDTO>


3. Understanding the WMS States

This is probably the MOST IMPORTANT thing to learn.

The workflow map tells the story:

    PWH_0001 = Goods Receiving

    PWH_0002 = SQE Receiving/Inspection

    PWH_0003 = Hold

    PWH_0004 = Scrap

    PWH_0005 = RTV

    PWH_0006 = Storage

    PWH_0007 = Lot assigned

    PWH_0008 = Picked

    PWH_0009 = Checked

    PWH_0010 = Issuance

    PWH_0011 = Borrowed

    PWH_0012 = Return

    PWH_0013 = IDM without HOST

    PWH_0014 = IDM with HOST

Visualized:
        Receiving
        (PWH_0001)
            ↓

        Inspection
        (PWH_0002)
            ↓

        Storage
        (PWH_0006)
            ↓

        Assigned
        (PWH_0007)
            ↓

        Picked
        (PWH_0008)
            ↓

        Checked
        (PWH_0009)
            ↓

        Issued
        (PWH_0010)


Alternative paths:
    Receiving
        ↓

    Hold
    (PWH_0003)

    Scrap
    (PWH_0004)

    RTV
    (PWH_0005)

The dashboard is mainly tracking where lots currently are in this workflow.

4. MaterialDetailsDTO is the Core WMS Object

If I were onboarding into this project, this class would be my starting point.
    MaterialDetailsDTO

represents a material lot.

Think: 1 Lot = 1 Material Record

new MaterialDetailsDTO
{
    LotId = "ABC123-001",
    PartNumber = "ABC123",
    Quantity = 500,
    Uom = "PCS",
    WorkflowStep = "PWH_0006",
    Category = "Inventory",
    ReceivingLocation = "A01"
}

Nearly every WMS module will probably use this DTO.

Examples:
    Dashboard
    Inventory
    Hold
    RTV
    Scrap
    Borrow
    Issue
    Receiving
    Inspection

all likely use: MaterialDetailsDTO

5. What the Dashboard REALLY Counts
Look at: updateWipCounts()

The KPIs are not counting lots.

They're counting: new Set(parts).size;

which means: Distinct Part Numbers

not: Distinct Lots

Example: 
    Part A
        Lot 1
        Lot 2
        Lot 3

    Part B
        Lot 4

Dashboard displays: Inventory = 2

not: Inventory = 4


6. Why GetMaterialCountByStep is Probably Legacy

Notice: //initWmsDashboardData();

is commented.

Meaning: GetMaterialCountByStep()


is no longer executed.

Instead: getWipMaterials()

loads all materials

then updateWipCounts()




computes the counts locally.

So today: GetMaterialCountByStep()

appears to be dead code.

I would verify whether any other page calls it.


7. Important Refactoring Already Happened

The biggest clue is this comment:

    Camstar records the WMS "Move VMI to Inventory"

    action as a pure SetLotAttribute call


This tells me the resigned dev struggled with a business issue.

Originally: VMI → Inventory transfer

was invisible.

Why?

Because Camstar only updated: WMSReceivingCategory

and didn't move workflow steps.

So users couldn't see the transfer in history.

The developer fixed it by creating a synthetic transaction:

    VMI to Inventory


inside: GetLotHistoryAsync()

This is likely one of the latest WMS fixes.


8. What I Would Study Next

Since more modules are being transferred to you, I'd open these next:

MaterialInventoryController

Referenced here: MaterialInventory

This is likely the largest WMS module.

SqeReceivingController

Referenced here:

C#
1
SqeReceiving


This handles inspection flow.

MaterialHoldController

Referenced here:

C#
1
MaterialHold


This handles quarantine materials.

MaterialScrapController

Referenced here:

C#
1
MaterialScrap


Terminal state.

MaterialRtvController

Referenced here:

C#
1
MaterialRtv


Return-to-vendor flow.

9. Things I Would Clean Up
Dead variable

countBySteps


inside:

C#
1
GetWipMaterials()


is calculated but never used.

Incorrect lock
C#
1
lock(countByStep)


inside:

C#
1
GetMaterialCountByStep()


does nothing useful.

Each task locks a different object.

If thread safety is needed it should lock:

countBySteps


or use:

ConcurrentBag<MaterialCountByStep>

Empty ViewModel
C#
1
WmsDashboardViewModel
2
{
3
}


appears unused.

If I had to summarize the WMS Dashboard in one sentence:

This dashboard is primarily a Camstar lot-monitoring screen that shows where material lots currently are (Receiving, Inspection, Storage, Hold, etc.) and provides a searchable transaction history for auditing lot movements and attribute changes.

Provide your feedback on BizChat



