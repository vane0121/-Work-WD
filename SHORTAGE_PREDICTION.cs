// BpaRecordDTO.cs
using System;

namespace M2OSS.DTO.DigitalWorkers
{
    public class BpaRecordDTO
    {
        public string MaterialNumber { get; set; }
        public decimal BalanceQty { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}

// GeneratePurchaseDocumentsRequestDTO.cs

namespace M2OSS.DTO.DigitalWorkers
{
    public class GeneratePurchaseDocumentsRequestDTO
    {
        public string MaterialCode { get; set; }

        public string PlantCode { get; set; }

        public string SupplierCode { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public bool CreatePurchaseOrder { get; set; }

        public string Reason { get; set; }
    }
}

// GeneratePurchaseDocumentsResultDTO.cs
namespace M2OSS.DTO.DigitalWorkers
{
    public class GeneratePurchaseDocumentsResultDTO
    {
        public string PurchaseRequisitionNumber { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string MaterialCode { get; set; }

        public string PlantCode { get; set; }

        public string SupplierCode { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }
    }
}

// IdmInventoryLotDTO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.DTO.DigitalWorkers
{
    public class IdmInventoryLotDTO
    {

        public string Workflow { get; set; }
        public string PartNumber { get; set; }
        public decimal Quantity { get; set; }
        public string LotNumber { get; set; }
        public string LotExpiryDate { get; set; }
        public string Description { get; set; }
    }
}

// PurchaseDocumentMapper.cs
using M2OSS.DTO.DigitalWorkers;
using M2OSS.Entities.DigitalWorkers;

namespace M2OSS.Mapper
{
    public static class PurchaseDocumentMapper
    {
        public static GeneratePurchaseDocumentsResultDTO ToDTO(
            PurchaseRequisition pr,
            PurchaseOrder po)
        {
            var dto = new GeneratePurchaseDocumentsResultDTO();

            if (pr != null)
            {
                dto.PurchaseRequisitionNumber = pr.RequisitionNumber;
                dto.MaterialCode = pr.MaterialCode;
                dto.PlantCode = pr.PlantCode;
                dto.SupplierCode = pr.SupplierCode;
                dto.Quantity = pr.Quantity;
                dto.UnitCost = pr.UnitCost;
            }

            if (po != null)
            {
                dto.PurchaseOrderNumber = po.PurchaseOrderNumber;

                if (dto.MaterialCode == null)
                    dto.MaterialCode = po.MaterialCode;

                if (dto.PlantCode == null)
                    dto.PlantCode = po.PlantCode;

                if (dto.SupplierCode == null)
                    dto.SupplierCode = po.SupplierCode;

                if (dto.Quantity == 0)
                    dto.Quantity = po.Quantity;

                if (dto.UnitCost == 0)
                    dto.UnitCost = po.UnitCost;
            }

            return dto;
        }
    }
}

// DigitalWorkerActionLog.cs
using System;

namespace M2OSS.Entities.DigitalWorkers
{
    public class DigitalWorkerActionLog
    {
        public string WorkerCode { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public string Target { get; set; }
        public string Status { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

// DigitalWorkerConfig.cs
using System.Collections.Generic;

namespace M2OSS.Entities.DigitalWorkers
{
    public class DigitalWorkerConfig
    {
        public int Id { get; set; }

        public string WorkerCode { get; set; }

        public string WorkerName { get; set; }

        public List<WorkerTrigger> Triggers { get; set; }

        public List<WorkerQuery> Queries { get; set; }

        public List<WorkerAnalysis> Analysis { get; set; }

        public List<WorkerDecision> Decisions { get; set; }

        public List<WorkerAction> Actions { get; set; }

        public DigitalWorkerConfig()
        {
            Triggers = new List<WorkerTrigger>();
            Queries = new List<WorkerQuery>();
            Analysis = new List<WorkerAnalysis>();
            Decisions = new List<WorkerDecision>();
            Actions = new List<WorkerAction>();
        }
    }

    public class WorkerTrigger
    {
        public string TriggerType { get; set; }

        public string TriggerCondition { get; set; }
    }

    public class WorkerQuery
    {
        public string QueryName { get; set; }

        public string SqlQuery { get; set; }

        public bool IsMainQuery { get; set; }
    }

    public class WorkerAnalysis
    {
        public string MetricName { get; set; }

        public string Formula { get; set; }
    }

    public class WorkerDecision
    {
        public string ConditionExpression { get; set; }

        public string DecisionType { get; set; }
    }

    public class WorkerAction
    {
        public string ActionType { get; set; }

        public string ActionConfig { get; set; }

        public bool Enabled { get; set; }
        public int Priority { get; set; }

    }
}

// PurchaseOrder.cs
using System;

namespace M2OSS.Entities.DigitalWorkers
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string MaterialCode { get; set; }

        public string PlantCode { get; set; }

        public string SupplierCode { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public string SourcePurchaseRequisitionNumber { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public decimal TotalValue
        {
            get { return Quantity * UnitCost; }
        }

        public PurchaseOrder()
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}

// PurchaseRequisition.cs
using System;

namespace M2OSS.Entities.DigitalWorkers
{
    public class PurchaseRequisition
    {
        public Guid Id { get; set; }

        public string RequisitionNumber { get; set; }

        public string MaterialCode { get; set; }

        public string PlantCode { get; set; }

        public string SupplierCode { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public string Reason { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public decimal TotalValue
        {
            get { return Quantity * UnitCost; }
        }

        public PurchaseRequisition()
        {
            Id = Guid.NewGuid();
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}

// WorkerExecutionAudit.cs
using System;

namespace M2OSS.Entities.DigitalWorkers
{
    public class WorkerExecutionAudit
    {
        public int Id { get; set; }

        public string WorkerCode { get; set; }

        public string Status { get; set; }

        public string Summary { get; set; }

        // INPUT
        public string Payload { get; set; }

        // OUTPUT
        public string Result { get; set; }

        public string CorrelationId { get; set; }

        public string RequestedBy { get; set; }

        public DateTimeOffset ExecutedAtUtc { get; set; }

        public WorkerExecutionAudit()
        {
            ExecutedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}

// WorkerExecutionContext.cs
using Newtonsoft.Json.Linq;
using System;

namespace M2OSS.Entities.DigitalWorkers
{
    public class WorkerExecutionContext
    {
        public string WorkerCode { get; }
        public JObject Payload { get; }
        public string RequestedBy { get; }
        public string CorrelationId { get; }
        public DateTimeOffset RequestedAt { get; }

        public WorkerExecutionContext(
            string workerCode,
            JObject payload,
            string requestedBy = null,
            string correlationId = null,
            DateTimeOffset? requestedAt = null)
        {
            WorkerCode = workerCode;
            Payload = payload ?? new JObject();
            RequestedBy = requestedBy ?? "SYSTEM";
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            RequestedAt = requestedAt ?? DateTimeOffset.UtcNow;
        }

        public T GetPayload<T>() where T : new()
        {
            if (Payload == null || !Payload.HasValues)
                return new T();

            return Payload.ToObject<T>();
        }
    }
}

// WorkerExecutionResult.cs
namespace M2OSS.Entities.DigitalWorkers
{
    public class WorkerExecutionResult
    {
        public string WorkerCode { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public string CorrelationId { get; set; }
        public object Data { get; set; }

        public static WorkerExecutionResult Success(string workerCode, string summary, object data = null, string correlationId = null)
        {
            return new WorkerExecutionResult
            {
                WorkerCode = workerCode,
                Status = "Success",
                Summary = summary,
                Data = data,
                CorrelationId = correlationId
            };
        }

        public static WorkerExecutionResult NoAction(string workerCode, string summary, object data = null, string correlationId = null)
        {
            return new WorkerExecutionResult
            {
                WorkerCode = workerCode,
                Status = "NoAction",
                Summary = summary,
                Data = data,
                CorrelationId = correlationId
            };
        }

        public static WorkerExecutionResult Failed(string workerCode, string summary, object data = null, string correlationId = null)
        {
            return new WorkerExecutionResult
            {
                WorkerCode = workerCode,
                Status = "Failed",
                Summary = summary,
                Data = data,
                CorrelationId = correlationId
            };
        }
    }
}

// IDigitalWorkerActionLogRepository.cs
using M2OSS.Entities.DigitalWorkers;
using System;
using System.Collections.Generic;

namespace M2OSS.Repository.DigitalWorkers.Interface
{
    public interface IDigitalWorkerActionLogRepository
    {
        void InsertLog(string workerCode, string actionType, string description, string target, string status, string correlationId);
        List<DigitalWorkerActionLog> GetRecentLogs(string workerCode, int limit = 50);
        bool HasProcessedOracleFile(string workerCode, string executionKey);
        bool HasSuccessfulReportToday(string workerCode, DateTime today);
        bool TryAcquireExecutionLock(string workerCode, DateTime runDate, string correlationId);
        void ReleaseExecutionLock(string workerCode, DateTime runDate);
    }
}

// IExecutionAuditRepository.cs
using M2OSS.Entities.DigitalWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M2OSS.Repository.DigitalWorkers.Interface
{
    public interface IExecutionAuditRepository
    {
        void InsertAudit(WorkerExecutionAudit audit);
        List<WorkerExecutionAudit> GetAll();
        List<WorkerExecutionAudit> GetRecentExecutions(string workerCode, int top = 10);
    }
}

// IPurchaseDocumentRepository.cs
using M2OSS.Entities.DigitalWorkers;
using System.Data;

namespace M2OSS.Repository.DigitalWorkers.Interface
{
    public interface IPurchaseDocumentRepository
    {
        void InsertPurchaseRequisition(PurchaseRequisition pr, IDbTransaction tx = null);
        void InsertPurchaseOrder(PurchaseOrder po, IDbTransaction tx = null);
    }
}

// IWorkerConfigurationRepository.cs
using M2OSS.Entities.DigitalWorkers;
using System.Collections;
using System.Collections.Generic;

namespace M2OSS.Repository.DigitalWorkers.Interface
{
    public interface IWorkerConfigurationRepository
    {
        DigitalWorkerConfig GetWorker(string workerCode);
        bool IsWorkerEnabled(string workerCode);
        IEnumerable<WorkerQuery> GetWorkerQueries(string workerCode);
    }
}

// DigitalWorkerActionLogRepository.cs
using Dapper;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace M2OSS.Repository.DigitalWorkers.Repository
{
    public class DigitalWorkerActionLogRepository : IDigitalWorkerActionLogRepository
    {
        private readonly IDbConnection _connection;

        public DigitalWorkerActionLogRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void InsertLog(string workerCode, string actionType, string description, string target, string status, string correlationId)
        {
            var sql = @"
                INSERT INTO DigitalWorkerActionLog
                (WorkerCode, ActionType, Description, Target, Status, CorrelationId)
                VALUES
                (@WorkerCode, @ActionType, @Description, @Target, @Status, @CorrelationId)";

            _connection.Execute(sql, new
            {
                WorkerCode = workerCode,
                ActionType = actionType,
                Description = description,
                Target = target,
                Status = status,
                CorrelationId = correlationId
            });
        }

        public List<DigitalWorkerActionLog> GetRecentLogs(string workerCode, int limit = 50)
        {
            var sql = @"
                SELECT TOP (@Limit) *
                FROM DigitalWorkerActionLog
                WHERE WorkerCode = @WorkerCode
                ORDER BY CreatedAt DESC";

            return _connection.Query<DigitalWorkerActionLog>(sql, new
            {
                WorkerCode = workerCode,
                Limit = limit
            }).ToList();
        }

        public bool HasProcessedOracleFile(string workerCode, string executionKey)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM DigitalWorkerActionLog
                WHERE WorkerCode = @WorkerCode
                  AND Target = 'OracleQOHFile'
                  AND Status = 'SUCCESS'
                  AND Description LIKE '%' + @ExecutionKey + '%'
            ";

            return _connection.ExecuteScalar<int>(sql, new
            {
                WorkerCode = workerCode,
                ExecutionKey = executionKey
            }) > 0;
        }

        public bool HasSuccessfulReportToday(string workerCode, DateTime today)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM DigitalWorkerActionLog
                WHERE WorkerCode = @WorkerCode
                  AND Target = 'Report'
                  AND Status = 'SUCCESS'
                  AND CAST(CreatedAt AS DATE) = CAST(@Today AS DATE)
            ";

            return _connection.ExecuteScalar<int>(sql, new
            {
                WorkerCode = workerCode,
                Today = today
            }) > 0;
        }
        public bool TryAcquireExecutionLock(string workerCode, DateTime runDate, string correlationId)
        {
            const string sql = @"
        INSERT INTO DigitalWorkerExecutionLock
            (WorkerCode, LockDate, LockedBy)
        VALUES
            (@WorkerCode, @LockDate, @LockedBy)
    ";

            try
            {
                _connection.Execute(sql, new
                {
                    WorkerCode = workerCode,
                    LockDate = runDate.Date,
                    LockedBy = correlationId
                });

                return true; 
            }
            catch
            {
                return false; 
            }
        }

        public void ReleaseExecutionLock(string workerCode, DateTime runDate)
        {
            const string sql = @"
                DELETE FROM DigitalWorkerExecutionLock
                WHERE WorkerCode = @WorkerCode
                  AND LockDate = @LockDate
            ";

            _connection.Execute(sql, new
            {
                WorkerCode = workerCode,
                LockDate = runDate.Date
            });
        }
    }
}

// ExecutionAuditRepository.cs
using Dapper;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace M2OSS.Repository.DigitalWorkers.Repository
{
    public class ExecutionAuditRepository : IExecutionAuditRepository
    {
        private readonly IDbConnection _connection;

        public ExecutionAuditRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void InsertAudit(WorkerExecutionAudit audit)
        {
            var sql = @"
            INSERT INTO DigitalWorkerExecutionLogs
            (WorkerCode, Status, Summary, Result, CorrelationId, RequestedBy, ExecutedAtUtc)
            VALUES
            (@WorkerCode, @Status, @Summary, @Result, @CorrelationId, @RequestedBy, @ExecutedAtUtc)";

            _connection.Execute(sql, audit);
        }

        public List<WorkerExecutionAudit> GetAll()
        {
            var sql = @"SELECT * FROM DigitalWorkerExecutionLogs ORDER BY ExecutedAtUtc DESC";
            return _connection.Query<WorkerExecutionAudit>(sql).ToList();
        }

        // New method to fetch recent executions for a specific worker
        public List<WorkerExecutionAudit> GetRecentExecutions(string workerCode, int top = 10)
        {
            var sql = @"
            SELECT TOP(@Top) *
            FROM DigitalWorkerExecutionLogs 
            WHERE WorkerCode = @WorkerCode
            ORDER BY ExecutedAtUtc DESC";

            return _connection.Query<WorkerExecutionAudit>(sql, new { WorkerCode = workerCode, Top = top }).ToList();
        }
    }
}

// PurchaseDocumentRepository.cs
using Dapper;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using System.Data;

namespace M2OSS.Repository.DigitalWorkers.Repository
{
    public class PurchaseDocumentRepository : IPurchaseDocumentRepository
    {
        private readonly IDbConnection _connection;

        public PurchaseDocumentRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void InsertPurchaseRequisition(PurchaseRequisition pr, IDbTransaction tx = null)
        {
            _connection.Execute(
                @"INSERT INTO MockPurchaseRequisition
            (
                Id,
                RequisitionNumber,
                MaterialCode,
                PlantCode,
                SupplierCode,
                Quantity,
                UnitCost,
                Reason,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @RequisitionNumber,
                @MaterialCode,
                @PlantCode,
                @SupplierCode,
                @Quantity,
                @UnitCost,
                @Reason,
                @CreatedAtUtc
            )",
                pr,
                tx);
        }

        public void InsertPurchaseOrder(PurchaseOrder po, IDbTransaction tx = null)
        {
            _connection.Execute(
                @"INSERT INTO MockPurchaseOrder
            (
                Id,
                PurchaseOrderNumber,
                MaterialCode,
                PlantCode,
                SupplierCode,
                Quantity,
                UnitCost,
                SourcePurchaseRequisitionNumber,
                CreatedAtUtc
            )
            VALUES
            (
                @Id,
                @PurchaseOrderNumber,
                @MaterialCode,
                @PlantCode,
                @SupplierCode,
                @Quantity,
                @UnitCost,
                @SourcePurchaseRequisitionNumber,
                @CreatedAtUtc
            )",
                po,
                tx);
        }
    }
}

// WorkerConfigurationRepository.cs
using Dapper;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace M2OSS.Repository.DigitalWorkers.Repository
{
    public class WorkerConfigurationRepository : IWorkerConfigurationRepository
    {
        private readonly IDbConnection _connection;

        public WorkerConfigurationRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public DigitalWorkerConfig GetWorker(string workerCode)
        {
            // Query the main worker table
            var worker = _connection.QueryFirstOrDefault<DigitalWorkerConfig>(
                @"SELECT * 
                  FROM DigitalWorkers
                  WHERE WorkerCode = @WorkerCode",
                new { WorkerCode = workerCode });

            if (worker == null)
                return null;

            // Query related triggers (if you use triggers)
            worker.Triggers = _connection.Query<WorkerTrigger>(
                @"SELECT * 
                  FROM DigitalWorkerTriggers
                  WHERE WorkerId = @WorkerId",
                new { WorkerId = worker.Id }).ToList();

            // Query related SQL queries
            worker.Queries = _connection.Query<WorkerQuery>(
                @"SELECT * 
                  FROM DigitalWorkerQueries
                  WHERE WorkerId = @WorkerId",
                new { WorkerId = worker.Id }).ToList();

            // Query analysis definitions
            worker.Analysis = _connection.Query<WorkerAnalysis>(
                @"SELECT * 
                  FROM DigitalWorkerAnalysis
                  WHERE WorkerId = @WorkerId",
                new { WorkerId = worker.Id }).ToList();

            // Query decision rules
            worker.Decisions = _connection.Query<WorkerDecision>(
                @"SELECT * 
                  FROM DigitalWorkerDecisions
                  WHERE WorkerId = @WorkerId",
                new { WorkerId = worker.Id }).ToList();

            // Query worker actions
            worker.Actions = _connection.Query<WorkerAction>(
                @"SELECT * 
                  FROM DigitalWorkerActions
                  WHERE WorkerId = @WorkerId",
                new { WorkerId = worker.Id }).ToList();

            return worker;
        }

        public IEnumerable<WorkerQuery> GetWorkerQueries(string workerCode)
        {
            var queries = _connection.Query<WorkerQuery>(
                @"
                SELECT q.QueryName, q.SqlQuery, q.IsMainQuery
                FROM DigitalWorkerQueries q
                INNER JOIN DigitalWorkers w ON q.WorkerId = w.Id
                WHERE w.WorkerCode = @WorkerCode
                ORDER BY q.Id
                ",
                        new { WorkerCode = workerCode }
            );

            return queries;
        }

        public bool IsWorkerEnabled(string workerCode)
        {
            var sql = @"
                SELECT IsEnabled
                FROM DigitalWorkers
                WHERE WorkerCode = @WorkerCode
            ";

            return _connection.ExecuteScalar<bool>(sql, new { WorkerCode = workerCode });
        }
    }
}

// DigitalWorkersController.cs
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Service.DigitalWorkers;
using M2OSS.Service.DigitalWorkers.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace M2OSS.Web.Controllers.DigitalWorkers
{
    public class DigitalWorkersController : BaseController
    {
        private readonly IDigitalWorkerExecutorService _executor;

        public DigitalWorkersController(IDigitalWorkerExecutorService executor)
        {
            _executor = executor;
        }

        [HttpPost]
        public async Task<JsonResult> ExecuteWorker(string workerCode, JObject payload)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workerCode))
                {
                    return Json(new
                    {
                        success = false,
                        message = "WorkerCode is required."
                    });
                }

                payload = payload ?? new JObject();

                var context = new WorkerExecutionContext(workerCode, payload);
                var result = await _executor.ExecuteAsync(workerCode, context);

                // Return detailed worker info like .NET 8
                return Json(new
                {
                    workerCode = result.WorkerCode,
                    status = result.Status,
                    summary = result.Summary,
                    correlationId = result.CorrelationId,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    workerCode = workerCode,
                    status = "Failed",
                    summary = ex.Message,
                    correlationId = Guid.NewGuid().ToString(),
                    data = (object)null
                });
            }
        }
    }
}

// IAutoPrPoGenerationService.cs
using M2OSS.DTO.DigitalWorkers;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IAutoPrPoGenerationService
    {
        Task<GeneratePurchaseDocumentsResultDTO> GenerateAsync(GeneratePurchaseDocumentsRequestDTO request);
    }
}

// IDigitalWorkerExecutorService.cs
using M2OSS.Entities.DigitalWorkers;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IDigitalWorkerExecutorService
    {
        Task<WorkerExecutionResult> ExecuteAsync(string workerCode, WorkerExecutionContext context);
    }
}

// IDigitalWorkerRegistryService.cs
namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IDigitalWorkerRegistryService
    {
        IDigitalWorkerService GetWorker(string workerCode);
    }
}

// IDigitalWorkerService.cs
using M2OSS.Entities.DigitalWorkers;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IDigitalWorkerService
    {
        // ALL workers implement this.
        string WorkerCode { get; }
        Task<WorkerExecutionResult> ExecuteAsync(WorkerExecutionContext context);
    }
}

// AutoPrPoGenerationService.cs
using M2OSS.DTO.DigitalWorkers;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Mapper;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Repository.DigitalWorkers.Repository;
using M2OSS.Service.DigitalWorkers.Interface;
using System;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class AutoPrPoGenerationService : IAutoPrPoGenerationService
    {
        private readonly IPurchaseDocumentRepository _repository;

        public AutoPrPoGenerationService(IPurchaseDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<GeneratePurchaseDocumentsResultDTO> GenerateAsync(GeneratePurchaseDocumentsRequestDTO request)
        {
            // CREATE PR
            var pr = new PurchaseRequisition
            {
                RequisitionNumber = "PR-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                MaterialCode = request.MaterialCode,
                PlantCode = request.PlantCode,
                SupplierCode = request.SupplierCode,
                Quantity = request.Quantity,
                UnitCost = request.UnitCost,
                Reason = request.Reason
            };

            _repository.InsertPurchaseRequisition(pr);

            // CREATE PO (OPTIONAL)
            PurchaseOrder po = null;

            if (request.CreatePurchaseOrder)
            {
                po = new PurchaseOrder
                {
                    PurchaseOrderNumber = "PO-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                    MaterialCode = request.MaterialCode,
                    PlantCode = request.PlantCode,
                    SupplierCode = request.SupplierCode,
                    Quantity = request.Quantity,
                    UnitCost = request.UnitCost,
                    SourcePurchaseRequisitionNumber = pr.RequisitionNumber
                };

                _repository.InsertPurchaseOrder(po);
            }

            // RETURN RESULT
            var result = PurchaseDocumentMapper.ToDTO(pr, po);

            return await Task.FromResult(result);
        }
    }
}

// DigitalWorkerExecutorService.cs
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Service.DigitalWorkers.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class DigitalWorkerExecutorService : IDigitalWorkerExecutorService
    {
        private readonly IDigitalWorkerRegistryService _registryService;
        private readonly IExecutionAuditRepository _auditRepository;

        public DigitalWorkerExecutorService(
            IDigitalWorkerRegistryService registryService,
            IExecutionAuditRepository auditRepository)
        {
            _registryService = registryService;
            _auditRepository = auditRepository;
        }

        // MAIN EXECUTION (with context)
        public async Task<WorkerExecutionResult> ExecuteAsync(string workerCode, WorkerExecutionContext context)
        {
            //var worker = _registryService.GetWorker(workerCode);

            if (!_workerCache.TryGetValue(workerCode, out var worker))
            {
                worker = _registryService.GetWorker(workerCode);
                _workerCache[workerCode] = worker;
            }

            if (worker == null)
                throw new Exception($"Worker {workerCode} not found");

            var result = await worker.ExecuteAsync(context);

            await SaveAudit(result, context);

            return result;
        }

        // Temporary, delete after test
        private readonly Dictionary<string, IDigitalWorkerService> _workerCache = new Dictionary<string, IDigitalWorkerService>();


        // OPTIONAL: Payload-based execution (like old version)
        public async Task<WorkerExecutionResult> ExecuteAsync(string workerCode, JObject payload)
        {
            var context = new WorkerExecutionContext(
                workerCode,
                payload,
                "SYSTEM",
                Guid.NewGuid().ToString()
            );

            return await ExecuteAsync(workerCode, context);
        }

        // OPTIONAL: Backward compatibility (no payload)
        public async Task<WorkerExecutionResult> ExecuteAsync(string workerCode)
        {
            return await ExecuteAsync(workerCode, new JObject());
        }

        // CENTRALIZED AUDIT LOGGING
        private async Task SaveAudit(WorkerExecutionResult result, WorkerExecutionContext context)
        {
            var resultJson = JsonConvert.SerializeObject(result);

            _auditRepository.InsertAudit(new WorkerExecutionAudit
            {
                WorkerCode = result.WorkerCode,
                Status = result.Status,
                Summary = result.Summary,
                Result = resultJson,
                CorrelationId = context.CorrelationId,
                RequestedBy = context.RequestedBy,
                ExecutedAtUtc = DateTime.UtcNow
            });

            await Task.CompletedTask;
        }
    }
}

// DigitalWorkerRegistryService.cs
using M2OSS.Service.DigitalWorkers.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class DigitalWorkerRegistryService : IDigitalWorkerRegistryService
    {
        private readonly IEnumerable<Lazy<IDigitalWorkerService>> _workers;

        public DigitalWorkerRegistryService(IEnumerable<Lazy<IDigitalWorkerService>> workers)
        {
            _workers = workers;
        }

        public IDigitalWorkerService GetWorker(string workerCode)
        {
            return _workers
            .Select(w => w.Value)
            .FirstOrDefault(w => w.WorkerCode == workerCode);
        }
    }
}

// ExpiryRiskService.cs
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Service.DigitalWorkers.Interface;
using System.Threading.Tasks;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class ExpiryRiskService : IDigitalWorkerService
    {
        public string WorkerCode => "EXPIRY_RISK";

        public Task<WorkerExecutionResult> ExecuteAsync(WorkerExecutionContext context)
        {
            return Task.FromResult(
                WorkerExecutionResult.Success(WorkerCode, "No logic yet")
            );
        }
    }
}

// ShortagePredictionService.cs
using M2OSS.DTO.Common;
using M2OSS.DTO.DigitalWorkers;
using M2OSS.DTO.Material;
using M2OSS.Entities.DigitalWorkers;
using M2OSS.Entities.WMS;
using M2OSS.Repository.Camstar.Interface;
using M2OSS.Repository.Common.Interface;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Repository.DV.Interface;
using M2OSS.Repository.IDMInventory;
using M2OSS.Service.DigitalWorkers.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class ShortagePredictionService : IDigitalWorkerService
    {
        private readonly IWorkerConfigurationRepository _configRepo;
        private readonly IAutoPrPoGenerationService _generationService;
        private readonly IDigitalWorkerActionLogRepository _actionLogRepo;
        private readonly ICamstarTransactionRepository _camstarRepo;
        private readonly IEmailService _emailService;
        private readonly IMaterialPlanningRepository _materialPlanningRepo;
        private readonly IIdmRepository _idmRepository;
        private readonly IVmiInventoryProviderService _phoVmiService;
        private readonly IThoSftpVmiInventoryProviderService _thoVmiService;
        private readonly IDvRepository _dvRepository;
        private DigitalWorkerConfig _cachedConfig;
        private DateTime _lastConfigLoad = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private readonly IBpaFileProviderService _bpaService;

        public string WorkerCode => "SHORTAGE_PREDICTION";

        public ShortagePredictionService(
            IWorkerConfigurationRepository configRepo,
            IAutoPrPoGenerationService generationService,
            IDigitalWorkerActionLogRepository actionLogRepo,
            ICamstarTransactionRepository camstarRepo,
            IEmailService emailService,
            IMaterialPlanningRepository materialPlanningRepo,
            IIdmRepository idmRepository,
            IVmiInventoryProviderService phoVmiService,
            IThoSftpVmiInventoryProviderService thoVmiService,
            IDvRepository dvRepository,
            IBpaFileProviderService bpaService)
        {
            _configRepo = configRepo;
            _generationService = generationService;
            _actionLogRepo = actionLogRepo;
            _camstarRepo = camstarRepo;
            _emailService = emailService;
            _materialPlanningRepo = materialPlanningRepo;
            _idmRepository = idmRepository;
            _phoVmiService = phoVmiService;
            _thoVmiService = thoVmiService;
            _dvRepository = dvRepository;
            _bpaService = bpaService;
        }

        private class InventoryLot
        {
            public decimal Qty { get; set; }
            public System.DateTime? Expiry { get; set; }
        }

        public async Task<WorkerExecutionResult> ExecuteAsync(WorkerExecutionContext context)
        {
            var correlationId = context.CorrelationId ?? Guid.NewGuid().ToString();

            try
            {
                if (_cachedConfig == null || DateTime.Now - _lastConfigLoad > _cacheDuration)
                {
                    _cachedConfig = _configRepo.GetWorker(WorkerCode)
                        ?? throw new InvalidOperationException("Worker configuration not found.");

                    _lastConfigLoad = DateTime.Now;
                }

                var config = _cachedConfig;

                var plantCode = context.Payload?["PlantCode"]?.ToString();
                if (string.IsNullOrWhiteSpace(plantCode))
                    throw new InvalidOperationException("PlantCode is required.");

                var queryConfig = config.Queries
                    .Select(q => new { Json = JObject.Parse(q.SqlQuery) })
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Json["PlantCode"]?.ToString(),
                            plantCode,
                            StringComparison.OrdinalIgnoreCase));

                if (queryConfig == null)
                    throw new InvalidOperationException($"No query defined for PlantCode '{plantCode}'");

                var querySettings = queryConfig.Json;
                var mode = ResolveMode(querySettings);

                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "MODE",
                    $"Running in {mode}",
                    plantCode,
                    "INFO",
                    correlationId
                );

