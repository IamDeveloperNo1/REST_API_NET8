using Global = System.Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using REST_API_NET8.Models.SAP;
using REST_API_NET8.Data;
using Microsoft.EntityFrameworkCore;

namespace REST_API_NET8.Services
{
    /// <summary>
    /// Service Sap Function
    /// </summary>
    public class SapService
    {
        private readonly IConfiguration _configuration;
        private readonly FaceScanDbContext _lineBotDbContext;
        private readonly MesDbContext _mesDbContext;
        /// <summary>
        /// ใส่เพื่อไม่ให้ขึ้น Warning เพราะมันขัดตา
        /// </summary>
        /// <param name="configuration"></param>
        public SapService(IConfiguration configuration, FaceScanDbContext faceScanDbContext, MesDbContext mesDbContext)
        {
            _lineBotDbContext = faceScanDbContext;
            _configuration = configuration;
            _mesDbContext = mesDbContext;
        }

        public async Task<List<SapResponse>> getDataSapAsync(getDataSapFilter argument)
        {
            try
            {
                var exePath = _configuration["ExePath:getDataCoois"];
                Console.WriteLine(exePath);
                if (exePath == null || !System.IO.File.Exists(exePath))
                {
                    return new List<SapResponse>();
                }
                var dateNow = DateTime.Now;

                string statrDate = new DateTime(dateNow.Year, dateNow.Month, 1).ToString("yyyyMMdd", new Global.CultureInfo("en-US"));
                string endDate = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month)).ToString("yyyyMMdd", new Global.CultureInfo("en-US"));

