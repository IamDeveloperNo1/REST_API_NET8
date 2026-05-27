using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace REST_API_NET8.Models.SAP
{
    [Keyless]
    public class CooisDto
    {
        public string? JobOrder { get; set; }
        public string? Target { get; set; }
        public string? MesToSap { get; set; }
        public string? Plant { get; set; }
        public DateTime? BasicStarDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
    public class CooisFilter
    {
        public string? plant { get; set; }
        /// <summary>
        /// Condition เลือกอย่างใดอย่าง 1
        /// </summary>
        public string? conditionLike { get; set; }
        public string? conditionByOne { get; set; }
        public string? startDate { get; set; }
        public string? endDate { get; set; }
        public bool? isRunProgram { get; set; }
    }
    public class MesForJoinSapDto
    {
        public string? OrderJob {get;set;}
        public string? ModelCode { get; set; }
        public string? PlantCode { get; set; }
        public string? ProductDesc { get; set; }
        public string? ProductionLine { get; set; }
        public int? OfflineT1 { get; set; }
        public int? Offline { get; set; }
        public int? Online { get; set; }
        public string? StartDate { get; set; }
        public string? Statuses { get; set; }
        public string? Unit { get; set; }
        public int? PlantQty { get; set; }
        public int? SapOk { get; set; }
        public int? SapPending { get; set; }
        public int? OfflineNotPostToSap { get; set; }
        public DateTime? LastScan { get; set; }
        public DateTime? MesStartDate { get; set; }
        public string? ProdVersion { get; set; }
    }

    public class SapResponse
    {
        public string? OrderJob { get; set; }
        public string? FgCode { get; set; }
        public string? FgDesc { get; set; }
        public string? Target { get; set; }
        public int? Online { get; set; }
        public DateTime? BasicStarDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Offline { get; set; }
        public string? MesToSap { get; set; }
        public string? LineCode { get; set; }
        public string? StartDate { get; set; }
        public string? Statuses { get; set; }
        public string? Unit { get; set; }
        public string? Plant { get; set; }
        public int? PlantQty { get; set; }
        public int? SapOk { get; set; }
        public int? SapPending { get; set; }
        public int? OfflineNotPostToSap { get; set; }
        public DateTime? LastScan { get; set; }
        public DateTime? MesStartDate { get; set; }
        public string? ProdVersion { get; set; }
    }
}