                var sourceSystem =
                    context.Payload?["SourceSystem"]?.ToString();

                if (mode == ExecutionMode.Production)
                {
                    if (string.Equals(
                        sourceSystem,
                        "SMRP_Dashboard",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return await ExecuteWithRealDataManualAsync(context);
                    }

                    return await ExecuteWithRealDataAsync(context);
                }
                else
                {
                    return await ExecuteDemoLegacy(context);
                }
            }
            catch (Exception ex)
            {
                return WorkerExecutionResult.Failed(
                    WorkerCode,
                    ex.Message,
                    correlationId);
            }
        }

        public async Task<WorkerExecutionResult> ExecuteWithRealDataAsync(WorkerExecutionContext context)
        {
            var correlationId = context.CorrelationId ?? Guid.NewGuid().ToString();

            try
            {
                if (_cachedConfig == null || DateTime.Now - _lastConfigLoad > _cacheDuration)
                {
                    var start = DateTime.Now;

                    _cachedConfig = _configRepo.GetWorker(WorkerCode) ?? throw new InvalidOperationException("Worker configuration not found.");

                    _lastConfigLoad = DateTime.Now;

                    _actionLogRepo.InsertLog(WorkerCode, "PERF", $"Config load took {(DateTime.Now - start).TotalSeconds}s", "SYSTEM", "INFO", Guid.NewGuid().ToString());
                }

                var config = _cachedConfig;

                var analysisConfigs = config.Analysis.Where(a => ShortageAnalysisOrder.Contains(a.MetricName)).ToDictionary(a => a.MetricName, a => a.Formula);

                if (analysisConfigs.Count != ShortageAnalysisOrder.Length) throw new InvalidOperationException("Incomplete analysis configuration.");

                var plantCode = context.Payload?["PlantCode"]?.ToString();
                var materialList = new List<string>();
                var materialsArray = context.Payload?["Materials"] as JArray;

                if (materialsArray != null && materialsArray.Any())
                {
                    materialList = materialsArray.Select(x => x["MaterialCode"]?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }

                var singleMaterial = context.Payload?["MaterialCode"]?.ToString();

                if (!string.IsNullOrWhiteSpace(singleMaterial))
                {
                    materialList.Add(singleMaterial);
                }

                materialList = materialList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (string.IsNullOrWhiteSpace(plantCode)) throw new InvalidOperationException("PlantCode is required.");

                var queryConfig = config.Queries.Select(q => new { q, Json = JObject.Parse(q.SqlQuery) }).FirstOrDefault(x => string.Equals(x.Json["PlantCode"]?.ToString(), plantCode, StringComparison.OrdinalIgnoreCase));

                if (queryConfig == null) throw new InvalidOperationException($"No query defined for PlantCode '{plantCode}'");

                var querySettings = queryConfig.Json;

                bool useMockStock = querySettings["Inventory"]?["UseMockStock"]?.ToObject<bool>() ?? false;
                bool useMock = querySettings["Supply"]?["UseMockSupply"]?.ToObject<bool>() ?? false;
                bool simulationMode = querySettings["Execution"]?["SimulationMode"]?.ToObject<bool>() ?? true;
                bool isDebugFromConfig = querySettings["Execution"]?["DebugMode"]?.ToObject<bool>() ?? false;
                bool isDebugFromPayload = context.Payload?["Debug"]?.ToObject<bool>() ?? false;
                bool isDebugMode = isDebugFromConfig || isDebugFromPayload;
                bool scanAllMaterials = querySettings["Execution"]?["ScanAllMaterials"]?.ToObject<bool>() ?? true;
                string testMaterial = querySettings["Execution"]?["TestMaterial"]?.ToString();

                testMaterial = NormalizeMaterial(testMaterial);

                if (!scanAllMaterials &&
                    string.IsNullOrWhiteSpace(testMaterial))
                {
                    throw new InvalidOperationException("ScanAllMaterials is FALSE but TestMaterial is not configured.");
                }

                var workflowSteps = queryConfig.Json["WorkflowSteps"]?.ToObject<List<string>>() ?? new List<string>();

                if (!workflowSteps.Any()) throw new InvalidOperationException("WorkflowSteps missing.");

                decimal leadTimeDays = queryConfig.Json["LeadTimeDays"]?.ToObject<decimal>() ?? 0m;

                Dictionary<string, List<InventoryLot>> inventoryLotMap;
                Dictionary<string, string> materialDescriptionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (plantCode == "THO")
                {
                    var inventory = await _idmRepository.GetAllInventoryAsync();

                    materialDescriptionMap = inventory.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");

                    inventoryLotMap = inventory.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.Select(x =>
                    {
                        System.DateTime? expiryParsed = null;

                        if (!string.IsNullOrWhiteSpace(x.LotExpiryDate))
                        {
                            if (DateTimeOffset.TryParse(x.LotExpiryDate, out var dto))
                            {
                                expiryParsed = dto.UtcDateTime;
                            }
                        }

                        return new InventoryLot
                        {
                            Qty = x.Quantity,
                            Expiry = expiryParsed
                        };
                    }).ToList()
                     );
                }
                else
                {
                    var xml = new XDocument(new XElement("Request", new XElement("operationNumber", workflowSteps.First())));
                    var rawResult = await _camstarRepo.GetMaterialLotsByFilterAsync(new MaterialDetails
                    {
                        WorkflowStep = workflowSteps.First(),
                        PartNumber = ""
                    }, xml);

                    _actionLogRepo.InsertLog(WorkerCode, "CAMSTAR_RAW_COUNT", $"Returned rows = {(rawResult != null ? rawResult.Count() : 0)}", plantCode, "INFO", correlationId);

                    if (rawResult != null)
                    {
                        materialDescriptionMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");

                        foreach (var sample in rawResult.Take(5))
                        {
                            _actionLogRepo.InsertLog(WorkerCode, "CAMSTAR_SAMPLE", $"Part={sample.PartNumber}, Qty={sample.Quantity}, Exp={sample.ExpirationDate}", plantCode, "DEBUG", correlationId);
                        }
                    }

                    materialDescriptionMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");
                    inventoryLotMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.Select(l => new InventoryLot
                    {
                        Qty = (decimal)l.Quantity,
                        Expiry = l.ExpirationDate
                    }).ToList());
                }

                if (useMockStock)
                {
                    var mockSupply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                    inventoryLotMap = mockSupply.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key)).ToDictionary(kvp => NormalizeMaterial(kvp.Key), kvp => new List<InventoryLot> { new InventoryLot { Qty = kvp.Value.Stock, Expiry = null } });
                    _actionLogRepo.InsertLog(WorkerCode, "MOCK_STOCK_OVERRIDE", $"Using ONLY mock stock. Count={inventoryLotMap.Keys.Count}", plantCode, "INFO", correlationId);
                }

                string source;

                if (useMockStock)
                {
                    source = "Mock";
                }
                else if (plantCode == "THO")
                {
                    source = "IDM";
                }
                else
                {
                    source = "Camstar";
                }

                _actionLogRepo.InsertLog(WorkerCode, "INFO", $"Evaluated {inventoryLotMap.Keys.Count} materials from {source} inventory", plantCode, "INFO", correlationId);