                var startInfo = new ProcessStartInfo()
                {
                    FileName = exePath,
                    Arguments = $@"{argument.plant},{argument.condition},{statrDate},{endDate}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,

                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                int exitCode = process.ExitCode;

                var sapSql = @$"
        select
            substr(sc.orderjob,2) as ""JobOrder"",
            sc.basequantity as ""Target"",
            sc.basicstartdate::date as ""BasicStarDate"",
            sc.mes_to_sap as ""MesToSap"",
            sc.plant as ""Plant"",
            sc.create_date as ""CreateDate""
        from
            public.sap_caufv sc
        where 1=1
            and sc.plant = '{argument.plant}'
            {(argument.condition != "1" ? $@"and sc.material like '{argument.condition}'" : "")}
        order by sc.create_date desc
    ";
                Console.WriteLine($@"Start Process Query SAP [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var sapList = await _lineBotDbContext.cooisDtos
                    .FromSqlRaw(sapSql)
                    .AsNoTracking()
                    .ToListAsync();

                if (sapList.Count == 0)
                    return new List<SapResponse>();

  
                Console.WriteLine($@"Start Process Select Order Where By Plant [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var jobOrders = sapList
                    .Where(x => x.Plant == argument.plant)
                    .Select(x => x.JobOrder)
                    .Distinct()
                    .ToList();
                Console.WriteLine($@"End Process Select Order Where By Plant [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var jobOrderIn = string.Join(",", jobOrders.Select(x => $"'{x}'"));

                var mesSql = $@"
                select
                    t1.Code as 'OrderJob',
                    t1.Prod_Code as 'ModelCode',
                    t1.FactoryNo as 'PlantCode',
                    t1.Prod_Desc as 'ProductDesc',
                    t1.Production_Line_Code as 'ProductionLine',
                    t1.ActualQuantity as 'OfflineT1',
                    sum(case when t2.`Type` = 1 then 1 else 0 end) as 'Offline',
                    sum(case when t2.`Type` = 0 then 1 else 0 end) as 'Online',
                    DATE_FORMAT(t1.EST, '%Y-%m-%d') as 'StartDate',
                    case
                        when t1.Active = 0 then 'Not Issued'
                        when t1.Active = 1 then 'Issued'
                        when t1.Active = 3 then 'toBeIssued'
                        else t1.Active
                    end as 'Statuses',
                    t1.EU as 'Unit',
                    t1.Quantity as 'PlantQty',
                    ifnull(t3.qty, 0) as 'SapOk',
                    ifnull(t4.qty, 0) as 'SapPending',
                    ifnull(t3.qty, 0) - ifnull(count(t2.ID), 0) as 'OfflineNotPostToSap',
                    MAX(t2.ScanTime) as 'LastScan',
                    DATE_FORMAT(t5.EST, '%Y-%m-%d') as 'MesStartDate',
                    t5.Edition as 'ProdVersion'
                from
                    cosmo_im_{argument.plant}.base_production_order_t t1
                left join cosmo_im_{argument.plant}.bns_pm_scanhistory_month t2 on
                    t1.Code = t2.Code
                    and t2.Code in ({jobOrderIn})
                left join (
                    select
                        orderid,
                        SUM(finAmount) as qty
                    from
                        cosmo_im_{argument.plant}.bns_pm_wmswork
                    where
                        {(argument.plant == "9774" || argument.plant == "9772" ? "Active" : "ZTYPE")} = 'I'
                        and orderid in ({jobOrderIn})
                    group by
                        orderid
                ) as t3 on
                    t1.Code = t3.orderid
                left join (
                    select
                        orderid,
                        SUM(finAmount) as qty
                    from
                        cosmo_im_{argument.plant}.bns_pm_wmswork
                    where
                        {(argument.plant == "9774" || argument.plant == "9772" ? "Active" : "ZTYPE")} in ('A', 'E')
                        and orderid in ({jobOrderIn})
                    group by
                        orderid
                ) as t4 on
                    t1.Code = t4.orderid
                left join cosmo_im_{argument.plant}.bns_pm_productionorder t5 on t5.Code = t1.Code
                where
                    1 = 1
                    and t1.ProdPlanType in ('EX00', 'DM00')
                    and t1.Edition not in ('ICP1', 'ICP2', 'ICPS')
                    and t1.Code in ({jobOrderIn})
                group by
                    t1.Code
                order by
                    t1.FactoryNo,
                    t1.EST,
                    t1.Code;
    ";
                Console.WriteLine($@"Start Process Query FG [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var mesList = await _mesDbContext.mesForJoinSapDtos
                    .FromSqlRaw(mesSql)
                    .AsNoTracking()
                    .ToListAsync();
                Console.WriteLine($@"End Process Query FG [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");

                string mesSfgSql = $@"
                select
                    t1.FactoryNo as 'PlantCode',
                    DATE_FORMAT(t1.EST, '%Y-%m-%d') as 'StartDate',
                    t1.Code as 'OrderJob',
                    case
                        when t1.Active = 0 then 'Not Issued'
                        when t1.Active = 1 then 'Issued'
                        when t1.Active = 3 then 'toBeIssued'
                        else t1.Active
                    end as 'Statuses',
                    t1.Production_Line_Code as 'ProductionLine',
                    t1.Prod_Code as 'ModelCode',
                    t1.Prod_Desc as 'ProductDesc',
                    t1.Quantity as 'PlantQty',
                    t1.EU as 'Unit',
                    t1.ActualQuantity as 'OfflineT1',
                    ifnull(t2.qty, 0) as 'Offline',
                    ifnull(t3.qty, 0) as 'SapOk',
                    ifnull(t4.qty, 0) as 'SapPending',
                    ifnull(t3.qty, 0) - ifnull(t2.qty, 0) as 'OfflineNotPostToSap',
                    t2.last_scan as 'LastScan',
                    null as 'Online',
                    DATE_FORMAT(t5.EST, '%Y-%m-%d') as 'MesStartDate',
                    t5.Edition as 'ProdVersion'
                from
                    cosmo_im_{argument.plant}.base_production_order_t t1
                left join (
                    select
                        WorkUser_MOrderCode,
                        SUM(Offline_Num) as qty,
                        MAX(ScanTime) as last_scan
                    from
                        cosmo_im_{argument.plant}.bns_pm_semioffline
                    where Prod_Code in ({jobOrderIn})
                    group by
                        WorkUser_MOrderCode
                ) t2 on
                    t1.Code = t2.WorkUser_MOrderCode
                left join (
                    select
                        orderid,
                        SUM(finAmount) as qty
                    from
                        cosmo_im_{argument.plant}.bns_pm_wmswork
                    where
                        {(argument.plant == "9774" || argument.plant == "9772" ? "Active" : "ZTYPE")} = 'I'
                        and orderid in ({jobOrderIn})
                    group by
                        orderid
                ) t3 on
                    t1.Code = t3.orderid
                left join (
                    select
                        orderid,
                        SUM(finAmount) as qty
                    from
                        cosmo_im_{argument.plant}.bns_pm_wmswork
                    where
                        {(argument.plant == "9774" || argument.plant == "9772" ? "Active" : "ZTYPE")} in ('A', 'E')
                        and orderid in ({jobOrderIn})
                    group by
                        orderid
                ) t4 on
                    t1.Code = t4.orderid
                left join cosmo_im_{argument.plant}.bns_pm_productionorder t5 on t5.Code = t1.Code
                where
                    1=1
                    and t1.ProdPlanType not in ('EX00', 'DM00')
                    and t1.Edition not in ('ICP1', 'ICP2', 'ICPS')
                    and t1.Code in ({jobOrderIn})
                group by
                    t1.Code
                order by
                    t1.FactoryNo,
                    t1.EST,
                    t1.Code;
            
            
            ";

                Console.WriteLine($@"Start Process Query SFG [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var mesSfgList = await _mesDbContext.mesForJoinSapDtos
                    .FromSqlRaw(mesSfgSql)
                    .AsNoTracking()
                    .ToListAsync();
                Console.WriteLine($@"End Process Query SFG [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
             
             
                Console.WriteLine($@"Start Process Join Data 3 Table [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                var dataJoinMesSfg =
                    from t1 in sapList

                    join t2 in mesList
                        on t1.JobOrder equals t2.OrderJob
                        into mesGroup
                    from t2 in mesGroup.DefaultIfEmpty()

                    join t3 in mesSfgList
                        on t1.JobOrder equals t3.OrderJob
                        into mesSfgGroup
                    from t3 in mesSfgGroup.DefaultIfEmpty()

                    select new SapResponse()
                    {
                        OrderJob = t1.JobOrder,
                        Target = t1.Target,
                        BasicStarDate = t1.BasicStarDate,
                        FgCode = t2?.ModelCode ?? t3?.ModelCode,
                        FgDesc = t2?.ProductDesc ?? t3?.ProductDesc,
                        LineCode = t2?.ProductionLine ?? t3?.ProductionLine,
                        Offline = t2?.Offline ?? t3?.Offline,
                        Online = t2?.Online ?? t3?.Online,
                        MesToSap = t1.MesToSap,
                        LastScan = t2?.LastScan ?? t3?.LastScan,
                        OfflineNotPostToSap = t2?.OfflineNotPostToSap ?? t3?.OfflineNotPostToSap,
                        PlantQty = t2?.PlantQty ?? t3?.PlantQty,
                        SapOk = t2?.SapOk ?? t3?.SapOk,
                        SapPending = t2?.SapPending ?? t3?.SapPending,
                        StartDate = t2?.StartDate ?? t3?.StartDate,
                        Statuses = t2?.Statuses ?? t3?.Statuses,
                        Unit = t2?.Unit ?? t3?.Unit,
                        Plant = t1.Plant,
                        CreateDate = t1.CreateDate,
                        MesStartDate = t2?.MesStartDate ?? t3?.MesStartDate,
                        ProdVersion = t2?.ProdVersion ?? t3?.ProdVersion
                    };
                Console.WriteLine($@"End Process API [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]");
                return dataJoinMesSfg.ToList();
            }
            catch (System.Exception ex)
            {

                return new List<SapResponse>();
            }
        }
    }
}