                if (!scanAllMaterials)
                {
                    string debugMaterial = testMaterial;
                    inventoryLotMap = inventoryLotMap.Where(x => x.Key.Equals(debugMaterial, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);

                    _actionLogRepo.InsertLog(WorkerCode, "DEBUG_FILTER", $"Test material filter applied: {debugMaterial}", plantCode, "INFO", correlationId);

                    if (materialList.Any())
                    {
                        inventoryLotMap = inventoryLotMap.Where(kvp => materialList.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        _actionLogRepo.InsertLog(WorkerCode, "FILTER_APPLIED", $"Filtered to {inventoryLotMap.Count} materials from input payload", plantCode, "INFO", correlationId);
                    }
                }

                var orgMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                 {
                     { "PHO", "MPHPHO" },
                     { "THO", "MTHTHO" }
                 };

                string org = orgMapping.ContainsKey(plantCode) ? orgMapping[plantCode] : plantCode;

                var planningProfiles = _materialPlanningRepo.GetPlanningProfiles(inventoryLotMap.Keys, org) ?? new Dictionary<string, MaterialPlanningProfileDTO>();

                if (isDebugMode)
                {
                    _actionLogRepo.InsertLog(WorkerCode, "PLANNING_PROFILE_COUNT", $"Loaded Planning Profiles: {planningProfiles.Count}", plantCode, "INFO", correlationId);

                    foreach (var kvp in planningProfiles.Take(10))
                    {
                        var p = kvp.Value;
                        _actionLogRepo.InsertLog(WorkerCode, "PLANNING_PROFILE_SAMPLE", $"Material={p.MaterialCode}, " + $"FreqType={p.IssuanceFrequencyType}, " + $"FreqValue={p.IssuanceFrequencyDays}, " + $"SafetyDays={p.SafetyDays}, " + $"SupplierCountry={p.SupplierType}", plantCode, "DEBUG", correlationId);
                    }
                }

                Dictionary<string, OpenPoInfo> openPoMap = new Dictionary<string, OpenPoInfo>();
                Dictionary<string, decimal> vmiMap = new Dictionary<string, decimal>();

                if (useMock)
                {
                    var supply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                    openPoMap = supply.GroupBy(x => NormalizeMaterial(x.Key)).ToDictionary(g => g.Key, g => new OpenPoInfo
                    {
                        Quantity = g.Sum(x => x.Value.OpenPo),
                        PddDate = null
                    });

                    _actionLogRepo.InsertLog(WorkerCode, "MOCK_OPENPO", "Using MOCK Open PO", plantCode, "INFO", correlationId);
                }
                else
                {
                    try
                    {
                        if (scanAllMaterials)
                        {
                            openPoMap = await _dvRepository.GetOpenPoQuantitiesAsync(plantCode, inventoryLotMap.Keys.ToList());
                            _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO", $"Loaded Open PO from TDV. Count={openPoMap.Count}", plantCode, "INFO", correlationId);
                        }
                        else
                        {
                            openPoMap = await _dvRepository.GetOpenPoQuantitiesAsync(plantCode, inventoryLotMap.Keys.ToList());
                            openPoMap = openPoMap.Where(x => x.Key.Equals(testMaterial, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                            _actionLogRepo.InsertLog(WorkerCode, "DEBUG_OPENPO_FILTER", $"OpenPO entries after filter = {openPoMap.Count}", testMaterial, "INFO", correlationId);
                            _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO", $"Loaded Open PO from TDV. Count={openPoMap.Count}", plantCode, "INFO", correlationId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO_ERROR", $"Fallback to MOCK due to error: {ex.Message}", plantCode, "FAILED", correlationId);

                        var supply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                        openPoMap = supply.GroupBy(x => NormalizeMaterial(x.Key)).ToDictionary(g => g.Key, g => new OpenPoInfo
                        {
                            Quantity = g.Sum(x => x.Value.OpenPo),
                            PddDate = null
                        });
                    }
                }

                if (plantCode == "THO")
                {
                    vmiMap = _thoVmiService.GetVmiQuantities(org);

                    if (!scanAllMaterials)
                    {
                        vmiMap = vmiMap.Where(x => x.Key.Equals(testMaterial, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                    }
                    else
                    {
                        vmiMap = vmiMap.GroupBy(kvp => NormalizeMaterial(kvp.Key)).ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
                        _actionLogRepo.InsertLog(WorkerCode, "VMI_SOURCE", $"Loaded VMI from SFTP (THO). Count={vmiMap.Count}", plantCode, "INFO", correlationId);
                    }

                    if (!scanAllMaterials)
                    {
                        foreach (var vmi in vmiMap)
                        {
                            _actionLogRepo.InsertLog(WorkerCode, "DEBUG_VMI_ENTRY", $"Material={vmi.Key}, Qty={vmi.Value}", vmi.Key, "INFO", correlationId);

                        }
                    }
                }
                else
                {
                    var vmiFolder = querySettings["VMI"]?["LocalFolder"]?.ToString();
                    vmiMap = _phoVmiService.GetVmiQuantities(org, vmiFolder);

                    if (!scanAllMaterials)
                    {
                        vmiMap = vmiMap.Where(x => x.Key.Equals(testMaterial, StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                    }
                    else
                    {
                        vmiMap = vmiMap
                              .GroupBy(kvp => NormalizeMaterial(kvp.Key))
                              .ToDictionary(
                                  g => g.Key,
                                  g => g.Sum(x => x.Value)
                              );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "VMI_SOURCE",
                            $"Loaded VMI from SharePoint (PHO). Count={vmiMap.Count}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }

                    if (!scanAllMaterials)
                    {
                        foreach (var vmi in vmiMap)
                        {
                            _actionLogRepo.InsertLog(
                                WorkerCode,
                                "DEBUG_VMI_ENTRY",
                                $"Material={vmi.Key}, Qty={vmi.Value}",
                                vmi.Key,
                                "INFO",
                                correlationId
                            );
                        }
                    }
                }

                var shortageItems = new List<object>();
                var shortageList = new List<Dictionary<string, object>>();

                Dictionary<string, dynamic> bpaMap = new Dictionary<string, dynamic>();

                if (plantCode == "THO")
                {
                    var bpaRaw = _bpaService.GetBpa(correlationId, isDebugMode);

                    bpaMap = bpaRaw
                    .GroupBy(x => NormalizeMaterial(x.MaterialNumber))
                    .ToDictionary(
                        g => g.Key,
                        g => (dynamic)new
                        {
                            Qty = g.Sum(x => x.BalanceQty),
                            Expiry = g.Max(x => x.ExpiryDate)
                        }
                    );

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "BPA_LOADED",
                            $"Loaded BPA materials: {bpaMap.Count}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }
                }
                else
                {
                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "BPA_SKIPPED",
                        "BPA quantity not available for PHO",
                        plantCode,
                        "INFO",
                        correlationId
                    );
                }

                int debugCount = 0;

                foreach (var item in inventoryLotMap)
                {
                    string materialCode = NormalizeMaterial(item.Key);
                    string materialDescription = "";
                    materialDescriptionMap.TryGetValue(
                        materialCode,
                        out materialDescription);
                    var lots = item.Value;
                    decimal stockQty = lots.Sum(l => l.Qty);

                    MaterialPlanningProfileDTO profile;
                    planningProfiles.TryGetValue(materialCode, out profile);

                    if (isDebugMode && debugCount < 20)
                    {
                        debugCount++;

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_PROFILE_MATCH",
                            profile != null
                                ? $"Material={materialCode}, " +
                                  $"FreqType={profile.IssuanceFrequencyType}, " +
                                  $"FreqValue={profile.IssuanceFrequencyDays}, " +
                                  $"SafetyDays={profile.SafetyDays}, " +
                                  $"SupplierCountry={profile.SupplierType}"
                                : $"Material={materialCode}, NO PROFILE FOUND",
                            materialCode,
                            profile != null ? "INFO" : "WARNING",
                            correlationId
                        );
                    }

                    bool hasValidProfile =
                        profile != null &&
                        profile.SafetyDays.HasValue &&
                        profile.SafetyDays.Value > 0;

                    decimal issuanceFreq =
                        profile?.IssuanceFrequencyDays > 0
                            ? profile.IssuanceFrequencyDays
                            : 1;
                    decimal safetyDays = profile?.SafetyDays ?? 0;
                    decimal openPoQty = 0m;
                    System.DateTime? poPddDate = null;
                    System.DateTime? bpaValidTo = null;
                    decimal bpaQty = 0m;
                    decimal vmiQty = 0m;
                    OpenPoInfo poInfo;
                    bool hasPoData = openPoMap.TryGetValue(materialCode, out poInfo);

                    if (hasPoData && poInfo != null)
                    {
                        openPoQty = poInfo.Quantity;
                        poPddDate = poInfo.PddDate;
                        bpaValidTo = poInfo.BpaValidTo;
                    }
                    else
                    {
                        openPoQty = -1;
                    }

                    if (bpaMap.TryGetValue(materialCode, out var bpa))
                    {
                        bpaQty = bpa.Qty;
                        bpaValidTo = bpa.Expiry;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "DEBUG_BPA",
                        $"Material={materialCode}, Qty={bpaQty}, Expiry={bpaValidTo}",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    vmiMap.TryGetValue(materialCode, out vmiQty);

                    if (isDebugMode && shortageItems.Count < 10)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "TRACE_SUPPLY_SAMPLE",
                            $"Material={materialCode} | Stock={stockQty} | OpenPO={openPoQty} | VMI={vmiQty}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                      WorkerCode,
                      "DEBUG_OPENPO",
                      $"Material={materialCode}, OpenPO={openPoQty}",
                      plantCode,
                      "INFO",
                      correlationId
                  );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_VMI",
                            $"Material={materialCode}, VMI={vmiQty}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var calcContext = new Dictionary<string, object>
                     {
                         { "StockQty", stockQty },
                         { "OpenPOQty", openPoQty < 0 ? 0 : openPoQty },
                         { "VmiQty", vmiQty },
                         { "BpaQty", bpaQty },
                         { "IssuanceFrequencyDays", issuanceFreq },
                         { "SafetyDays", safetyDays },
                         { "LeadTimeDays", leadTimeDays },
                         { "TODAY", System.DateTime.Today },
                         { "PO_PddDate", poPddDate ?? (object)DBNull.Value }
                     };

                    foreach (var metric in ShortageAnalysisOrder)
                    {
                        var result = EvaluateAnalysisFormula(analysisConfigs[metric], calcContext);
                        calcContext[metric] = result;
                    }

                    decimal avgDaily =
                        Convert.ToDecimal(calcContext["AvgDailyConsumption"]);

                    decimal availableSupply =
                        Convert.ToDecimal(calcContext["AvailableSupply"]);

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "DEBUG_AVAILABLE_SUPPLY",
                        $"AvailableSupply={availableSupply} (Stock={stockQty}, OpenPO={openPoQty}, VMI={vmiQty}, BPA={bpaQty})",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    decimal daysOfSupply;

                    if (avgDaily > 0)
                    {
                        daysOfSupply = Convert.ToDecimal(calcContext["DaysOfSupply"]);
                    }
                    else
                    {
                        daysOfSupply = 0;
                    }

                    decimal reorderPoint =
                        Convert.ToDecimal(calcContext["ReorderPoint"]);

                    decimal shortageDays = Convert.ToDecimal(calcContext["ShortageDate"]);
                    System.DateTime shortageDate = System.DateTime.Today.AddDays((double)shortageDays);
                    System.DateTime shortageDate_before = shortageDate;

                    bool hasPO = openPoQty > 0;
                    bool hasValidBPA = bpaQty > 0 && bpaValidTo.HasValue && bpaValidTo.Value > DateTime.Today;
                    bool poLate = poPddDate.HasValue && shortageDate < poPddDate.Value;
                    bool expiryTriggered = lots.Any(l => l.Expiry.HasValue && l.Expiry.Value < shortageDate);

                    var usableLots = lots
                        .Where(l =>
                            !l.Expiry.HasValue
                            || l.Expiry.Value >= shortageDate
                        )
                        .ToList();

                    decimal usableStock = usableLots.Sum(l => l.Qty);
                    decimal expiredBeforeUse = lots
                        .Where(l => l.Expiry.HasValue && l.Expiry.Value < shortageDate)
                        .Sum(l => l.Qty);

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "EXPIRY_IMPACT",
                        $"Material={materialCode}, OriginalStock={stockQty}, RemovedBeforeUse={expiredBeforeUse}, FinalStock={usableStock}",
                        materialCode,
                        "INFO",
                        correlationId
                    );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "EXPIRY_AWARE_ADJUSTMENT",
                            $"Material={materialCode}, RemovedBeforeUse={expiredBeforeUse}, UsableStock={usableStock}, InitialShortage={shortageDate}",
                            materialCode,
                            "INFO",
                            correlationId
                        );

                    }

                    // SECOND calculation (after expiry adjustment)
                    calcContext["StockQty"] = usableStock;

                    foreach (var metric in ShortageAnalysisOrder)
                    {
                        var result = EvaluateAnalysisFormula(analysisConfigs[metric], calcContext);
                        calcContext[metric] = result;
                    }

                    string scenario = ResolveScenario(
                        openPoQty,
                        usableStock,
                        Convert.ToDecimal(calcContext["DaysOfSupply"]),
                        safetyDays,
                        poPddDate,
                        shortageDate,
                        bpaValidTo
                    );

                    if (scenario == "SAFE")
                    {
                        if (isDebugMode)
                        {
                            _actionLogRepo.InsertLog(
                               WorkerCode,
                               "SAFE_SKIPPED",
                               $"Material={materialCode}, DOS={calcContext["DaysOfSupply"]}, SafetyDays={safetyDays}",
                               materialCode,
                               "INFO",
                               correlationId
                           );
                        }

                        continue;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                         WorkerCode,
                         "SCENARIO",
                         $"Material={materialCode}, Scenario={scenario}, DOS={calcContext["DaysOfSupply"]}, SafetyDays={safetyDays}",
                         materialCode,
                         "INFO",
                         correlationId);
                    }

                    if (profile == null ||
                         !profile.SafetyDays.HasValue ||
                         profile.SafetyDays.Value <= 0)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "EXPIRED_NO_PROFILE",
                            $"Material={materialCode} | OriginalStock={stockQty} | UsableStock={usableStock} | ExpiredRemoved={expiredBeforeUse}",
                            materialCode,
                            "WARNING",
                            correlationId
                        );

                        // SCENARIO 6
                        if (usableStock <= 0)
                        {
                            shortageList.Add(new Dictionary<string, object>
                             {
                                { "Material", materialCode },
                                { "Description", materialDescription },

                                { "Status", "EXPIRED + NO PROFILE" },

                                { "StockQty", stockQty },
                                { "OpenPoQty", openPoQty < 0 ? 0 : openPoQty },
                                { "VmiQty", vmiQty },
                                { "BpaQty", bpaQty },

                                { "PoPddDate", poPddDate?.ToString("yyyy-MM-dd") ?? "" },
                                { "BpaExpirationDate", bpaValidTo?.ToString("yyyy-MM-dd") ?? "" },

                                { "ExpiryDate", string.Join(";",
                                        lots.Where(l => l.Expiry.HasValue)
                                            .Select(l => l.Expiry.Value.ToString("yyyy-MM-dd")))
                                },

                                { "RfqLeadTime", 0 },
                                { "BpaLeadTime", 0 },
                                { "PrLeadTime", 0 },
                                { "SupplierLeadTime", leadTimeDays },

                                { "DOS", 0 },

                                { "Action", "Review Planning Profile" },

                                { "TriggerDate", DateTime.Today.ToString("MMM dd") },
                                { "DeliveryDate", "" },
                                { "ShortageDate", "" },

                                { "Reason", "Expired stock with missing planning profile" }
                             });

                            // start: delete if issue did not resolve
                            shortageItems.Add(new
                            {
                                item = materialCode,
                                status = "EXPIRED + NO PROFILE",
                                action = "Review Planning Profile",
                                metrics = new
                                {
                                    AvailableSupply = usableStock
                                },
                                actionResults = new List<object>()
                            });
                            // end: delete if issue did not resolve

                            // start: uncomment if above code works
                            //shortageItems.Add(new
                            //{
                            //    item = materialCode,
                            //    metrics = new { AvailableSupply = usableStock },
                            //    actionResults = new List<object>()
                            //});
                            // end: uncomment if above code works
                        }

                        continue;
                    }

                    avgDaily = Convert.ToDecimal(calcContext["AvgDailyConsumption"]);
                    availableSupply = Convert.ToDecimal(calcContext["AvailableSupply"]);

                    daysOfSupply = avgDaily > 0
                        ? Convert.ToDecimal(calcContext["DaysOfSupply"])
                        : 0;

                    reorderPoint = Convert.ToDecimal(calcContext["ReorderPoint"]);

                    if (!scanAllMaterials)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "VALIDATION",
                            $"Material={materialCode} | " +
                            $"Stock={stockQty} | " +
                            $"OpenPO={openPoQty} | " +
                            $"VMI={vmiQty} | " +
                            $"BPA={bpaQty} | " +
                            $"IssuanceFreq={issuanceFreq} | " +
                            $"SafetyDays={safetyDays} | " +
                            $"PO_PDD={(poPddDate.HasValue ? poPddDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"BPAExpiry={(bpaValidTo.HasValue ? bpaValidTo.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"AvailableSupply={availableSupply} | " +
                            $"AvgDaily={avgDaily} | " +
                            $"DOS={daysOfSupply} | " +
                            $"ShortageDate={shortageDate:yyyy-MM-dd} | " +
                            $"DeliveryDate={(poPddDate.HasValue ? poPddDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"Scenario={scenario}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var metrics = new Dictionary<string, object>
                     {
                         { "AvgDailyConsumption", avgDaily },
                         { "AvailableSupply", availableSupply },
                         { "DaysOfSupply", daysOfSupply },
                         { "ReorderPoint", reorderPoint },
                         { "SafetyDays", profile.SafetyDays ?? 0 }
                     };

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SHORTAGE_DATE_IMPACT",
                        $"Material={materialCode}, Before={shortageDate_before:yyyy-MM-dd}, After={shortageDate:yyyy-MM-dd}",
                        materialCode,
                        "INFO",
                        correlationId
                    );
                        _actionLogRepo.InsertLog(
                          WorkerCode,
                          "DEBUG_PO_PDD",
                          $"Material={materialCode}, PDD={poPddDate}, Shortage={shortageDate}",
                          materialCode,
                          "INFO",
                          correlationId
                      );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_FULL_CONTEXT",
                            $"Material={materialCode}, Stock={stockQty}, OpenPO={(openPoQty < 0 ? 0 : openPoQty)}, VMI={vmiQty}, PDD={poPddDate}, BPA={bpaValidTo}, Shortage={shortageDate}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var rowDict = new Dictionary<string, object>
                     {
                         { "ItemId", materialCode },
                         { "StockQty", stockQty },
                         { "AvgDailyConsumption", avgDaily },
                         { "AvailableSupply", availableSupply },
                         { "DaysOfSupply", daysOfSupply },
                         { "ReorderPoint", reorderPoint },
                         { "LeadTimeDays", leadTimeDays },
                         { "PlantCode", plantCode },
                         { "OpenPOQty", openPoQty < 0 ? 0 : openPoQty },
                         { "ShortageDate", shortageDate },
                         { "PO_PddDate", poPddDate.HasValue ? (object)poPddDate.Value : DBNull.Value },
                         { "BpaValidTo", bpaValidTo.HasValue ? (object)bpaValidTo.Value : DBNull.Value },
                     };

                    rowDict["Scenario"] = scenario;

                    rowDict["TODAY"] = System.DateTime.Today;
                    bool triggered = false;
                    var actionResults = new List<GeneratePurchaseDocumentsResultDTO>();

                    foreach (var decision in config.Decisions)
                    {
                        if (decision == null || string.IsNullOrWhiteSpace(decision.ConditionExpression))
                        {
                            continue;
                        }

                        var decodedExpression = System.Net.WebUtility.HtmlDecode(decision.ConditionExpression);

                        bool decisionResult =
                            EvaluateCondition(decodedExpression, metrics, rowDict, isDebugMode);

                        if (isDebugMode)
                        {
                            _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DECISION_EVALUATED",
                            $"{decodedExpression} => {decisionResult}",
                            materialCode,
                            decisionResult ? "TRUE" : "FALSE",
                            correlationId
                        );
                        }

                        bool forceTrigger =
                            scenario == "CRITICAL" ||
                            scenario == "EXPIRY" ||
                            scenario == "PO_LATE" ||
                            scenario == "BPA_PR" ||
                            scenario == "RFQ";

                        if (!decisionResult)
                            continue;

                        triggered = true;

                        var matchedActions = config.Actions
                            .Where(a =>
                            {
                                if (string.IsNullOrWhiteSpace(a.ActionConfig))
                                    return false;

                                try
                                {
                                    var cfg = JObject.Parse(a.ActionConfig);
                                    var decisionType = cfg["DecisionType"]?.ToString();

                                    return string.Equals(
                                        decisionType,
                                        decision.DecisionType,
                                        StringComparison.OrdinalIgnoreCase);
                                }
                                catch
                                {
                                    return false;
                                }
                            })
                            .ToList();

                        actionResults.AddRange(
                            await ExecuteActions(
                                matchedActions,
                                rowDict,
                                correlationId,
                                simulationMode));
                    }

                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "ACTION_SUMMARY",
                        $"Material={materialCode} | Actions={(actionResults.Any() ? "EXECUTED" : "NONE")}",
                        materialCode,
                        actionResults.Any() ? "EXECUTED" : "NONE",
                        correlationId
                    );

                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SUMMARY",
                        $"Material={materialCode} | Supply={availableSupply} | Reorder={reorderPoint} | Shortage={(availableSupply < reorderPoint)}",
                        materialCode,
                        triggered ? "ACTION" : "NO_ACTION",
                        correlationId
                    );

                    if (!triggered)
                        continue;

                    string displayStatus;
                    string actionText;

                    switch (scenario)
                    {
                        // SCENARIO 7
                        case "CRITICAL":
                            displayStatus = "EXPIRY + PO LATE";
                            actionText = "Expedite delivery + Review expired stock";
                            break;

                        // SCENARIO 1
                        case "RFQ":
                            displayStatus = "NO PO + NO BPA";
                            actionText = "Create RFQ";
                            break;

                        //SCENARIO 2
                        case "BPA_PR":
                            displayStatus = "BPA AVAILABLE";
                            actionText = "Create PR";
                            break;

                        // SCENARIO 3
                        case "EXPIRY":
                            displayStatus = "EXPIRY";
                            actionText = "Create RFQ";
                            break;

                        // SCENARIO 4
                        case "PO_LATE":
                            displayStatus = "PO EXISTS (LATE)";
                            actionText = "Expedite delivery";
                            break;

                        // SCENARIO 5
                        case "LOW_STOCK":
                            displayStatus = "LOW STOCK";
                            actionText = "Monitor";
                            break;

                        // SCENARIO 8
                        case "SAFE":
                            displayStatus = "HEALTHY STOCK";
                            actionText = "Monitor";
                            break;

                        default:
                            displayStatus = scenario;
                            actionText = "";
                            break;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "REPORT_ROW",
                        $"Material={materialCode}, Scenario={scenario}, Status={displayStatus}, Action={actionText}",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    string triggerStr = DateTime.Today.ToString("MMM dd");
                    string shortageStr = shortageDate.ToString("MMM dd");
                    string deliveryStr = poPddDate.HasValue
                        ? poPddDate.Value.ToString("MMM dd")
                        : "";

                    shortageList.Add(new Dictionary<string, object>
                    {
                        { "Material", materialCode },
                        { "Description", materialDescription },
                        { "Status", displayStatus },

                        { "StockQty", stockQty },
                        { "OpenPoQty", openPoQty < 0 ? 0 : openPoQty },
                        { "VmiQty", vmiQty },
                        { "BpaQty", bpaQty },

                        { "PoPddDate", poPddDate?.ToString("yyyy-MM-dd") ?? "" },
                        { "BpaExpirationDate", bpaValidTo?.ToString("yyyy-MM-dd") ?? "" },

                        { "ExpiryDate", string.Join(";", lots
                            .Where(l => l.Expiry.HasValue)
                            .Select(l => l.Expiry.Value.ToString("yyyy-MM-dd"))) },

                        { "RfqLeadTime", 0 },
                        { "BpaLeadTime", 0 },
                        { "PrLeadTime", 0 },
                        { "SupplierLeadTime", leadTimeDays },

                        { "DOS", Math.Round(daysOfSupply, 0) },
                        { "Action", actionText },

                        { "TriggerDate", triggerStr },
                        { "DeliveryDate", deliveryStr },
                        { "ShortageDate", shortageStr }
                    });

                    shortageItems.Add(new
                    {
                        item = materialCode,
                        metrics,
                        actionResults
                    });
                }

                if (shortageList.Any())
                    await SendShortageSummaryEmail(
                        config.Actions,
                        shortageList,
                        correlationId,
                        plantCode);

                return shortageItems.Any()
                    ? WorkerExecutionResult.Success(
                        WorkerCode,
                        "Shortage prediction completed",
                        new { totalItems = shortageItems.Count, items = shortageItems })
                    : WorkerExecutionResult.NoAction(
                        WorkerCode,
                        "No shortages detected");
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "EXECUTION_ERROR",
                    $"Execution failure: {ex.Message}",
                    context.Payload?["PlantCode"]?.ToString(),
                    "ERROR",
                    correlationId
                );

                return WorkerExecutionResult.Failed(
                    WorkerCode,
                    ex.Message,
                    correlationId);
            }
        }

        public async Task<WorkerExecutionResult> ExecuteWithRealDataManualAsync(WorkerExecutionContext context)
        {
            var correlationId = context.CorrelationId ?? Guid.NewGuid().ToString();

            try
            {
                if (_cachedConfig == null || DateTime.Now - _lastConfigLoad > _cacheDuration)
                {
                    var start = DateTime.Now;

                    _cachedConfig = _configRepo.GetWorker(WorkerCode) ?? throw new InvalidOperationException("Worker configuration not found.");

                    _lastConfigLoad = DateTime.Now;

                    _actionLogRepo.InsertLog(WorkerCode, "PERF", $"Config load took {(DateTime.Now - start).TotalSeconds}s", "SYSTEM", "INFO", Guid.NewGuid().ToString());
                }

                var config = _cachedConfig;

                var analysisConfigs = config.Analysis.Where(a => ShortageAnalysisOrder.Contains(a.MetricName)).ToDictionary(a => a.MetricName, a => a.Formula);

                if (analysisConfigs.Count != ShortageAnalysisOrder.Length) throw new InvalidOperationException("Incomplete analysis configuration.");

                var plantCode = context.Payload?["PlantCode"]?.ToString();
                var requestedByEmail = context.Payload?["RequestedByEmail"]?.ToString();
                var materialList = new List<string>();
                var materialsArray = context.Payload?["Materials"] as JArray;

                if (materialsArray != null && materialsArray.Any())
                {
                    materialList = materialsArray.Select(x => x["MaterialCode"]?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }

                var singleMaterial = context.Payload?["MaterialCode"]?.ToString();

                if (!string.IsNullOrWhiteSpace(singleMaterial))
                {
                    materialList.Add(singleMaterial);
                }

                materialList = materialList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (string.IsNullOrWhiteSpace(plantCode)) throw new InvalidOperationException("PlantCode is required.");

                var queryConfig = config.Queries.Select(q => new { q, Json = JObject.Parse(q.SqlQuery) }).FirstOrDefault(x => string.Equals(x.Json["PlantCode"]?.ToString(), plantCode, StringComparison.OrdinalIgnoreCase));

                if (queryConfig == null) throw new InvalidOperationException($"No query defined for PlantCode '{plantCode}'");

                var querySettings = queryConfig.Json;

                bool useMockStock = querySettings["Inventory"]?["UseMockStock"]?.ToObject<bool>() ?? false;
                bool useMock = querySettings["Supply"]?["UseMockSupply"]?.ToObject<bool>() ?? false;
                bool simulationMode = querySettings["Execution"]?["SimulationMode"]?.ToObject<bool>() ?? true;
                bool isDebugFromConfig = querySettings["Execution"]?["DebugMode"]?.ToObject<bool>() ?? false;
                bool isDebugFromPayload = context.Payload?["Debug"]?.ToObject<bool>() ?? false;
                bool isDebugMode = isDebugFromConfig || isDebugFromPayload;
                bool scanAllMaterials = context.Payload?["ScanAllMaterials"]?.ToObject<bool>() ?? false;
                var targetMaterials = materialList.Select(NormalizeMaterial).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (!scanAllMaterials && !targetMaterials.Any())
                {
                    throw new InvalidOperationException("No materials supplied for manual execution.");
                }

                var workflowSteps = queryConfig.Json["WorkflowSteps"]?.ToObject<List<string>>() ?? new List<string>();

                if (!workflowSteps.Any()) throw new InvalidOperationException("WorkflowSteps missing.");

                decimal leadTimeDays = queryConfig.Json["LeadTimeDays"]?.ToObject<decimal>() ?? 0m;

                Dictionary<string, List<InventoryLot>> inventoryLotMap;
                Dictionary<string, string> materialDescriptionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (plantCode == "THO")
                {
                    var inventory = await _idmRepository.GetAllInventoryAsync();

                    materialDescriptionMap = inventory.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");

                    inventoryLotMap = inventory.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.Select(x =>
                    {
                        System.DateTime? expiryParsed = null;

                        if (!string.IsNullOrWhiteSpace(x.LotExpiryDate))
                        {
                            if (DateTimeOffset.TryParse(x.LotExpiryDate, out var dto))
                            {
                                expiryParsed = dto.UtcDateTime;
                            }
                        }

                        return new InventoryLot
                        {
                            Qty = x.Quantity,
                            Expiry = expiryParsed
                        };
                    }).ToList()
                     );
                }
                else
                {
                    var xml = new XDocument(new XElement("Request", new XElement("operationNumber", workflowSteps.First())));
                    var rawResult = await _camstarRepo.GetMaterialLotsByFilterAsync(new MaterialDetails
                    {
                        WorkflowStep = workflowSteps.First(),
                        PartNumber = ""
                    }, xml);

                    _actionLogRepo.InsertLog(WorkerCode, "CAMSTAR_RAW_COUNT", $"Returned rows = {(rawResult != null ? rawResult.Count() : 0)}", plantCode, "INFO", correlationId);

                    if (rawResult != null)
                    {
                        materialDescriptionMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");

                        foreach (var sample in rawResult.Take(5))
                        {
                            _actionLogRepo.InsertLog(WorkerCode, "CAMSTAR_SAMPLE", $"Part={sample.PartNumber}, Qty={sample.Quantity}, Exp={sample.ExpirationDate}", plantCode, "DEBUG", correlationId);
                        }
                    }

                    materialDescriptionMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.First().Description ?? "");
                    inventoryLotMap = rawResult.Where(x => !string.IsNullOrWhiteSpace(x.PartNumber)).GroupBy(x => NormalizeMaterial(x.PartNumber)).ToDictionary(g => g.Key, g => g.Select(l => new InventoryLot
                    {
                        Qty = (decimal)l.Quantity,
                        Expiry = l.ExpirationDate
                    }).ToList());
                }

                if (useMockStock)
                {
                    var mockSupply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                    inventoryLotMap = mockSupply.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key)).ToDictionary(kvp => NormalizeMaterial(kvp.Key), kvp => new List<InventoryLot> { new InventoryLot { Qty = kvp.Value.Stock, Expiry = null } });
                    _actionLogRepo.InsertLog(WorkerCode, "MOCK_STOCK_OVERRIDE", $"Using ONLY mock stock. Count={inventoryLotMap.Keys.Count}", plantCode, "INFO", correlationId);
                }

                string source;

                if (useMockStock)
                {
                    source = "Mock";
                }
                else if (plantCode == "THO")
                {
                    source = "IDM";
                }
                else
                {
                    source = "Camstar";
                }

                _actionLogRepo.InsertLog(WorkerCode, "INFO", $"Evaluated {inventoryLotMap.Keys.Count} materials from {source} inventory", plantCode, "INFO", correlationId);

                if (!scanAllMaterials)
                {
                    inventoryLotMap = inventoryLotMap.Where(x => targetMaterials.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);

                    _actionLogRepo.InsertLog(WorkerCode, "MANUAL_FILTER", $"Filtered to {inventoryLotMap.Count} selected materials", plantCode, "INFO", correlationId);
                }

                var orgMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                   {
                       { "PHO", "MPHPHO" },
                       { "THO", "MTHTHO" }
                   };

                string org = orgMapping.ContainsKey(plantCode) ? orgMapping[plantCode] : plantCode;

                var planningProfiles = _materialPlanningRepo.GetPlanningProfiles(inventoryLotMap.Keys, org) ?? new Dictionary<string, MaterialPlanningProfileDTO>();

                if (isDebugMode)
                {
                    _actionLogRepo.InsertLog(WorkerCode, "PLANNING_PROFILE_COUNT", $"Loaded Planning Profiles: {planningProfiles.Count}", plantCode, "INFO", correlationId);

                    foreach (var kvp in planningProfiles.Take(10))
                    {
                        var p = kvp.Value;
                        _actionLogRepo.InsertLog(WorkerCode, "PLANNING_PROFILE_SAMPLE", $"Material={p.MaterialCode}, " + $"FreqType={p.IssuanceFrequencyType}, " + $"FreqValue={p.IssuanceFrequencyDays}, " + $"SafetyDays={p.SafetyDays}, " + $"SupplierCountry={p.SupplierType}", plantCode, "DEBUG", correlationId);
                    }
                }

                Dictionary<string, OpenPoInfo> openPoMap = new Dictionary<string, OpenPoInfo>();
                Dictionary<string, decimal> vmiMap = new Dictionary<string, decimal>();

                if (useMock)
                {
                    var supply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                    openPoMap = supply.GroupBy(x => NormalizeMaterial(x.Key)).ToDictionary(g => g.Key, g => new OpenPoInfo
                    {
                        Quantity = g.Sum(x => x.Value.OpenPo),
                        PddDate = null
                    });

                    _actionLogRepo.InsertLog(WorkerCode, "MOCK_OPENPO", "Using MOCK Open PO", plantCode, "INFO", correlationId);
                }
                else
                {
                    try
                    {
                        if (scanAllMaterials)
                        {
                            openPoMap = await _dvRepository.GetOpenPoQuantitiesAsync(plantCode, inventoryLotMap.Keys.ToList());
                            _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO", $"Loaded Open PO from TDV. Count={openPoMap.Count}", plantCode, "INFO", correlationId);
                        }
                        else
                        {
                            openPoMap = await _dvRepository.GetOpenPoQuantitiesAsync(plantCode, inventoryLotMap.Keys.ToList());
                            openPoMap = openPoMap.Where(x => targetMaterials.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                            _actionLogRepo.InsertLog(WorkerCode, "DEBUG_OPENPO_FILTER", $"OpenPO entries after filter = {openPoMap.Count}", string.Join(",", targetMaterials), "INFO", correlationId);
                            _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO", $"Loaded Open PO from TDV. Count={openPoMap.Count}", plantCode, "INFO", correlationId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _actionLogRepo.InsertLog(WorkerCode, "TDV_OPENPO_ERROR", $"Fallback to MOCK due to error: {ex.Message}", plantCode, "FAILED", correlationId);

                        var supply = _materialPlanningRepo.GetMockMaterialSupply(plantCode);
                        openPoMap = supply.GroupBy(x => NormalizeMaterial(x.Key)).ToDictionary(g => g.Key, g => new OpenPoInfo
                        {
                            Quantity = g.Sum(x => x.Value.OpenPo),
                            PddDate = null
                        });
                    }
                }

                if (plantCode == "THO")
                {
                    vmiMap = _thoVmiService.GetVmiQuantities(org);

                    if (!scanAllMaterials)
                    {
                        vmiMap = vmiMap.Where(x => targetMaterials.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                    }
                    else
                    {
                        vmiMap = vmiMap.GroupBy(kvp => NormalizeMaterial(kvp.Key)).ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
                        _actionLogRepo.InsertLog(WorkerCode, "VMI_SOURCE", $"Loaded VMI from SFTP (THO). Count={vmiMap.Count}", plantCode, "INFO", correlationId);
                    }

                    if (!scanAllMaterials)
                    {
                        foreach (var vmi in vmiMap)
                        {
                            _actionLogRepo.InsertLog(WorkerCode, "DEBUG_VMI_ENTRY", $"Material={vmi.Key}, Qty={vmi.Value}", vmi.Key, "INFO", correlationId);

                        }
                    }
                }
                else
                {
                    var vmiFolder = querySettings["VMI"]?["LocalFolder"]?.ToString();
                    vmiMap = _phoVmiService.GetVmiQuantities(org, vmiFolder);

                    if (!scanAllMaterials)
                    {
                        vmiMap = vmiMap.Where(x => targetMaterials.Contains(x.Key, StringComparer.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value);
                    }
                    else
                    {
                        vmiMap = vmiMap
                              .GroupBy(kvp => NormalizeMaterial(kvp.Key))
                              .ToDictionary(
                                  g => g.Key,
                                  g => g.Sum(x => x.Value)
                              );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "VMI_SOURCE",
                            $"Loaded VMI from SharePoint (PHO). Count={vmiMap.Count}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }

                    if (!scanAllMaterials)
                    {
                        foreach (var vmi in vmiMap)
                        {
                            _actionLogRepo.InsertLog(
                                WorkerCode,
                                "DEBUG_VMI_ENTRY",
                                $"Material={vmi.Key}, Qty={vmi.Value}",
                                vmi.Key,
                                "INFO",
                                correlationId
                            );
                        }
                    }
                }

                var shortageItems = new List<object>();
                var shortageList = new List<Dictionary<string, object>>();

                Dictionary<string, dynamic> bpaMap = new Dictionary<string, dynamic>();

                if (plantCode == "THO")
                {
                    var bpaRaw = _bpaService.GetBpa(correlationId, isDebugMode);

                    bpaMap = bpaRaw
                    .GroupBy(x => NormalizeMaterial(x.MaterialNumber))
                    .ToDictionary(
                        g => g.Key,
                        g => (dynamic)new
                        {
                            Qty = g.Sum(x => x.BalanceQty),
                            Expiry = g.Max(x => x.ExpiryDate)
                        }
                    );

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "BPA_LOADED",
                            $"Loaded BPA materials: {bpaMap.Count}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }
                }
                else
                {
                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "BPA_SKIPPED",
                        "BPA quantity not available for PHO",
                        plantCode,
                        "INFO",
                        correlationId
                    );
                }

                int debugCount = 0;

                foreach (var item in inventoryLotMap)
                {
                    string materialCode = NormalizeMaterial(item.Key);
                    string materialDescription = "";
                    materialDescriptionMap.TryGetValue(
                        materialCode,
                        out materialDescription);
                    var lots = item.Value;
                    decimal stockQty = lots.Sum(l => l.Qty);

                    MaterialPlanningProfileDTO profile;
                    planningProfiles.TryGetValue(materialCode, out profile);

                    if (isDebugMode && debugCount < 20)
                    {
                        debugCount++;

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_PROFILE_MATCH",
                            profile != null
                                ? $"Material={materialCode}, " +
                                  $"FreqType={profile.IssuanceFrequencyType}, " +
                                  $"FreqValue={profile.IssuanceFrequencyDays}, " +
                                  $"SafetyDays={profile.SafetyDays}, " +
                                  $"SupplierCountry={profile.SupplierType}"
                                : $"Material={materialCode}, NO PROFILE FOUND",
                            materialCode,
                            profile != null ? "INFO" : "WARNING",
                            correlationId
                        );
                    }

                    bool hasValidProfile =
                        profile != null &&
                        profile.SafetyDays.HasValue &&
                        profile.SafetyDays.Value > 0;

                    decimal issuanceFreq =
                        profile?.IssuanceFrequencyDays > 0
                            ? profile.IssuanceFrequencyDays
                            : 1;
                    decimal safetyDays = profile?.SafetyDays ?? 0;
                    decimal openPoQty = 0m;
                    System.DateTime? poPddDate = null;
                    System.DateTime? bpaValidTo = null;
                    decimal bpaQty = 0m;
                    decimal vmiQty = 0m;

                    OpenPoInfo poInfo;
                    bool hasPoData = openPoMap.TryGetValue(materialCode, out poInfo);

                    if (hasPoData && poInfo != null)
                    {
                        openPoQty = poInfo.Quantity;
                        poPddDate = poInfo.PddDate;
                        bpaValidTo = poInfo.BpaValidTo;
                    }
                    else
                    {
                        openPoQty = -1;
                    }

                    if (bpaMap.TryGetValue(materialCode, out var bpa))
                    {
                        bpaQty = bpa.Qty;
                        bpaValidTo = bpa.Expiry;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "DEBUG_BPA",
                        $"Material={materialCode}, Qty={bpaQty}, Expiry={bpaValidTo}",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    vmiMap.TryGetValue(materialCode, out vmiQty);

                    if (isDebugMode && shortageItems.Count < 10)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "TRACE_SUPPLY_SAMPLE",
                            $"Material={materialCode} | Stock={stockQty} | OpenPO={openPoQty} | VMI={vmiQty}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                      WorkerCode,
                      "DEBUG_OPENPO",
                      $"Material={materialCode}, OpenPO={openPoQty}",
                      plantCode,
                      "INFO",
                      correlationId
                  );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_VMI",
                            $"Material={materialCode}, VMI={vmiQty}",
                            plantCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var calcContext = new Dictionary<string, object>
                   {
                       { "StockQty", stockQty },
                       { "OpenPOQty", openPoQty < 0 ? 0 : openPoQty },
                       { "VmiQty", vmiQty },
                       { "BpaQty", bpaQty },
                       { "IssuanceFrequencyDays", issuanceFreq },
                       { "SafetyDays", safetyDays },
                       { "LeadTimeDays", leadTimeDays },
                       { "TODAY", System.DateTime.Today },
                       { "PO_PddDate", poPddDate ?? (object)DBNull.Value }
                   };

                    foreach (var metric in ShortageAnalysisOrder)
                    {
                        var result = EvaluateAnalysisFormula(analysisConfigs[metric], calcContext);
                        calcContext[metric] = result;
                    }

                    decimal avgDaily =
                        Convert.ToDecimal(calcContext["AvgDailyConsumption"]);

                    decimal availableSupply =
                        Convert.ToDecimal(calcContext["AvailableSupply"]);

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "DEBUG_AVAILABLE_SUPPLY",
                        $"AvailableSupply={availableSupply} (Stock={stockQty}, OpenPO={openPoQty}, VMI={vmiQty}, BPA={bpaQty})",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    decimal daysOfSupply;

                    if (avgDaily > 0)
                    {
                        daysOfSupply = Convert.ToDecimal(calcContext["DaysOfSupply"]);
                    }
                    else
                    {
                        daysOfSupply = 0;
                    }

                    decimal reorderPoint =
                        Convert.ToDecimal(calcContext["ReorderPoint"]);

                    decimal shortageDays = Convert.ToDecimal(calcContext["ShortageDate"]);
                    System.DateTime shortageDate = System.DateTime.Today.AddDays((double)shortageDays);
                    System.DateTime shortageDate_before = shortageDate;

                    bool hasPO = openPoQty > 0;
                    bool hasValidBPA = bpaQty > 0 && bpaValidTo.HasValue && bpaValidTo.Value > DateTime.Today;
                    bool poLate = poPddDate.HasValue && shortageDate < poPddDate.Value;
                    bool expiryTriggered = lots.Any(l => l.Expiry.HasValue && l.Expiry.Value < shortageDate);

                    var usableLots = lots
                        .Where(l =>
                            !l.Expiry.HasValue
                            || l.Expiry.Value >= shortageDate
                        )
                        .ToList();

                    decimal usableStock = usableLots.Sum(l => l.Qty);
                    decimal expiredBeforeUse = lots
                        .Where(l => l.Expiry.HasValue && l.Expiry.Value < shortageDate)
                        .Sum(l => l.Qty);

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "EXPIRY_IMPACT",
                        $"Material={materialCode}, OriginalStock={stockQty}, RemovedBeforeUse={expiredBeforeUse}, FinalStock={usableStock}",
                        materialCode,
                        "INFO",
                        correlationId
                    );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "EXPIRY_AWARE_ADJUSTMENT",
                            $"Material={materialCode}, RemovedBeforeUse={expiredBeforeUse}, UsableStock={usableStock}, InitialShortage={shortageDate}",
                            materialCode,
                            "INFO",
                            correlationId
                        );

                    }

                    // SECOND calculation (after expiry adjustment)
                    calcContext["StockQty"] = usableStock;

                    foreach (var metric in ShortageAnalysisOrder)
                    {
                        var result = EvaluateAnalysisFormula(analysisConfigs[metric], calcContext);
                        calcContext[metric] = result;
                    }

                    string scenario = ResolveScenario(
                        openPoQty,
                        usableStock,
                        Convert.ToDecimal(calcContext["DaysOfSupply"]),
                        safetyDays,
                        poPddDate,
                        shortageDate,
                        bpaValidTo
                    );

                    if (scenario == "SAFE")
                    {
                        if (isDebugMode)
                        {
                            _actionLogRepo.InsertLog(
                               WorkerCode,
                               "SAFE_SKIPPED",
                               $"Material={materialCode}, DOS={calcContext["DaysOfSupply"]}, SafetyDays={safetyDays}",
                               materialCode,
                               "INFO",
                               correlationId
                           );
                        }

                        continue;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SCENARIO",
                        $"Material={materialCode}, Scenario={scenario}, DOS={calcContext["DaysOfSupply"]}, SafetyDays={safetyDays}",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    if (profile == null ||
                         !profile.SafetyDays.HasValue ||
                         profile.SafetyDays.Value <= 0)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "EXPIRED_NO_PROFILE",
                            $"Material={materialCode} | OriginalStock={stockQty} | UsableStock={usableStock} | ExpiredRemoved={expiredBeforeUse}",
                            materialCode,
                            "WARNING",
                            correlationId
                        );

                        // SCENARIO 6
                        if (usableStock <= 0)
                        {
                            shortageList.Add(new Dictionary<string, object>
                            {
                                { "Material", materialCode },
                                { "Description", materialDescription },

                                { "Status", "EXPIRED + NO PROFILE" },

                                { "StockQty", stockQty },
                                { "OpenPoQty", openPoQty < 0 ? 0 : openPoQty },
                                { "VmiQty", vmiQty },
                                { "BpaQty", bpaQty },

                                { "PoPddDate", poPddDate?.ToString("yyyy-MM-dd") ?? "" },
                                { "BpaExpirationDate", bpaValidTo?.ToString("yyyy-MM-dd") ?? "" },

                                { "ExpiryDate", string.Join(";",
                                    lots.Where(l => l.Expiry.HasValue)
                                        .Select(l => l.Expiry.Value.ToString("yyyy-MM-dd")))
                                },

                                { "RfqLeadTime", 0 },
                                { "BpaLeadTime", 0 },
                                { "PrLeadTime", 0 },
                                { "SupplierLeadTime", leadTimeDays },

                                { "DOS", 0 },

                                { "Action", "Review Planning Profile" },

                                { "TriggerDate", DateTime.Today.ToString("MMM dd") },
                                { "DeliveryDate", "" },
                                { "ShortageDate", "" },

                                { "Reason", "Expired stock with missing planning profile" }
                            });

                            // start: delete if issue did not resolve
                            shortageItems.Add(new
                            {
                                item = materialCode,
                                status = "EXPIRED + NO PROFILE",
                                action = "Review Planning Profile",
                                metrics = new
                                {
                                    AvailableSupply = usableStock
                                },
                                actionResults = new List<object>()
                            });
                            // end: delete if issue did not resolve

                            // start: uncomment if above code works
                            //shortageItems.Add(new
                            //{
                            //    item = materialCode,
                            //    metrics = new { AvailableSupply = usableStock },
                            //    actionResults = new List<object>()
                            //});
                            // end: uncomment if above code works
                        }

                        continue;
                    }

                    avgDaily = Convert.ToDecimal(calcContext["AvgDailyConsumption"]);
                    availableSupply = Convert.ToDecimal(calcContext["AvailableSupply"]);

                    daysOfSupply = avgDaily > 0
                        ? Convert.ToDecimal(calcContext["DaysOfSupply"])
                        : 0;

                    reorderPoint = Convert.ToDecimal(calcContext["ReorderPoint"]);

                    if (!scanAllMaterials)
                    {
                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "VALIDATION",
                            $"Material={materialCode} | " +
                            $"Stock={stockQty} | " +
                            $"OpenPO={openPoQty} | " +
                            $"VMI={vmiQty} | " +
                            $"BPA={bpaQty} | " +
                            $"IssuanceFreq={issuanceFreq} | " +
                            $"SafetyDays={safetyDays} | " +
                            $"PO_PDD={(poPddDate.HasValue ? poPddDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"BPAExpiry={(bpaValidTo.HasValue ? bpaValidTo.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"AvailableSupply={availableSupply} | " +
                            $"AvgDaily={avgDaily} | " +
                            $"DOS={daysOfSupply} | " +
                            $"ShortageDate={shortageDate:yyyy-MM-dd} | " +
                            $"DeliveryDate={(poPddDate.HasValue ? poPddDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                            $"Scenario={scenario}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var metrics = new Dictionary<string, object>
                   {
                       { "AvgDailyConsumption", avgDaily },
                       { "AvailableSupply", availableSupply },
                       { "DaysOfSupply", daysOfSupply },
                       { "ReorderPoint", reorderPoint },
                       { "SafetyDays", profile.SafetyDays ?? 0 }
                   };

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SHORTAGE_DATE_IMPACT",
                        $"Material={materialCode}, Before={shortageDate_before:yyyy-MM-dd}, After={shortageDate:yyyy-MM-dd}",
                        materialCode,
                        "INFO",
                        correlationId
                    );
                        _actionLogRepo.InsertLog(
                          WorkerCode,
                          "DEBUG_PO_PDD",
                          $"Material={materialCode}, PDD={poPddDate}, Shortage={shortageDate}",
                          materialCode,
                          "INFO",
                          correlationId
                      );

                        _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DEBUG_FULL_CONTEXT",
                            $"Material={materialCode}, Stock={stockQty}, OpenPO={(openPoQty < 0 ? 0 : openPoQty)}, VMI={vmiQty}, PDD={poPddDate}, BPA={bpaValidTo}, Shortage={shortageDate}",
                            materialCode,
                            "INFO",
                            correlationId
                        );
                    }

                    var rowDict = new Dictionary<string, object>
                       {
                           { "ItemId", materialCode },
                           { "StockQty", stockQty },
                           { "AvgDailyConsumption", avgDaily },
                           { "AvailableSupply", availableSupply },
                           { "DaysOfSupply", daysOfSupply },
                           { "ReorderPoint", reorderPoint },
                           { "LeadTimeDays", leadTimeDays },
                           { "PlantCode", plantCode },
                           { "OpenPOQty", openPoQty < 0 ? 0 : openPoQty },
                           { "ShortageDate", shortageDate },
                           { "PO_PddDate", poPddDate.HasValue ? (object)poPddDate.Value : DBNull.Value },
                           { "BpaValidTo", bpaValidTo.HasValue ? (object)bpaValidTo.Value : DBNull.Value },
                       };

                    rowDict["Scenario"] = scenario;

                    rowDict["TODAY"] = System.DateTime.Today;
                    bool triggered = false;
                    var actionResults = new List<GeneratePurchaseDocumentsResultDTO>();

                    foreach (var decision in config.Decisions)
                    {
                        if (decision == null || string.IsNullOrWhiteSpace(decision.ConditionExpression))
                        {
                            continue;
                        }

                        var decodedExpression = System.Net.WebUtility.HtmlDecode(decision.ConditionExpression);

                        bool decisionResult =
                            EvaluateCondition(decodedExpression, metrics, rowDict, isDebugMode);

                        if (isDebugMode)
                        {
                            _actionLogRepo.InsertLog(
                            WorkerCode,
                            "DECISION_EVALUATED",
                            $"{decodedExpression} => {decisionResult}",
                            materialCode,
                            decisionResult ? "TRUE" : "FALSE",
                            correlationId
                        );
                        }

                        bool forceTrigger =
                            scenario == "CRITICAL" ||
                            scenario == "EXPIRY" ||
                            scenario == "PO_LATE" ||
                            scenario == "BPA_PR" ||
                            scenario == "RFQ";

                        if (!decisionResult)
                            continue;

                        triggered = true;

                        var matchedActions = config.Actions
                            .Where(a =>
                            {
                                if (string.IsNullOrWhiteSpace(a.ActionConfig))
                                    return false;

                                try
                                {
                                    var cfg = JObject.Parse(a.ActionConfig);
                                    var decisionType = cfg["DecisionType"]?.ToString();

                                    return string.Equals(
                                        decisionType,
                                        decision.DecisionType,
                                        StringComparison.OrdinalIgnoreCase);
                                }
                                catch
                                {
                                    return false;
                                }
                            })
                            .ToList();

                        actionResults.AddRange(
                            await ExecuteActions(
                                matchedActions,
                                rowDict,
                                correlationId,
                                simulationMode));
                    }

                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "ACTION_SUMMARY",
                        $"Material={materialCode} | Actions={(actionResults.Any() ? "EXECUTED" : "NONE")}",
                        materialCode,
                        actionResults.Any() ? "EXECUTED" : "NONE",
                        correlationId
                    );

                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SUMMARY",
                        $"Material={materialCode} | Supply={availableSupply} | Reorder={reorderPoint} | Shortage={(availableSupply < reorderPoint)}",
                        materialCode,
                        triggered ? "ACTION" : "NO_ACTION",
                        correlationId
                    );

                    if (!triggered)
                        continue;

                    string displayStatus;
                    string actionText;

                    switch (scenario)
                    {
                        // SCENARIO 7
                        case "CRITICAL":
                            displayStatus = "EXPIRY + PO LATE";
                            actionText = "Expedite delivery + Review expired stock";
                            break;

                        // SCENARIO 1
                        case "RFQ":
                            displayStatus = "NO PO + NO BPA";
                            actionText = "Create RFQ";
                            break;

                        //SCENARIO 2
                        case "BPA_PR":
                            displayStatus = "BPA AVAILABLE";
                            actionText = "Create PR";
                            break;

                        // SCENARIO 3
                        case "EXPIRY":
                            displayStatus = "EXPIRY";
                            actionText = "Create RFQ";
                            break;

                        // SCENARIO 4
                        case "PO_LATE":
                            displayStatus = "PO EXISTS (LATE)";
                            actionText = "Expedite delivery";
                            break;

                        // SCENARIO 5
                        case "LOW_STOCK":
                            displayStatus = "LOW STOCK";
                            actionText = "Monitor";
                            break;

                        // SCENARIO 8
                        case "SAFE":
                            displayStatus = "HEALTHY STOCK";
                            actionText = "Monitor";
                            break;

                        default:
                            displayStatus = scenario;
                            actionText = "";
                            break;
                    }

                    if (isDebugMode)
                    {
                        _actionLogRepo.InsertLog(
                        WorkerCode,
                        "REPORT_ROW",
                        $"Material={materialCode}, Scenario={scenario}, Status={displayStatus}, Action={actionText}",
                        materialCode,
                        "INFO",
                        correlationId);
                    }

                    string triggerStr = DateTime.Today.ToString("MMM dd");
                    string shortageStr = shortageDate.ToString("MMM dd");
                    string deliveryStr = poPddDate.HasValue
                        ? poPddDate.Value.ToString("MMM dd")
                        : "";

                    shortageList.Add(new Dictionary<string, object>
                      {
                          { "Material", materialCode },
                          { "Description", materialDescription },
                          { "Status", displayStatus },

                          { "StockQty", stockQty },
                          { "OpenPoQty", openPoQty < 0 ? 0 : openPoQty },
                          { "VmiQty", vmiQty },
                          { "BpaQty", bpaQty },

                          { "PoPddDate", poPddDate?.ToString("yyyy-MM-dd") ?? "" },
                          { "BpaExpirationDate", bpaValidTo?.ToString("yyyy-MM-dd") ?? "" },

                          { "ExpiryDate", string.Join(";", lots
                              .Where(l => l.Expiry.HasValue)
                              .Select(l => l.Expiry.Value.ToString("yyyy-MM-dd"))) },

                          { "RfqLeadTime", 0 },
                          { "BpaLeadTime", 0 },
                          { "PrLeadTime", 0 },
                          { "SupplierLeadTime", leadTimeDays },

                          { "DOS", Math.Round(daysOfSupply, 0) },
                          { "Action", actionText },

                          { "TriggerDate", triggerStr },
                          { "DeliveryDate", deliveryStr },
                          { "ShortageDate", shortageStr }
                      });

                    shortageItems.Add(new
                    {
                        item = materialCode,
                        metrics,
                        actionResults
                    });
                }

                if (shortageList.Any())
                    await SendShortageSummaryEmailManualAsync(
                        shortageList,
                        correlationId,
                        requestedByEmail,
                        plantCode);

                return shortageItems.Any()
                    ? WorkerExecutionResult.Success(
                        WorkerCode,
                        "Shortage prediction completed",
                        new { totalItems = shortageItems.Count, items = shortageItems })
                    : WorkerExecutionResult.NoAction(
                        WorkerCode,
                        "No shortages detected");
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "EXECUTION_ERROR",
                    $"Execution failure: {ex.Message}",
                    context.Payload?["PlantCode"]?.ToString(),
                    "ERROR",
                    correlationId
                );

                return WorkerExecutionResult.Failed(
                    WorkerCode,
                    ex.Message,
                    correlationId);
            }
        }

        private async Task<WorkerExecutionResult> ExecuteDemoLegacy(WorkerExecutionContext context)
        {
            var correlationId = context.CorrelationId ?? Guid.NewGuid().ToString();

            try
            {
                if (_cachedConfig == null || DateTime.Now - _lastConfigLoad > _cacheDuration)
                {
                    var start = DateTime.Now;

                    _cachedConfig = _configRepo.GetWorker(WorkerCode)
                        ?? throw new InvalidOperationException("Worker configuration not found.");

                    _lastConfigLoad = DateTime.Now;

                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "PERF",
                        $"Config load took {(DateTime.Now - start).TotalSeconds}s",
                        "SYSTEM",
                        "INFO",
                        Guid.NewGuid().ToString()
                    );
                }

                var config = _cachedConfig;

                var plantCode = context.Payload?["PlantCode"]?.ToString();
                if (string.IsNullOrWhiteSpace(plantCode))
                    throw new InvalidOperationException("PlantCode is required.");

                //   ADD HERE (RIGHT AFTER PlantCode)

                var materialList = new List<string>();

                // Case 1: multiple materials
                var materialsArray = context.Payload?["Materials"] as JArray;

                if (materialsArray != null && materialsArray.Any())
                {
                    materialList = materialsArray
                        .Select(x => x["MaterialCode"]?.ToString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }

                // Case 2: single material
                var singleMaterial = context.Payload?["MaterialCode"]?.ToString();

                if (!string.IsNullOrWhiteSpace(singleMaterial))
                {
                    materialList.Add(singleMaterial);
                }

                // Normalize
                materialList = materialList
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var queryConfig = config.Queries
                    .Select(q => new { q, Json = JObject.Parse(q.SqlQuery) })
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Json["PlantCode"]?.ToString(),
                            plantCode,
                            StringComparison.OrdinalIgnoreCase));

                if (queryConfig == null)
                    throw new InvalidOperationException($"No query defined for PlantCode '{plantCode}'");

                var querySettings = queryConfig.Json;

                bool useMockStock = querySettings["Inventory"]?["UseMockStock"]?.ToObject<bool>() ?? false;
                bool useMock = querySettings["Supply"]?["UseMockSupply"]?.ToObject<bool>() ?? false;
                bool simulationMode = querySettings["Execution"]?["SimulationMode"]?.ToObject<bool>() ?? true;
                bool isDebugMode = querySettings["Execution"]?["DebugMode"]?.ToObject<bool>() ?? false;

                decimal totalLeadTimeConfig = queryConfig.Json["LeadTimeDays"]?.ToObject<decimal>() ?? 0m;

                // LOAD INVENTORY
                Dictionary<string, List<InventoryLot>> inventoryLotMap;

                if (plantCode == "THO")
                {
                    var inventory = await _idmRepository.GetAllInventoryAsync();

                    inventoryLotMap = inventory
                        .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                        .GroupBy(x => NormalizeMaterial(x.PartNumber))
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => new InventoryLot
                            {
                                Qty = x.Quantity,
                                Expiry = null
                            }).ToList()
                        );
                }
                else // PHO
                {
                    var workflowSteps = querySettings["WorkflowSteps"]?.ToObject<List<string>>();

                    if (workflowSteps == null || !workflowSteps.Any())
                        throw new Exception("WorkflowSteps missing");

                    var workflowStep = workflowSteps.First();

                    // If no payload → full scan
                    if (!materialList.Any())
                    {
                        var material = new MaterialDetails
                        {
                            WorkflowStep = workflowStep,
                            PartNumber = "" // allowed = ALL
                        };

                        var xml = new XDocument(
                            new XElement("Request",
                                new XElement("operationNumber", workflowStep)
                            )
                        );

                        var camstarData = await _camstarRepo
                            .GetMaterialLotsByFilterAsync(material, xml);

                        inventoryLotMap = camstarData
                            .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                            .GroupBy(x => NormalizeMaterial(x.PartNumber))
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(l => new InventoryLot
                                {
                                    Qty = (decimal)l.Quantity,
                                    Expiry = l.ExpirationDate
                                }).ToList()
                            );
                    }
                    else
                    {
                        // filtered mode: call per material
                        var allResults = new List<MaterialDetails>();

                        foreach (var partNumber in materialList)
                        {
                            var material = new MaterialDetails
                            {
                                WorkflowStep = workflowStep,
                                PartNumber = partNumber
                            };

                            var xml = new XDocument(
                                new XElement("Request",
                                    new XElement("operationNumber", workflowStep)
                                )
                            );

                            var result = await _camstarRepo
                                .GetMaterialLotsByFilterAsync(material, xml);

                            allResults.AddRange(result);
                        }

                        inventoryLotMap = allResults
                            .Where(x => !string.IsNullOrWhiteSpace(x.PartNumber))
                            .GroupBy(x => NormalizeMaterial(x.PartNumber))
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(l => new InventoryLot
                                {
                                    Qty = (decimal)l.Quantity,
                                    Expiry = l.ExpirationDate
                                }).ToList()
                            );
                    }
                }

                // MOCK LOAD
                Dictionary<string, MockMaterialSupplyDTO> mockSupplyFull = null;

                if (useMockStock || useMock)
                {
                    mockSupplyFull = _materialPlanningRepo.GetMockMaterialSupplyFull(plantCode);
                }

                if (useMockStock)
                {
                    inventoryLotMap = mockSupplyFull
                        .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                        .ToDictionary(
                            x => NormalizeMaterial(x.Key),
                            x => new List<InventoryLot>
                            {
                new InventoryLot
                {
                    Qty = x.Value.StockQty,
                    Expiry = x.Value.ExpiryDate
                }
                            }
                        );
                }

                Dictionary<string, OpenPoInfo> openPoMap = new Dictionary<string, OpenPoInfo>();
                Dictionary<string, decimal> vmiMap = new Dictionary<string, decimal>();

                var org = plantCode == "PHO" ? "MPHPHO" : "MTHTHO";

                if (useMock)
                {
                    openPoMap = mockSupplyFull.ToDictionary(
                        x => NormalizeMaterial(x.Key),
                        x => new OpenPoInfo
                        {
                            Quantity = x.Value.OpenPoQty,
                            PddDate = x.Value.PoPddDate,
                            BpaValidTo = x.Value.BpaExpirationDate
                        });

                    vmiMap = mockSupplyFull
                        .GroupBy(x => NormalizeMaterial(x.Key))
                        .ToDictionary(
                            g => g.Key,
                            g => g.Sum(x => x.Value.VmiQty)
                        );
                }
                else
                {
                    openPoMap = await _dvRepository.GetOpenPoQuantitiesAsync(
                        plantCode,
                        inventoryLotMap.Keys.ToList());

                    var vmiFolder = querySettings["VMI"]?["LocalFolder"]?.ToString();
                    vmiMap = plantCode == "THO"
                        ? _thoVmiService.GetVmiQuantities(org)
                        : _phoVmiService.GetVmiQuantities(org, vmiFolder);
                }

                // ANALYSIS CONFIG   
                var analysisConfigs = config.Analysis
                    .Where(a => ShortageAnalysisOrder.Contains(a.MetricName))
                    .ToDictionary(a => a.MetricName, a => a.Formula);

                var shortageItems = new List<object>();
                var shortageList = new List<Dictionary<string, object>>();
                var recentLogs = _actionLogRepo.GetRecentLogs(WorkerCode, 100);

                // MAIN LOOP
                foreach (var item in inventoryLotMap)
                {
                    string materialCode = NormalizeMaterial(item.Key);

                    var lots = item.Value;

                    decimal stockQty = lots.Sum(l => l.Qty);

                    decimal issuanceFreq = 1;

                    MockMaterialSupplyDTO mock = null;

                    if (mockSupplyFull != null)
                    {
                        mock = mockSupplyFull
                            .FirstOrDefault(x =>
                                NormalizeMaterial(x.Key) == materialCode)
                            .Value;
                    }


                    decimal safetyDays =
                        mock?.SafetyDays > 0
                        ? mock.SafetyDays.Value
                        : 0;

                    decimal openPoQty = 0;
                    DateTime? poPddDate = null;
                    DateTime? bpaValidTo = null;
                    decimal vmiQty = 0;

                    if (openPoMap.TryGetValue(materialCode, out var po))
                    {
                        openPoQty = po.Quantity;
                        poPddDate = po.PddDate;
                        bpaValidTo = po.BpaValidTo;
                    }

                    vmiMap.TryGetValue(materialCode, out vmiQty);

                    decimal avgDaily =
                        mock?.AvgDailyConsumption > 0
                        ? mock.AvgDailyConsumption.Value
                        : 5m;

                    if (avgDaily <= 0)
                    {
                        shortageList.Add(new Dictionary<string, object>
                        {
                            { "Material", materialCode },
                            { "StockQty", stockQty },
                            { "AvgDailyConsumption", avgDaily },
                            { "DaysOfSupply", 0 },
                            { "ReorderPoint", 0 },
                            { "AvailableSupply", stockQty },
                            { "TotalLeadTime", 0 },
                            { "Reason", "No consumption - no shortage risk" }
                        });

                        continue;
                    }

                    var daysOfSupply = Math.Ceiling(stockQty / avgDaily);

                    // EXPIRY
                    DateTime? expiryDate = mock?.ExpiryDate;
                    decimal usableStock = stockQty;

                    if (expiryDate.HasValue)
                    {
                        decimal expireQty = stockQty * 0.2m;

                        usableStock = stockQty - expireQty;
                    }

                    decimal bpaQty = mock?.BpaQty ?? 0;
                    DateTime? bpaExpiry = mock?.BpaExpirationDate;

                    decimal adjustedSupply = usableStock + openPoQty + vmiQty + bpaQty;

                    decimal adjustedDOS = Math.Ceiling(adjustedSupply / avgDaily);
                    DateTime adjustedShortageDate = DateTime.Today.AddDays((double)adjustedDOS);

                    DateTime shortageDate = adjustedShortageDate;

                    int safetyBufferDays = safetyDays > 0 ? (int)safetyDays : 10; // or fixed 10 if needed
                    DateTime requiredDeliveryDate = shortageDate.AddDays(-safetyBufferDays);

                    int rfqLead = (int)(mock?.RfqLeadTime ?? 0);
                    int bpaLead = (int)(mock?.BpaLeadTime ?? 0);
                    int prLead = (int)(mock?.PrLeadTime ?? 0);
                    int supplierLead = (int)(mock?.SupplierLeadTime ?? 0);

                    // Scenario conditions
                    bool hasPO = openPoQty > 0;
                    bool hasValidBPA = bpaQty > 0 && bpaExpiry.HasValue && bpaExpiry.Value > DateTime.Today;
                    bool poLate = poPddDate.HasValue && shortageDate < poPddDate.Value;
                    bool expiryTriggered = expiryDate.HasValue && expiryDate.Value < shortageDate;

                    int totalLeadTime;

                    if (hasPO)
                    {
                        // Scenario 4: PO exists → supplier only
                        totalLeadTime = supplierLead;
                    }
                    else if (expiryTriggered)
                    {
                        // Scenario 3: Expiry must come BEFORE BPA
                        totalLeadTime = rfqLead + bpaLead + prLead + supplierLead;
                    }
                    else if (hasValidBPA)
                    {
                        // Scenario 2: BPA → PR + supplier
                        totalLeadTime = prLead + supplierLead;
                    }
                    else
                    {
                        // Scenario 1: No PO/BPA → full process
                        totalLeadTime = rfqLead + bpaLead + prLead + supplierLead;
                    }

                    DateTime triggerDate = requiredDeliveryDate.AddDays(-totalLeadTime);

                    // Check if RFQ already created for this material
                    // DEMO LOGIC ONLY (NOT FOR PRODUCTION)
                    // This temporarily excludes materials if DONE_PR / DONE_RFQ is logged.
                    // In production, this MUST be replaced with real PR/RFQ validation (ERP/SAP/Oracle).
                    // DONE_* is user-driven and NOT a reliable source of truth.
                    bool alreadyHandled = recentLogs.Any(x =>
                        x.Target != null &&
                        x.Target.Equals(materialCode, StringComparison.OrdinalIgnoreCase) &&
                        x.Status == "SUCCESS" &&
                        (
                            x.ActionType == "CREATE_PURCHASE_REQUISITION" ||
                            x.ActionType == "CREATE_RFQ" ||
                            x.ActionType == "DONE_PR" ||
                            x.ActionType == "DONE_RFQ"
                        )
                    );

                    var calcContext = new Dictionary<string, object>
                    {
                        { "StockQty", stockQty },
                        { "OpenPOQty", openPoQty },
                        { "VmiQty", vmiQty },
                        { "IssuanceFrequencyDays", issuanceFreq },
                        { "SafetyDays", safetyDays },
                        { "TotalLeadTime", totalLeadTime },
                        { "PO_PddDate", poPddDate ?? (object)DBNull.Value },
                        { "TODAY", DateTime.Today },
                        { "AvgDailyConsumption", avgDaily },
                        { "AvailableSupply", adjustedSupply },
                        { "DaysOfSupply", adjustedDOS }
                    };

                    // FORMULA COMPUTATION
                    foreach (var metric in ShortageAnalysisOrder)
                    {
                        if (metric == "AvgDailyConsumption" &&
                            calcContext.ContainsKey("AvgDailyConsumption"))
                            continue;

                        calcContext[metric] =
                            EvaluateAnalysisFormula(analysisConfigs[metric], calcContext);
                    }

                    bool isDue = DateTime.Today >= triggerDate;

                    bool shouldTrigger = isDue && !alreadyHandled;

                    string status;

                    if (DateTime.Today > triggerDate)
                        status = "LATE";
                    else if (DateTime.Today == triggerDate)
                        status = "ON TIME";
                    else
                        status = "WAIT";

                    string reason;

                    if (poLate)
                    {
                        reason = "PO arrives after shortage → Expedite delivery";
                    }
                    else if (openPoQty <= 0 && hasValidBPA)
                    {
                        reason = "BPA available → Create PR";
                    }
                    else if (openPoQty <= 0)
                    {
                        reason = "No PO/BPA → Create RFQ";
                    }
                    else if (expiryTriggered)
                    {
                        reason = "Stock expires before use → Replenish (RFQ)";
                    }
                    else if (adjustedDOS <= safetyDays)
                    {
                        reason = "Low stock warning → Monitor";
                    }
                    else
                    {
                        reason = "Supply sufficient";
                    }

                    var metrics = new Dictionary<string, object>
                    {
                        { "AvgDailyConsumption", avgDaily },
                        { "AvailableSupply", adjustedSupply },
                        { "DaysOfSupply", adjustedDOS },
                        { "ReorderPoint", calcContext["ReorderPoint"] },
                        { "SafetyDays", safetyDays }
                    };

                    var rowDict = new Dictionary<string, object>
                    {
                        { "ItemId", materialCode },
                        { "StockQty", stockQty },
                        { "AvailableSupply", adjustedSupply },
                        { "ReorderPoint", calcContext["ReorderPoint"] },
                        { "PlantCode", plantCode },
                        { "OpenPOQty", openPoQty },
                        { "ShortageDate", adjustedShortageDate },
                        { "PO_PddDate", poPddDate ?? (object)DBNull.Value },
                        { "BpaValidTo", bpaValidTo ?? (object)DBNull.Value },
                        { "TODAY", DateTime.Today }
                    };

                    bool triggered = false;
                    var actionResults = new List<GeneratePurchaseDocumentsResultDTO>();

                    if (shouldTrigger)
                    {
                        foreach (var decision in config.Decisions)
                        {
                            var expr = System.Net.WebUtility.HtmlDecode(decision.ConditionExpression);
                            bool result = EvaluateCondition(expr, metrics, rowDict, isDebugMode);

                            if (!result) continue;

                            triggered = true;

                            var actions = config.Actions
                                .Where(a => a.ActionConfig.Contains(decision.DecisionType))
                                .ToList();

                            actionResults.AddRange(
                                await ExecuteActions(actions, rowDict, correlationId, simulationMode));
                        }
                    }

                    if (!triggered)
                        continue;

                    string displayStatus;
                    string actionText;

                    if (poLate)
                    {
                        displayStatus = "PO is late";
                        actionText = "Expedite delivery";
                    }
                    else if (expiryTriggered)
                    {
                        displayStatus = "Stock will expire";
                        actionText = "Create RFQ";
                    }
                    else if (!hasPO && hasValidBPA)
                    {
                        displayStatus = "BPA available";
                        actionText = "Create PR";
                    }
                    else
                    {
                        displayStatus = "No supply available";
                        actionText = "Create RFQ";
                    }

                    string triggerStr = triggerDate.ToString("MMM dd");
                    string shortageStr = shortageDate.ToString("MMM dd");
                    string deliveryStr = requiredDeliveryDate.ToString("MMM dd");

                    shortageList.Add(new Dictionary<string, object>
                    {
                        { "Material", materialCode },
                        { "Status", displayStatus },

                        { "StockQty", stockQty },
                        { "OpenPoQty", openPoQty },
                        { "VmiQty", vmiQty },

                        { "PoPddDate", poPddDate?.ToString("yyyy-MM-dd") ?? "" },
                        { "BpaExpirationDate", bpaValidTo?.ToString("yyyy-MM-dd") ?? "" },
                        { "ExpiryDate", expiryDate?.ToString("yyyy-MM-dd") ?? "" },

                        { "RfqLeadTime", rfqLead },
                        { "BpaLeadTime", bpaLead },
                        { "PrLeadTime", prLead },
                        { "SupplierLeadTime", supplierLead },

                        { "DOS", adjustedDOS },
                        { "Action", actionText },
                        { "TriggerDate", triggerStr },
                        { "DeliveryDate", deliveryStr },
                        { "ShortageDate", shortageStr }
                    });

                    shortageItems.Add(new
                    {
                        item = materialCode,
                        metrics,
                        actionResults
                    });
                }

                // EMAIL
                if (shortageList.Any())
                {
                    await SendShortageSummaryEmail(config.Actions, shortageList, correlationId, plantCode);
                }

                return shortageItems.Any()
                    ? WorkerExecutionResult.Success(
                        WorkerCode,
                        "Shortage prediction completed",
                        new { totalItems = shortageItems.Count, items = shortageItems })
                    : WorkerExecutionResult.NoAction(
                        WorkerCode,
                        "No shortages detected");
            }
            catch (Exception ex)
            {
                return WorkerExecutionResult.Failed(
                    WorkerCode,
                    ex.Message,
                    correlationId);
            }
        }

        private async Task<List<GeneratePurchaseDocumentsResultDTO>> ExecuteActions(List<WorkerAction> actions, IDictionary<string, object> rowDict, string correlationId, bool simulationMode)
        {
            var results = new List<GeneratePurchaseDocumentsResultDTO>();
            var materialCode = rowDict.ContainsKey("ItemId") ? rowDict["ItemId"].ToString() : "UNKNOWN";

            foreach (var action in actions)
            {
                if (string.IsNullOrWhiteSpace(action.ActionType)) continue;

                try
                {
                    switch (action.ActionType)
                    {
                        case "CREATE_PURCHASE_REQUISITION":
                            if (simulationMode)
                            {
                                _actionLogRepo.InsertLog(
                                    WorkerCode,
                                    action.ActionType,
                                    $"[SIMULATION] Would execute action for {materialCode}",
                                    materialCode,
                                    "SIMULATED",
                                    correlationId
                                );

                                results.Add(new GeneratePurchaseDocumentsResultDTO());
                            }
                            else
                            {
                                var dto = await _generationService.GenerateAsync(
                                    new GeneratePurchaseDocumentsRequestDTO
                                    {
                                        MaterialCode = materialCode,
                                        PlantCode = rowDict["PlantCode"].ToString(),
                                        SupplierCode = "SUP-001",
                                        Quantity = 100,
                                        UnitCost = 10,
                                        CreatePurchaseOrder = false,
                                        Reason = "Generated by Digital Worker"
                                    });

                                results.Add(dto);

                                _actionLogRepo.InsertLog(
                                    WorkerCode,
                                    action.ActionType,
                                    $"Generated PR for {materialCode}",
                                    materialCode,
                                    "SUCCESS",
                                    correlationId
                                );
                            }

                            break;

                        case "SEND_NOTIFICATION":
                            var recipientEmail = JObject.Parse(action.ActionConfig)?["RecipientEmail"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(recipientEmail))
                                _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Email notification will be sent to {recipientEmail}", recipientEmail, "PENDING", correlationId);
                            break;

                        case "CREATE_SHORTAGE_EVENT":
                            _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Shortage event created for {materialCode}", materialCode, "SUCCESS", correlationId);
                            break;

                        case "GENERATE_SHORTAGE_REPORT":
                            _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Shortage report generated for {materialCode}", materialCode, "SUCCESS", correlationId);
                            break;

                        case "LOG_SHORTAGE_PREDICTION":
                            _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Logged shortage prediction for {materialCode}", materialCode, "SUCCESS", correlationId);
                            break;

                        default:
                            _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Executed action for {materialCode}", materialCode, "SUCCESS", correlationId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _actionLogRepo.InsertLog(WorkerCode, action.ActionType, $"Action failed: {ex.Message}", materialCode, "FAILED", correlationId);
                }
            }

            return results;
        }

        private async Task SendShortageSummaryEmail(List<WorkerAction> actions, List<Dictionary<string, object>> shortages, string correlationId, string plantCode)
        {
            if (shortages.Count == 0) return;

            var notificationActions = actions
                .Where(a =>
                {
                    if (a.ActionType != "SEND_NOTIFICATION" ||
                        string.IsNullOrWhiteSpace(a.ActionConfig))
                    {
                        return false;
                    }

                    try
                    {
                        var cfg = JObject.Parse(a.ActionConfig);

                        return string.Equals(
                            cfg["PlantCode"]?.ToString(),
                            plantCode,
                            StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToList();

            if (!notificationActions.Any()) return;

            var recipients = new List<string>();

            foreach (var action in notificationActions)
            {
                try
                {
                    var config = JObject.Parse(action.ActionConfig);
                    var emails = config["RecipientEmails"]?.ToObject<List<string>>();
                    if (emails != null && emails.Any())
                    {
                        foreach (var email in emails)
                        {
                            if (!string.IsNullOrWhiteSpace(email))
                                recipients.Add(email.Trim());
                        }
                    }
                    else
                    {
                        var singleEmail = config["RecipientEmail"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(singleEmail))
                            recipients.Add(singleEmail.Trim());
                    }
                }
                catch
                {
                }
            }

            recipients = recipients
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct()
                .ToList();

            if (!recipients.Any()) return;

            var html = BuildEmailBody(shortages);

            var emailDto = new EmailDTO
            {
                To = recipients,
                Cc = new List<string>(),
                Bcc = new List<string>(),
                Subject = $"{plantCode} - Shortage Prediction Alert ({shortages.Count} materials)",
                Body = html,
                IsHtml = true,
                Attachments = new List<EmailAttachmentDTO>()
            };

            var reportBytes = ShortageExcelReportBuilderService.Build(shortages);

            var fileName =
                $"ShortageReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var outputDir =
                @"C:\MOSS\GeneratedFiles\ShortagePrediction";

            Directory.CreateDirectory(outputDir);

            var tempFilePath =
                Path.Combine(outputDir, fileName);

            File.WriteAllBytes(tempFilePath, reportBytes);

            emailDto.Attachments.Add(
                new EmailAttachmentDTO
                {
                    FileName = fileName,
                    FilePath = tempFilePath
                });

            try
            {
                await _emailService.SendEmailAsync(emailDto);

                foreach (var email in recipients)
                {
                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "SEND_NOTIFICATION",
                        $"Email sent to {email} with {shortages.Count} materials",
                        email,
                        "SUCCESS",
                        correlationId
                    );
                }
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "SEND_NOTIFICATION",
                    $"Email failed: {ex.Message}",
                    string.Join(",", recipients),
                    "FAILED",
                    correlationId
                );

                return;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        private async Task SendShortageSummaryEmailManualAsync(List<Dictionary<string, object>> shortages, string correlationId, string recipientEmail, string plantCode)
        {
            if (shortages == null || shortages.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "SEND_NOTIFICATION",
                    "Manual execution email skipped. Requestor email is empty.",
                    "SYSTEM",
                    "FAILED",
                    correlationId);

                return;
            }

            var recipients = new List<string>
    {
        recipientEmail.Trim()
    };

            var html = BuildEmailBody(shortages);

            var emailDto = new EmailDTO
            {
                To = recipients,
                Cc = new List<string>(),
                Bcc = new List<string>(),
                Subject = $"{plantCode} - Shortage Prediction Alert ({shortages.Count} materials)",
                Body = html,
                IsHtml = true,
                Attachments = new List<EmailAttachmentDTO>()
            };

            var reportBytes = ShortageExcelReportBuilderService.Build(shortages);

            var fileName =
                $"ShortageReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var outputDir =
                @"C:\MOSS\GeneratedFiles\ShortagePrediction";

            Directory.CreateDirectory(outputDir);

            var tempFilePath =
                Path.Combine(outputDir, fileName);

            File.WriteAllBytes(tempFilePath, reportBytes);

            emailDto.Attachments.Add(
                new EmailAttachmentDTO
                {
                    FileName = fileName,
                    FilePath = tempFilePath
                });

            try
            {
                await _emailService.SendEmailAsync(emailDto);

                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "SEND_NOTIFICATION",
                    $"Manual execution email sent to {recipientEmail} with {shortages.Count} materials",
                    recipientEmail,
                    "SUCCESS",
                    correlationId);
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "SEND_NOTIFICATION",
                    $"Manual execution email failed: {ex.Message}",
                    recipientEmail,
                    "FAILED",
                    correlationId);

                return;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        private string BuildHtmlTable(List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
                return "<p>No shortages detected.</p>";

            var columns = new List<string>
            {
                "Material",
                "Status",
                "StockQty",
                "OpenPoQty",
                "VmiQty",
                "BpaQty",
                "PoPddDate",
                "BpaExpirationDate",
                "ExpiryDate",
                "RfqLeadTime",
                "BpaLeadTime",
                "PrLeadTime",
                "SupplierLeadTime",
                "DOS",
                "Action",
                "TriggerDate",
                "DeliveryDate",
                "ShortageDate"
            };

            var html = "<h3>📦 Materials Requiring Action</h3>";

            html += "<table style='border-collapse: collapse; width:100%; font-family: Arial;'>";

            html += "<tr style='background-color:#f2f2f2;'>";
            foreach (var col in columns)
                html += $"<th style='border:1px solid #000;padding:6px'>{col}</th>";
            html += "</tr>";

            foreach (var row in data)
            {
                html += "<tr>";

                foreach (var col in columns)
                {
                    object value = row.ContainsKey(col) ? row[col] : "";

                    string displayValue = FormatDecimal(value);
                    html += $"<td style='border:1px solid #000;padding:6px'>{displayValue}</td>";
                }

                html += "</tr>";
            }

            html += "</table>";

            return html;
        }

        private string BuildCsv(List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
                return "No Data";

            var columns = new List<string>
            {
                "Material",
                "Status",
                "StockQty",
                "OpenPoQty",
                "VmiQty",
                "BpaQty",
                "PoPddDate",
                "BpaExpirationDate",
                "ExpiryDate",
                "RfqLeadTime",
                "BpaLeadTime",
                "PrLeadTime",
                "SupplierLeadTime",
                "DOS",
                "Action",
                "TriggerDate",
                "DeliveryDate",
                "ShortageDate"
            };

            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", columns));

            foreach (var row in data)
            {
                var values = columns.Select(col =>
                    row.ContainsKey(col)
                        ? FormatDecimal(row[col])
                        : "");

                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        private string BuildEmailBody(List<Dictionary<string, object>> shortages)
        {
            var table = BuildHtmlTable(shortages);

            var html = new StringBuilder();

            html.Append("<html><body style='font-family:Arial, Helvetica, sans-serif; font-size:14px;'>");

            html.Append("<p>Dear Team,</p>");

            html.Append("<p>");
            html.Append("The <b>Shortage Monitoring System</b> has identified materials that require attention based on forecasted stock depletion.");
            html.Append("</p>");

            html.Append("<p><b>⚠️ Action Required:</b> Please review and take action before the required dates to avoid supply disruption.</p>");

            html.Append($"<p><b>Summary:</b> {shortages.Count} materials require action.</p>");

            //html.Append(table);

            html.Append("<p>For full details, please refer to the attached report.</p>");

            html.Append("<p>Please coordinate with procurement and planning teams accordingly.</p>");

            html.Append("<br/>");

            html.Append("<p>Best regards,<br>");
            html.Append("<b>Digital Worker Shortage Monitoring System</b></p>");

            html.Append("<hr>");

            html.Append("<p style='color:gray; font-size:12px;'>");
            html.Append("This is a system-generated email. Please do not reply.");
            html.Append("</p>");

            html.Append("</body></html>");

            return html.ToString();
        }

        private string FormatDecimal(object value)
        {
            if (value == null) return "0";

            if (decimal.TryParse(value.ToString(), out var d))
            {
                if (d % 1 == 0)
                    return ((int)d).ToString();

                return Math.Round(d, 0).ToString();
            }

            return value.ToString();
        }

        private bool EvaluateCondition(string expression, Dictionary<string, object> metrics, IDictionary<string, object> rowDict, bool isDebugMode)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            expression = System.Net.WebUtility.HtmlDecode(expression);

            var allValues = new Dictionary<string, object>();

            foreach (var m in metrics)
                allValues[m.Key] = m.Value;

            foreach (var r in rowDict)
                allValues[r.Key] = r.Value;

            foreach (var kv in allValues)
            {
                if (kv.Value == null)
                {
                    _actionLogRepo.InsertLog(
                        WorkerCode,
                        "DEBUG_NULL_SHORTCIRCUIT",
                        $"Condition skipped due to NULL: {kv.Key}",
                        rowDict.ContainsKey("ItemId") ? rowDict["ItemId"]?.ToString() : "UNKNOWN",
                        "INFO",
                        Guid.NewGuid().ToString()
                    );

                    expression = Regex.Replace(expression, $@"\b{kv.Key}\b", "null");
                    continue;
                }
            }

            foreach (var kv in allValues.OrderByDescending(k => k.Key.Length))
            {
                string value;

                if (kv.Value == null || kv.Value == DBNull.Value)
                {
                    value = "0";
                }
                else if (kv.Value is System.DateTime dt)
                {
                    value = ((System.DateTime)kv.Value).ToOADate().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    value = Convert.ToString(kv.Value, CultureInfo.InvariantCulture);
                }

                expression = Regex.Replace(expression, $@"\b{kv.Key}\b", value);
            }

            if (isDebugMode)
            {
                _actionLogRepo.InsertLog(
               WorkerCode,
               "DEBUG_EXPRESSION",
               expression,
               rowDict.ContainsKey("ItemId") ? rowDict["ItemId"]?.ToString() : "UNKNOWN",
               "INFO",
               Guid.NewGuid().ToString()
           );
            }

            expression = Regex.Replace(
                expression,
                @"#.*?#\s*<> null\s*AND\s*",
                "",
                RegexOptions.IgnoreCase
            );

            expression = Regex.Replace(
                expression,
                @"#.*?#\s*= null\s*OR\s*",
                "",
                RegexOptions.IgnoreCase
            );

            expression = expression.Trim();

            try
            {
                var result = new DataTable().Compute(expression, "");

                if (result is bool b) return b;
                if (result is int i) return i != 0;
                if (result is double d) return d != 0;

                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "CONDITION_ERROR",
                    $"{expression} | ERROR: {ex.Message}",
                    rowDict.ContainsKey("ItemId") ? rowDict["ItemId"]?.ToString() : "UNKNOWN",
                    "FAILED",
                    Guid.NewGuid().ToString()
                );

                return false;
            }
        }

        private decimal EvaluateAnalysisFormula(string formula, Dictionary<string, object> context)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0m;

            var expr = formula;

            expr = Regex.Replace(expr, @"\bIS NOT NULL\b", "<> null", RegexOptions.IgnoreCase);
            expr = Regex.Replace(expr, @"\bIS NULL\b", "= null", RegexOptions.IgnoreCase);

            foreach (var kv in context.OrderByDescending(k => k.Key.Length))
            {
                string value;

                if (kv.Value == null || kv.Value == DBNull.Value)
                {
                    value = "null";
                }
                else if (kv.Value is System.DateTime dt)
                {
                    value = dt.ToOADate().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    value = Convert.ToString(kv.Value, CultureInfo.InvariantCulture);
                }

                expr = Regex.Replace(expr, $@"\b{kv.Key}\b", value);
            }

            expr = Regex.Replace(expr, @"null\s*<> null", "false", RegexOptions.IgnoreCase);
            expr = Regex.Replace(expr, @"null\s*= null", "true", RegexOptions.IgnoreCase);
            expr = Regex.Replace(expr, @"\bnull\b\s*(<|>|<=|>=)\s*[-\d.]+", "false");

            try
            {
                var result = new DataTable().Compute(expr, "");

                if (result == DBNull.Value || result == null)
                    return 0m;

                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                _actionLogRepo.InsertLog(
                    WorkerCode,
                    "FORMULA_ERROR",
                    $"Expr: {expr} | Error: {ex.Message}",
                    "SYSTEM",
                    "FAILED",
                    Guid.NewGuid().ToString()
                );

                return 0m;
            }
        }

        private static readonly string[] ShortageAnalysisOrder =
        {
            "AvailableSupply",
            "AvgDailyConsumption",
            "MaxDailyConsumption",
            "SafetyDays",
            "SafetyStock",
            "DaysOfSupply",
            "ReorderPoint",
            "ShortageDate"
        };

        private string NormalizeMaterial(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .ToUpper();
        }

        private ExecutionMode ResolveMode(JObject querySettings)
        {
            var mode = querySettings["Execution"]?["Mode"]?.ToString();

            if (string.Equals(mode, "Production", StringComparison.OrdinalIgnoreCase))
                return ExecutionMode.Production;

            return ExecutionMode.Demo;
        }

        private string ResolveScenario(decimal openPoQty, decimal usableStock, decimal daysOfSupply, decimal safetyDays, DateTime? poPddDate, DateTime shortageDate, DateTime? bpaValidTo)
        {
            bool hasPO = openPoQty > 0;

            bool validBPA =
                bpaValidTo.HasValue &&
                bpaValidTo.Value > DateTime.Today;

            bool poLate =
                hasPO &&
                poPddDate.HasValue &&
                shortageDate < poPddDate.Value;

            bool expiryRisk =
                usableStock <= 0;

            // PRIORITY ORDER

            if (expiryRisk && poLate)
                return "CRITICAL";

            if (expiryRisk)
                return "EXPIRY";

            if (poLate)
                return "PO_LATE";

            if (!hasPO && validBPA)
                return "BPA_PR";

            if (!hasPO && !validBPA)
                return "RFQ";

            if (daysOfSupply <= safetyDays)
                return "LOW_STOCK";

            return "SAFE";
        }

    }
}

// IMaterialPlanningRepository.cs
using M2OSS.DTO.Material;
using System.Collections.Generic;

namespace M2OSS.Repository.DigitalWorkers.Interface
{
    public interface IMaterialPlanningRepository
    {
        Dictionary<string, MaterialPlanningProfileDTO> GetPlanningProfiles(
            IEnumerable<string> materialCodes
        );

        Dictionary<string, decimal> GetOpenPoQuantities(string plantCode);

        Dictionary<string, (decimal Stock, decimal OpenPo, decimal Vmi)> GetMockMaterialSupply(string plantCode);

        // Use for Mock data
        Dictionary<string, MockMaterialSupplyDTO> GetMockMaterialSupplyFull(string plantCode);
    }
}

// MaterialPlanningRepository.cs
using M2OSS.Data;
using M2OSS.DTO.Material;
using M2OSS.Repository.DigitalWorkers.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace M2OSS.Repository.DigitalWorkers.Repository
{
    public class MaterialPlanningRepository : IMaterialPlanningRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbConnectionFactory _connectionFactory;

        public MaterialPlanningRepository(IDbConnection connection, IDbConnectionFactory connectionFactory)
        {
            _connection = connection;
            _connectionFactory = connectionFactory;
        }

        public Dictionary<string, MaterialPlanningProfileDTO> GetPlanningProfiles(IEnumerable<string> materialCodes)
        {
            var result = new Dictionary<string, MaterialPlanningProfileDTO>();

            if (materialCodes == null || !materialCodes.Any(c => !string.IsNullOrWhiteSpace(c)))
                return result;

            var codeList = materialCodes
                .Where(c => !string.IsNullOrWhiteSpace(c)) 
                .Distinct()
                .Select(c => $"'{c.Replace("'", "''")}'");

            var sql = $@"
                SELECT
                    PartNumber AS MaterialCode,
                    IssuanceFrequencyType,
                    FrequencyValue AS IssuanceFrequencyDays,
                    SafetyDays,
                    SupplierCountry
                FROM [MOSSDB].[Ref].[MaterialPartNumbers]
                WHERE PartNumber IN ({string.Join(",", codeList)})
            ";

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dto = new MaterialPlanningProfileDTO
                        {
                            MaterialCode = reader["MaterialCode"]?.ToString(),
                            IssuanceFrequencyType = reader["IssuanceFrequencyType"] as string,

                            IssuanceFrequencyDays = reader["IssuanceFrequencyDays"] != DBNull.Value
                                ? Convert.ToDecimal(reader["IssuanceFrequencyDays"])
                                : 0m,

                            SafetyDays = reader["SafetyDays"] != DBNull.Value
                                ? Convert.ToDecimal(reader["SafetyDays"])
                                : (decimal?)null,

                            SupplierType = reader["SupplierCountry"] as string
                        };

                        if (!string.IsNullOrWhiteSpace(dto.MaterialCode))
                        {
                            result[dto.MaterialCode] = dto;
                        }
                    }
                }
            }

            return result;
        }


        public Dictionary<string, decimal> GetOpenPoQuantities(string plantCode)
        {
            using (var conn = _connectionFactory.CreateConnection("TDVConnection"))
            {
                conn.Open();


                const string sql = @"
                    SELECT
                        ITEM_NUMBER,
                        SUM(PO_QUANTITY) AS OPEN_PO_QTY
                    FROM IDM_PROC_PO_DETAILS_VW
                    WHERE ORGANIZATION_CODE = ?
                      AND PO_HEADER_STATUS = 'Open'
                      AND PO_LINE_STATUS   = 'Open'
                    GROUP BY ITEM_NUMBER
                ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    var p = cmd.CreateParameter();
                    string orgCode = plantCode == "PHO" ? "MPHPHO" : plantCode;

                    p.Value = orgCode;
                    cmd.Parameters.Add(p);

                    var result = new Dictionary<string, decimal>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result[reader["ITEM_NUMBER"].ToString()] =
                                reader["OPEN_PO_QTY"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["OPEN_PO_QTY"])
                                    : 0m;
                        }
                    }

                    return result;
                }
            }
        }

        public Dictionary<string, (decimal Stock, decimal OpenPo, decimal Vmi)> GetMockMaterialSupply(string plantCode)
        {
            var result = new Dictionary<string, (decimal, decimal, decimal)>();

            const string sql = @"
                SELECT
                    ItemNumber,
                    ISNULL(StockQty, 0) AS StockQty,
                    ISNULL(OpenPoQty, 0) AS OpenPoQty,
                    ISNULL(VmiQty, 0) AS VmiQty
                FROM Mock_MaterialSupply
                WHERE PlantCode = @PlantCode
            ";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@PlantCode";
                p.Value = plantCode;
                cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = reader["ItemNumber"]?.ToString();
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        var stock = reader["StockQty"] != DBNull.Value
                            ? Convert.ToDecimal(reader["StockQty"])
                            : 0m;

                        var openPo = reader["OpenPoQty"] != DBNull.Value
                            ? Convert.ToDecimal(reader["OpenPoQty"])
                            : 0m;

                        var vmi = reader["VmiQty"] != DBNull.Value
                            ? Convert.ToDecimal(reader["VmiQty"])
                            : 0m;

                        result[item] = (stock, openPo, vmi);
                    }
                }
            }

            return result;
        }

        // Use for Mock data
        public Dictionary<string, MockMaterialSupplyDTO> GetMockMaterialSupplyFull(string plantCode)
        {
            var result = new Dictionary<string, MockMaterialSupplyDTO>();

            const string sql = @"
             SELECT
                ItemNumber,
                ISNULL(StockQty, 0) AS StockQty,
                ISNULL(OpenPoQty, 0) AS OpenPoQty,
                ISNULL(VmiQty, 0) AS VmiQty,
                ISNULL(BpaQty, 0) AS BpaQty,

                ISNULL(AvgDailyConsumption, 0) AS AvgDailyConsumption,  
                ISNULL(SafetyDays, 0) AS SafetyDays,     
                ISNULL(LeadTimeDays, 0) AS LeadTimeDays,

                PoPddDate,
                BpaExpirationDate,
                ExpiryDate
            FROM Mock_MaterialSupply
            WHERE PlantCode = @PlantCode
        ";

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@PlantCode";
                p.Value = plantCode;
                cmd.Parameters.Add(p);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = reader["ItemNumber"]?.ToString();
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        var dto = new MockMaterialSupplyDTO
                        {
                            StockQty = reader["StockQty"] != DBNull.Value
                                ? Convert.ToDecimal(reader["StockQty"])
                                : 0m,

                            OpenPoQty = reader["OpenPoQty"] != DBNull.Value
                                ? Convert.ToDecimal(reader["OpenPoQty"])
                                : 0m,

                            VmiQty = reader["VmiQty"] != DBNull.Value
                                ? Convert.ToDecimal(reader["VmiQty"])
                                : 0m,

                            BpaQty = reader["BpaQty"] != DBNull.Value
                                ? Convert.ToDecimal(reader["BpaQty"])
                                : 0m,

                            PoPddDate = reader["PoPddDate"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["PoPddDate"])
                                : null,

                            BpaExpirationDate = reader["BpaExpirationDate"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["BpaExpirationDate"])
                                : null,

                            ExpiryDate = reader["ExpiryDate"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["ExpiryDate"])
                                : null,

                        AvgDailyConsumption = reader["AvgDailyConsumption"] != DBNull.Value
                            ? Convert.ToDecimal(reader["AvgDailyConsumption"])
                            : 0m,

                                                    SafetyDays = reader["SafetyDays"] != DBNull.Value
                            ? Convert.ToDecimal(reader["SafetyDays"])
                            : 0m,

                                                    LeadTimeDays = reader["LeadTimeDays"] != DBNull.Value
                            ? Convert.ToDecimal(reader["LeadTimeDays"])
                            : 0m,
                        };

                        result[item.ToUpper()] = dto;
                    }
                }
            }

            return result;
        }
    }
}

// MaterialPlanningProfileDTO.cs
using System;

namespace M2OSS.DTO.Material
{
    public class MaterialPlanningProfileDTO
    {
        public string MaterialCode { get; set; }
        public string IssuanceFrequencyType { get; set; }
        public decimal IssuanceFrequencyDays { get; set; }
        public decimal? SafetyDays { get; set; }
        public string SupplierType { get; set; }
    }
    public class MockMaterialSupplyDTO
    {
        public decimal StockQty { get; set; }
        public decimal OpenPoQty { get; set; }
        public decimal VmiQty { get; set; }

        public DateTime? PoPddDate { get; set; }
        public decimal? BpaQty { get; set; }
        public DateTime? BpaExpirationDate { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}

// SharePointVmiInventoryProviderService.cs
using ClosedXML.Excel;
using M2OSS.Service.DigitalWorkers.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class SharePointVmiInventoryProviderService : IVmiInventoryProviderService
    {
        public Dictionary<string, decimal> GetVmiQuantities(string inventoryOrganization, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new Exception("VMI folder path is not configured.");
            }

            if (!Directory.Exists(folderPath))
            {
                throw new Exception($"VMI folder does not exist: {folderPath}");
            }

            var files = new DirectoryInfo(folderPath)
                 .GetFiles("IPE324 QOH Intransit and Shipment Report*.xlsx")
                 .Where(f =>
                     !f.Name.Contains("All Orgs") &&
                     System.Text.RegularExpressions.Regex.IsMatch(
                         f.Name,
                         @"\d{4}-\d{2}-\d{2}T"
                     )
                 )
                 .OrderByDescending(f => f.LastWriteTime)
                 .ToList();

            if (!files.Any())
                throw new Exception("No VMI file found.");

            var latestFile = files.First();

            using (var stream = new FileStream(
                latestFile.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheets
                        .FirstOrDefault(w =>
                            w.Name.Equals("Detailed Data", StringComparison.OrdinalIgnoreCase));

                    if (sheet == null)
                        throw new Exception("Worksheet 'Detailed Data' not found.");

                    var result = new Dictionary<string, decimal>(
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var row in sheet.RowsUsed().Skip(1))
                    {
                        var org = row.Cell(10).GetString().Trim();
                        var subinventory = row.Cell(11).GetString().Trim();

                        // FILTER 1: Organization
                        if (!org.Equals(inventoryOrganization, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // FILTER 2: PVMI ONLY
                        if (!subinventory.Equals("PVMI", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var material = row.Cell(1).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(material))
                            continue;

                        decimal qty = 0m;
                        var qtyCell = row.Cell(14);

                        if (qtyCell.TryGetValue<double>(out var parsed))
                        {
                            qty = Convert.ToDecimal(parsed);
                        }

                        if (!result.ContainsKey(material))
                            result[material] = 0m;

                        result[material] += qty;
                    }

                    return result;
                }
            }
        }
    }
}

// IVmiInventoryProviderService.cs
using System.Collections.Generic;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IVmiInventoryProviderService
    {
        Dictionary<string, decimal> GetVmiQuantities(string inventoryOrganization, string folderPath);
    }
}

// ThoSftpVmiInventoryProviderService.cs
using M2OSS.Service.DigitalWorkers.Interface;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class ThoSftpVmiInventoryProviderService : IThoSftpVmiInventoryProviderService
    {
        public Dictionary<string, decimal> GetVmiQuantities(string inventoryOrganization)
        {
            var stream = DownloadFile();
            return ParseFile(stream);
        }

        private Stream DownloadFile()
        {
            var host = "sftp2.wdc.com";
            var username = ConfigurationManager.AppSettings["SftpUser"];
            var password = ConfigurationManager.AppSettings["SftpPassword"];

            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("SFTP username is missing in config");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("SFTP password is missing in config");

            using (var client = new SftpClient(host, username, password))
            {
                client.Connect();

                var files = client.ListDirectory(".")
                    .Where(f => !f.IsDirectory && f.Name.EndsWith(".dat"))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                if (!files.Any())
                    throw new Exception("No .dat file found in SFTP.");

                var latestFile = files.First();

                var stream = new MemoryStream();

                client.DownloadFile(latestFile.FullName, stream);

                stream.Position = 0;

                client.Disconnect();

                return stream;
            }
        }

        private Dictionary<string, decimal> ParseFile(Stream stream)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            using (var reader = new StreamReader(stream))
            {
                var header = reader.ReadLine(); // skip header

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var cols = line.Split('|');

                    if (cols.Length < 10)
                        continue;

                    var material = cols[3]?.Trim();      // PART_NO
                    decimal availableQty = 0;

                    decimal.TryParse(cols[9], out availableQty); // AVAILABLE_QTY

                    if (string.IsNullOrWhiteSpace(material))
                        continue;

                    if (!result.ContainsKey(material))
                        result[material] = 0;

                    result[material] += availableQty;
                }
            }

            return result;
        }
    }
}

// IThoSftpVmiInventoryProviderService.cs
using System.Collections.Generic;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IThoSftpVmiInventoryProviderService
    {
        Dictionary<string, decimal> GetVmiQuantities(string inventoryOrganization);
    }
}

// DvRepository.cs
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using M2OSS.Entities.E_POU;
using M2OSS.Repository.Databases.Interface;
using M2OSS.Repository.DV.Interface;
using M2OSS.Repository.Helper;
using M2OSS.Repository.RepositoryBases;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace M2OSS.Repository.DV.Repository
{
    public class DvRepository : DvRepositoryBase, IDvRepository
    {
       
        public DvRepository(IDvDatabaseRepository db) : base(db)
        {

        }

        private static readonly MemoryCache _cache = MemoryCache.Default;
   
        public async Task<Dictionary<string, decimal>> GetOpenPoQuantitiesAsync(string plantCode, List<string> materials)
        {
            var orgMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "PHO", "MPHPHO" },
                { "THO", "MTHTHO" }
            };

            var orgCode = orgMapping.ContainsKey(plantCode)
                ? orgMapping[plantCode]
                : plantCode;

            // UNIQUE cache key (important!)
            var cacheKey = $"OPENPO_{plantCode}_{materials.Count}";

            // CHECK CACHE FIRST
            if (_cache.Contains(cacheKey))
            {
                return (Dictionary<string, decimal>)_cache.Get(cacheKey);
            }

            var finalResult = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            int batchSize = 300;

            var materialBatches = materials
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.value).ToList())
                .ToList();

            foreach (var batch in materialBatches)
            {
                var placeholders = string.Join(",", batch.Select(x => "?"));

                var sql = $@"
            SELECT
                ITEM_NUMBER,
                SUM(PO_QUANTITY) AS OPEN_PO_QTY
            FROM IDM_PROC_PO_DETAILS_VW
            WHERE ORGANIZATION_CODE = ?
            AND PO_HEADER_STATUS = 'Open'
            AND PO_LINE_STATUS   = 'Open'
            AND ITEM_NUMBER IN ({placeholders})
            GROUP BY ITEM_NUMBER
        ";

                var parameters = new DynamicParameters();
                parameters.Add("1", orgCode);

                int index = 2;
                foreach (var item in batch)
                {
                    parameters.Add(index.ToString(), item);
                    index++;
                }

                var rows = await _db.QueryAsync<dynamic>(sql, parameters);

                foreach (var row in rows)
                {
                    string item = row.ITEM_NUMBER != null
                        ? row.ITEM_NUMBER.ToString()
                        : string.Empty;

                    decimal qty = row.OPEN_PO_QTY != null
                        ? Convert.ToDecimal(row.OPEN_PO_QTY)
                        : 0m;

                    if (finalResult.ContainsKey(item))
                        finalResult[item] += qty;
                    else
                        finalResult[item] = qty;
                }
            }

            // STORE IN CACHE (5 minutes)
            _cache.Add(
                cacheKey,
                finalResult,
                DateTimeOffset.Now.AddMinutes(5)
            );

            return finalResult;
        }
    }
}


////////// Get BPA ////////// 

// BpaRecordDTO.cs
using System;

namespace M2OSS.DTO.DigitalWorkers
{
    public class BpaRecordDTO
    {
        public string MaterialNumber { get; set; }
        public decimal BalanceQty { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}

// IBpaFileProviderService.cs
using M2OSS.DTO.DigitalWorkers;
using System.Collections.Generic;

namespace M2OSS.Service.DigitalWorkers.Interface
{
    public interface IBpaFileProviderService
    {
        IReadOnlyCollection<BpaRecordDTO> GetBpa(string correlationId, bool isDebugMode);
    }
}

// BpaFileProviderService.cs
using ClosedXML.Excel;
using M2OSS.DTO.DigitalWorkers;
using M2OSS.Repository.DigitalWorkers.Interface;
using M2OSS.Service.DigitalWorkers.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace M2OSS.Service.DigitalWorkers.Service
{
    public class BpaFileProviderService : IBpaFileProviderService
    {
        private readonly IDigitalWorkerActionLogRepository _actionLogRepo;


        public BpaFileProviderService(IDigitalWorkerActionLogRepository actionLogRepo)
        {
            _actionLogRepo = actionLogRepo;
        }

        public IReadOnlyCollection<BpaRecordDTO> GetBpa(string correlationId, bool isDebugMode)
        {
            var filePath = @"C:\MOSS\Bpa\WD iCatalog BPA Report_template new (22).xlsx";

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var results = new List<BpaRecordDTO>();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var workbook = new XLWorkbook(stream))
            {
                var sheet = workbook.Worksheet(1);
                var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

                int debugSuccessCount = 0;
                int debugSkippedCount = 0;

                for (int row = 16; row <= lastRow; row++)
                {
                    string material = sheet.Cell($"AC{row}").GetString().Trim().ToUpper();
                    if (string.IsNullOrWhiteSpace(material))
                        continue;

                    string status = sheet.Cell($"AD{row}").GetString()?.Trim().ToUpper();
                    if (status != "OPEN")
                        continue;

                    string type = sheet.Cell($"AO{row}").GetString()?.Trim().ToUpper();
                    if (type != "GOODS")
                        continue;

                    DateTime? expiry = TryParseDate(sheet.Cell($"V{row}").Value);
                    if (expiry.HasValue && expiry.Value < DateTime.Today)
                        continue;

                    // SAFE NUMERIC READING
                    decimal AT = GetSafeDecimal(sheet.Cell($"AT{row}"));
                    decimal AJ = GetSafeDecimal(sheet.Cell($"AJ{row}"));
                    decimal AK = GetSafeDecimal(sheet.Cell($"AK{row}"));
                    decimal AE = GetSafeDecimal(sheet.Cell($"AE{row}"));
                    decimal AR = GetSafeDecimal(sheet.Cell($"AR{row}"));
                    decimal releaseAmount = GetSafeDecimal(sheet.Cell($"AL{row}"));

                    decimal balanceQty = 0;
                    decimal releasedQty = AK;

                    // LOGIC
                    if (AT > 0)
                    {
                        balanceQty = AT;
                    }
                    else
                    {
                        if (releaseAmount > 0 && AR > 0)
                        {
                            releasedQty = releaseAmount / AR;
                        }

                        if (AJ > 0)
                        {
                            balanceQty = AJ - releasedQty;
                        }
                        else if (AE > 0 && AR > 0)
                        {
                            balanceQty = AE / AR;
                        }
                    }

                    // SKIP INVALID
                    if (balanceQty <= 0)
                    {
                        if (isDebugMode && debugSkippedCount < 10)
                        {
                            debugSkippedCount++;

                            _actionLogRepo.InsertLog(
                                "BPA_SERVICE",
                                "BPA_SKIPPED",
                                $"Material={material} skipped | ComputedQty={balanceQty}",
                                material,
                                "WARNING",
                                correlationId
                            );
                        }

                        continue;
                    }

                    // ADD RESULT (IMPORTANT — WAS MISSING)
                    results.Add(new BpaRecordDTO
                    {
                        MaterialNumber = material,
                        BalanceQty = balanceQty,
                        ExpiryDate = expiry
                    });

                    // DEBUG SUCCESS
                    if (isDebugMode && debugSuccessCount < 10)
                    {
                        debugSuccessCount++;

                        _actionLogRepo.InsertLog(
                            "BPA_SERVICE",
                            "BPA_DEBUG",
                            $"Material={material} | AT={AT} | AJ={AJ} | AK={AK} | AE={AE} | AR={AR} | FinalQty={balanceQty}",
                            material,
                            "INFO",
                            correlationId
                        );
                    }
                }
            }

            return results;
        }

        private decimal TryParseDecimal(object value)
        {
            if (value == null) return 0;

            var str = value.ToString()?.Trim();

            if (string.IsNullOrEmpty(str))
                return 0;

            str = str.Replace(",", ""); 

            decimal.TryParse(str, out var result);

            return result;
        }

        private DateTime? TryParseDate(object value)
        {
            if (DateTime.TryParse(value?.ToString(), out var dt))
                return dt;
            return null;
        }

        private decimal GetSafeDecimal(IXLCell cell)
        {
            if (cell == null)
                return 0;

            try
            {
                return cell.GetValue<decimal>();
            }
            catch
            {
                var raw = cell.GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(raw))
                    return 0;

                raw = raw.Replace(",", "");

                decimal.TryParse(raw, out var result);

                return result;
            }
        }
    }
}



